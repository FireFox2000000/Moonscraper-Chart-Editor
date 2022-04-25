// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoonscraperChartEditor.Song;

public class GroupSelect : ToolObject {
    public Transform selectedArea;

    SpriteRenderer draggingArea;

    bool addMode = true;

    Vector2 initWorld2DPos = Vector2.zero;
    Vector2 endWorld2DPos = Vector2.zero;
    uint startWorld2DChartPos = 0;
    uint endWorld2DChartPos = 0;

    Song prevSong;
    Chart prevChart;

    List<ChartObject> data = new List<ChartObject>();
    Clipboard.SelectionArea area;

    protected override void Awake()
    {
        base.Awake();

        draggingArea = GetComponent<SpriteRenderer>();

        prevSong = editor.currentSong;
        prevChart = editor.currentChart;

        data = new List<ChartObject>();
        area = new Clipboard.SelectionArea(new Rect(), 0, 0);
    }

    public override void ToolDisable()
    {
        editor.selectedObjectsManager.SetCurrentSelectedObjects(data);

        reset();
        selectedArea.gameObject.SetActive(false);
    }

    public override void ToolEnable()
    {
        base.ToolEnable();
        selectedArea.gameObject.SetActive(true);
        editor.selectedObjectsManager.currentSelectedObject = null;
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
        area = new Clipboard.SelectionArea(new Rect(), 0, 0);
    }

    bool userDraggingSelectArea = false;
    protected override void Update()
    {
        if (prevChart != editor.currentChart || prevSong != editor.currentSong)
            reset();
        if (editor.currentState == ChartEditor.State.Editor)
        {
            UpdateSnappedPos();

            // Update the corner positions
            if (Input.GetMouseButtonDown(0) && editor.services.mouseMonitorSystem.world2DPosition != null)
            {
                initWorld2DPos = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
                initWorld2DPos.y = editor.currentSong.TickToWorldYPosition(objectSnappedChartPos);
                startWorld2DChartPos = objectSnappedChartPos;

                Color col = Color.green;
                col.a = draggingArea.color.a;

                if (Globals.secondaryInputActive)
                    addMode = true;
                else if (Globals.modifierInputActive)
                {
                    addMode = false;
                    col = Color.red;
                    col.a = draggingArea.color.a;                    
                }
                else
                {
                    addMode = true;
                    data = new List<ChartObject>();
                    area = new Clipboard.SelectionArea(new Rect(), startWorld2DChartPos, endWorld2DChartPos);
                }

                draggingArea.color = col;

                userDraggingSelectArea = true;
            }

            if (Input.GetMouseButton(0) && editor.services.mouseMonitorSystem.world2DPosition != null)
            {
                endWorld2DPos = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
                endWorld2DPos.y = editor.currentSong.TickToWorldYPosition(objectSnappedChartPos);

                endWorld2DChartPos = objectSnappedChartPos;
            }

            UpdateSelectionAreaVisual(transform, initWorld2DPos, endWorld2DPos);
            UpdateSelectionAreaVisual(selectedArea, area);

            // User has finished creating a selection area
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
                    {
                        RemoveFromSelection(ScanArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos));

                        area -= new Clipboard.SelectionArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos);
                    }
                }
                else
                {
                    if (addMode)
                    {
                        AddToSelection(ScanArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos));

                        area += new Clipboard.SelectionArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos);
                    }
                    else
                    {
                        RemoveFromSelection(ScanArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos));

                        area -= new Clipboard.SelectionArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos);
                    }
                }
                selfAreaDisable();
                userDraggingSelectArea = false;
            }

            // Handle copy and cut functions
            if (data.Count > 0)
            {
                if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ClipboardCut))
                {
                    Cut();
                }
                else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ClipboardCopy))
                {
                    Copy(data, area);
                }
            }
        }

        prevSong = editor.currentSong;
        prevChart = editor.currentChart;

        editor.selectedObjectsManager.SetCurrentSelectedObjects(data);
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
        float minTickWorldPos = editor.currentSong.TickToWorldYPosition(area.tickMin);
        float maxTickWorldPos = editor.currentSong.TickToWorldYPosition(area.tickMax);

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
        int index, length;
        var chartObjects = editor.currentChart.chartObjects;
        SongObjectHelper.GetRange(chartObjects, minLimitInclusive, maxLimitNonInclusive, out index, out length);

        for (int i = index; i < index + length; ++i)
        {
            if (chartObjects[i].tick < maxLimitNonInclusive && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(chartObjects[i]), areaRect))
            {
                chartObjectsList.Add(chartObjects[i]);
            }
        }
        
        return chartObjectsList.ToArray();
    }

    void Copy(IEnumerable data, Clipboard.SelectionArea area)
    {
        List<ChartObject> chartObjectsCopy = new List<ChartObject>();
        foreach (ChartObject chartObject in data)
        {
            ChartObject objectToAdd = (ChartObject)chartObject.Clone();

            chartObjectsCopy.Add(objectToAdd);
        }

        ClipboardObjectController.SetData(chartObjectsCopy.ToArray(), area, editor.currentSong);
    }

    void Cut()
    {
        Copy(data, area);
        editor.Delete();
    }

    void AddToSelection(IEnumerable<ChartObject> chartObjects)
    {
        // Insertion sort
        foreach(ChartObject chartObject in chartObjects)
        {
            if (!data.Contains(chartObject))
            {
                int pos = SongObjectHelper.FindClosestPosition(chartObject, data.ToArray());
                if (pos != SongObjectHelper.NOTFOUND)
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
}
