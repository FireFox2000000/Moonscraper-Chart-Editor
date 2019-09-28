using UnityEngine;
using MSE.Input;

public static class GuitarInput
{
    public static bool GetFretInput(GamepadDevice gamepad, Note.GuitarFret fret)
    {
        switch (fret)
        {
            case Note.GuitarFret.Green:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretGreen);

            case Note.GuitarFret.Red:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretRed);

            case Note.GuitarFret.Yellow:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretYellow);

            case Note.GuitarFret.Blue:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretBlue);

            case Note.GuitarFret.Orange:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretOrange);

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
        return MSChartEditorInput.GetInputDown(MSChartEditorInputActions.GuitarStrumDown) || MSChartEditorInput.GetInputDown(MSChartEditorInputActions.GuitarStrumUp);
    }

    public static float GetWhammyInput(GamepadDevice gamepad)
    {
        if (gamepad == null || !gamepad.Connected)
            return 0;

        return gamepad.GetAxis(GamepadDevice.Axis.RightStickX);
    }
}
