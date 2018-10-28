// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public abstract class SongObjectController : SelectableClick {
    public const float CHART_CENTER_POS = 0;

    protected ChartEditor editor;
    protected SongObject songObject = null;
    protected bool isDirty = false;
    Bounds bounds;

    public abstract void UpdateSongObject();
    public bool disableCancel = true;

    protected void Awake()
    {
        editor = ChartEditor.Instance;
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

    protected bool moveCheck { get { return Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0) && !Input.GetMouseButton(1) 
                && editor.currentSelectedObject != null; } }

    public SongObject GetSongObject()
    {
        return songObject;
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        // This is now being done via the cursor tool
        /*if (moveCheck)
        {
            editor.groupMove.SetSongObjects(songObject);
            songObject.Delete();
        }*/
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
            if (Globals.applicationMode == Globals.ApplicationMode.Editor)
                UpdateSongObject();
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
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            // Shift-clicking
            // Find the closest object already selected
            // Select all objects in range of that found and the clicked object
            
            if (Globals.viewMode == Globals.ViewMode.Chart && (Globals.modifierInputActive || Globals.secondaryInputActive))
            {
                // Ctrl-clicking
                if (Globals.modifierInputActive)
                {
                    if (editor.IsSelected(songObject))
                        editor.RemoveFromSelectedObjects(songObject);
                    else
                        editor.AddToSelectedObjects(songObject);
                }
                // Shift-clicking
                else
                {
                    int pos = SongObjectHelper.FindClosestPosition(this.songObject, editor.currentSelectedObjects);

                    if (pos != SongObjectHelper.NOTFOUND)
                    {
                        uint min;
                        uint max;

                        if (editor.currentSelectedObjects[pos].tick > songObject.tick)
                        {
                            max = editor.currentSelectedObjects[pos].tick;
                            min = songObject.tick;
                        }
                        else
                        {
                            min = editor.currentSelectedObjects[pos].tick;
                            max = songObject.tick;
                        }

                        var chartObjects = editor.currentChart.chartObjects;
                        int index, length;
                        SongObjectHelper.GetRange(chartObjects, min, max, out index, out length);
                        editor.currentSelectedObjects.Clear();
                        for (int i = index; i < index + length; ++i)
                        {
                            editor.currentSelectedObjects.Add(chartObjects[i]);
                        }
                    }
                }
            }
            else if (!editor.IsSelected(songObject))
                editor.currentSelectedObject = songObject;
        }

        // Delete the object on erase tool or by holding right click and pressing left-click
        else if (Globals.applicationMode == Globals.ApplicationMode.Editor && 
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
                editor.currentSelectedObject = null;
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
            case (SongObject.ID.BPM):
                position = BPMController.position;
                break;
            case (SongObject.ID.TimeSignature):
                position = TimesignatureController.position;
                break;
            case (SongObject.ID.Section):
                position = SectionController.position;
                break;
            default:
                position = 0;
                break;
        }

        return position;
    }
}
