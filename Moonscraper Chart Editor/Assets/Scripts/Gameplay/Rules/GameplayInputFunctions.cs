
public static class GameplayInputFunctions  {

    public static bool ValidateFrets(Note note, int inputMask, uint noteStreak)
    {
        if (inputMask == 0)
        {
            if (note.fret_type == Note.Fret_Type.OPEN)
                return true;
            else
                return false;
        }
        else
        {
            // Chords
            if (note.IsChord)
            {
                // Regular chords
                if (noteStreak == 0 || note.type == Note.Note_Type.Strum)
                {
                    if (inputMask == note.mask)
                        return true;
                    else
                        return false;
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

                    if (shiftedInputMask == shiftedNoteMask)
                        return true;
                    else
                        return false;
                }
            }
            // Single notes
            else
            {
                int singleNoteInput = inputMask >> (int)note.fret_type;     // Anchor logic
                if (singleNoteInput == 1)
                    return true;
                else
                    return false;
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

    // Todo- Keyboard controls and figure out kick input
    /*
    public static int GetDrumsInputMask(GamePadState? gamePad)
    {
        int inputMask = 0;

        if (GameplayManager.gamepad != null)
        {
            GamePadState gamepad = (GamePadState)GameplayManager.gamepad;

            if (gamepad.Buttons.A == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Drum_Fret_Type.GREEN;

            if (gamepad.Buttons.B == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Drum_Fret_Type.RED;

            if (gamepad.Buttons.Y == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Drum_Fret_Type.YELLOW;

            if (gamepad.Buttons.X == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Drum_Fret_Type.BLUE;

            if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Drum_Fret_Type.ORANGE;

            //if (gamepad.Buttons.RightShoulder == ButtonState.Pressed)
            //    inputMask |= 1 << (int)Note.Drum_Fret_Type.KICK;
        }
        else
        {
            const int drumsKey = 0;

            // Keyboard controls
            for (int i = 0; i < 5; ++i)
            {
                if (Input.GetKey((i + 1).ToString()))
                    inputMask |= 1 << i;
            }
        
            if (Input.GetKey((drumsKey).ToString()))
                inputMask |= 1 << (int)Note.Drum_Fret_Type.KICK;
        }

        return inputMask;
    }*/
}
