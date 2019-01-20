// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

// DEPRECATED IN FAVOUR OF GroupMove.cs

using UnityEngine;
using System.Collections.Generic;

public class MoveStarpower : PlaceStarpower {

    protected override void Controls()
    {
        MovementControls();
    }

    public void Init(Starpower starpower)
    {
        this.starpower = new Starpower(starpower);
        controller.starpower = this.starpower;
        initObject = this.starpower.Clone();
    }

    protected override void AddObject()
    {
        /*
        StarPower starpowerToAdd = new StarPower(starpower);
        editor.currentChart.Add(starpowerToAdd);
        editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;

        editor.actionHistory.Insert(new ActionHistory.Action[] { new ActionHistory.Delete(initObject), new ActionHistory.Add(starpowerToAdd) });*/
        List<ActionHistory.Action> record = new List<ActionHistory.Action>();
        record.Add(new ActionHistory.Delete(initObject));
        //record.AddRange(AddObjectToCurrentChart(starpower, editor));

        if (!initObject.AllValuesCompare(starpower))
            editor.actionHistory.Insert(record.ToArray());
    }
}
