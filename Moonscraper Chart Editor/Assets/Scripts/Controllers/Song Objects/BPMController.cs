using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class BPMController : SongObjectController {
    public BPM bpm { get { return (BPM)songObject; } set { songObject = value; } }
    public Text bpmText;
    public float position = 0.0f;

    public void Init(BPM _bpm)
    {
        base.Init(_bpm);
        bpm = _bpm;
        bpm.controller = this;
    }

    public override void UpdateSongObject()
    {
        if (bpm.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, bpm.worldYPosition, 0); 
        }

        bpmText.text = "BPM: " + ((float)bpm.value / 1000.0f).ToString();
    }

    public override void Delete(bool update = true)
    {
        // First bpm cannot be removed, block the functionality
        if (bpm.position != 0)
        {
            bpm.song.Remove(bpm, update);

            Destroy(gameObject);
        }
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

            Delete();
        }
    }
}
