using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupSelect : ToolObject {
    ChartObject[] chartObjectsList = new ChartObject[0];

    Vector2 initWorld2DPos = Vector2.zero;
    Vector2 endWorld2DPos = Vector2.zero;

    public override void ToolDisable()
    {
        initWorld2DPos = Vector2.zero;
        endWorld2DPos = Vector2.zero;
    }

    protected override void Update()
    {
        // Update the corner positions
        if (Input.GetMouseButtonDown(0) && Mouse.world2DPosition != null)
        {
            initWorld2DPos = (Vector2)Mouse.world2DPosition;
            chartObjectsList = new ChartObject[0];
        }

        if (Input.GetMouseButton(0) && Mouse.world2DPosition != null)
            endWorld2DPos = (Vector2)Mouse.world2DPosition;

        UpdateVisuals();

        UpdateChartObjectList();
    }

    void UpdateVisuals()
    {
        Vector2 diff = new Vector2(Mathf.Abs(initWorld2DPos.x - endWorld2DPos.x), Mathf.Abs(initWorld2DPos.y - endWorld2DPos.y));

        // Set size
        transform.localScale = new Vector3(diff.x, diff.y, transform.localScale.z);

        // Calculate center pos
        Vector3 pos = transform.position;
        if (initWorld2DPos.x < endWorld2DPos.x)
            pos.x = initWorld2DPos.x + diff.x / 2;
        else
            pos.x = endWorld2DPos.x + diff.x / 2;

        if (initWorld2DPos.y < endWorld2DPos.y)
            pos.y = initWorld2DPos.y + diff.y / 2;
        else
            pos.y = endWorld2DPos.y + diff.y / 2;

        transform.position = pos;
    }

    void UpdateChartObjectList()
    {
        // Update what objects are currently within the selected range
        uint minChartPos;
        uint maxChartPos;

        if (initWorld2DPos.y < endWorld2DPos.y)
        {
            minChartPos = editor.currentSong.WorldYPositionToChartPosition(initWorld2DPos.y);
            maxChartPos = editor.currentSong.WorldYPositionToChartPosition(endWorld2DPos.y);
        }
        else
        {
            minChartPos = editor.currentSong.WorldYPositionToChartPosition(endWorld2DPos.y);
            maxChartPos = editor.currentSong.WorldYPositionToChartPosition(initWorld2DPos.y);
        }

        chartObjectsList = SongObject.GetRange(editor.currentChart.chartObjects, minChartPos, maxChartPos);

        Debug.Log(chartObjectsList.Length);
    }

    public void SetNoteType(AppliedNoteType type)
    {
        Note[] notes = chartObjectsList.OfType<Note>().ToArray();

        foreach (Note note in notes)
            SetNoteType(note, type);
    }

    public void SetNoteType(Note note, AppliedNoteType noteType)
    {
        note.flags &= ~Note.Flags.TAP;
        switch (noteType)
        {
            case (AppliedNoteType.Strum):
                if (note.IsChord)
                    note.flags &= ~Note.Flags.FORCED;
                else
                {
                    if (note.IsHopoUnforced)
                        note.flags |= Note.Flags.FORCED;
                    else
                        note.flags &= ~Note.Flags.FORCED;
                }

                break;

            case (AppliedNoteType.Hopo):
                if (note.IsChord)
                    note.flags |= Note.Flags.FORCED;
                else
                {
                    if (!note.IsHopoUnforced)
                        note.flags |= Note.Flags.FORCED;
                    else
                        note.flags &= ~Note.Flags.FORCED;
                }

                break;

            case (AppliedNoteType.Tap):
                note.flags |= Note.Flags.TAP;
                break;

            default:
                break;
        }

        note.applyFlagsToChord();

        ChartEditor.editOccurred = true;
    }

    public enum AppliedNoteType
    {
        Natural, Strum, Hopo, Tap
    }
}
