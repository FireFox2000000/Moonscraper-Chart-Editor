using UnityEngine;
using System.Collections;

public class MoveStarpower : PlaceStarpower {

    protected override void Controls()
    {
        if (Input.GetMouseButtonUp(0))
        {
            AddObject();

            Destroy(gameObject);
        }
    }

    public void Init(StarPower starpower)
    {
        this.starpower = starpower;
        GetComponent<StarpowerController>().Init(starpower);
    }

    protected override void AddObject()
    {
        StarPower starpowerToAdd = new StarPower(starpower);
        editor.currentChart.Add(starpowerToAdd);
        editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;
    }
}
