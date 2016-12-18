using UnityEngine;
using System.Collections;
using System;

public class PlaceBPM : ToolObject {
    protected BPM bpm;
    BPMController controller;

    protected override void Awake()
    {
        base.Awake();
        bpm = new BPM(editor.currentSong);

        controller = GetComponent<BPMController>();
        controller.bpm = bpm;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        bpm.song = editor.currentSong;
        bpm.position = objectSnappedChartPos;
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.BPM && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            AddObject();
        }
    }

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;
    }

    void OnEnable()
    {
        Update();
    }

    protected override void AddObject()
    {
        BPM bpmToAdd = new BPM(bpm);
        editor.currentSong.Add(bpmToAdd);
        editor.CreateBPMObject(bpmToAdd);
        editor.currentSelectedObject = bpmToAdd;
    }
}
