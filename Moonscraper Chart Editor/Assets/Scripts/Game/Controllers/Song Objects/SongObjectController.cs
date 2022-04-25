// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public abstract class SongObjectController : SelectableClick {
    public const float CHART_CENTER_POS = 0;

    protected ChartEditor editor;
    protected SongObject songObject = null;
    protected bool isDirty = false;
    Bounds bounds;

    public abstract void UpdateSongObject();
    public bool disableCancel = true;
    private bool isTool = false;

    protected void Awake()
    {
        editor = ChartEditor.Instance;
        isTool = GetComponent<ToolObject>();
    }

    protected virtual void OnEnable()
    {
        if (songObject != null)
            UpdateSongObject();
    }
    
    void OnDisable()
    {
        if (disableCancel && songObject != null)
        {
            Init(null, null);
        }
    }

    protected bool moveCheck
    {
        get
        {
            return editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Cursor && editor.currentState == ChartEditor.State.Editor && Input.GetMouseButton(0) && !Input.GetMouseButton(1) 
                && editor.selectedObjectsManager.currentSelectedObject != null;
        }
    }

    public SongObject GetSongObject()
    {
        return songObject;
    }

    public override void OnSelectableMouseDrag()
    {
    }

    void Update()
    {
        if (songObject != null && songObject.song != null)
            UpdateCheck();
        else
            gameObject.SetActive(false);
    }

    protected virtual void UpdateCheck()
    {
        if (songObject != null && songObject.tick >= editor.minPos && songObject.tick < editor.maxPos)
        {
            if (editor.currentState == ChartEditor.State.Editor)
            {
                UpdateSongObject();
            }
        }
        else if (songObject != null)
            gameObject.SetActive(false);
    }
    
    protected void OnBecameVisible()
    {
        UpdateCheck();
    }

    protected void Init(SongObject _songObject, SongObjectController controller)
    {
        if (_songObject == null && songObject != null)
        {
            songObject.controller = null;
        }

        songObject = _songObject;

        if (songObject != null)
            songObject.controller = controller;
    }

    public void SetDirty()
    {
        isDirty = true;
    }

    public override void OnSelectableMouseDown()
    {
        if (editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Cursor && editor.currentState == ChartEditor.State.Editor && Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            // Shift-clicking
            // Find the closest object already selected
            // Select all objects in range of that found and the clicked object

            var selectedObjectsManager = editor.selectedObjectsManager;

            if (Globals.viewMode == Globals.ViewMode.Chart && (Globals.modifierInputActive || Globals.secondaryInputActive))
            {
                // Ctrl-clicking
                if (Globals.modifierInputActive)
                {
                    if (selectedObjectsManager.IsSelected(songObject))
                        selectedObjectsManager.RemoveFromSelectedObjects(songObject);
                    else
                        selectedObjectsManager.AddToSelectedObjects(songObject);
                }
                // Shift-clicking
                else
                {
                    int pos = SongObjectHelper.FindClosestPosition(this.songObject, editor.selectedObjectsManager.currentSelectedObjects);

                    if (pos != SongObjectHelper.NOTFOUND)
                    {
                        uint min;
                        uint max;

                        if (editor.selectedObjectsManager.currentSelectedObjects[pos].tick > songObject.tick)
                        {
                            max = editor.selectedObjectsManager.currentSelectedObjects[pos].tick;
                            min = songObject.tick;
                        }
                        else
                        {
                            min = editor.selectedObjectsManager.currentSelectedObjects[pos].tick;
                            max = songObject.tick;
                        }

                        var chartObjects = editor.currentChart.chartObjects;
                        int index, length;
                        SongObjectHelper.GetRange(chartObjects, min, max, out index, out length);
                        editor.selectedObjectsManager.currentSelectedObjects.Clear();
                        for (int i = index; i < index + length; ++i)
                        {
                            editor.selectedObjectsManager.currentSelectedObjects.Add(chartObjects[i]);
                        }
                    }
                }
            }
            else if (!selectedObjectsManager.IsSelected(songObject))
                selectedObjectsManager.currentSelectedObject = songObject;
        }

        // Delete the object on erase tool or by holding right click and pressing left-click
        else if (editor.currentState == ChartEditor.State.Editor && 
            Input.GetMouseButtonDown(0) && Input.GetMouseButton(1)
            )
        {
            if ((songObject.classID != (int)SongObject.ID.BPM && songObject.classID != (int)SongObject.ID.TimeSignature) || songObject.tick != 0)
            {
                if (Input.GetMouseButton(1))
                {
                    Debug.Log("Deleted " + songObject + " at position " + songObject.tick + " with hold-right left-click shortcut");
                    editor.commandStack.Push(new SongEditDelete(songObject));
                }
                editor.selectedObjectsManager.currentSelectedObject = null;
            }
        }
    }

    public static float GetXPos (SongObject songObject)
    {
        float position;

        switch ((SongObject.ID)songObject.classID)
        {
            case (SongObject.ID.Starpower):
                position = StarpowerController.position;
                break;
            case (SongObject.ID.Note):
                position = NoteController.NoteToXPos((Note)songObject);
                break;
            case (SongObject.ID.ChartEvent):
                position = ChartEventController.position;
                break;
            case (SongObject.ID.BPM):
                position = BPMController.position;
                break;
            case (SongObject.ID.TimeSignature):
                position = TimesignatureController.position;
                break;
            case (SongObject.ID.Section):
                position = SectionController.position;
                break;
            case (SongObject.ID.Event):
                position = EventController.position;
                break;
            default:
                position = 0;
                break;
        }

        return position;
    }

    protected float desiredWorldYPosition
    {
        get
        {
            return ChartEditor.WorldYPosition(songObject);
        }
    }
}
