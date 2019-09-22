using UnityEngine;
using System.Collections.Generic;
using MSE.Input;

public static class DrumsInput
{
    static readonly Dictionary<int, Dictionary<Note.DrumPad, GamepadDevice.Button?>> laneCountGamepadOverridesDict = new Dictionary<int, Dictionary<Note.DrumPad, GamepadDevice.Button?>>()
    {
        {
            4, new Dictionary<Note.DrumPad, GamepadDevice.Button?>()
            {
                { Note.DrumPad.Orange, GamepadDevice.Button.A },
                { Note.DrumPad.Green, null }
            }
        }
    };

    public static bool GetPadPressedInput(GamepadDevice gamepad, Note.DrumPad drumFret, LaneInfo laneInfo)
    {
        if (gamepad == null || !gamepad.Connected)
            return false;

        Dictionary<Note.DrumPad, GamepadDevice.Button?> inputOverrideDict;
        GamepadDevice.Button? overrideInput;

        if (laneCountGamepadOverridesDict.TryGetValue(laneInfo.laneCount, out inputOverrideDict) && inputOverrideDict.TryGetValue(drumFret, out overrideInput))
        {
            bool inputFound = false;

            if (overrideInput != null)
                inputFound = gamepad.GetButtonPressed((GamepadDevice.Button)overrideInput);

            return inputFound;
        }

        switch (drumFret)
        {
            case (Note.DrumPad.Red):
                return gamepad.GetButtonPressed(GamepadDevice.Button.B);

            case (Note.DrumPad.Yellow):
                return gamepad.GetButtonPressed(GamepadDevice.Button.Y);

            case (Note.DrumPad.Blue):
                return gamepad.GetButtonPressed(GamepadDevice.Button.X);

            case (Note.DrumPad.Orange):
                return gamepad.GetButtonPressed(GamepadDevice.Button.RB);

            case (Note.DrumPad.Green):
                return gamepad.GetButtonPressed(GamepadDevice.Button.A);

            case (Note.DrumPad.Kick):
                return gamepad.GetButtonPressed(GamepadDevice.Button.LB);

            default:
                Debug.LogError("Unhandled note type for drum input: " + drumFret);
                break;
        }

        return false;
    }

    public static int GetPadPressedInputMask(GamepadDevice gamepad, LaneInfo laneInfo)
    {
        int inputMask = 0;

        foreach (Note.DrumPad pad in EnumX<Note.DrumPad>.Values)
        {
            if (GetPadPressedInput(gamepad, pad, laneInfo))
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

    public static bool GetPadInputControllerOrKeyboard(GamepadDevice gamepad, Note.DrumPad drumFret, LaneInfo laneInfo)
    {
        return GetPadPressedInput(gamepad, drumFret, laneInfo) || GetPadPressedInputKeyboard(drumFret, laneInfo);
    }

    public static int GetPadPressedInputMaskKeyboard(LaneInfo laneInfo)
    {
        int inputMask = 0;

        foreach (Note.DrumPad pad in EnumX<Note.DrumPad>.Values)
        {
            if (GetPadPressedInputKeyboard(pad, laneInfo))
            {
                inputMask |= 1 << (int)pad;
            }
        }

        return inputMask;
    }

    public static int GetPadPressedInputMaskControllerOrKeyboard(GamepadDevice gamepad, LaneInfo laneInfo)
    {
        int gamepadMask = GetPadPressedInputMask(gamepad, laneInfo);
        return gamepadMask != 0 ? gamepadMask : GetPadPressedInputMaskKeyboard(laneInfo);
    }
}
