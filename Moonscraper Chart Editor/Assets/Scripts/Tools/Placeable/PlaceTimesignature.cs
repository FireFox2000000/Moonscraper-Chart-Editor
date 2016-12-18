using UnityEngine;
using System.Collections;

public class PlaceTimesignature : ToolObject {

    protected TimeSignature ts;
    TimesignatureController controller;

    protected override void Awake()
    {
        base.Awake();
        ts = new TimeSignature(editor.currentSong);

        controller = GetComponent<TimesignatureController>();
        controller.ts = ts;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        ts.song = editor.currentSong;
        ts.position = objectSnappedChartPos;
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Timesignature && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
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
        TimeSignature tsToAdd = new TimeSignature(ts);
        editor.currentSong.Add(tsToAdd);
        editor.CreateTSObject(tsToAdd);
        editor.currentSelectedObject = tsToAdd;
    }
}
