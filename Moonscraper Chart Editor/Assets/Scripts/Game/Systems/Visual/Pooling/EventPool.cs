// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class EventPool : SongObjectPool
{
    public EventPool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<EventController>())
            throw new System.Exception("No EventController attached to prefab");
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        EventController controller = sCon as EventController;

        // Assign pooled objects
        controller.songEvent = (MoonscraperChartEditor.Song.Event)songObject;
        controller.gameObject.SetActive(true);
    }
}
