// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class BPMController : SongObjectController {
    public BPM bpm { get { return (BPM)songObject; } set { Init(value, this); } }
    public Text bpmText;
    public const float position = -1.0f;

    [SerializeField]
    bool anchorColourSwitch;

    [SerializeField]
    Material ogMat;
    [SerializeField]
    Material anchorMat;

    Renderer ren;
    uint prevBPMValue = 0;

    void Start()
    {
        ren = GetComponent<Renderer>();
    }

    public override void UpdateSongObject()
    {
        if (bpm.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, bpm.worldYPosition, 0); 
        }

        bpmText.text = "BPM: " + ((float)bpm.value / 1000.0f).ToString();

        if (anchorColourSwitch && ren)
        {
            if (bpm.anchor != null)
                ren.sharedMaterial = anchorMat;
            else
                ren.sharedMaterial = ogMat;
        }

        if (prevBPMValue != bpm.value)
        {
            editor.songObjectPoolManager.SetAllPoolsDirty();
        }

        prevBPMValue = bpm.value;
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
