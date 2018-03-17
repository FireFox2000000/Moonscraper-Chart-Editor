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

    BPM draggingInitialBpm = null;

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

        if (Input.GetMouseButton(1))
        {     
            BPM previousBpm = SongObjectHelper.GetPreviousNonInclusive(bpm.song.bpms, bpm.position);
            if (previousBpm != null && previousBpm.anchor == null)
            {
                float desiredWorldPos;
                if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
                    desiredWorldPos = ((Vector2)Mouse.world2DPosition).y;
                else
                    desiredWorldPos = editor.mouseYMaxLimit.position.y;

                float desiredTime = TickFunctions.WorldYPositionToTime(desiredWorldPos);
                if (desiredTime < previousBpm.time)
                    desiredTime = previousBpm.time;

                BPM nextBpm = SongObjectHelper.GetNextNonInclusive(bpm.song.bpms, bpm.position);
                if (nextBpm != null && nextBpm.anchor != null && desiredTime >= nextBpm.time)
                {
                    desiredTime = nextBpm.time - 0.01f;
                }

                uint newBpmValue = (uint)(Mathf.Ceil((float)TickFunctions.DisToBpm(previousBpm.position, bpm.position, desiredTime - previousBpm.time, bpm.song.resolution)) * 1000);
                if (newBpmValue > 0)
                    previousBpm.value = newBpmValue;

                editor.songObjectPoolManager.SetAllPoolsDirty();
                ChartEditor.isDirty = true;
                editor.currentSong.UpdateCache();
                editor.FixUpBPMAnchors();
            }
        }
    }

    public override void OnSelectableMouseDown()
    {
        base.OnSelectableMouseDown();

        if (Input.GetMouseButtonDown(1))
        {
            BPM previousBpm = SongObjectHelper.GetPreviousNonInclusive(bpm.song.bpms, bpm.position);
            if (previousBpm != null && previousBpm.anchor == null)
                draggingInitialBpm = (BPM)previousBpm.Clone();
        }
    }

    public override void OnSelectableMouseUp()
    {
        base.OnSelectableMouseUp();

        if (Input.GetMouseButtonUp(1))
        {
            BPM previousBpm = SongObjectHelper.GetPreviousNonInclusive(bpm.song.bpms, bpm.position);
            if (draggingInitialBpm != null && previousBpm.value != draggingInitialBpm.value)
                editor.actionHistory.Insert(new ActionHistory.Modify(draggingInitialBpm, previousBpm));

            draggingInitialBpm = null;
        }
    }
}
