using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public class DrumRollPool : SongObjectPool
{
    public DrumRollPool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<DrumRollController>())
            throw new System.Exception("No DrumRollController attached to prefab");
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        DrumRollController controller = sCon as DrumRollController;

        // Assign pooled objects
        controller.drumRoll = (DrumRoll)songObject;
        controller.gameObject.SetActive(true);
    }
}
