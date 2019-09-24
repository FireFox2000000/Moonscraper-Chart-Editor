using UnityEngine;
using MSE.Input;

public static class GuitarInput
{
    public static bool GetFretInput(GamepadDevice gamepad, Note.GuitarFret fret)
    {
        switch (fret)
        {
            case Note.GuitarFret.Green:
                return GameplayInput.GetInput(GameplayAction.GuitarFretGreen);

            case Note.GuitarFret.Red:
                return GameplayInput.GetInput(GameplayAction.GuitarFretRed);

            case Note.GuitarFret.Yellow:
                return GameplayInput.GetInput(GameplayAction.GuitarFretYellow);

            case Note.GuitarFret.Blue:
                return GameplayInput.GetInput(GameplayAction.GuitarFretBlue);

            case Note.GuitarFret.Orange:
                return GameplayInput.GetInput(GameplayAction.GuitarFretOrange);

            case Note.GuitarFret.Open:
                return false;

            default:
                Debug.LogError("Unhandled note type for guitar input: " + fret);
                break;
        }

        return false;
    }

    public static int GetFretInputMask(GamepadDevice gamepad)
    {
        int inputMask = 0;

        foreach (Note.GuitarFret fret in EnumX<Note.GuitarFret>.Values)
        {
            if (GetFretInput(gamepad, fret))
                inputMask |= 1 << (int)fret;
        }

        return inputMask;
    }

    public static bool GetStrumInput(GamepadDevice gamepad)
    {
        return GameplayInput.GetInputDown(GameplayAction.GuitarStrumDown) || GameplayInput.GetInputDown(GameplayAction.GuitarStrumUp);
    }

    public static float GetWhammyInput(GamepadDevice gamepad)
    {
        if (gamepad == null || !gamepad.Connected)
            return 0;

        return gamepad.GetAxis(GamepadDevice.Axis.RightStickX);
    }
}
