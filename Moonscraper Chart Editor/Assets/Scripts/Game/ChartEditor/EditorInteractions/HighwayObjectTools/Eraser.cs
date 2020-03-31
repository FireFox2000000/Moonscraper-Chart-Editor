﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public class Eraser : ToolObject {

    public static List<SongObject> dragEraseHistory = new List<SongObject>();

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

        if (Input.GetMouseButton(0))
        {
            GameObject currentMouseHover = editor.services.mouseMonitorSystem.currentSelectableUnderMouse;
            if (currentMouseHover)
            {
                SongObjectController soController = currentMouseHover.GetComponent<SongObjectController>();
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
                    else if((songObject.classID != (int)SongObject.ID.BPM && songObject.classID != (int)SongObject.ID.TimeSignature) || songObject.tick != 0)
                    {
                        dragEraseHistory.Add(songObject);                       
                    }

                    ExecuteCurrentDeletes();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragEraseHistory.Clear();
        }   
    }

    void ExecuteCurrentDeletes()
    {
        if (dragEraseHistory.Count > 1)
            editor.commandStack.Pop();

        editor.commandStack.Push(new SongEditDelete(dragEraseHistory));
    }
}
