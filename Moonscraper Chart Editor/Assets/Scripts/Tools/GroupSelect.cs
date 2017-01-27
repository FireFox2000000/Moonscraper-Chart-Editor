using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupSelect : ToolObject {
    public GameObject selectedHighlight;
    public Transform selectedArea;

    GameObject highlightPoolParent;
    GameObject[] highlightPool = new GameObject[100];

    SpriteRenderer ren;

    bool addMode = true;

    Vector2 initWorld2DPos = Vector2.zero;
    Vector2 endWorld2DPos = Vector2.zero;
    uint startWorld2DChartPos = 0;
    uint endWorld2DChartPos = 0;

    Song prevSong;
    Chart prevChart;

    //Clipboard data;
    List<ChartObject> data = new List<ChartObject>();
    //Rect rect;
    Clipboard.SelectionArea area;

    protected override void Awake()
    {
        base.Awake();
        ren = GetComponent<SpriteRenderer>();
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
        area = new Clipboard.SelectionArea(new Rect(), 0, 0);
        //ren.sharedMaterial.color = new Color(1, 1, 1, 1);
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

    void selfAreaDisable()
    {
        transform.localScale = new Vector3(0, 0, transform.localScale.z);
        initWorld2DPos = Vector2.zero;
        endWorld2DPos = Vector2.zero;
        startWorld2DChartPos = 0;
        endWorld2DChartPos = 0;
    }

    public void reset()
    {
        selfAreaDisable();
        data = new List<ChartObject>();
        //rect = new Rect();
        area = new Clipboard.SelectionArea(new Rect(), 0, 0);
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

                Color col = Color.green;
                col.a = ren.color.a;

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    addMode = true;
                else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    addMode = false;
                    col = Color.red;
                    col.a = ren.color.a;                    
                }
                else
                {
                    addMode = true;
                    data = new List<ChartObject>();
                    area = new Clipboard.SelectionArea(new Rect(), startWorld2DChartPos, endWorld2DChartPos);
                }

                ren.color = col;

                userDraggingSelectArea = true;
            }

            if (Input.GetMouseButton(0) && Mouse.world2DPosition != null)
            {
                endWorld2DPos = (Vector2)Mouse.world2DPosition;
                endWorld2DPos.y = editor.currentSong.ChartPositionToWorldYPosition(objectSnappedChartPos);

                endWorld2DChartPos = objectSnappedChartPos;
            }

            UpdateSelectionAreaVisual(transform, initWorld2DPos, endWorld2DPos);
            UpdateSelectionAreaVisual(selectedArea, area);

            if (Input.GetMouseButtonUp(0) && userDraggingSelectArea)
            {
                if (startWorld2DChartPos > endWorld2DChartPos)
                {
                    if (addMode)
                    {
                        AddToSelection(ScanArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos));

                        area += new Clipboard.SelectionArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos);
                    }
                    else
                        RemoveFromSelection(ScanArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos));
                }
                else
                {
                    if (addMode)
                    {
                        AddToSelection(ScanArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos));

                        area += new Clipboard.SelectionArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos);
                    }
                    else
                        RemoveFromSelection(ScanArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos));
                }
                selfAreaDisable();
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

    void UpdateSelectionAreaVisual(Transform areaTransform, Vector2 initWorld2DPos, Vector2 endWorld2DPos)
    {
        Vector2 diff = new Vector2(Mathf.Abs(initWorld2DPos.x - endWorld2DPos.x), Mathf.Abs(initWorld2DPos.y - endWorld2DPos.y));

        // Set size
        areaTransform.localScale = new Vector3(diff.x, diff.y, transform.localScale.z);

        // Calculate center pos
        Vector3 pos = areaTransform.position;
        if (initWorld2DPos.x < endWorld2DPos.x)
            pos.x = initWorld2DPos.x + diff.x / 2;
        else
            pos.x = endWorld2DPos.x + diff.x / 2;

        if (initWorld2DPos.y < endWorld2DPos.y)
            pos.y = initWorld2DPos.y + diff.y / 2;
        else
            pos.y = endWorld2DPos.y + diff.y / 2;

        areaTransform.position = pos;
    }

    void UpdateSelectionAreaVisual(Transform areaTransform, Clipboard.SelectionArea area)
    {
        float minTickWorldPos = editor.currentSong.ChartPositionToWorldYPosition(area.tickMin);
        float maxTickWorldPos = editor.currentSong.ChartPositionToWorldYPosition(area.tickMax);

        Vector3 scale = new Vector3(area.width, maxTickWorldPos - minTickWorldPos, areaTransform.localScale.z);
        Vector3 position = new Vector3(area.xPos + (area.width / 2), (minTickWorldPos + maxTickWorldPos) / 2, areaTransform.position.z);

        areaTransform.localScale = scale;
        areaTransform.position = position;
    }

    ChartObject[] ScanArea(Vector2 cornerA, Vector2 cornerB, uint minLimitInclusive, uint maxLimitNonInclusive)
    {
        Clipboard.SelectionArea area = new Clipboard.SelectionArea(cornerA, cornerB, minLimitInclusive, maxLimitNonInclusive);
        Rect areaRect = area.GetRect(editor.currentSong);

        List<ChartObject> chartObjectsList = new List<ChartObject>();

        foreach (ChartObject chartObject in editor.currentChart.chartObjects)
        {
            if (chartObject.position >= minLimitInclusive && chartObject.position < maxLimitNonInclusive && chartObject.controller && chartObject.controller.AABBcheck(areaRect))
            {
                chartObjectsList.Add(chartObject);
            }
        }

        return chartObjectsList.ToArray();
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

        //Clipboard.SelectionArea area = new Clipboard.SelectionArea(rect, startWorld2DChartPos, endWorld2DChartPos);
        
        if (startWorld2DChartPos < endWorld2DChartPos)
            ClipboardObjectController.clipboard = new Clipboard(chartObjectsCopy.ToArray(), area, editor.currentSong);
        else
            ClipboardObjectController.clipboard = new Clipboard(chartObjectsCopy.ToArray(), area, editor.currentSong);

        //ClipboardObjectController.clipboard = new Clipboard(chartObjectsCopy.ToArray(), rect, editor.currentSong);
    }

    void Cut()
    {
        Copy();
        Delete();
    }

    void AddToSelection(IEnumerable<ChartObject> chartObjects)
    {
        foreach(ChartObject chartObject in chartObjects)
        {
            if (!data.Contains(chartObject))
            {
                int pos = SongObject.FindClosestPosition(chartObject, data.ToArray());
                if (pos != Globals.NOTFOUND)
                {
                    if (data[pos] > chartObject)
                        data.Insert(pos, chartObject);
                    else
                        data.Insert(pos + 1, chartObject);
                }
                else
                    data.Add(chartObject);
            }
        }
    }

    void RemoveFromSelection(IEnumerable<ChartObject> chartObjects)
    {
        foreach (ChartObject chartObject in chartObjects)
        {
            data.Remove(chartObject);
        }
    }

    public enum AppliedNoteType
    {
        Natural, Strum, Hopo, Tap
    }
}
