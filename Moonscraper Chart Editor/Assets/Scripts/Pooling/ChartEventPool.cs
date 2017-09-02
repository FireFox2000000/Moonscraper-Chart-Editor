using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Activate(ChartEvent[] range, int index, int length)
    {
        base.Activate(range, index, length);
    }
}
