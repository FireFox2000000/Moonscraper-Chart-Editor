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
    }

    protected override void Update()
    {
        starpower.chart.Remove(starpower);
        base.Update();
        starpower.chart.Add(starpower);
    }

    protected override void AddObject()
    {
        StarPower starpowerToAdd = new StarPower(starpower);
        editor.currentChart.Add(starpowerToAdd);
        editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;
    }
}
