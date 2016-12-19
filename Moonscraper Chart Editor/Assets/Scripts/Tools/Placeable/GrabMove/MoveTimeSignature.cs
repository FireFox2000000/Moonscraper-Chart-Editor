using UnityEngine;
using System.Collections;

public class MoveTimeSignature : PlaceTimesignature {

    protected override void Controls()
    {
        MovementControls();
    }

    public void Init(TimeSignature ts)
    {
        this.ts = ts;
        controller.ts = ts;
        editor.currentSelectedObject = ts;
        controller.tsText.text = ts.value.ToString() + "/4";       // Fixes 1-frame text mutation
    }

    protected override void AddObject()
    {
        TimeSignature tsToAdd = new TimeSignature(ts);
        editor.currentSong.Add(tsToAdd);
        editor.CreateTSObject(tsToAdd);
        editor.currentSelectedObject = tsToAdd;
    }
}
