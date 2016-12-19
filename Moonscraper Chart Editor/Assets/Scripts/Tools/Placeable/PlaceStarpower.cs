using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(StarpowerController))]
public class PlaceStarpower : PlaceSongObject {
    public StarPower starpower { get { return (StarPower)songObject; } set { songObject = value; } }

    protected override void Awake()
    {
        base.Awake();
        starpower = new StarPower(editor.currentSong, editor.currentChart, 0, 0);

        controller = GetComponent<StarpowerController>();
        ((StarpowerController)controller).starpower = starpower;
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
    }
}
