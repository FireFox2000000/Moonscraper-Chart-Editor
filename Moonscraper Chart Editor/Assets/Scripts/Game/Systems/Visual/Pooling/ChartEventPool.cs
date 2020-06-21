// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class ChartEventPool : SongObjectPool
{
    public ChartEventPool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<ChartEventController>())
            throw new System.Exception("No EventController attached to prefab");
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        ChartEventController controller = sCon as ChartEventController;

        // Assign pooled objects
        controller.chartEvent = (ChartEvent)songObject;
        controller.gameObject.SetActive(true);
    }
}
