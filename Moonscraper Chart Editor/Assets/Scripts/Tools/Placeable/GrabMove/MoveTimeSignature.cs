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
        initObject = this.ts.Clone();
    }

    protected override void AddObject()
    {
        TimeSignature tsToAdd = new TimeSignature(ts);
        editor.currentSong.Add(tsToAdd);
        editor.CreateTSObject(tsToAdd);
        editor.currentSelectedObject = tsToAdd;

        if (!initObject.AllValuesCompare(tsToAdd))
            editor.actionHistory.Insert(new ActionHistory.Action[] { new ActionHistory.Delete(initObject), new ActionHistory.Add(tsToAdd) });
    }
}
