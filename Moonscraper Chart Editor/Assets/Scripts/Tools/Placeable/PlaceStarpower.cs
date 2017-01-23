using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(StarpowerController))]
public class PlaceStarpower : PlaceSongObject {
    public StarPower starpower { get { return (StarPower)songObject; } set { songObject = value; } }
    new public StarpowerController controller { get { return (StarpowerController)base.controller; } set { base.controller = value; } }

    StarPower lastPlacedSP = null;
    Renderer spRen;
    StarPower overwrittenSP = null;

    protected override void Awake()
    {
        base.Awake();
        starpower = new StarPower(0, 0);

        controller = GetComponent<StarpowerController>();
        controller.starpower = starpower;
        spRen = GetComponent<Renderer>();
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Starpower && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            if (lastPlacedSP == null)
            {
                // Check if there's a starpower already in that position
                int arrayPos = SongObject.FindObjectPosition(starpower, editor.currentChart.starPower);
                if (arrayPos != Globals.NOTFOUND)       // Found an object that matches
                {
                    overwrittenSP = (StarPower)editor.currentChart.starPower[arrayPos].Clone();
                }

                AddObject();
            }
            else
            {
                ((StarpowerController)lastPlacedSP.controller).TailDrag();
            }
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        starpower.chart = editor.currentChart;
        base.Update();

        if (Input.GetMouseButtonUp(0))
        {
            if (lastPlacedSP != null)
            {
                // Make a record of the last SP
                if (overwrittenSP == null)
                    editor.actionHistory.Insert(new ActionHistory.Add(lastPlacedSP));
                else
                    editor.actionHistory.Insert(new ActionHistory.Modify(overwrittenSP, lastPlacedSP));
            }

            // Reset
            lastPlacedSP = null;
            overwrittenSP = null;
        }

        if (lastPlacedSP != null)
            spRen.enabled = false;
        else
            spRen.enabled = true;
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

        lastPlacedSP = starpowerToAdd;
    }

    public static void AddObjectToCurrentChart(StarPower starpower, ChartEditor editor, bool update = true)
    {
        StarPower starpowerToAdd = new StarPower(starpower);
        editor.currentChart.Add(starpowerToAdd, update);
        editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;
    }

    protected static ActionHistory.Action CapSP(StarPower sp)
    {
        // Cap the previous sp

        // Cap own sp


        return null;
    }
}
