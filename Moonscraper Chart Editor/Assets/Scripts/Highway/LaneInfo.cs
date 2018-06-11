using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneInfo : MonoBehaviour {

    public Color[] laneColourPalette;

    public int[] guitarFretColourMap;
    public int[] drumPadColourMap;
    public int[] ghlGuitarFretColourMap;
    public int[] drumPadColourMap4LaneOverride;

    Dictionary<Chart.GameMode, int[]> standardGamemodePaletteMap;
    Dictionary<Chart.GameMode, Dictionary<int, int[]>> laneCountPaletteMapOverrides;

    int m_laneCount = 5;
    Dictionary<Chart.GameMode, int> standardGamemodeToLaneCountMap = new Dictionary<Chart.GameMode, int>()
    {
        { Chart.GameMode.Guitar, 5 },
        { Chart.GameMode.Drums, 5 },
        { Chart.GameMode.GHLGuitar, 6 },
    };

    public const float positionRangeMin = -2, positionRangeMax = 2;

    // Use this for initialization
    void Start()
    {
        standardGamemodePaletteMap = new Dictionary<Chart.GameMode, int[]>()
        {
            { Chart.GameMode.Guitar, guitarFretColourMap },
            { Chart.GameMode.Drums, drumPadColourMap },
            { Chart.GameMode.GHLGuitar, ghlGuitarFretColourMap },
        };

        laneCountPaletteMapOverrides = new Dictionary<Chart.GameMode, Dictionary<int, int[]>>()
        {
            {
                Chart.GameMode.Drums, new Dictionary<int, int[]>()
                {
                    { 4, drumPadColourMap4LaneOverride },
                }
            }
        };

        EventsManager.onLeftyFlipToggledEventList.Add(OnLanesUpdated);
        EventsManager.onChartReloadEventList.Add(OnLanesUpdated);
    }

    public int laneCount
    {
        get
        {
            return m_laneCount;
        }
        set
        {
            m_laneCount = value;
            EventsManager.FireLanesChangedEvent(laneCount);
        }
    }

    public int laneMask
    {
        get
        {
            return (1 << laneCount) - 1;
        }
    }

    public Color[] laneColours
    {
        get
        {
            Color[] colours = new Color[laneCount];
            int[] paletteMap;
            Chart.GameMode gameMode = ChartEditor.GetInstance().currentGameMode;

            if (!standardGamemodePaletteMap.TryGetValue(gameMode, out paletteMap))
                throw new System.Exception("Unable to find standard palette for current game mode");

            {
                Dictionary<int, int[]> overrideDict;
                int[] overridePaletteMap;
                if (laneCountPaletteMapOverrides.TryGetValue(gameMode, out overrideDict) && overrideDict.TryGetValue(laneCount, out overridePaletteMap))
                {
                    paletteMap = overridePaletteMap;
                }
            }

            for (int i = 0; i < colours.Length; ++i)
            {
                colours[i] = laneColourPalette[paletteMap[i]];
            }

            return colours;
        }
    }

    void OnLanesUpdated()
    {
        int newLaneCount;
        if (standardGamemodeToLaneCountMap.TryGetValue(ChartEditor.GetInstance().currentGameMode, out newLaneCount))
            laneCount = newLaneCount;
    }

    public float GetLanePosition(int laneNum, bool flipLefty)
    {
        const float startOffset = positionRangeMin;
        const float endOffset = positionRangeMax;

        float incrementFactor = (endOffset - startOffset) / (laneCount - 1.0f);
        float position = startOffset + laneNum * incrementFactor;

        if (flipLefty)
            position *= -1;

        return position;
    }
}
