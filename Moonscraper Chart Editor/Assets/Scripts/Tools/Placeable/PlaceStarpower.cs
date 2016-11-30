using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(StarpowerController))]
public class PlaceStarpower : ToolObject {
    protected StarPower starpower;
    StarpowerController controller;

    protected override void Awake()
    {
        base.Awake();
        starpower = new StarPower(editor.currentSong, editor.currentChart, 0, 0);

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
        base.Update();

        starpower.song = editor.currentSong;
        starpower.chart = editor.currentChart;
        starpower.position = objectSnappedChartPos;
    }

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;
    }

    void OnEnable()
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
