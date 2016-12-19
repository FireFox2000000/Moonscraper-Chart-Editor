using UnityEngine;
using System.Collections;

public class MoveSection : PlaceSection {

    protected override void Controls()
    {
        if (Input.GetMouseButtonUp(0))
        {
            AddObject();

            Destroy(gameObject);
        }
    }

    public void Init(Section section)
    {
        this.songObject = section;
        ((SectionController)controller).section = section;
        editor.currentSelectedObject = section;
        ((SectionController)controller).sectionText.text = section.title;       // Fixes 1-frame text mutation
    }

    protected override void AddObject()
    {
        Section sectionToAdd = new Section((Section)songObject);
        editor.currentSong.Add(sectionToAdd);
        editor.CreateSectionObject(sectionToAdd);
        editor.currentSelectedObject = sectionToAdd;
    }
}
