using UnityEngine;
using System.Collections.Generic;
using MSE.Input;

public static class DrumsInput
{
    static readonly Dictionary<int, Dictionary<Note.DrumPad, Shortcut?>> laneCountGamepadOverridesDict = new Dictionary<int, Dictionary<Note.DrumPad, Shortcut?>>()
    {
        {
            4, new Dictionary<Note.DrumPad, Shortcut?>()
            {
                { Note.DrumPad.Orange, Shortcut.DrumPadGreen },
                { Note.DrumPad.Green, null }
            }
        }
    };

    public static bool GetPadPressedInput(GamepadDevice gamepad, Note.DrumPad drumFret, LaneInfo laneInfo)
    {
        if (gamepad == null || !gamepad.Connected)
            return false;

        Dictionary<Note.DrumPad, Shortcut?> inputOverrideDict;
        Shortcut? overrideInput;

        if (laneCountGamepadOverridesDict.TryGetValue(laneInfo.laneCount, out inputOverrideDict) && inputOverrideDict.TryGetValue(drumFret, out overrideInput))
        {
            bool inputFound = false;

            if (overrideInput != null)
                inputFound = ShortcutInput.GetInputDown((Shortcut)overrideInput);

            return inputFound;
        }

        switch (drumFret)
        {
            case Note.DrumPad.Red:
                return ShortcutInput.GetInputDown(Shortcut.DrumPadRed);

            case Note.DrumPad.Yellow:
                return ShortcutInput.GetInputDown(Shortcut.DrumPadYellow);

            case Note.DrumPad.Blue:
                return ShortcutInput.GetInputDown(Shortcut.DrumPadBlue);

            case Note.DrumPad.Orange:
                return ShortcutInput.GetInputDown(Shortcut.DrumPadOrange);

            case Note.DrumPad.Green:
                return ShortcutInput.GetInputDown(Shortcut.DrumPadGreen);

            case Note.DrumPad.Kick:
                return ShortcutInput.GetInputDown(Shortcut.DrumPadKick);

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
}
