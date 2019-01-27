// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System;

public class PlaceSection : PlaceSongObject {
    public Section section { get { return (Section)songObject; } set { songObject = value; } }
    new public SectionController controller { get { return (SectionController)base.controller; } set { base.controller = value; } }

    protected override void SetSongObjectAndController()
    {
        section = new Section("Default", 0);

        controller = GetComponent<SectionController>();
        controller.section = section;
    }

    protected override void Controls()
    {
        if (!GameSettings.keysModeEnabled)
        {
            if (Toolpane.currentTool == Toolpane.Tools.Section && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
            {
                Section sectionSearched = sectionSearch(section.tick);
                if (sectionSearched == null)
                {
                    AddObject();
                }
                else
                    editor.currentSelectedObject = sectionSearched;
            }
        }
        else if (ShortcutInput.GetInputDown(Shortcut.AddSongObject))
        {
            var searchArray = editor.currentSong.sections;
            int pos = SongObjectHelper.FindObjectPosition(section, searchArray);
            if (pos == SongObjectHelper.NOTFOUND)
            {
                AddObject();
            }
            else
            {
                editor.commandStack.Push(new SongEditDelete(searchArray[pos]));
                editor.currentSelectedObject = null;
            }
        } 
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(new Section(this.section)));
        editor.SelectSongObject(section, editor.currentSong.sections);
    }

    Section sectionSearch(uint pos)
    {
        int index, length;
        SongObjectHelper.FindObjectsAtPosition(pos, editor.currentSong.sections, out index, out length);

        if (length > 0)
            return editor.currentSong.sections[index];
        else
            return null;
    }
}
