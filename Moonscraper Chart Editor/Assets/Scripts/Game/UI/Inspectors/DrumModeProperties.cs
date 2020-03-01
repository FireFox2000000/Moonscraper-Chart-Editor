using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    readonly Dictionary<LaneCountOptions, int> r_laneOptionToLaneCount = new Dictionary<LaneCountOptions, int>()
    {
        { LaneCountOptions.LaneCount5, 5 },
        { LaneCountOptions.LaneCount4, 4 },
    };

    LaneCountOptions m_lastKnownLaneCount = LaneCountOptions.LaneCount5;

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
            int intLastKnownLaneCount = (int)m_lastKnownLaneCount;
            bool forceReload = intLastKnownLaneCount != ChartEditor.Instance.laneInfo.laneCount;
            m_laneCountDropdown.value = intLastKnownLaneCount;
            if (forceReload)
            {
                int desiredLaneCount;
                if (r_laneOptionToLaneCount.TryGetValue(m_lastKnownLaneCount, out desiredLaneCount))
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

        m_lastKnownLaneCount = option;

        int desiredLaneCount;
        if (r_laneOptionToLaneCount.TryGetValue(m_lastKnownLaneCount, out desiredLaneCount))
        {
            editor.uiServices.menuBar.SetLaneCount(desiredLaneCount);
            editor.uiServices.menuBar.LoadCurrentInstumentAndDifficulty();
        }
    }
}
