// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using TMPro;
using MoonscraperChartEditor.Song;

public class BPMController : SongObjectController {
    public BPM bpm { get { return (BPM)songObject; } set { Init(value, this); } }
    public TextMeshPro bpmText;
    public const float position = -1.0f;

    [SerializeField]
    bool anchorColourSwitch;

    [SerializeField]
    Material ogMat;
    [SerializeField]
    Material anchorMat;

    Renderer ren;
    uint prevBPMValue = 0;
    bool hasPushed = false;

    BPM draggingInitialBpm = null;

    void Start()
    {
        ren = GetComponent<Renderer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        hasPushed = false;
        if (bpm != null)
            UpdateBpmDisplay();
    }

    public override void UpdateSongObject()
    {
        if (bpm.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, desiredWorldYPosition, 0); 
        }

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
            UpdateBpmDisplay();
        }

        prevBPMValue = bpm.value;
    }

    void UpdateBpmDisplay()
    {
        bpmText.text = "BPM: " + ((float)bpm.value / 1000.0f).ToString();
    }

    public override void OnSelectableMouseDrag()
    {
        // Move object
        if (bpm.tick != 0)
        {
            base.OnSelectableMouseDrag();
        }

        if (draggingInitialBpm != null && MoonscraperEngine.Input.KeyboardDevice.ctrlKeyBeingPressed && Input.GetMouseButton(1))
        {     
            BPM previousBpm = SongObjectHelper.GetPreviousNonInclusive(bpm.song.bpms, bpm.tick);
            if (previousBpm != null && previousBpm.anchor == null)
            {
                float desiredWorldPos;
                if (editor.services.mouseMonitorSystem.world2DPosition != null && ((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y < editor.mouseYMaxLimit.position.y)
                    desiredWorldPos = ((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y;
                else
                    desiredWorldPos = editor.mouseYMaxLimit.position.y;

                float desiredTime = ChartEditor.WorldYPositionToTime(desiredWorldPos);
                if (desiredTime < previousBpm.time)
                    desiredTime = previousBpm.time;

                BPM nextBpm = SongObjectHelper.GetNextNonInclusive(bpm.song.bpms, bpm.tick);
                if (nextBpm != null && nextBpm.anchor != null && desiredTime >= nextBpm.time)
                {
                    desiredTime = nextBpm.time - 0.01f;
                }

                double disToBpm = TickFunctions.DisToBpm(previousBpm.tick, bpm.tick, desiredTime - previousBpm.time, bpm.song.resolution);
                uint newBpmValue = (uint)Mathf.Ceil((float)disToBpm * 1000.0f);
                if (newBpmValue > 0)
                {
                    if (hasPushed)
                        editor.commandStack.Pop();

                    editor.commandStack.Push(new SongEditModify<BPM>(previousBpm, new BPM(previousBpm.tick, newBpmValue, previousBpm.anchor)));
                    hasPushed = true;
                }
            }
        }
    }

    public override void OnSelectableMouseDown()
    {
        base.OnSelectableMouseDown();

        if (MoonscraperEngine.Input.KeyboardDevice.ctrlKeyBeingPressed && Input.GetMouseButtonDown(1))
        {
            hasPushed = false;

            // This is to allow dragging to be used while the BPM placement tool is active
            BPM previousBpm = SongObjectHelper.GetPreviousNonInclusive(bpm.song.bpms, bpm.tick);
            if (previousBpm != null && previousBpm.anchor == null)
                draggingInitialBpm = (BPM)previousBpm.Clone();
        }
    }

    public override void OnSelectableMouseUp()
    {
        base.OnSelectableMouseUp();

        if (draggingInitialBpm != null && Input.GetMouseButtonUp(1))
        {
            draggingInitialBpm = null;
        }

        hasPushed = false;
    }
}
