using UnityEngine;

namespace DrumsInput
{
    public static class GamepadInputExtension
    {
        public static bool GetPadPressedInput(this GamepadInput gamepad, Note.Drum_Fret_Type drumFret)
        {
            switch (drumFret)
            {
                case (Note.Drum_Fret_Type.RED):
                    return gamepad.GetButtonPressed(GamepadInput.Button.B);

                case (Note.Drum_Fret_Type.YELLOW):
                    return gamepad.GetButtonPressed(GamepadInput.Button.Y);

                case (Note.Drum_Fret_Type.BLUE):
                    return gamepad.GetButtonPressed(GamepadInput.Button.X);

                case (Note.Drum_Fret_Type.ORANGE):
                    return gamepad.GetButtonPressed(GamepadInput.Button.LB);

                case (Note.Drum_Fret_Type.GREEN):
                    return gamepad.GetButtonPressed(GamepadInput.Button.A);

                case (Note.Drum_Fret_Type.KICK):
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

            foreach (Note.Drum_Fret_Type pad in System.Enum.GetValues(typeof(Note.Drum_Fret_Type)))
            {
                if (gamepad.GetPadPressedInput(pad))
                    inputMask |= 1 << (int)pad;
            }

            return inputMask;
        }

        /******************************** Keyboard Alts ********************************************/

        public static bool GetPadPressedInputKeyboard(Note.Drum_Fret_Type drumFret)
        {
            switch (drumFret)
            {
                case (Note.Drum_Fret_Type.RED):
                    return Input.GetKeyDown(KeyCode.Alpha1);

                case (Note.Drum_Fret_Type.YELLOW):
                    return Input.GetKeyDown(KeyCode.Alpha2);

                case (Note.Drum_Fret_Type.BLUE):
                    return Input.GetKeyDown(KeyCode.Alpha3);

                case (Note.Drum_Fret_Type.ORANGE):
                    return Input.GetKeyDown(KeyCode.Alpha4);

                case (Note.Drum_Fret_Type.GREEN):
                    return Input.GetKeyDown(KeyCode.Alpha5);

                case (Note.Drum_Fret_Type.KICK):
                    return Input.GetKeyDown(KeyCode.Alpha0);

                default:
                    Debug.LogError("Unhandled note type for drum input: " + drumFret);
                    break;
            }

            return false;
        }

        public static bool GetPadInputControllerOrKeyboard(this GamepadInput gamepad, Note.Drum_Fret_Type drumFret)
        {
            return GetPadPressedInput(gamepad, drumFret) || GetPadPressedInputKeyboard(drumFret);
        }

        public static int GetPadPressedInputMaskKeyboard()
        {
            int inputMask = 0;

            foreach (Note.Drum_Fret_Type pad in System.Enum.GetValues(typeof(Note.Drum_Fret_Type)))
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
