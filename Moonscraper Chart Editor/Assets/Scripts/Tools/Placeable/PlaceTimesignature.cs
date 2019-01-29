// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public class PlaceTimesignature : PlaceSongObject {
    public TimeSignature ts { get { return (TimeSignature)songObject; } set { songObject = value; } }
    new public TimesignatureController controller { get { return (TimesignatureController)base.controller; } set { base.controller = value; } }

    protected override void SetSongObjectAndController()
    {
        ts = new TimeSignature();

        controller = GetComponent<TimesignatureController>();
        controller.ts = ts;
    }

    protected override void Update()
    {
        base.Update();

        //UpdateSnappedPos(4);
        songObject.tick = objectSnappedChartPos;
    }

    protected override void Controls()
    {
        if (!GameSettings.keysModeEnabled)
        {
            if (Toolpane.currentTool == Toolpane.Tools.Timesignature && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
            {
                AddObject();
            }
        }
        else if (ShortcutInput.GetInputDown(Shortcut.AddSongObject))
        {
            IList<SyncTrack> searchArray = editor.currentSong.syncTrack;
            int pos = SongObjectHelper.FindObjectPosition(ts, searchArray);
            if (pos == SongObjectHelper.NOTFOUND)
            {
                AddObject();
            }
            else if (searchArray[pos].tick != 0)
            {
                editor.commandStack.Push(new SongEditDelete(searchArray[pos]));
                editor.currentSelectedObject = null;
            }
        }
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(new TimeSignature(ts)));
        editor.SelectSongObject(ts, editor.currentSong.syncTrack);
    }
}
