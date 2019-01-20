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
    Renderer spRen;

    protected override void SetSongObjectAndController()
    {
        starpower = new Starpower(0, 0);

        controller = GetComponent<StarpowerController>();
        controller.starpower = starpower;
        spRen = GetComponent<Renderer>();
    }

    protected override void Controls()
    {
        if (!GameSettings.keysModeEnabled)
        {
            if (Toolpane.currentTool == Toolpane.Tools.Starpower && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
            {
                if (lastPlacedSP == null)
                {
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

        uint newLength = lastPlacedSP.GetCappedLengthForPos(objectSnappedChartPos);

        if (prevSpLength != newLength)
        {
            editor.commandStack.Pop();
            editor.commandStack.Push(new SongEditAdd(new Starpower(lastPlacedSP.tick, newLength)));
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        starpower.chart = editor.currentChart;
        if (!GameSettings.keysModeEnabled)
            spRen.enabled = lastPlacedSP == null;

        base.Update();

        if ((Input.GetMouseButtonUp(0) && !GameSettings.keysModeEnabled) || (GameSettings.keysModeEnabled && Input.GetButtonUp("Add Object")))
        {
            // Reset
            lastPlacedSP = null;
        } 
    }

    protected override void OnEnable()
    {
        editor.currentSelectedObject = starpower;
        
        Update();
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(new Starpower(starpower)));

        int insertionIndex = SongObjectHelper.FindObjectPosition(starpower, editor.currentChart.starPower);
        Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Song event failed to be inserted?");
        lastPlacedSP = editor.currentChart.starPower[insertionIndex];
    }
}
