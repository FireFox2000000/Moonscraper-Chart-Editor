using UnityEngine;

public class GuitarInput : GamepadInput {

    public bool GetFretInput(Note.Fret_Type fret)
    {
        switch (fret)
        {
            case (Note.Fret_Type.GREEN):
                return GetButton(Button.A);

            case (Note.Fret_Type.RED):
                return GetButton(Button.B);

            case (Note.Fret_Type.YELLOW):
                return GetButton(Button.Y);

            case (Note.Fret_Type.BLUE):
                return GetButton(Button.X);

            case (Note.Fret_Type.ORANGE):
                return GetButton(Button.LB);

            case (Note.Fret_Type.OPEN):
                return false;

            default:
                Debug.LogError("Unhandled note type for guitar input: " + fret);
                break;
        }

        return false;
    }

    public int GetFretInputMask()
    {
        int inputMask = 0;

        var fretEnums = System.Enum.GetValues(typeof(Note.Fret_Type));

        foreach (Note.Fret_Type fret in System.Enum.GetValues(typeof(Note.Fret_Type)))
        {
            if (GetFretInput(fret))
                inputMask |= 1 << (int)fret;
        }

        return inputMask;
    }

    public bool GetStrumInput()
    {
        return GetButtonPressed(Button.DPadDown) || GetButtonPressed(Button.DPadUp);
    }

    public float GetWhammyInput()
    {
        return GetAxis(Axis.RightStickX);
    }
}
