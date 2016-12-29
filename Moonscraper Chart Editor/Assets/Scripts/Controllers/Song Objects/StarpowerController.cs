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

    public override void Delete(bool update = true)
    {
        starpower.chart.Remove(starpower, update);
        Destroy(gameObject);
    }

    protected override void Update()
    {
#if false
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
#else
        if (starpower != null)
        {
            uint endPosition = starpower.position + starpower.length;

            if (    (starpower.position >= editor.minPos && starpower.position < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (starpower.position < editor.minPos && endPosition >= editor.maxPos))
                UpdateSongObject();
            else
                gameObject.SetActive(false);
        }
        else
            gameObject.SetActive(false);
#endif
    }

    public override void UpdateSongObject()
    {
        if (starpower.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS - 3, starpower.worldYPosition, 0);

            UpdateTailLength();
        }
    }

    public void UpdateTailLength()
    {
        float length = starpower.song.ChartPositionToWorldYPosition(starpower.position + starpower.length) - starpower.worldYPosition;

        Vector3 scale = tail.transform.localScale;
        scale.y = length;
        tail.transform.localScale = scale;

        Vector3 position = transform.position;
        position.y += length / 2.0f;
        tail.transform.position = position;
    }

    public void TailDrag()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            ChartEditor.editOccurred = true;
            uint snappedChartPos;

            if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
            {
                snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(starpower.song.WorldYPositionToChartPosition(((Vector2)Mouse.world2DPosition).y), Globals.step, starpower.song.resolution);           
            }
            else
            {
                snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(starpower.song.WorldYPositionToChartPosition(editor.mouseYMaxLimit.position.y), Globals.step, starpower.song.resolution);
            }

            if (snappedChartPos > starpower.position)
                starpower.length = snappedChartPos - starpower.position;
            else
                starpower.length = 0;
        }
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            // Pass note data to a ghost note
            GameObject moveSP = Instantiate(editor.starpowerPrefab);
            moveSP.SetActive(true);
            moveSP.name = "Moving starpower";
            Destroy(moveSP.GetComponent<PlaceStarpower>());
            moveSP.AddComponent<MoveStarpower>().Init(starpower);
                
            editor.currentSelectedObject = starpower;
            moveSP.SetActive(true);

            Delete();
        }
        else if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            TailDrag();
        }
    }
}
