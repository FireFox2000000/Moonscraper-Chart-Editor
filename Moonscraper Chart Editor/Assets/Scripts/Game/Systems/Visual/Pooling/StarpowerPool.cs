// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class StarpowerPool : SongObjectPool
{
    public StarpowerPool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<StarpowerController>())
            throw new System.Exception("No StarpowerController attached to prefab");
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        StarpowerController controller = sCon as StarpowerController;

        // Assign pooled objects
        controller.starpower = (Starpower)songObject;
        controller.gameObject.SetActive(true);
    }
}
