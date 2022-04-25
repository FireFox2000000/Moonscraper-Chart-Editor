// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

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

        ChartEditor.Instance.events.leftyFlipToggledEvent.Register(OnLanesUpdated);
        ChartEditor.Instance.events.chartReloadedEvent.Register(OnLanesUpdated);
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
            ChartEditor.Instance.events.lanesChangedEvent.Fire(laneCount);
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
            Chart.GameMode gameMode = ChartEditor.Instance.currentGameMode;

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
        int newLaneCount = -1;
        if (ChartEditor.Instance.currentGameMode == Chart.GameMode.Drums && laneCount == SongConfig.PRO_DRUMS_LANE_COUNT)
        {
            newLaneCount = laneCount;
        }
        else if (!standardGamemodeToLaneCountMap.TryGetValue(ChartEditor.Instance.currentGameMode, out newLaneCount))
        {
            newLaneCount = -1;
        }

        if (laneCount >= 0)
            laneCount = newLaneCount;
    }

    public float GetLanePosition(int laneNum, bool flipLefty)
    {
        const float startOffset = positionRangeMin;
        const float endOffset = positionRangeMax;

        float incrementFactor = (endOffset - startOffset) / (laneCount - 1.0f);
        float position = Mathf.Min(startOffset + laneNum * incrementFactor, endOffset);

        if (flipLefty)
            position *= -1;

        return position;
    }
}
