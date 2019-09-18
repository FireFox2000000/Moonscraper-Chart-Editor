using UnityEngine;

namespace GuitarInput
{
    public static class GamepadInputExtension
    {
        public static bool GetFretInput(this GamepadInput gamepad, Note.GuitarFret fret)
        {
            switch (fret)
            {
                case (Note.GuitarFret.Green):
                    return gamepad.GetButton(GamepadInput.Button.A);

                case (Note.GuitarFret.Red):
                    return gamepad.GetButton(GamepadInput.Button.B);

                case (Note.GuitarFret.Yellow):
                    return gamepad.GetButton(GamepadInput.Button.Y);

                case (Note.GuitarFret.Blue):
                    return gamepad.GetButton(GamepadInput.Button.X);

                case (Note.GuitarFret.Orange):
                    return gamepad.GetButton(GamepadInput.Button.LB);

                case (Note.GuitarFret.Open):
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

            foreach (Note.GuitarFret fret in EnumX<Note.GuitarFret>.Values)
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

        public static bool GetFretInputControllerOrKeyboard(this GamepadInput gamepad, Note.GuitarFret fret)
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

        public static int GetFretInputMaskControllerOrKeyboard(this GamepadInput gamepad)
        {
            int gamepadMask = GetFretInputMask(gamepad);
            return gamepadMask != 0 ? gamepadMask : GetFretInputMaskKeyboard();
        }

        public static bool GetStrumInputKeyboard()
        {
            return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow);
        }

        public static bool GetStrumInputControllerOrKeyboard(this GamepadInput gamepad)
        {
            return GetStrumInput(gamepad) || GetStrumInputKeyboard();
        }
    }
}
