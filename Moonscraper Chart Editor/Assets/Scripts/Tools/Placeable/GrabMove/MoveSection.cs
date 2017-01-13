using UnityEngine;
using System.Collections;

public class MoveSection : PlaceSection {
    protected override void Controls()
    {
        MovementControls();
    }

    public void Init(Section section)
    {
        this.section = section;
        controller.section = section;
        editor.currentSelectedObject = section;
        controller.sectionText.text = section.title;       // Fixes 1-frame text mutation
        initObject = this.section.Clone();
    }

    protected override void AddObject()
    {
        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd);
        editor.CreateSectionObject(sectionToAdd);
        editor.currentSelectedObject = sectionToAdd;

        editor.actionHistory.Insert(new ActionHistory.Action[] { new ActionHistory.Delete(initObject), new ActionHistory.Add(sectionToAdd) });
    }
}
