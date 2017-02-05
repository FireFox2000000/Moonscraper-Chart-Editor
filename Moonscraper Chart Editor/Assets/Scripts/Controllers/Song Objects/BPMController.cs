using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class BPMController : SongObjectController {
    public BPM bpm { get { return (BPM)songObject; } set { Init(value, this); } }
    public Text bpmText;
    public float position = 0.0f;

    public override void UpdateSongObject()
    {
        if (bpm.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, bpm.worldYPosition, 0); 
        }

        bpmText.text = "BPM: " + ((float)bpm.value / 1000.0f).ToString();
    }

    public override void OnSelectableMouseDrag()
    {
        // Move object
        if (bpm.position != 0 && Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            // Pass note data to a ghost bpm
            GameObject moveBPM = Instantiate(editor.ghostBPM);
            moveBPM.SetActive(true);

            moveBPM.name = "Moving BPM";
            Destroy(moveBPM.GetComponent<PlaceBPM>());
            MoveBPM movement = moveBPM.AddComponent<MoveBPM>();
            movement.Init(bpm);

            bpm.Delete();
        }
    }
}
