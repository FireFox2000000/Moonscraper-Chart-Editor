// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

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

    protected override void SetSongObjectAndController()
    {
        starpower = new Starpower(0, 0);

        controller = GetComponent<StarpowerController>();
        controller.starpower = starpower;
        spRen = GetComponent<Renderer>();
        record = new List<ActionHistory.Action>();
    }

    protected override void Controls()
    {
        if (!GameSettings.keysModeEnabled)
        {
            if (Toolpane.currentTool == Toolpane.Tools.Starpower && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
            {
                if (lastPlacedSP == null)
                {
                    // Check if there's a starpower already in that position
                    int arrayPos = SongObjectHelper.FindObjectPosition(starpower, editor.currentChart.starPower);
                    if (arrayPos != SongObjectHelper.NOTFOUND)       // Found an object that matches
                    {
                        overwrittenSP = (Starpower)editor.currentChart.starPower[arrayPos].Clone();
                    }

                    AddObject();
                }
                else
                {
                    UpdateLastPlacedSp();
                }
            }
        }
        else if (ShortcutInput.GetInput(Shortcut.AddSongObject))
        {
            if (ShortcutInput.GetInputDown(Shortcut.AddSongObject))
            {
                var searchArray = editor.currentChart.starPower;
                int pos = SongObjectHelper.FindObjectPosition(starpower, searchArray);
                if (pos == SongObjectHelper.NOTFOUND)
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
            else if (lastPlacedSP != null)
            {
                UpdateLastPlacedSp();
            }
        }
    }

    void UpdateLastPlacedSp()
    {
        uint prevSpLength = lastPlacedSP.length;

        lastPlacedSP.SetLengthByPos(objectSnappedChartPos);

        if (prevSpLength != lastPlacedSP.length)
        {
            int index, length;
            var notes = editor.currentChart.notes;
            uint maxLength = prevSpLength > lastPlacedSP.length ? prevSpLength : lastPlacedSP.length;

            SongObjectHelper.GetRange(notes, lastPlacedSP.tick, lastPlacedSP.tick + maxLength, out index, out length);

            for (int i = index; i < index + length; ++i)
            {
                if (notes[i].controller)
                    notes[i].controller.SetDirty();
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

        if ((Input.GetMouseButtonUp(0) && !GameSettings.keysModeEnabled) || (GameSettings.keysModeEnabled && Input.GetButtonUp("Add Object")))
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

        SetNotesDirty(starpowerToAdd);
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

        SetNotesDirty(starpowerToAdd);

        return record.ToArray();
    }

    static void SetNotesDirty(Starpower sp)
    {
        int start, length;
        var notes = sp.chart.notes;
        SongObjectHelper.GetRange(notes, sp.tick, sp.tick + sp.length, out start, out length);

        for (int i = start; i < start + length; ++i)
        {
            if (notes[i].controller)
                notes[i].controller.SetDirty();
        }
    }

    static ActionHistory.Action[] CapPrevAndNextPreInsert(Starpower sp, Chart chart)
    {
        List<ActionHistory.Action> record = new List<ActionHistory.Action>();
        int arrayPos = SongObjectHelper.FindClosestPosition(sp, chart.starPower);

        if (arrayPos != SongObjectHelper.NOTFOUND)       // Found an object that matches
        {
            if (chart.starPower[arrayPos] < sp)
            {
                ++arrayPos;
            }
           
            if (arrayPos > 0 && chart.starPower[arrayPos - 1].tick < sp.tick)
            {
                
                Starpower prevSp = chart.starPower[arrayPos - 1];
                // Cap previous sp
                if (prevSp.tick + prevSp.length > sp.tick)
                {
                    Starpower originalPrev = (Starpower)prevSp.Clone();
                    
                    prevSp.length = sp.tick - prevSp.tick;
                    record.Add(new ActionHistory.Modify(originalPrev, prevSp));
                }
            }

            if (arrayPos < chart.starPower.Count && chart.starPower[arrayPos].tick > sp.tick)
            {       
                Starpower nextSp = chart.starPower[arrayPos];

                // Cap self
                if (sp.tick + sp.length > nextSp.tick)
                {
                    Starpower originalNext = (Starpower)nextSp.Clone();
                    sp.length = nextSp.tick - sp.tick;
                    record.Add(new ActionHistory.Modify(originalNext, nextSp));
                }
            }
        }

        return record.ToArray();
    }
}
