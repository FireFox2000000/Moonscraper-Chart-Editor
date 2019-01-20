// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System;

public class StarpowerController : SongObjectController
{
    public GameObject tail;
    public Starpower starpower { get { return (Starpower)songObject; } set { Init(value, this); } }
    public const float position = -3.0f;
    
    Starpower unmodifiedSP = null;
    bool wantPop = false;

    new void Awake()
    {
        base.Awake();
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
                if (Globals.applicationMode == Globals.ApplicationMode.Editor)
                    UpdateSongObject();
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
            transform.position = new Vector3(CHART_CENTER_POS + position, starpower.worldYPosition, 0);

            UpdateTailLength();
        }
    }

    public void UpdateTailLength()
    {
        float length = starpower.song.TickToWorldYPosition(starpower.tick + starpower.length) - starpower.worldYPosition;

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

        if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.TickToSnappedTick(starpower.song.WorldYPositionToTick(((Vector2)Mouse.world2DPosition).y), GameSettings.step, starpower.song.resolution);           
        }
        else
        {
            snappedChartPos = Snapable.TickToSnappedTick(starpower.song.WorldYPositionToTick(editor.mouseYMaxLimit.position.y), GameSettings.step, starpower.song.resolution);
        }

        uint newLength = starpower.GetCappedLengthForPos(snappedChartPos);
        if (newLength != starpower.length)
        {
            if (wantPop)
                editor.commandStack.Pop();

            editor.commandStack.Push(new SongEditModify<Starpower>(starpower, new Starpower(starpower.tick, newLength)));

            wantPop = true;
        }
    }
    
    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (!DragCheck())
            base.OnSelectableMouseDrag();
    }

    public bool DragCheck()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
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
        wantPop = false;
    }
}
