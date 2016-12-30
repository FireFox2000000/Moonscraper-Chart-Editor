using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(StarpowerController))]
public class PlaceStarpower : PlaceSongObject {
    public StarPower starpower { get { return (StarPower)songObject; } set { songObject = value; } }
    new public StarpowerController controller { get { return (StarpowerController)base.controller; } set { base.controller = value; } }

    protected override void Awake()
    {
        base.Awake();
        starpower = new StarPower(0, 0);

        controller = GetComponent<StarpowerController>();
        controller.starpower = starpower;
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Starpower && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            AddObject();
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        starpower.chart = editor.currentChart;
        base.Update();       
    }

    protected override void OnEnable()
    {
        editor.currentSelectedObject = starpower;
        Update();
    }

    protected override void AddObject()
    {
        StarPower starpowerToAdd = new StarPower(starpower);
        editor.currentChart.Add(starpowerToAdd);
        editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;
    }
}
