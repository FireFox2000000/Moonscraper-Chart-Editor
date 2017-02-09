using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class BPMController : SongObjectController {
    public BPM bpm { get { return (BPM)songObject; } set { Init(value, this); } }
    public Text bpmText;
    public const float position = -1.0f;

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
        if (bpm.position != 0)
        {
            base.OnSelectableMouseDrag();
        }
    }
}
