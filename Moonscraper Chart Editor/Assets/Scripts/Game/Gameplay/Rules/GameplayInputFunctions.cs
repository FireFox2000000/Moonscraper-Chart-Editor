// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

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
            return note.guitarFret == Note.GuitarFret.Open;
        }
        else
        {
            // Chords
            if (note.isChord)
            {
                // Regular chords
                if (noteStreak == 0 || note.type == Note.NoteType.Strum)
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
                int singleNoteInput = inputMask >> (int)note.guitarFret;     // Anchor logic
                return singleNoteInput == 1;
            }
        }
    }

    public static bool ValidateStrum(Note note, bool canTap, bool strummed, uint noteStreak)
    {
        switch (note.type)
        {
            case (Note.NoteType.Tap):
                return canTap || strummed;

            case (Note.NoteType.Hopo):
                return noteStreak > 0 || strummed;

            default:    // Strum
                return strummed;

        }
    }
}
