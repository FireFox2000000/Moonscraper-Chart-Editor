using UnityEngine;
using System.Collections;

public class MoveStarpower : PlaceStarpower {

    protected override void Controls()
    {
        MovementControls();
    }

    public void Init(StarPower starpower)
    {
        this.starpower = starpower;
        controller.Init(starpower);
        initObject = this.starpower.Clone();
    }

    protected override void AddObject()
    {
        StarPower starpowerToAdd = new StarPower(starpower);
        editor.currentChart.Add(starpowerToAdd);
        editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;

        editor.actionHistory.Insert(new ActionHistory.Action[] { new ActionHistory.Delete(initObject), new ActionHistory.Add(starpowerToAdd) });
    }
}
