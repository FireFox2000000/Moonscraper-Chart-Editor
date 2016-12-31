using UnityEngine;
using System.Collections;

public class GameplayManager : MonoBehaviour {
    uint noteStreak = 0;

    void Update()
    {
        /*
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown("joystick 1 button " + i))
            {
                print("joystick 1 button " + i);
            }
        }
        */
    }

    bool ValidateStrum(Note note)
    {
        switch (note.type)
        {
            case (Note.Note_Type.TAP):
                return true;
            case (Note.Note_Type.HOPO):
                if (noteStreak > 0)
                    return true;
                else if (Input.GetButtonDown("Strum"))
                    return true;
                else
                    return false;
            default:    // Strum
                if (Input.GetButtonDown("Strum"))
                    return true;
                else
                    return false;
        }
    }

	bool ValidateFrets(Note note)
    {
        int inputMask = 0;

        if (Input.GetButton("FretGreen"))
            inputMask |= 1 << (int)Note.Fret_Type.GREEN;

        if (Input.GetButton("FretRed"))
            inputMask |= 1 << (int)Note.Fret_Type.RED;

        if (Input.GetButton("FretYellow"))
            inputMask |= 1 << (int)Note.Fret_Type.YELLOW;

        if (Input.GetButton("FretBlue"))
            inputMask |= 1 << (int)Note.Fret_Type.BLUE;

        if (Input.GetButton("FretOrange"))
            inputMask |= 1 << (int)Note.Fret_Type.ORANGE;

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
                if (note.type == Note.Note_Type.STRUM)
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
                    int shiftedInputMask = inputMask;
                    while ((shiftedInputMask & 1) != 1)
                        shiftedInputMask >>= 1;

                    int shiftedNoteMask = note.mask;
                    while ((shiftedNoteMask & 1) != 1)
                        shiftedNoteMask >>= 1;

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
}
