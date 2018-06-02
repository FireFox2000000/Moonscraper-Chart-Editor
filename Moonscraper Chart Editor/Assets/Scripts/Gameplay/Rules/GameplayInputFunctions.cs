
public static class GameplayInputFunctions  {

    public static bool ValidateFrets(Note note, int inputMask, uint noteStreak)
    {
        if (inputMask == 0)
        {
            return note.fret_type == Note.Fret_Type.OPEN;
        }
        else
        {
            // Chords
            if (note.IsChord)
            {
                // Regular chords
                if (noteStreak == 0 || note.type == Note.Note_Type.Strum)
                {
                    return inputMask == note.mask;
                }
                // HOPO or tap chords. Insert Exile chord anchor logic.
                else
                {
                    // Bit-shift to the right to compensate for anchor logic
                    int shiftedNoteMask = note.mask;
                    int shiftCount = 0;

                    while ((shiftedNoteMask & 1) != 1)
                    {
                        shiftedNoteMask >>= 1;
                        ++shiftCount;
                    }

                    int shiftedInputMask = inputMask;

                    shiftedInputMask >>= shiftCount;

                    return shiftedInputMask == shiftedNoteMask;
                }
            }
            // Single notes
            else
            {
                int singleNoteInput = inputMask >> (int)note.fret_type;     // Anchor logic
                return singleNoteInput == 1;
            }
        }
    }

    public static bool ValidateStrum(Note note, bool canTap, bool strummed, uint noteStreak)
    {
        switch (note.type)
        {
            case (Note.Note_Type.Tap):
                return canTap || strummed;

            case (Note.Note_Type.Hopo):
                return noteStreak > 0 || strummed;

            default:    // Strum
                return strummed;

        }
    }
}
