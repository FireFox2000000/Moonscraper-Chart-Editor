// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class SectionPool : SongObjectPool
{
    public SectionPool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<SectionController>())
            throw new System.Exception("No SectionController attached to prefab");
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        SectionController controller = sCon as SectionController;

        // Assign pooled objects
        controller.section = (Section)songObject;
        controller.gameObject.SetActive(true);
    }
}
