using UnityEngine;
using System.Collections.Generic;

namespace DrumsInput
{
    public static class GamepadInputExtension
    {
        static readonly Dictionary<int, Dictionary<Note.DrumPad, GamepadInput.Button>> laneCountGamepadOverridesDict = new Dictionary<int, Dictionary<Note.DrumPad, GamepadInput.Button>>()
        {
            {
                4, new Dictionary<Note.DrumPad, GamepadInput.Button>()
                {
                    { Note.DrumPad.Orange, GamepadInput.Button.A }
                }
            }
        };

        public static bool GetPadPressedInput(this GamepadInput gamepad, Note.DrumPad drumFret, LaneInfo laneInfo)
        {
            Dictionary<Note.DrumPad, GamepadInput.Button> inputOverrideDict;
            GamepadInput.Button overrideInput;

            if (laneCountGamepadOverridesDict.TryGetValue(laneInfo.laneCount, out inputOverrideDict) && inputOverrideDict.TryGetValue(drumFret, out overrideInput))
            {
                return gamepad.GetButtonPressed(overrideInput);
            }

            switch (drumFret)
            {
                case (Note.DrumPad.Red):
                    return gamepad.GetButtonPressed(GamepadInput.Button.B);

                case (Note.DrumPad.Yellow):
                    return gamepad.GetButtonPressed(GamepadInput.Button.Y);

                case (Note.DrumPad.Blue):
                    return gamepad.GetButtonPressed(GamepadInput.Button.X);

                case (Note.DrumPad.Orange):
                    return gamepad.GetButtonPressed(GamepadInput.Button.RB);

                case (Note.DrumPad.Green):
                    return gamepad.GetButtonPressed(GamepadInput.Button.A);

                case (Note.DrumPad.Kick):
                    return gamepad.GetButtonPressed(GamepadInput.Button.LB);

                default:
                    Debug.LogError("Unhandled note type for drum input: " + drumFret);
                    break;
            }

            return false;
        }

        public static int GetPadPressedInputMask(this GamepadInput gamepad, LaneInfo laneInfo)
        {
            int inputMask = 0;

            foreach (Note.DrumPad pad in System.Enum.GetValues(typeof(Note.DrumPad)))
            {
                if (gamepad.GetPadPressedInput(pad, laneInfo))
                    inputMask |= 1 << (int)pad;
            }

            return inputMask;
        }

        /******************************** Keyboard Alts ********************************************/

        public static bool GetPadPressedInputKeyboard(Note.DrumPad drumFret, LaneInfo laneInfo)
        {
            switch (drumFret)
            {
                case (Note.DrumPad.Red):
                    return Input.GetKeyDown(KeyCode.Alpha1);

                case (Note.DrumPad.Yellow):
                    return Input.GetKeyDown(KeyCode.Alpha2);

                case (Note.DrumPad.Blue):
                    return Input.GetKeyDown(KeyCode.Alpha3);

                case (Note.DrumPad.Orange):
                    return Input.GetKeyDown(KeyCode.Alpha4);

                case (Note.DrumPad.Green):
                    return Input.GetKeyDown(KeyCode.Alpha5);

                case (Note.DrumPad.Kick):
                    return Input.GetKeyDown(KeyCode.Alpha0);

                default:
                    Debug.LogError("Unhandled note type for drum input: " + drumFret);
                    break;
            }

            return false;
        }

        public static bool GetPadInputControllerOrKeyboard(this GamepadInput gamepad, Note.DrumPad drumFret, LaneInfo laneInfo)
        {
            return GetPadPressedInput(gamepad, drumFret, laneInfo) || GetPadPressedInputKeyboard(drumFret, laneInfo);
        }

        public static int GetPadPressedInputMaskKeyboard(LaneInfo laneInfo)
        {
            int inputMask = 0;

            foreach (Note.DrumPad pad in System.Enum.GetValues(typeof(Note.DrumPad)))
            {
                if (GetPadPressedInputKeyboard(pad, laneInfo))
                {
                    inputMask |= 1 << (int)pad;
                }
            }

            return inputMask;
        }

        public static int GetPadPressedInputMaskControllerOrKeyboard(this GamepadInput gamepad, LaneInfo laneInfo)
        {
            int gamepadMask = GetPadPressedInputMask(gamepad, laneInfo);
            return gamepadMask != 0 ? gamepadMask : GetPadPressedInputMaskKeyboard(laneInfo);
        }
    }
}
