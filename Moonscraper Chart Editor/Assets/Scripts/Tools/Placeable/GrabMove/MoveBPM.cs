using UnityEngine;
using System.Collections;

public class MoveBPM : PlaceBPM {

    protected override void Controls()
    {
        MovementControls();
    }

    public void Init(BPM bpm)
    {
        this.bpm = bpm;
        controller.bpm = bpm;
        editor.currentSelectedObject = bpm;
        controller.bpmText.text = "BPM: " + ((float)bpm.value / 1000.0f).ToString();       // Fixes 1-frame text mutation
    }

    protected override void AddObject()
    {
        BPM bpmToAdd = new BPM(bpm);
        editor.currentSong.Add(bpmToAdd);
        editor.CreateBPMObject(bpmToAdd);
        editor.currentSelectedObject = bpmToAdd;
    }
}
