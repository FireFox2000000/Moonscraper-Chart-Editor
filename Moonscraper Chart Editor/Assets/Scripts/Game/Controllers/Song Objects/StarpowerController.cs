// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;


public class StarpowerController : SongObjectController
{
    public GameObject tail;
    [SerializeField]
    StarpowerVisualsManager visualsManager;
    public Starpower starpower { get { return (Starpower)songObject; } set { Init(value, this); } }
    public const float position = -3.0f;
    
    Starpower unmodifiedSP = null;
    bool wantPop = false;

    new void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Reset();
    }

    protected override void UpdateCheck()
    {
        if (starpower != null)
        {
            uint endPosition = starpower.tick + starpower.length;

            if ((starpower.tick >= editor.minPos && starpower.tick < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (starpower.tick < editor.minPos && endPosition >= editor.maxPos))
            {
                ChartEditor.State currentState = editor.currentState;
                if (currentState == ChartEditor.State.Editor)
                {
                    UpdateSongObject();
                }
            }
            else
                gameObject.SetActive(false);
        }
        else
            gameObject.SetActive(false);
    }

    public override void UpdateSongObject()
    {
        if (starpower.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, desiredWorldYPosition, 0);

            UpdateTailLength();

            //if (isDirty)
            {
                visualsManager.UpdateVisuals();
            }
        }
    }

    public void UpdateTailLength()
    {
        float length = starpower.song.TickToWorldYPosition(starpower.tick + starpower.length) - desiredWorldYPosition;

        Vector3 scale = tail.transform.localScale;
        scale.y = length;
        tail.transform.localScale = scale;

        Vector3 position = transform.position;
        position.y += length / 2.0f;
        tail.transform.position = position;
    }

    void TailDrag()
    {
        uint snappedChartPos;

        if (editor.services.mouseMonitorSystem.world2DPosition != null && ((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.TickToSnappedTick(starpower.song.WorldYPositionToTick(((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y), Globals.gameSettings.step, starpower.song);           
        }
        else
        {
            snappedChartPos = Snapable.TickToSnappedTick(starpower.song.WorldYPositionToTick(editor.mouseYMaxLimit.position.y), Globals.gameSettings.step, starpower.song);
        }

        // Cap to within the range of the song
        snappedChartPos = (uint)Mathf.Min(editor.maxPos, snappedChartPos);

        uint newLength = starpower.GetCappedLengthForPos(snappedChartPos);
        if (newLength != starpower.length)
        {
            if (wantPop)
                editor.commandStack.Pop();

            editor.commandStack.Push(new SongEditModify<Starpower>(starpower, new Starpower(starpower.tick, newLength, starpower.flags)));

            wantPop = true;
        }
    }

    public override void OnSelectableMouseDown()
    {
        Reset();
        base.OnSelectableMouseDown();
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (!DragCheck())
            base.OnSelectableMouseDrag();
    }

    public bool DragCheck()
    {
        if (editor.currentState == ChartEditor.State.Editor && Input.GetMouseButton(1))
        {
            if (unmodifiedSP == null)
                unmodifiedSP = (Starpower)starpower.Clone();

            TailDrag();
            return true;
        }

        return false;
    }

    public override void OnSelectableMouseUp()
    {
        Reset();
    }

    public void Reset()
    {
        unmodifiedSP = null;
        wantPop = false;
    }
}
