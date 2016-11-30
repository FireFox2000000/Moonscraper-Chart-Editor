using UnityEngine;
using System.Collections;
using System;

public class PlaceSection : ToolObject {
    protected Section section;
    SectionController controller;

    protected override void Awake()
    {
        base.Awake();
        section = new Section(editor.currentSong, "Default", 0);

        controller = GetComponent<SectionController>();
        controller.section = section;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        section.song = editor.currentSong;
        section.position = objectSnappedChartPos;
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Section && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            AddObject();
        }
    }

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;
    }

    void OnEnable()
    {
        //editor.currentSelectedObject = section;
        Update();
    }

    protected override void AddObject()
    {
        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd);
        editor.CreateSectionObject(sectionToAdd);
        editor.currentSelectedObject = sectionToAdd;
    }

}
