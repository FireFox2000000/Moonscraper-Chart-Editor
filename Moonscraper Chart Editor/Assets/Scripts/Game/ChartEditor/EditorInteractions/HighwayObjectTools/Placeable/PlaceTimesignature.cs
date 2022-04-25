// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using MoonscraperChartEditor.Song;

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
        if (!Globals.gameSettings.keysModeEnabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                AddObject();
            }
        }
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.AddSongObject))
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
                editor.selectedObjectsManager.currentSelectedObject = null;
            }
        }
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(new TimeSignature(ts)));
        editor.selectedObjectsManager.SelectSongObject(ts, editor.currentSong.syncTrack);
    }
}
