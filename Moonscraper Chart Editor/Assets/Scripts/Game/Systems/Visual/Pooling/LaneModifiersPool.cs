// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class LaneModifiersPool : SongObjectPool
{
    public LaneModifiersPool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<LaneModifierController>())
            throw new System.Exception("No LaneModifierController attached to prefab");
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        LaneModifierController controller = sCon as LaneModifierController;

        // Assign pooled objects
        controller.laneModifier = (LaneModifier)songObject;
        controller.gameObject.SetActive(true);
    }
}
