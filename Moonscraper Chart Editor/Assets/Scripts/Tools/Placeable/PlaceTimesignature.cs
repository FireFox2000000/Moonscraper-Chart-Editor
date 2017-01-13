using UnityEngine;
using System.Collections;

public class PlaceTimesignature : PlaceSongObject {
    public TimeSignature ts { get { return (TimeSignature)songObject; } set { songObject = value; } }
    new public TimesignatureController controller { get { return (TimesignatureController)base.controller; } set { base.controller = value; } }

    protected override void Awake()
    {
        base.Awake();
        ts = new TimeSignature();

        controller = GetComponent<TimesignatureController>();
        controller.ts = ts;
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Timesignature && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            AddObject();
        }
    }

    protected override void AddObject()
    {
        AddObjectToCurrentSong(ts, editor);
        /*
        TimeSignature tsToAdd = new TimeSignature(ts);
        editor.currentSong.Add(tsToAdd);
        editor.CreateTSObject(tsToAdd);

        // Only show the panel once the object has been placed down
        editor.currentSelectedObject = tsToAdd;*/
    }

    public static void AddObjectToCurrentSong(TimeSignature ts, ChartEditor editor, bool update = true)
    {
        TimeSignature tsToAdd = new TimeSignature(ts);
        editor.currentSong.Add(tsToAdd, update);
        editor.CreateTSObject(tsToAdd);

        // Only show the panel once the object has been placed down
        editor.currentSelectedObject = tsToAdd;
    }
}
