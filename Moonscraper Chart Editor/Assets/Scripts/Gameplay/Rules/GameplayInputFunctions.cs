#define GAMEPAD

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public static class GameplayInputFunctions  {

    public static int GetFretInputMask(GamePadState? gamePad)
    {
        int inputMask = 0;
#if GAMEPAD
        if (GameplayManager.gamepad != null)
        {
            GamePadState gamepad = (GamePadState)GameplayManager.gamepad;

            if (gamepad.Buttons.A == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.GREEN;

            if (gamepad.Buttons.B == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.RED;

            if (gamepad.Buttons.Y == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.YELLOW;

            if (gamepad.Buttons.X == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.BLUE;

            if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.ORANGE;
        }
        else
        {
            // Keyboard controls
            for (int i = 0; i < 5; ++i)
            {
                if (Input.GetKey((i + 1).ToString()))
                    inputMask |= 1 << i;
            }
        }
#else

        if (Input.GetButton("Fret0"))
            inputMask |= 1 << (int)Note.Fret_Type.GREEN;

        if (Input.GetButton("Fret1"))
            inputMask |= 1 << (int)Note.Fret_Type.RED;

        if (Input.GetButton("Fret2"))
            inputMask |= 1 << (int)Note.Fret_Type.YELLOW;

        if (Input.GetButton("Fret3"))
            inputMask |= 1 << (int)Note.Fret_Type.BLUE;

        if (Input.GetButton("Fret4"))
            inputMask |= 1 << (int)Note.Fret_Type.ORANGE;

#endif
        return inputMask;
    }

    public static bool GetStrumInput(GamePadState? gamepad, float previousStrumValue, out float strumValue)
    {
        strumValue = 0;
#if GAMEPAD
        if (gamepad != null)
        {
            if (((GamePadState)gamepad).DPad.Down == ButtonState.Pressed)
                strumValue = -1;
            else if (((GamePadState)gamepad).DPad.Up == ButtonState.Pressed)
                strumValue = 1;
        }
        else
        {
            // Keyboard controls
            if (Input.GetButtonDown("Strum Up"))
                strumValue = 1;
            else if (Input.GetButtonDown("Strum Down"))
                strumValue = -1;
        }
#else
        strumValue = Input.GetAxisRaw("Strum");    
#endif

        // Finalise if a strum has occured or not
        if (strumValue != 0 && strumValue != previousStrumValue)
            return true;
        else
            return false;
    }

    public static bool GetStrumInput(GamePadState? gamepad, float previousStrumValue)
    {
        float dummy;
        return GetStrumInput(gamepad, previousStrumValue, out dummy);
    }

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
}
