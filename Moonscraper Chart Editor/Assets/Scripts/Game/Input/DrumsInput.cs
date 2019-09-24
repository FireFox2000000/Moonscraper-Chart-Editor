using UnityEngine;
using System.Collections.Generic;
using MSE.Input;

public static class DrumsInput
{
    static readonly Dictionary<int, Dictionary<Note.DrumPad, GameplayAction?>> laneCountGamepadOverridesDict = new Dictionary<int, Dictionary<Note.DrumPad, GameplayAction?>>()
    {
        {
            4, new Dictionary<Note.DrumPad, GameplayAction?>()
            {
                { Note.DrumPad.Orange, GameplayAction.DrumPadGreen },
                { Note.DrumPad.Green, null }
            }
        }
    };

    public static bool GetPadPressedInput(GamepadDevice gamepad, Note.DrumPad drumFret, LaneInfo laneInfo)
    {
        if (gamepad == null || !gamepad.Connected)
            return false;

        Dictionary<Note.DrumPad, GameplayAction?> inputOverrideDict;
        GameplayAction? overrideInput;

        if (laneCountGamepadOverridesDict.TryGetValue(laneInfo.laneCount, out inputOverrideDict) && inputOverrideDict.TryGetValue(drumFret, out overrideInput))
        {
            bool inputFound = false;

            if (overrideInput != null)
                inputFound = GameplayInput.GetInputDown((GameplayAction)overrideInput);

            return inputFound;
        }

        switch (drumFret)
        {
            case Note.DrumPad.Red:
                return GameplayInput.GetInputDown(GameplayAction.DrumPadRed);

            case Note.DrumPad.Yellow:
                return GameplayInput.GetInputDown(GameplayAction.DrumPadYellow);

            case Note.DrumPad.Blue:
                return GameplayInput.GetInputDown(GameplayAction.DrumPadBlue);

            case Note.DrumPad.Orange:
                return GameplayInput.GetInputDown(GameplayAction.DrumPadOrange);

            case Note.DrumPad.Green:
                return GameplayInput.GetInputDown(GameplayAction.DrumPadGreen);

            case Note.DrumPad.Kick:
                return GameplayInput.GetInputDown(GameplayAction.DrumPadKick);

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
