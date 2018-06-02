
public static class GameplayInputFunctions  {
    public static int BitshiftToIgnoreLowerUnusedFrets(int bitmaskToShift, out int shiftCount)
    {
        shiftCount = 0;

        if (bitmaskToShift == 0)
            return 0;

        while ((bitmaskToShift & 1) != 1)
        {
            bitmaskToShift >>= 1;
            ++shiftCount;
        }

        return bitmaskToShift;
    }

    public static bool ValidateFrets(Note note, int inputMask, uint noteStreak, int extendedSustainsMask = 0)
    {
        inputMask &= ~(extendedSustainsMask & ~note.mask);

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
                    int shiftCount = 0;
                    int shiftedNoteMask = BitshiftToIgnoreLowerUnusedFrets(note.mask, out shiftCount);
                    
                    int shiftedInputMask = inputMask >> shiftCount;

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
