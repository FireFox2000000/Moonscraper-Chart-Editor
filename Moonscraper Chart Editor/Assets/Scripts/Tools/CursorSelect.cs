// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSelect : ToolObject
{
    // Cursor variables
    [SerializeField]
    GroupMove groupMove;
    bool mouseDownOverUI = false;
    Vector3 mousePos = Vector3.zero;
    GameObject clickedSelectableObject;

    // Group selection variables
    [SerializeField]
    SpriteRenderer draggingArea;            // Shows the current area the user is dragging in
    Color initColor;

    bool addMode = true;
    bool userDraggingSelectArea = false;
    Vector2 initWorld2DPos = Vector2.zero;
    Vector2 endWorld2DPos = Vector2.zero;
    uint startWorld2DChartPos = 0;
    uint endWorld2DChartPos = 0;

    bool block = false;

    protected override void Awake()
    {
        base.Awake();

        initColor = draggingArea.color;
    }

    protected override void Update()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && !block)
        {
            UpdateSnappedPos();

            if (Input.GetMouseButtonDown(0))
            {
                mousePos = Input.mousePosition;
                mouseDownOverUI = Mouse.IsUIUnderPointer();
                clickedSelectableObject = Mouse.currentSelectableUnderMouse;

                // Reset if the user is making a new selection or deselecting the old
                if (!clickedSelectableObject && !Globals.modifierInputActive && !Globals.secondaryInputActive && !mouseDownOverUI)
                {
                    editor.currentSelectedObject = null;
                }

                if (/*Globals.viewMode == Globals.ViewMode.Chart &&*/ Mouse.world2DPosition != null && !Mouse.currentSelectableUnderMouse && !Mouse.IsUIUnderPointer())
                    InitGroupSelect();
            }
            else if (Input.GetMouseButtonUp(0))
                clickedSelectableObject = null;

            // Dragging mouse for group select
            if (userDraggingSelectArea && Input.GetMouseButton(0) && !clickedSelectableObject && !mouseDownOverUI)
            {
                if (Mouse.world2DPosition != null)
                {
                    endWorld2DPos = (Vector2)Mouse.world2DPosition;
                    endWorld2DPos.y = editor.currentSong.TickToWorldYPosition(objectSnappedChartPos);

                    endWorld2DChartPos = objectSnappedChartPos;
                }

                UpdateSelectionAreaVisual(draggingArea.transform, initWorld2DPos, endWorld2DPos);
            }

            // Dragging mouse for group move
            else if (Input.GetMouseButton(0) && mousePos != Input.mousePosition && editor.currentSelectedObjects.Count > 0 && clickedSelectableObject && !mouseDownOverUI &&
                !Globals.modifierInputActive && !Globals.secondaryInputActive)
            {
                // Find anchor point
                int anchorPoint = SongObjectHelper.NOTFOUND;

                if (clickedSelectableObject)
                {
                    for (int i = 0; i < editor.currentSelectedObjects.Count; ++i)
                    {
                        if (editor.currentSelectedObjects[i].controller != null && editor.currentSelectedObjects[i].controller.gameObject == clickedSelectableObject)
                        {
                            anchorPoint = i;
                            break;
                        }
                    }
                }

                groupMove.StartMoveAction(editor.currentSelectedObjects, anchorPoint, true);
            }

            // User has finished creating a group selection area
            if (Input.GetMouseButtonUp(0) && userDraggingSelectArea)
            {
                if (startWorld2DChartPos > endWorld2DChartPos)
                {
                    if (addMode)
                        AddToSelection(ScanArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos));
                    else
                        RemoveFromSelection(ScanArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos));
                }
                else
                {
                    if (addMode)
                        AddToSelection(ScanArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos));
                    else
                        RemoveFromSelection(ScanArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos));
                }
                selfAreaDisable();
                userDraggingSelectArea = false;
            }

            // Check for deselection of all objects
            if (Input.GetMouseButtonUp(0) && !Mouse.currentSelectableUnderMouse && !Mouse.IsUIUnderPointer() && mousePos == Input.mousePosition && !Globals.modifierInputActive)
            {
                editor.currentSelectedObject = null;
                mousePos = Vector3.zero;
            }
        }
        else
            block = true;

        if (block && Globals.applicationMode == Globals.ApplicationMode.Editor && !Input.GetMouseButton(0))
            block = false;
    }

    public override void ToolDisable()
    {
        mousePos = Vector3.zero;
    }

    // Resets all the group selection properties
    void InitGroupSelect()
    {
        initWorld2DPos = (Vector2)Mouse.world2DPosition;
        initWorld2DPos.y = editor.currentSong.TickToWorldYPosition(objectSnappedChartPos);
        endWorld2DPos = initWorld2DPos;
        startWorld2DChartPos = objectSnappedChartPos;
        endWorld2DChartPos = startWorld2DChartPos;

        Color col = initColor;
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
        }

        draggingArea.color = col;

        userDraggingSelectArea = true;
    }

    void UpdateGroupSelectSize()
    {
        endWorld2DPos = (Vector2)Mouse.world2DPosition;
        endWorld2DPos.y = editor.currentSong.TickToWorldYPosition(objectSnappedChartPos);

        endWorld2DChartPos = objectSnappedChartPos;
    }

    void FinishGroupSelect()
    {
        if (startWorld2DChartPos > endWorld2DChartPos)
        {
            if (addMode)
                AddToSelection(ScanArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos));
            else
                RemoveFromSelection(ScanArea(initWorld2DPos, endWorld2DPos, endWorld2DChartPos, startWorld2DChartPos));
        }
        else
        {
            if (addMode)
                AddToSelection(ScanArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos));
            else
                RemoveFromSelection(ScanArea(initWorld2DPos, endWorld2DPos, startWorld2DChartPos, endWorld2DChartPos));
        }

        selfAreaDisable();
        userDraggingSelectArea = false;
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

    void selfAreaDisable()
    {
        draggingArea.transform.localScale = new Vector3(0, 0, draggingArea.transform.localScale.z);
        initWorld2DPos = Vector2.zero;
        endWorld2DPos = Vector2.zero;
        startWorld2DChartPos = 0;
        endWorld2DChartPos = 0;
    }

    void AddToSelection(IEnumerable<SongObject> chartObjects)
    {
        editor.AddToSelectedObjects((IEnumerable <SongObject>)chartObjects);
    }

    void RemoveFromSelection(IEnumerable<SongObject> chartObjects)
    {
        editor.RemoveFromSelectedObjects((IEnumerable<SongObject>)chartObjects);
    }

    SongObject[] ScanArea(Vector2 cornerA, Vector2 cornerB, uint minLimitInclusive, uint maxLimitNonInclusive)
    {
        Clipboard.SelectionArea area = new Clipboard.SelectionArea(cornerA, cornerB, minLimitInclusive, maxLimitNonInclusive);
        Rect areaRect = area.GetRect(editor.currentSong);

        List<SongObject> chartObjectsList = new List<SongObject>();
        if (Globals.viewMode == Globals.ViewMode.Chart)
        {
            int index, length;
            SongObjectHelper.GetRange(editor.currentChart.chartObjects, minLimitInclusive, maxLimitNonInclusive, out index, out length);

            for (int i = index; i < index + length; ++i)
            {
                ChartObject chartObject = editor.currentChart.chartObjects[i];
                float offset = 0;
                if ((SongObject.ID)chartObject.classID == SongObject.ID.ChartEvent)
                    offset = ChartEventController.GetOffset(editor, (ChartEvent)chartObject);

                if (chartObject.tick < maxLimitNonInclusive && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(chartObject, 0, offset), areaRect))
                    chartObjectsList.Add(chartObject);
            }
        }
        else
        {
            // Gather synctrack, sections and events
            int index, length;
            SongObjectHelper.GetRange(editor.currentSong.syncTrack, minLimitInclusive, maxLimitNonInclusive, out index, out length);

            // Synctrack
            for (int i = index; i < index + length; ++i)
            {
                SongObject chartObject = editor.currentSong.syncTrack[i];

                if (chartObject.tick < maxLimitNonInclusive && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(chartObject), areaRect))
                    chartObjectsList.Add(chartObject);
            }

            SongObjectHelper.GetRange(editor.currentSong.eventsAndSections, minLimitInclusive, maxLimitNonInclusive, out index, out length);

            // Events and sections
            for (int i = index; i < index + length; ++i)
            {
                SongObject chartObject = editor.currentSong.eventsAndSections[i];
                float offset = 0;
                if ((SongObject.ID)chartObject.classID == SongObject.ID.Event)
                    offset = EventController.GetOffset(editor, (Event)chartObject);

                if (chartObject.tick < maxLimitNonInclusive && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(chartObject, 0, offset), areaRect))
                    chartObjectsList.Add(chartObject);
            }
        }

        return chartObjectsList.ToArray();
    }
}
