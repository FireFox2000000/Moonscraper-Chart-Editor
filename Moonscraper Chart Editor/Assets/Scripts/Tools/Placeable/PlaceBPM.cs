// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System;

public class PlaceBPM : PlaceSongObject {
    public BPM bpm { get { return (BPM)songObject; } set { songObject = value; } }
    new public BPMController controller { get { return (BPMController)base.controller; } set { base.controller = value; } }
    protected bool setAsLastBpm = true;

    protected override void SetSongObjectAndController()
    {
        bpm = new BPM();

        controller = GetComponent<BPMController>();
        controller.bpm = bpm;
    }

    protected override void Controls()
    {
        if (!GameSettings.keysModeEnabled)
        {
            if (Toolpane.currentTool == Toolpane.Tools.BPM && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
            {
                RecordAddActionHistory(bpm, editor.currentSong.bpms);

                AddObject();
            }
        }
        else if (ShortcutInput.GetInputDown(Shortcut.AddSongObject))
        {
            SongObject[] searchArray = editor.currentSong.syncTrack;
            int pos = SongObjectHelper.FindObjectPosition(bpm, searchArray);
            if (pos == SongObjectHelper.NOTFOUND)
            {
                editor.actionHistory.Insert(new ActionHistory.Add(bpm));
                AddObject();
            }
            else if (searchArray[pos].tick != 0)
            {
                editor.actionHistory.Insert(new ActionHistory.Delete(searchArray[pos]));
                searchArray[pos].Delete();
                editor.currentSelectedObject = null;
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (setAsLastBpm)
        {
            // Set BPM value to the last bpm in the chart from the current position
            int lastBpmArrayPos = SongObjectHelper.FindClosestPosition(bpm.tick, editor.currentSong.bpms);

            if (editor.currentSong.bpms[lastBpmArrayPos].tick > bpm.tick)
                --lastBpmArrayPos;

            if (lastBpmArrayPos != SongObjectHelper.NOTFOUND && lastBpmArrayPos >= 0)
            {
                bpm.value = editor.currentSong.bpms[lastBpmArrayPos].value;
            }
        }
    }

    protected override void AddObject()
    {
        AddObjectToCurrentSong(bpm, editor);
        /*
        BPM bpmToAdd = new BPM(bpm);
        editor.currentSong.Add(bpmToAdd);
        editor.CreateBPMObject(bpmToAdd);
        editor.currentSelectedObject = bpmToAdd;*/
    }

    public static void AddObjectToCurrentSong(BPM bpm, ChartEditor editor, bool update = true)
    {
        BPM bpmToAdd = new BPM(bpm);
        editor.currentSong.Add(bpmToAdd, update);
        //editor.CreateBPMObject(bpmToAdd);
        editor.currentSelectedObject = bpmToAdd;

        if (bpmToAdd.anchor != null)
        {
            bpmToAdd.anchor = bpmToAdd.song.LiveTickToTime(bpmToAdd.tick, bpmToAdd.song.resolution);
        }

        ChartEditor.GetInstance().songObjectPoolManager.SetAllPoolsDirty();
    }
}
