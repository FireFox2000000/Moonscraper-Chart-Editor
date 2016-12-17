using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class BPMController : SongObjectController {
    public BPM bpm;
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

            bpmText.text = ((float)bpm.value / 1000.0f).ToString();
        }
    }

    public override void Delete()
    {
        if (bpm.position != 0)
        {
            bpm.song.Remove(bpm);

            Destroy(gameObject);
        }
    }
}
