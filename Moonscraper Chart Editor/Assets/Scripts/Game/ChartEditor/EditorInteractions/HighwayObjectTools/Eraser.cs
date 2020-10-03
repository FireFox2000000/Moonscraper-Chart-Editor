// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using MoonscraperChartEditor.Song;

public class Eraser : ToolObject {

    public static List<SongObject> dragEraseHistory = new List<SongObject>();
    Vector2? prevFrameMouseWorldPos = null;

    public override void ToolEnable()
    {
        editor.selectedObjectsManager.currentSelectedObject = null;
    }

    public override void ToolDisable()
    {
        base.ToolDisable();

        if (dragEraseHistory.Count > 0)
        {
            dragEraseHistory.Clear();
        }
    }

    protected override void Update()
    {
        base.Update();

        var mouseMonitor = editor.services.mouseMonitorSystem;
        Vector2? mouseWorldPos = mouseMonitor.world2DPosition;
        GameObject currentMouseHover = mouseMonitor.currentSelectableUnderMouse;

        if (Input.GetMouseButtonDown(0))
        {
            prevFrameMouseWorldPos = mouseWorldPos;

            if (currentMouseHover)
            {
                SongObjectController soController = currentMouseHover.GetComponent<SongObjectController>();
                DeleteSongObject(soController);
            }
        }
        else if (Input.GetMouseButton(0) && mouseWorldPos != prevFrameMouseWorldPos)    // Don't delete kick note and regular note at the same time
        {
            if (currentMouseHover)
            {
                SongObjectController soController = currentMouseHover.GetComponent<SongObjectController>();
                DeleteSongObject(soController);
            }

            prevFrameMouseWorldPos = mouseWorldPos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragEraseHistory.Clear();
        }   
    }

    void DeleteSongObject(SongObjectController soController)
    {
        if (soController && soController.GetSongObject() != null)       // Dunno why song object would ever be null, but may as well be safe
        {
            SongObject songObject = soController.GetSongObject();
            if (MSChartEditorInput.GetInput(MSChartEditorInputActions.ChordSelect) && songObject.classID == (int)SongObject.ID.Note)
            {
                foreach (Note note in ((Note)songObject).chord)
                {
                    dragEraseHistory.Add(note);
                }
            }
            else if ((songObject.classID != (int)SongObject.ID.BPM && songObject.classID != (int)SongObject.ID.TimeSignature) || songObject.tick != 0)
            {
                dragEraseHistory.Add(songObject);
            }

            ExecuteCurrentDeletes();
        }
    }

    void ExecuteCurrentDeletes()
    {
        if (dragEraseHistory.Count > 1)
            editor.commandStack.Pop();

        editor.commandStack.Push(new SongEditDelete(dragEraseHistory));
    }
}
