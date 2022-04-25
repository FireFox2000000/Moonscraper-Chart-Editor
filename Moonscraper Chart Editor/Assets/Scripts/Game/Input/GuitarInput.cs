// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public static class GuitarInput
{
    public const float kNoWhammy = -1.0f;

    public static bool GetFretInput(Note.GuitarFret fret)
    {
        switch (fret)
        {
            case Note.GuitarFret.Green:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretGreen);

            case Note.GuitarFret.Red:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretRed);

            case Note.GuitarFret.Yellow:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretYellow);

            case Note.GuitarFret.Blue:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretBlue);

            case Note.GuitarFret.Orange:
                return MSChartEditorInput.GetInput(MSChartEditorInputActions.GuitarFretOrange);

            case Note.GuitarFret.Open:
                return false;

            default:
                Debug.LogError("Unhandled note type for guitar input: " + fret);
                break;
        }

        return false;
    }

    public static int GetFretInputMask()
    {
        int inputMask = 0;

        foreach (Note.GuitarFret fret in EnumX<Note.GuitarFret>.Values)
        {
            if (GetFretInput(fret))
                inputMask |= 1 << (int)fret;
        }

        return inputMask;
    }

    public static bool GetStrumInput()
    {
        return MSChartEditorInput.GetInputDown(MSChartEditorInputActions.GuitarStrumDown) || MSChartEditorInput.GetInputDown(MSChartEditorInputActions.GuitarStrumUp);
    }

    public static float GetWhammyInput()
    {
        float? whammyValue = MSChartEditorInput.GetAxisMaybe(MSChartEditorInputActions.Whammy);
        return whammyValue.HasValue ? whammyValue.Value : kNoWhammy;
    }
}
