using System.Collections.Generic;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public class IndicatorPressedInputSystem : SystemManagerState.System
{
    HitAnimation[] animations;

    readonly Dictionary<Note.GuitarFret, bool> bannedFretInputs = new Dictionary<Note.GuitarFret, bool>()
    {
        {   Note.GuitarFret.Open, true },
    };

    readonly Dictionary<Note.DrumPad, bool> bannedDrumPadInputs = new Dictionary<Note.DrumPad, bool>()
    {
        {   Note.DrumPad.Kick, true },
    };

    delegate void UpdateFn();

    UpdateFn updateFn;

    public IndicatorPressedInputSystem(HitAnimation[] animations, Chart.GameMode gameMode)
    {
        this.animations = animations;

        switch (gameMode)
        {
            case Chart.GameMode.Drums:
                {
                    updateFn = UpdateDrumPadPresses;
                    break;
                }
            default:
                {
                    updateFn = UpdateGuitarFretPresses;
                    break;
                }
        } 
    }

    public override void SystemUpdate()
    {
        updateFn();
    }

    public override void SystemExit()
    {
        for (int i = 0; i < animations.Length; ++i)
        {
            if (!animations[i].running)
                animations[i].Release();
        }
    }

    void UpdateGuitarFretPresses()
    {
        foreach (Note.GuitarFret fret in EnumX<Note.GuitarFret>.Values)
        {
            if (bannedFretInputs.ContainsKey(fret))
                continue;

            if (GuitarInput.GetFretInput(fret))
            {
                animations[(int)fret].Press();
            }
            else
                animations[(int)fret].Release();
        }
    }

    void UpdateDrumPadPresses()
    {
        ChartEditor editor = ChartEditor.Instance;
        LaneInfo laneInfo = editor.laneInfo;

        foreach (Note.DrumPad drumPad in EnumX<Note.DrumPad>.Values)
        {
            if (bannedDrumPadInputs.ContainsKey(drumPad))
                continue;

            bool lanePressed = false;
            switch (Globals.gameSettings.drumsModeOptions)
            {
                case GameSettings.DrumModeOptions.ProDrums:
                    {
                        lanePressed = DrumsInput.GetTomPressedInput(drumPad, laneInfo) || DrumsInput.GetCymbalPressedInput(drumPad, laneInfo);
                        break;
                    }
                default:
                    {
                        lanePressed = DrumsInput.GetPadPressedInput(drumPad, laneInfo);
                        break;
                    }
            }

            if (lanePressed)
                animations[(int)drumPad].Press();
            else
                animations[(int)drumPad].Release();
        }
    }
}
