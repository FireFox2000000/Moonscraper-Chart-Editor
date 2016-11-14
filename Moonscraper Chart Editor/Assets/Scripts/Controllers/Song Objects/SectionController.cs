using UnityEngine;
using System.Collections;

public class SectionController : SongObjectController
{
    public SectionGuiController sectionGui;
    Section section;

    public override void UpdateSongObject()
    {
        transform.position = new Vector3(7, section.song.ChartPositionToWorldYPosition(section.position), 0);
    }

    public override void Delete()
    {
        Destroy(sectionGui);

        section.song.Remove(section);
  
        Destroy(gameObject);
    }

    public void Init(MovementController movement, Section _section, TimelineHandler _timelineHandler, GameObject bpmGuiParent)
    {
        Init(movement);
        section = _section;
        sectionGui.Init(_section, _timelineHandler, bpmGuiParent);
    }
}
