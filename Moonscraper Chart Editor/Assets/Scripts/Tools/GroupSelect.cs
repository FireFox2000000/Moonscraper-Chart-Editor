using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupSelect : ToolObject {
    public GameObject selectedHighlight;

    List<ChartObject> chartObjectsList = new List<ChartObject>();
    GameObject[] highlightPool = new GameObject[100];

    Vector2 initWorld2DPos = Vector2.zero;
    Vector2 endWorld2DPos = Vector2.zero;
    uint endWorld2DChartPos = 0;

    Globals globals;

    protected override void Awake()
    {
        base.Awake();
        globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<Globals>();

        for (int i = 0; i < highlightPool.Length; ++i)
        {
            highlightPool[i] = GameObject.Instantiate(selectedHighlight);
            highlightPool[i].SetActive(false);
        }
    }

    public override void ToolDisable()
    {
        initWorld2DPos = Vector2.zero;
        endWorld2DPos = Vector2.zero;
    }

    bool userDraggingSelectArea = false;
    protected override void Update()
    {
        UpdateSnappedPos();

        // Update the corner positions
        if (Input.GetMouseButtonDown(0) && Mouse.world2DPosition != null)
        {
            initWorld2DPos = (Vector2)Mouse.world2DPosition;
            initWorld2DPos.y = editor.currentSong.ChartPositionToWorldYPosition(objectSnappedChartPos);
            chartObjectsList.Clear();

            userDraggingSelectArea = true;
        }

        if (Input.GetMouseButton(0) && Mouse.world2DPosition != null)
        {
            endWorld2DPos = (Vector2)Mouse.world2DPosition;
            endWorld2DPos.y = editor.currentSong.ChartPositionToWorldYPosition(objectSnappedChartPos);
            endWorld2DChartPos = objectSnappedChartPos;
        }

        UpdateVisuals();

        if (Input.GetMouseButtonUp(0) && userDraggingSelectArea)
        {
            UpdateChartObjectList(endWorld2DChartPos);
            userDraggingSelectArea = false;
        }

        UpdateHighlights();

        if (Input.GetKeyDown(KeyCode.Delete))
            Delete();
    }

    void UpdateHighlights()
    {
        // Show a highlight over each selected object
        int arrayPos = SongObject.FindClosestPosition(editor.minPos, chartObjectsList.ToArray());
        int poolPos = 0;

        while (arrayPos != Globals.NOTFOUND && arrayPos < chartObjectsList.Count && poolPos < highlightPool.Length && chartObjectsList[arrayPos].position < editor.maxPos)
        {
            if (chartObjectsList[arrayPos].controller)
            {
                highlightPool[poolPos].transform.position = chartObjectsList[arrayPos].controller.transform.position;
                /*
                try
                {
                    Vector3 size = chartObjectsList[arrayPos].controller.GetAABBBoundsRect().size;
                    size.z = transform.lossyScale.z;
                    highlightPool[poolPos].transform.localScale = size;
                }
                catch { }*/

                highlightPool[poolPos].SetActive(true);
                ++poolPos;
            }

            ++arrayPos;
        }

        while (poolPos < highlightPool.Length)
        {
            highlightPool[poolPos++].SetActive(false);
        }
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

    void UpdateChartObjectList(uint? maxLimitNonInclusive = null)
    {
        Rect rect;
        Vector2 min = new Vector2();

        if (initWorld2DPos.x < endWorld2DPos.x)
            min.x = initWorld2DPos.x;
        else
            min.x = endWorld2DPos.x;

        if (initWorld2DPos.y < endWorld2DPos.y)
            min.y = initWorld2DPos.y;
        else
            min.y = endWorld2DPos.y;

        Vector2 size = new Vector2(Mathf.Abs(initWorld2DPos.x - endWorld2DPos.x), Mathf.Abs(initWorld2DPos.y - endWorld2DPos.y));
        rect = new Rect(min, size);

        chartObjectsList.Clear();

        foreach(ChartObject chartObject in editor.currentChart.chartObjects)
        {
            if (chartObject.controller && chartObject.controller.AABBcheck(rect))
            {
                if (maxLimitNonInclusive != null)
                {
                    if (chartObject.position < maxLimitNonInclusive)
                        chartObjectsList.Add(chartObject);
                }
                else
                    chartObjectsList.Add(chartObject);
            }
        }

        //Debug.Log(chartObjectsList.Count);
    }

    public void SetNatural()
    {
        SetNoteType(AppliedNoteType.Natural);
    }

    public void SetStrum()
    {
        SetNoteType(AppliedNoteType.Strum);
    }

    public void SetHopo()
    {
        SetNoteType(AppliedNoteType.Hopo);
    }

    public void SetTap()
    {
        SetNoteType(AppliedNoteType.Tap);
    }

    public void SetNoteType(AppliedNoteType type)
    {
        //Note[] notes = chartObjectsList.OfType<Note>().ToArray();

        foreach (ChartObject note in chartObjectsList)
        {
            if (note.classID == (int)SongObject.ID.Note)
                SetNoteType(note as Note, type);
        }
    }

    public void SetNoteType(Note note, AppliedNoteType noteType)
    {
        note.flags = Note.Flags.NONE;
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

    void Delete()
    {
        foreach (ChartObject cObject in chartObjectsList)
        {
            if (cObject.controller)
                cObject.controller.Delete(false);
        }
        editor.currentChart.updateArrays();
        chartObjectsList.Clear();
    }

    void Copy()
    {
        ChartEditor.clipboard = chartObjectsList.ToArray();
    }

    void Cut()
    {
        Copy();
        Delete();
    }

    public enum AppliedNoteType
    {
        Natural, Strum, Hopo, Tap
    }
}
