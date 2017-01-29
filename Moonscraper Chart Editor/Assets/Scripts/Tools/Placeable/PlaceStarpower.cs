using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(StarpowerController))]
public class PlaceStarpower : PlaceSongObject {
    public StarPower starpower { get { return (StarPower)songObject; } set { songObject = value; } }
    new public StarpowerController controller { get { return (StarpowerController)base.controller; } set { base.controller = value; } }

    StarPower lastPlacedSP = null;
    List<ActionHistory.Action> record;
    Renderer spRen;
    StarPower overwrittenSP = null;

    protected override void Awake()
    {
        base.Awake();
        starpower = new StarPower(0, 0);

        controller = GetComponent<StarpowerController>();
        controller.starpower = starpower;
        spRen = GetComponent<Renderer>();
        record = new List<ActionHistory.Action>();
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
                    record.Add(new ActionHistory.Add(lastPlacedSP));
                else if (!overwrittenSP.AllValuesCompare(lastPlacedSP))
                    record.Add(new ActionHistory.Modify(overwrittenSP, lastPlacedSP));
            }

            if (record.Count > 0)
            {
                //Debug.Log(record.Count);
                editor.actionHistory.Insert(record.ToArray());
            }

            // Reset
            lastPlacedSP = null;
            overwrittenSP = null;
            record.Clear();
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
        record.AddRange(CapPrevAndNextPreInsert(starpowerToAdd, editor.currentChart));
        editor.currentChart.Add(starpowerToAdd);
        editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;

        lastPlacedSP = starpowerToAdd;
    }

    public static ActionHistory.Action[] AddObjectToCurrentChart(StarPower starpower, ChartEditor editor, bool update = true)
    {
        List<ActionHistory.Action> record = new List<ActionHistory.Action>();

        StarPower starpowerToAdd = new StarPower(starpower);
        record.AddRange(CapPrevAndNextPreInsert(starpowerToAdd, editor.currentChart));
        ActionHistory.Action overwriteRecord = OverwriteActionHistory(starpowerToAdd, editor.currentChart.starPower);
        if (overwriteRecord != null)
            record.Add(overwriteRecord);

        editor.currentChart.Add(starpowerToAdd, update);
        editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;

        return record.ToArray();
    }

    static ActionHistory.Action[] CapPrevAndNextPreInsert(StarPower sp, Chart chart)
    {
        List<ActionHistory.Action> record = new List<ActionHistory.Action>();
        int arrayPos = SongObject.FindClosestPosition(sp, chart.starPower);

        if (arrayPos != Globals.NOTFOUND)       // Found an object that matches
        {
            if (chart.starPower[arrayPos] < sp)
            {
                ++arrayPos;
            }
           
            if (arrayPos > 0 && chart.starPower[arrayPos - 1].position < sp.position)
            {
                
                StarPower prevSp = chart.starPower[arrayPos - 1];
                // Cap previous sp
                if (prevSp.position + prevSp.length > sp.position)
                {
                    StarPower originalPrev = (StarPower)prevSp.Clone();
                    
                    prevSp.length = sp.position - prevSp.position;
                    record.Add(new ActionHistory.Modify(originalPrev, prevSp));
                }
            }

            if (arrayPos < chart.starPower.Length && chart.starPower[arrayPos].position > sp.position)
            {       
                StarPower nextSp = chart.starPower[arrayPos];

                // Cap self
                if (sp.position + sp.length > nextSp.position)
                {
                    StarPower originalNext = (StarPower)nextSp.Clone();
                    sp.length = nextSp.position - sp.position;
                    record.Add(new ActionHistory.Modify(originalNext, nextSp));
                }
            }
        }

        return record.ToArray();
    }
}
