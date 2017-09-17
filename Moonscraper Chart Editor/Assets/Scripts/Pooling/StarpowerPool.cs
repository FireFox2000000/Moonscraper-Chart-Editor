// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Activate(Starpower[] range, int index, int length)
    {
        base.Activate(range, index, length);
    }
}
