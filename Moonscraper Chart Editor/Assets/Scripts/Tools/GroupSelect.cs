using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupSelect : ToolObject {
    List<ChartObject> chartObjectsList = new List<ChartObject>();

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
            chartObjectsList.Clear();
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

        ChartObject[] allChartObjects = editor.currentChart.chartObjects;
        int minPos = SongObject.FindClosestPosition(minChartPos, allChartObjects);

    }

    public void SetNoteType(Note.Note_Type type)
    {
        Note[] notes = chartObjectsList.OfType<Note>().ToArray();

        foreach (Note note in notes)
            note.SetNoteType(type);
    }
}
