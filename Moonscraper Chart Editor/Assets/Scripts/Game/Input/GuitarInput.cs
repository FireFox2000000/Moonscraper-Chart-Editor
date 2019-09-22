using UnityEngine;
using MSE.Input;

public static class GuitarInput
{
    public static bool GetFretInput(GamepadDevice gamepad, Note.GuitarFret fret)
    {
        if (gamepad == null || !gamepad.Connected)
            return false;

        switch (fret)
        {
            case (Note.GuitarFret.Green):
                return gamepad.GetButton(GamepadDevice.Button.A);

            case (Note.GuitarFret.Red):
                return gamepad.GetButton(GamepadDevice.Button.B);

            case (Note.GuitarFret.Yellow):
                return gamepad.GetButton(GamepadDevice.Button.Y);

            case (Note.GuitarFret.Blue):
                return gamepad.GetButton(GamepadDevice.Button.X);

            case (Note.GuitarFret.Orange):
                return gamepad.GetButton(GamepadDevice.Button.LB);

            case (Note.GuitarFret.Open):
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
        if (gamepad == null || !gamepad.Connected)
            return false;

        return gamepad.GetButtonPressed(GamepadDevice.Button.DPadDown) || gamepad.GetButtonPressed(GamepadDevice.Button.DPadUp);
    }

    public static float GetWhammyInput(GamepadDevice gamepad)
    {
        if (gamepad == null || !gamepad.Connected)
            return 0;

        return gamepad.GetAxis(GamepadDevice.Axis.RightStickX);
    }

    /******************************** Keyboard Alts ********************************************/

    public static bool GetFretInputKeyboard(Note.GuitarFret fret)
    {
        switch (fret)
        {
            case (Note.GuitarFret.Green):
                return Input.GetKey(KeyCode.Alpha1);

            case (Note.GuitarFret.Red):
                return Input.GetKey(KeyCode.Alpha2);

            case (Note.GuitarFret.Yellow):
                return Input.GetKey(KeyCode.Alpha3);

            case (Note.GuitarFret.Blue):
                return Input.GetKey(KeyCode.Alpha4);

            case (Note.GuitarFret.Orange):
                return Input.GetKey(KeyCode.Alpha5);

            case (Note.GuitarFret.Open):
                return false;

            default:
                Debug.LogError("Unhandled note type for guitar input: " + fret);
                break;
        }

        return false;
    }

    public static bool GetFretInputControllerOrKeyboard(GamepadDevice gamepad, Note.GuitarFret fret)
    {
        return GetFretInput(gamepad, fret) || GetFretInputKeyboard(fret);
    }

    public static int GetFretInputMaskKeyboard()
    {
        int inputMask = 0;

        foreach (Note.GuitarFret fret in EnumX<Note.GuitarFret>.Values)
        {
            if (GetFretInputKeyboard(fret))
                inputMask |= 1 << (int)fret;
        }

        return inputMask;
    }

    public static int GetFretInputMaskControllerOrKeyboard(GamepadDevice gamepad)
    {
        int gamepadMask = GetFretInputMask(gamepad);
        return gamepadMask != 0 ? gamepadMask : GetFretInputMaskKeyboard();
    }

    public static bool GetStrumInputKeyboard()
    {
        return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow);
    }

    public static bool GetStrumInputControllerOrKeyboard(GamepadDevice gamepad)
    {
        return GetStrumInput(gamepad) || GetStrumInputKeyboard();
    }
}
