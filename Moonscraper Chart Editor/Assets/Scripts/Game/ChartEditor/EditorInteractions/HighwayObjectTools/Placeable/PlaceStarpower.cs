// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

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
        if (!Globals.gameSettings.keysModeEnabled)
        {
            if (Input.GetMouseButton(0))
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
        else if (MSChartEditorInput.GetInput(MSChartEditorInputActions.AddSongObject))
        {
            if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.AddSongObject))
            {
                var searchArray = editor.currentChart.starPower;
                int pos = SongObjectHelper.FindObjectPosition(starpower, searchArray);
                if (pos == SongObjectHelper.NOTFOUND)
                {
                    AddObject();
                }
                else
                {
                    editor.commandStack.Push(new SongEditDelete(searchArray[pos]));
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
            editor.commandStack.Push(new SongEditAdd(new Starpower(lastPlacedSP.tick, newLength, lastPlacedSP.flags)));
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        starpower.chart = editor.currentChart;
        if (!Globals.gameSettings.keysModeEnabled)
            spRen.enabled = lastPlacedSP == null;

        base.Update();

        if ((Input.GetMouseButtonUp(0) && !Globals.gameSettings.keysModeEnabled) || (Globals.gameSettings.keysModeEnabled && Input.GetButtonUp("Add Object")))
        {
            // Reset
            lastPlacedSP = null;
        } 
    }

    protected override void OnEnable()
    {
        editor.selectedObjectsManager.currentSelectedObject = starpower;

        base.OnEnable();
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(new Starpower(starpower)));

        int insertionIndex = SongObjectHelper.FindObjectPosition(starpower, editor.currentChart.starPower);
        Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Song event failed to be inserted?");
        lastPlacedSP = editor.currentChart.starPower[insertionIndex];
    }
}
