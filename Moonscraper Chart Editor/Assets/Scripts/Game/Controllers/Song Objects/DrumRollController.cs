using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public class DrumRollController : SongObjectController
{
    public DrumRoll drumRoll { get { return (DrumRoll)songObject; } set { Init(value, this); } }
    public const float position = 0.0f;
    bool m_wantPop = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        Reset();
    }

    public override void UpdateSongObject()
    {
        if (drumRoll.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, desiredWorldYPosition, 0);

            if (isDirty)
            {
                UpdateLength();

                //visualsManager.UpdateVisuals();
            }
        }
    }

    public override void OnSelectableMouseDown()
    {
        Reset();
        base.OnSelectableMouseDown();
    }

    public override void OnSelectableMouseDrag()
    {
        if (!DragCheck())
        {
            base.OnSelectableMouseDrag();
        }
    }

    public override void OnSelectableMouseUp()
    {
        Reset();
    }

    public void Reset()
    {
        m_wantPop = false;
    }

    bool DragCheck()
    {
        if (editor.currentState == ChartEditor.State.Editor && Input.GetMouseButton(1))
        {
            TailDrag();
            return true;
        }

        return false;
    }

    void UpdateLength()
    {
        //float length = drumRoll.song.TickToWorldYPosition(drumRoll.tick + drumRoll.length) - desiredWorldYPosition;
        //
        //Vector3 scale = tail.transform.localScale;
        //scale.y = length;
        //tail.transform.localScale = scale;
        //
        //Vector3 position = transform.position;
        //position.y += length / 2.0f;
        //tail.transform.position = position;
    }

    void TailDrag()
    {
        uint snappedChartPos;

        if (editor.services.mouseMonitorSystem.world2DPosition != null && ((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.TickToSnappedTick(drumRoll.song.WorldYPositionToTick(((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y), Globals.gameSettings.step, drumRoll.song);
        }
        else
        {
            snappedChartPos = Snapable.TickToSnappedTick(drumRoll.song.WorldYPositionToTick(editor.mouseYMaxLimit.position.y), Globals.gameSettings.step, drumRoll.song);
        }

        // Cap to within the range of the song
        snappedChartPos = (uint)Mathf.Min(editor.maxPos, snappedChartPos);

        uint newLength = drumRoll.GetCappedLengthForPos(snappedChartPos);
        if (newLength != drumRoll.length)
        {
            if (m_wantPop)
            {
                editor.commandStack.Pop();
            }

            editor.commandStack.Push(new SongEditModify<DrumRoll>(drumRoll, new DrumRoll(drumRoll.tick, newLength, drumRoll.type)));

            m_wantPop = true;
        }
    }
}
