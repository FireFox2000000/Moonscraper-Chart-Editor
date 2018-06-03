using UnityEngine;

namespace GuitarInput
{
    public static class GamepadInputExtension
    {
        public static bool GetFretInput(this GamepadInput gamepad, Note.Fret_Type fret)
        {
            switch (fret)
            {
                case (Note.Fret_Type.GREEN):
                    return gamepad.GetButton(GamepadInput.Button.A);

                case (Note.Fret_Type.RED):
                    return gamepad.GetButton(GamepadInput.Button.B);

                case (Note.Fret_Type.YELLOW):
                    return gamepad.GetButton(GamepadInput.Button.Y);

                case (Note.Fret_Type.BLUE):
                    return gamepad.GetButton(GamepadInput.Button.X);

                case (Note.Fret_Type.ORANGE):
                    return gamepad.GetButton(GamepadInput.Button.LB);

                case (Note.Fret_Type.OPEN):
                    return false;

                default:
                    Debug.LogError("Unhandled note type for guitar input: " + fret);
                    break;
            }

            return false;
        }

        public static int GetFretInputMask(this GamepadInput gamepad)
        {
            int inputMask = 0;

            foreach (Note.Fret_Type fret in System.Enum.GetValues(typeof(Note.Fret_Type)))
            {
                if (gamepad.GetFretInput(fret))
                    inputMask |= 1 << (int)fret;
            }

            return inputMask;
        }

        public static bool GetStrumInput(this GamepadInput gamepad)
        {
            return gamepad.GetButtonPressed(GamepadInput.Button.DPadDown) || gamepad.GetButtonPressed(GamepadInput.Button.DPadUp);
        }

        public static float GetWhammyInput(this GamepadInput gamepad)
        {
            return gamepad.GetAxis(GamepadInput.Axis.RightStickX);
        }
    }
}
