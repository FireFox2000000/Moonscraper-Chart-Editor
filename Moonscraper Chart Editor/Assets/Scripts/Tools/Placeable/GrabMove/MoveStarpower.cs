using UnityEngine;
using System.Collections.Generic;

public class MoveStarpower : PlaceStarpower {

    protected override void Controls()
    {
        MovementControls();
    }

    public void Init(StarPower starpower)
    {
        this.starpower = new StarPower(starpower);
        controller.Init(this.starpower);
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
        record.AddRange(AddObjectToCurrentChart(starpower, editor));

        if (!initObject.AllValuesCompare(starpower))
            editor.actionHistory.Insert(record.ToArray());
    }
}
