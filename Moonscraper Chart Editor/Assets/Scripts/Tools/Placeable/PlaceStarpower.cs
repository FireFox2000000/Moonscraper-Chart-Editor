using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(StarpowerController))]
public class PlaceStarpower : PlaceSongObject {
    public Starpower starpower { get { return (Starpower)songObject; } set { songObject = value; } }
    new public StarpowerController controller { get { return (StarpowerController)base.controller; } set { base.controller = value; } }

    Starpower lastPlacedSP = null;
    List<ActionHistory.Action> record;
    Renderer spRen;
    Starpower overwrittenSP = null;

    protected override void Awake()
    {
        base.Awake();
        starpower = new Starpower(0, 0);

        controller = GetComponent<StarpowerController>();
        controller.starpower = starpower;
        spRen = GetComponent<Renderer>();
        record = new List<ActionHistory.Action>();
    }

    protected override void Controls()
    {
        if (!Globals.lockToStrikeline)
        {
            if (Toolpane.currentTool == Toolpane.Tools.Starpower && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
            {
                if (lastPlacedSP == null)
                {
                    // Check if there's a starpower already in that position
                    int arrayPos = SongObject.FindObjectPosition(starpower, editor.currentChart.starPower);
                    if (arrayPos != Globals.NOTFOUND)       // Found an object that matches
                    {
                        overwrittenSP = (Starpower)editor.currentChart.starPower[arrayPos].Clone();
                    }

                    AddObject();
                }
                else
                {
                    lastPlacedSP.SetLengthByPos(objectSnappedChartPos);
                }
            }
        }
        else if (Input.GetButtonDown("Add Object"))
        {
            SongObject[] searchArray = editor.currentChart.starPower;
            int pos = SongObject.FindObjectPosition(starpower, searchArray);
            if (pos == Globals.NOTFOUND)
            {
                editor.actionHistory.Insert(new ActionHistory.Add(starpower));
                AddObject();
            }
            else
            {
                editor.actionHistory.Insert(new ActionHistory.Delete(searchArray[pos]));
                searchArray[pos].Delete();
                editor.currentSelectedObject = null;
            }
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        starpower.chart = editor.currentChart;
        if (lastPlacedSP != null)
            spRen.enabled = false;
        else
            spRen.enabled = true;
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
    }

    protected override void OnEnable()
    {
        editor.currentSelectedObject = starpower;
        
        Update();
    }

    protected override void AddObject()
    {
        Starpower starpowerToAdd = new Starpower(starpower);
        record.AddRange(CapPrevAndNextPreInsert(starpowerToAdd, editor.currentChart));
        editor.currentChart.Add(starpowerToAdd);
        //editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;

        lastPlacedSP = starpowerToAdd;
    }

    public static ActionHistory.Action[] AddObjectToCurrentChart(Starpower starpower, ChartEditor editor, bool update = true, bool copy = true)
    {
        List<ActionHistory.Action> record = new List<ActionHistory.Action>();

        Starpower starpowerToAdd;
        if (copy)
            starpowerToAdd = new Starpower(starpower);
        else
            starpowerToAdd = starpower;

        record.AddRange(CapPrevAndNextPreInsert(starpowerToAdd, editor.currentChart));
        ActionHistory.Action overwriteRecord = OverwriteActionHistory(starpowerToAdd, editor.currentChart.starPower);
        if (overwriteRecord != null)
            record.Add(overwriteRecord);

        editor.currentChart.Add(starpowerToAdd, update);
        //editor.CreateStarpowerObject(starpowerToAdd);
        editor.currentSelectedObject = starpowerToAdd;

        return record.ToArray();
    }

    static ActionHistory.Action[] CapPrevAndNextPreInsert(Starpower sp, Chart chart)
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
                
                Starpower prevSp = chart.starPower[arrayPos - 1];
                // Cap previous sp
                if (prevSp.position + prevSp.length > sp.position)
                {
                    Starpower originalPrev = (Starpower)prevSp.Clone();
                    
                    prevSp.length = sp.position - prevSp.position;
                    record.Add(new ActionHistory.Modify(originalPrev, prevSp));
                }
            }

            if (arrayPos < chart.starPower.Length && chart.starPower[arrayPos].position > sp.position)
            {       
                Starpower nextSp = chart.starPower[arrayPos];

                // Cap self
                if (sp.position + sp.length > nextSp.position)
                {
                    Starpower originalNext = (Starpower)nextSp.Clone();
                    sp.length = nextSp.position - sp.position;
                    record.Add(new ActionHistory.Modify(originalNext, nextSp));
                }
            }
        }

        return record.ToArray();
    }
}
