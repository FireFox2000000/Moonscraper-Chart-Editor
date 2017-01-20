using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SectionController : SongObjectController
{
    public Section section { get { return (Section)songObject; } set { songObject = value; } }
    public float position = 4.5f;
    public Text sectionText;

    public override void UpdateSongObject()
    {
        if (section.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, section.worldYPosition, 0);

            sectionText.text = section.title;
        }
    }

    public override void Delete(bool update = true)
    {
        section.song.Remove(section, update);
  
        Destroy(gameObject);
    }

    public override void DestroyGameObject()
    {
        base.DestroyGameObject();
    }

    public void Init(Section _section)
    {
        base.Init(_section);
        section = _section;
        section.controller = this;
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            // Pass note data to a ghost note
            GameObject moveSection = Instantiate(editor.ghostSection);
            moveSection.SetActive(true);

            moveSection.name = "Moving section";
            Destroy(moveSection.GetComponent<PlaceSection>());
            MoveSection movement = moveSection.AddComponent<MoveSection>();
            movement.Init(section);
                
            Delete();
        }
    }
}
