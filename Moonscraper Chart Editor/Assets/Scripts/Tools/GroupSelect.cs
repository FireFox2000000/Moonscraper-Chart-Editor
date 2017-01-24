using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupSelect : ToolObject {
    public GameObject selectedHighlight;

    GameObject highlightPoolParent;
    GameObject[] highlightPool = new GameObject[100];

    Vector2 initWorld2DPos = Vector2.zero;
    Vector2 endWorld2DPos = Vector2.zero;
    uint startWorld2DChartPos = 0;
    uint endWorld2DChartPos = 0;

    Song prevSong;
    Chart prevChart;

    //Clipboard data;
    List<ChartObject> data = new List<ChartObject>();
    Rect rect;

    protected override void Awake()
    {
        base.Awake();

        highlightPoolParent = new GameObject("Group Select Highlights");
        for (int i = 0; i < highlightPool.Length; ++i)
        {
            highlightPool[i] = GameObject.Instantiate(selectedHighlight);
            highlightPool[i].transform.SetParent(highlightPoolParent.transform);
            highlightPool[i].SetActive(false);
        }

        prevSong = editor.currentSong;
        prevChart = editor.currentChart;

        data = new List<ChartObject>();
    }

    public override void ToolDisable()
    {
        reset();
        foreach (GameObject highlight in highlightPool)
        {
            highlight.SetActive(false);
        }
    }

    public override void ToolEnable()
    {
        base.ToolEnable();

        editor.currentSelectedObject = null;
    }

    public void reset()
    {
        transform.localScale = new Vector3(0, 0, transform.localScale.z);
        initWorld2DPos = Vector2.zero;
        endWorld2DPos = Vector2.zero;
        startWorld2DChartPos = 0;
        endWorld2DChartPos = 0;
        data = new List<ChartObject>();
        rect = new Rect();
    }

    bool userDraggingSelectArea = false;
    protected override void Update()
    {
        if (prevChart != editor.currentChart || prevSong != editor.currentSong)
            reset();
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            UpdateSnappedPos();

            // Update the corner positions
            if (Input.GetMouseButtonDown(0) && Mouse.world2DPosition != null)
            {
                initWorld2DPos = (Vector2)Mouse.world2DPosition;
                initWorld2DPos.y = editor.currentSong.ChartPositionToWorldYPosition(objectSnappedChartPos);
                startWorld2DChartPos = objectSnappedChartPos;
                data = new List<ChartObject>();

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
                if (startWorld2DChartPos > endWorld2DChartPos)
                    UpdateGroupedData(endWorld2DChartPos, startWorld2DChartPos);
                else
                    UpdateGroupedData(startWorld2DChartPos, endWorld2DChartPos);
                userDraggingSelectArea = false;
            }

            UpdateHighlights();

            if (Input.GetButtonDown("Delete"))
                Delete();

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand))
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    Cut();
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    Copy();
                }
            }
        }

        prevSong = editor.currentSong;
        prevChart = editor.currentChart;
    }

    void UpdateHighlights()
    {
        ChartObject[] chartObjects = data.ToArray();

        // Show a highlight over each selected object
        int arrayPos = SongObject.FindClosestPosition(editor.minPos, chartObjects);
        int poolPos = 0;

        while (arrayPos != Globals.NOTFOUND && arrayPos < chartObjects.Length && poolPos < highlightPool.Length && chartObjects[arrayPos].position < editor.maxPos)
        {
            if (chartObjects[arrayPos].controller)
            {
                highlightPool[poolPos].transform.position = chartObjects[arrayPos].controller.transform.position;

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

    void UpdateGroupedData(uint minLimitInclusive, uint maxLimitNonInclusive)
    {
        Vector2 position = new Vector2();

        // Bottom left corner is position
        if (initWorld2DPos.x < endWorld2DPos.x)
            position.x = initWorld2DPos.x;
        else
            position.x = endWorld2DPos.x;

        if (initWorld2DPos.y < endWorld2DPos.y)
            position.y = initWorld2DPos.y;
        else
            position.y = endWorld2DPos.y;

        Vector2 size = new Vector2(Mathf.Abs(initWorld2DPos.x - endWorld2DPos.x), Mathf.Abs(initWorld2DPos.y - endWorld2DPos.y));
        rect = new Rect(position, size);

        List<ChartObject> chartObjectsList = new List<ChartObject>();

        foreach (ChartObject chartObject in editor.currentChart.chartObjects)
        {
            if (chartObject.position >= minLimitInclusive && chartObject.position < maxLimitNonInclusive && chartObject.controller && chartObject.controller.AABBcheck(rect))
            {
                chartObjectsList.Add(chartObject);
            }
        }

        //Debug.Log(chartObjectsList.Count);
        data = chartObjectsList;
        //data = new Clipboard(chartObjectsList.ToArray(), rect, editor.currentSong);
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
        List<ActionHistory.Action> actions = new List<ActionHistory.Action>();

        foreach (ChartObject note in data)
        {
            if (note.classID == (int)SongObject.ID.Note)
            {
                // Need to record the whole chord
                Note unmodified = (Note)note.Clone();
                Note[] chord = ((Note)note).GetChord();

                ActionHistory.Action[] deleteRecord = new ActionHistory.Action[chord.Length];
                for (int i = 0; i < deleteRecord.Length; ++i)
                    deleteRecord[i] = new ActionHistory.Delete(chord[i]);

                SetNoteType(note as Note, type);

                chord = ((Note)note).GetChord();

                ActionHistory.Action[] addRecord = new ActionHistory.Action[chord.Length];
                for (int i = 0; i < addRecord.Length; ++i)
                    addRecord[i] = new ActionHistory.Add(chord[i]);

                if (((Note)note).flags != unmodified.flags)
                {
                    actions.AddRange(deleteRecord);
                    actions.AddRange(addRecord);
                }
            }
        }

        editor.actionHistory.Insert(actions.ToArray());
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
                    if (note.IsNaturalHopo)
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
                    if (!note.IsNaturalHopo)
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
        List<SongObject> deletedObjects = new List<SongObject>();

        foreach (ChartObject cObject in data)
        {
            deletedObjects.Add(cObject);
            if (cObject.controller)
                cObject.controller.Delete(false);
            else
                editor.currentChart.Remove(cObject);
        }

        editor.actionHistory.Insert(new ActionHistory.Delete(deletedObjects.ToArray()));
        editor.currentChart.updateArrays();

        reset();
    }

    void Copy()
    {
        List<ChartObject> chartObjectsCopy = new List<ChartObject>();
        foreach (ChartObject chartObject in data)
        {
            ChartObject objectToAdd = (ChartObject)chartObject.Clone();

            chartObjectsCopy.Add(objectToAdd);
        }
        
        if (startWorld2DChartPos < endWorld2DChartPos)
            ClipboardObjectController.clipboard = new Clipboard(chartObjectsCopy.ToArray(), rect, editor.currentSong, startWorld2DChartPos, endWorld2DChartPos);
        else
            ClipboardObjectController.clipboard = new Clipboard(chartObjectsCopy.ToArray(), rect, editor.currentSong, endWorld2DChartPos, startWorld2DChartPos);

        //ClipboardObjectController.clipboard = new Clipboard(chartObjectsCopy.ToArray(), rect, editor.currentSong);
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
