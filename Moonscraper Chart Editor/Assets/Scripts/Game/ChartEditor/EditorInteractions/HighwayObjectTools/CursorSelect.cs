// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public class CursorSelect : ToolObject
{
    // Cursor variables
    [SerializeField]
    GroupMove groupMove = null;
    bool mouseDownOverUI = false;
    Vector3 mousePos = Vector3.zero;
    GameObject clickedSelectableObject;

    // Group selection variables
    [SerializeField]
    SpriteRenderer draggingArea = null;            // Shows the current area the user is dragging in
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
        if (!block)
        {
            UpdateSnappedPos();
            MouseMonitor mouseMonitor = editor.services.mouseMonitorSystem;

            if (Input.GetMouseButtonDown(0))
            {
                mousePos = Input.mousePosition;
                mouseDownOverUI = MouseMonitor.IsUIUnderPointer();
                clickedSelectableObject = mouseMonitor.currentSelectableUnderMouse;

                // Reset if the user is making a new selection or deselecting the old
                if (!clickedSelectableObject && !Globals.modifierInputActive && !Globals.secondaryInputActive && !mouseDownOverUI)
                {
                    editor.selectedObjectsManager.currentSelectedObject = null;
                }

                if (editor.services.mouseMonitorSystem.world2DPosition != null && !mouseMonitor.currentSelectableUnderMouse && !MouseMonitor.IsUIUnderPointer())
                    InitGroupSelect();
            }
            else if (Input.GetMouseButtonUp(0))
                clickedSelectableObject = null;

            // Dragging mouse for group select
            if (userDraggingSelectArea && Input.GetMouseButton(0) && !clickedSelectableObject && !mouseDownOverUI)
            {
                if (editor.services.mouseMonitorSystem.world2DPosition != null)
                {
                    endWorld2DPos = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
                    endWorld2DPos.y = editor.currentSong.TickToWorldYPosition(objectSnappedChartPos);

                    endWorld2DChartPos = objectSnappedChartPos;
                }

                UpdateSelectionAreaVisual(draggingArea.transform, initWorld2DPos, endWorld2DPos);
            }

            // Dragging mouse for group move
            else if (Input.GetMouseButton(0) && mousePos != Input.mousePosition && editor.selectedObjectsManager.currentSelectedObjects.Count > 0 && clickedSelectableObject && !mouseDownOverUI &&
                !Globals.modifierInputActive && !Globals.secondaryInputActive)
            {
                // Find anchor point
                int anchorPoint = SongObjectHelper.NOTFOUND;

                if (clickedSelectableObject)
                {
                    for (int i = 0; i < editor.selectedObjectsManager.currentSelectedObjects.Count; ++i)
                    {
                        if (editor.selectedObjectsManager.currentSelectedObjects[i].controller != null && editor.selectedObjectsManager.currentSelectedObjects[i].controller.gameObject == clickedSelectableObject)
                        {
                            anchorPoint = i;
                            break;
                        }
                    }
                }

                groupMove.StartMoveAction(editor.selectedObjectsManager.currentSelectedObjects, anchorPoint, true);
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
                SelfAreaDisable();
                userDraggingSelectArea = false;
            }

            // Check for deselection of all objects
            if (Input.GetMouseButtonUp(0) && !mouseMonitor.currentSelectableUnderMouse && !MouseMonitor.IsUIUnderPointer() && mousePos == Input.mousePosition && !Globals.modifierInputActive)
            {
                editor.selectedObjectsManager.currentSelectedObject = null;
                mousePos = Vector3.zero;
            }
        }
        else
            block = true;

        if (block && !Input.GetMouseButton(0))
            block = false;
    }

    public override void ToolDisable()
    {
        mousePos = Vector3.zero;
        SelfAreaDisable();
    }

    private void OnDisable()
    {
        draggingArea.transform.localScale = new Vector3(0, 0, draggingArea.transform.localScale.z);
    }

    // Resets all the group selection properties
    void InitGroupSelect()
    {
        initWorld2DPos = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
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
        endWorld2DPos = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
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

        SelfAreaDisable();
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

    void SelfAreaDisable()
    {
        draggingArea.transform.localScale = new Vector3(0, 0, draggingArea.transform.localScale.z);
        initWorld2DPos = Vector2.zero;
        endWorld2DPos = Vector2.zero;
        startWorld2DChartPos = 0;
        endWorld2DChartPos = 0;
    }

    void AddToSelection(IEnumerable<SongObject> chartObjects)
    {
        editor.selectedObjectsManager.AddToSelectedObjects((IEnumerable <SongObject>)chartObjects);
    }

    void RemoveFromSelection(IEnumerable<SongObject> chartObjects)
    {
        editor.selectedObjectsManager.RemoveFromSelectedObjects((IEnumerable<SongObject>)chartObjects);
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

            float offset = 0;
            for (int i = index; i < index + length; ++i)
            {
                ChartObject chartObject = editor.currentChart.chartObjects[i];

                if ((SongObject.ID)chartObject.classID == SongObject.ID.Note)
                {
                    // If the object is within a lane that is not currently included we need to skip over this note
                    if (((Note)chartObject).ShouldBeCulledFromLanes(editor.laneInfo))
                    {
                        continue;
                    }
                }

                // Add the whole length for roll lanes, not just the root
                if ((SongObject.ID)chartObject.classID == SongObject.ID.DrumRoll)
                {
                    // This is handled in a second loop below. Drum rolls need the length of the whole trigger.
                    // SongObjectHelper.GetRange has not accounted for the tail of the trigger.
                    // Either we search backward indefinately or do a second loop over just the drum roll objects
                    // themselves, which is prefering to avoid scanning the whole chart backwards every time.
                    continue;
                }
                else if (chartObject.tick < maxLimitNonInclusive && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(chartObject, 0, offset), areaRect))
                {
                    chartObjectsList.Add(chartObject);
                }
            }

            // Drum roll specific search to avoid indefinite backwards search
            {
                SongObjectHelper.GetRange(editor.currentChart.drumRoll, minLimitInclusive, maxLimitNonInclusive, out index, out length);
                if (index > 0)
                {
                    --index;
                    ++length;
                }

                if (index + length <= editor.currentChart.drumRoll.Count - 1)
                {
                    ++length;
                }

                for (int i = index; i < index + length; ++i)
                {
                    DrumRoll drumRoll = editor.currentChart.drumRoll[i];

                    uint endTick = drumRoll.tick + drumRoll.length;
                    bool tailInView = endTick >= minLimitInclusive && endTick < maxLimitNonInclusive;
                    bool rootInView = drumRoll.tick >= minLimitInclusive && drumRoll.tick < maxLimitNonInclusive;
                    bool bodyInView = drumRoll.tick <= minLimitInclusive && endTick >= maxLimitNonInclusive;

                    bool drumRollInTickRange = tailInView || rootInView || bodyInView;
                    if (drumRollInTickRange && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(drumRoll, 0, offset), areaRect))
                    {
                        chartObjectsList.Add(drumRoll);
                    }
                }
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

                if (chartObject.tick < maxLimitNonInclusive && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(chartObject, 0, offset), areaRect))
                    chartObjectsList.Add(chartObject);
            }
        }

        return chartObjectsList.ToArray();
    }
}
