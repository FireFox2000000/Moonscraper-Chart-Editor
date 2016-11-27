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
        Debug.Log("Init");
        this.section = section;
        GetComponent<SectionController>().section = section;
        editor.currentSelectedObject = section;
    }

    protected override void AddObject()
    {
        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd);
        editor.CreateSectionObject(sectionToAdd);
        editor.currentSelectedObject = sectionToAdd;
    }
}
