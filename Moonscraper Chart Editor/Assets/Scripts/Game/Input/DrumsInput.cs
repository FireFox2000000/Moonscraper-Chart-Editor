// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using MSE.Input;

public static class DrumsInput
{
    static readonly Dictionary<int, Dictionary<Note.DrumPad, MSChartEditorInputActions?>> laneCountGamepadOverridesDict = new Dictionary<int, Dictionary<Note.DrumPad, MSChartEditorInputActions?>>()
    {
        {
            4, new Dictionary<Note.DrumPad, MSChartEditorInputActions?>()
            {
                { Note.DrumPad.Orange, MSChartEditorInputActions.DrumPadGreen },
                { Note.DrumPad.Green, null }
            }
        }
    };

    public static bool GetPadPressedInput(Note.DrumPad drumFret, LaneInfo laneInfo)
    {
        Dictionary<Note.DrumPad, MSChartEditorInputActions?> inputOverrideDict;
        MSChartEditorInputActions? overrideInput;

        if (laneCountGamepadOverridesDict.TryGetValue(laneInfo.laneCount, out inputOverrideDict) && inputOverrideDict.TryGetValue(drumFret, out overrideInput))
        {
            bool inputFound = false;

            if (overrideInput != null)
                inputFound = MSChartEditorInput.GetInputDown((MSChartEditorInputActions)overrideInput);

            return inputFound;
        }

        switch (drumFret)
        {
            case Note.DrumPad.Red:
                return MSChartEditorInput.GetInputDown(MSChartEditorInputActions.DrumPadRed);

            case Note.DrumPad.Yellow:
                return MSChartEditorInput.GetInputDown(MSChartEditorInputActions.DrumPadYellow);

            case Note.DrumPad.Blue:
                return MSChartEditorInput.GetInputDown(MSChartEditorInputActions.DrumPadBlue);

            case Note.DrumPad.Orange:
                return MSChartEditorInput.GetInputDown(MSChartEditorInputActions.DrumPadOrange);

            case Note.DrumPad.Green:
                return MSChartEditorInput.GetInputDown(MSChartEditorInputActions.DrumPadGreen);

            case Note.DrumPad.Kick:
                return MSChartEditorInput.GetInputDown(MSChartEditorInputActions.DrumPadKick);

            default:
                Debug.LogError("Unhandled note type for drum input: " + drumFret);
                break;
        }

        return false;
    }

    public static int GetPadPressedInputMask(LaneInfo laneInfo)
    {
        int inputMask = 0;

        foreach (Note.DrumPad pad in EnumX<Note.DrumPad>.Values)
        {
            if (GetPadPressedInput(pad, laneInfo))
                inputMask |= 1 << (int)pad;
        }

        return inputMask;
    }
}
