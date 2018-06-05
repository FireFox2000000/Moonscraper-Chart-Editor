using UnityEngine;

namespace DrumsInput
{
    public static class GamepadInputExtension
    {
        public static bool GetPadPressedInput(this GamepadInput gamepad, Note.DrumPad drumFret)
        {
            switch (drumFret)
            {
                case (Note.DrumPad.RED):
                    return gamepad.GetButtonPressed(GamepadInput.Button.B);

                case (Note.DrumPad.YELLOW):
                    return gamepad.GetButtonPressed(GamepadInput.Button.Y);

                case (Note.DrumPad.BLUE):
                    return gamepad.GetButtonPressed(GamepadInput.Button.X);

                case (Note.DrumPad.ORANGE):
                    return gamepad.GetButtonPressed(GamepadInput.Button.LB);

                case (Note.DrumPad.GREEN):
                    return gamepad.GetButtonPressed(GamepadInput.Button.A);

                case (Note.DrumPad.KICK):
                    return gamepad.GetButtonPressed(GamepadInput.Button.RB);

                default:
                    Debug.LogError("Unhandled note type for drum input: " + drumFret);
                    break;
            }

            return false;
        }

        public static int GetPadPressedInputMask(this GamepadInput gamepad)
        {
            int inputMask = 0;

            foreach (Note.DrumPad pad in System.Enum.GetValues(typeof(Note.DrumPad)))
            {
                if (gamepad.GetPadPressedInput(pad))
                    inputMask |= 1 << (int)pad;
            }

            return inputMask;
        }

        /******************************** Keyboard Alts ********************************************/

        public static bool GetPadPressedInputKeyboard(Note.DrumPad drumFret)
        {
            switch (drumFret)
            {
                case (Note.DrumPad.RED):
                    return Input.GetKeyDown(KeyCode.Alpha1);

                case (Note.DrumPad.YELLOW):
                    return Input.GetKeyDown(KeyCode.Alpha2);

                case (Note.DrumPad.BLUE):
                    return Input.GetKeyDown(KeyCode.Alpha3);

                case (Note.DrumPad.ORANGE):
                    return Input.GetKeyDown(KeyCode.Alpha4);

                case (Note.DrumPad.GREEN):
                    return Input.GetKeyDown(KeyCode.Alpha5);

                case (Note.DrumPad.KICK):
                    return Input.GetKeyDown(KeyCode.Alpha0);

                default:
                    Debug.LogError("Unhandled note type for drum input: " + drumFret);
                    break;
            }

            return false;
        }

        public static bool GetPadInputControllerOrKeyboard(this GamepadInput gamepad, Note.DrumPad drumFret)
        {
            return GetPadPressedInput(gamepad, drumFret) || GetPadPressedInputKeyboard(drumFret);
        }

        public static int GetPadPressedInputMaskKeyboard()
        {
            int inputMask = 0;

            foreach (Note.DrumPad pad in System.Enum.GetValues(typeof(Note.DrumPad)))
            {
                if (GetPadPressedInputKeyboard(pad))
                {
                    inputMask |= 1 << (int)pad;
                }
            }

            return inputMask;
        }

        public static int GetPadPressedInputMaskControllerOrKeyboard(this GamepadInput gamepad)
        {
            int gamepadMask = GetPadPressedInputMask(gamepad);
            return gamepadMask != 0 ? gamepadMask : GetPadPressedInputMaskKeyboard();
        }
    }
}
