// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using MoonscraperChartEditor.Song;

public class DrumModeProperties : UpdateableService
{
    enum LaneCountOptions
    {
        // Keep this in the same order as the UI
        LaneCount5,
        LaneCount4,     
    }

    [SerializeField]
    Dropdown m_laneCountDropdown;

    [SerializeField]
    Dropdown m_drumsModeOptionDropdown;

    readonly static Dictionary<LaneCountOptions, int> r_laneOptionToLaneCount = new Dictionary<LaneCountOptions, int>()
    {
        { LaneCountOptions.LaneCount5, 5 },
        { LaneCountOptions.LaneCount4, 4 },
    };

    readonly static Dictionary<int, LaneCountOptions> r_laneCountToLaneOption = r_laneOptionToLaneCount.ToDictionary((i) => i.Value, (i) => i.Key);

    protected override void Start()
    {
        base.Start();

        ChartEditor editor = ChartEditor.Instance;
        editor.events.chartReloadedEvent.Register(OnChartReload);

        
        OnChartReload();
    }

    public override void OnServiceUpdate()
    {
        
    }

    void OnChartReload()
    {
        bool isDrums = ChartEditor.Instance.currentChart.gameMode == Chart.GameMode.Drums;
        gameObject.SetActive(isDrums);

        if (isDrums)
        {
            LaneCountOptions option;

            if (!r_laneCountToLaneOption.TryGetValue(Globals.gameSettings.drumsLaneCount, out option))
            {
                option = LaneCountOptions.LaneCount5;
            }

            int intLastKnownLaneCount = (int)option;
            bool forceReload = intLastKnownLaneCount != ChartEditor.Instance.laneInfo.laneCount;

            m_drumsModeOptionDropdown.value = (int)Globals.gameSettings.drumsModeOptions;
            m_laneCountDropdown.value = intLastKnownLaneCount;

            if (forceReload)
            {
                int desiredLaneCount;
                if (r_laneOptionToLaneCount.TryGetValue(option, out desiredLaneCount))
                {
                    ChartEditor.Instance.uiServices.menuBar.SetLaneCount(desiredLaneCount);
                }
            }
        }
    }

    public void OnLaneCountDropdownValueChanged(int value)
    {
        LaneCountOptions option = (LaneCountOptions)value;
        ChartEditor editor = ChartEditor.Instance;

        int desiredLaneCount;
        if (r_laneOptionToLaneCount.TryGetValue(option, out desiredLaneCount))
        {
            Globals.gameSettings.drumsLaneCount.value = desiredLaneCount;
            editor.uiServices.menuBar.SetLaneCount(desiredLaneCount);
            editor.uiServices.menuBar.LoadCurrentInstumentAndDifficulty();       
        }

        // Not allowed 5 lane pro drums
        if (option == LaneCountOptions.LaneCount5 && Globals.gameSettings.drumsModeOptions == GameSettings.DrumModeOptions.ProDrums)
        {
            m_drumsModeOptionDropdown.value = (int)GameSettings.DrumModeOptions.Standard;
        }
    }

    public void OnModeOptionDropdownValueChanged(int value)
    {
        GameSettings.DrumModeOptions option = (GameSettings.DrumModeOptions)value;
        Globals.gameSettings.drumsModeOptions = option;

        // Not allowed 5 lane pro drums 
        if (option == GameSettings.DrumModeOptions.ProDrums && ChartEditor.Instance.laneInfo.laneCount != SongConfig.PRO_DRUMS_LANE_COUNT)
        {
            m_laneCountDropdown.value = (int)LaneCountOptions.LaneCount4;
        }

        ChartEditor.Instance.events.chartReloadedEvent.Fire();
    }
}
