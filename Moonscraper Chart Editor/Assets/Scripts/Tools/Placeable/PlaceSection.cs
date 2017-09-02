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
        if (!Globals.lockToStrikeline)
        {
            if (Toolpane.currentTool == Toolpane.Tools.Section && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
            {
                Section sectionSearched = sectionSearch(section.position);
                if (sectionSearched == null)
                {
                    RecordAddActionHistory(section, editor.currentSong.sections);

                    AddObject();
                }
                else
                    editor.currentSelectedObject = sectionSearched;
            }
        }
        else if (Input.GetButtonDown("Add Object"))
        {
            SongObject[] searchArray = editor.currentSong.sections;
            int pos = SongObject.FindObjectPosition(section, searchArray);
            if (pos == SongObject.NOTFOUND)
            {
                editor.actionHistory.Insert(new ActionHistory.Add(section));
                AddObject();
            }
            else
            {
                editor.actionHistory.Insert(new ActionHistory.Delete(searchArray[pos]));
                searchArray[pos].Delete();
                editor.currentSelectedObject = null;
            }
        } 
    }

    protected override void AddObject()
    {
        AddObjectToCurrentSong(section, editor);
    }

    public static void AddObjectToCurrentSong(Section section, ChartEditor editor, bool update = true)
    {
        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd, update);
        //editor.CreateSectionObject(sectionToAdd);
        editor.currentSelectedObject = sectionToAdd;
    }

    Section sectionSearch(uint pos)
    {
        Section[] sectionsFound = SongObject.FindObjectsAtPosition(pos, editor.currentSong.sections);

        if (sectionsFound.Length > 0)
            return sectionsFound[0];
        else
            return null;
    }
}
