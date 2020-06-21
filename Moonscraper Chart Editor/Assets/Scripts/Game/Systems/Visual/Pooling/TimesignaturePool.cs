// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class TimesignaturePool : SongObjectPool
{
    public TimesignaturePool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<TimesignatureController>())
            throw new System.Exception("No TimesignatureController attached to prefab");
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        TimesignatureController controller = sCon as TimesignatureController;

        // Assign pooled objects
        controller.ts = (TimeSignature)songObject;
        controller.gameObject.SetActive(true);
    }
}
