using UnityEngine;
using System.Collections;
using System;

public class StarpowerController : SongObjectController
{
    public GameObject tail;
    public StarPower starpower;

    Renderer spRen, spTailRen;

    new void Awake()
    {
        base.Awake();
        spRen = GetComponent<SpriteRenderer>();
        spTailRen = tail.GetComponent<Renderer>();
    }

    public void Init(StarPower _starpower)
    {
        base.Init(_starpower);
        starpower = _starpower;
        starpower.controller = this;
    }

    public override void Delete()
    {
        starpower.chart.Remove(starpower);
        Destroy(gameObject);
    }

    protected override void Update()
    {
        if (spRen.isVisible || spTailRen.isVisible)
            UpdateSongObject();
        else if (starpower != null)
        {
            uint endPosition = starpower.position + starpower.length;

            if ((starpower.position > editor.minPos && starpower.position < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (starpower.position < editor.minPos && endPosition > editor.maxPos))
                UpdateSongObject();
        }
    }

    public override void UpdateSongObject()
    {
        if (starpower != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS - 3, starpower.worldYPosition, 0);

            UpdateTailLength();
        }
    }

    public void UpdateTailLength()
    {
        float length = starpower.song.ChartPositionToWorldYPosition(starpower.position + starpower.length) - starpower.song.ChartPositionToWorldYPosition(starpower.position);

        Vector3 scale = tail.transform.localScale;
        scale.y = length;
        tail.transform.localScale = scale;

        Vector3 position = transform.position;
        position.y += length / 2.0f;
        tail.transform.position = position;
    }

    public void TailDrag()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1) && Mouse.world2DPosition != null)
        {
            ChartEditor.editOccurred = true;

            uint snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(starpower.song.WorldYPositionToChartPosition(((Vector2)Mouse.world2DPosition).y), Globals.step, starpower.song.resolution);

            if (snappedChartPos > starpower.position)
                starpower.length = snappedChartPos - starpower.position;
            else
                starpower.length = 0;
        }
    }

    void OnMouseDrag()
    {
        // Move note
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            // Prevent note from snapping if the user is just clicking and not dragging
            if (prevMousePos != (Vector2)Input.mousePosition)
            {
                // Pass sp data to starpower tool placement

            }
            else
            {
                prevMousePos = Input.mousePosition;
            }
        }
        else if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            TailDrag();
        }
    }
}
