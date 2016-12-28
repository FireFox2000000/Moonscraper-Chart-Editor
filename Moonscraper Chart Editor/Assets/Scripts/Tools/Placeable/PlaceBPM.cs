using UnityEngine;
using System.Collections;
using System;

public class PlaceBPM : PlaceSongObject {
    public BPM bpm { get { return (BPM)songObject; } set { songObject = value; } }
    new public BPMController controller { get { return (BPMController)base.controller; } set { base.controller = value; } }

    protected override void Awake()
    {
        base.Awake();
        bpm = new BPM();

        controller = GetComponent<BPMController>();
        controller.bpm = bpm;
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.BPM && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            AddObject();
        }
    }

    protected override void AddObject()
    {
        BPM bpmToAdd = new BPM(bpm);
        editor.currentSong.Add(bpmToAdd);
        editor.CreateBPMObject(bpmToAdd);
        editor.currentSelectedObject = bpmToAdd;
    }
}
