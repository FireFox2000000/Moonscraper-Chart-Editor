using UnityEngine;
using System.Collections;
using System;

public class PlaceSection : PlaceSongObject {
    public Section section { get { return (Section)songObject; } set { songObject = value; } }
    new public SectionController controller { get { return (SectionController)base.controller; } set { base.controller = value; } }

    protected override void Awake()
    {
        base.Awake();
        section = new Section(editor.currentSong, "Default", 0);

        controller = GetComponent<SectionController>();
        controller.section = section;
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Section && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            AddObject();
        }
    }

    protected override void AddObject()
    {
        AddObjectToCurrentSong(section, editor);
        /*
        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd);
        editor.CreateSectionObject(sectionToAdd);
        editor.currentSelectedObject = sectionToAdd;*/
    }

    public static void AddObjectToCurrentSong(Section section, ChartEditor editor, bool update = true)
    {
        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd, update);
        editor.CreateSectionObject(sectionToAdd);
        editor.currentSelectedObject = sectionToAdd;
    }
}
