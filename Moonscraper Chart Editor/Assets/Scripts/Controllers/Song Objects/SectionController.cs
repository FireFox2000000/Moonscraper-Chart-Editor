using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SectionController : SongObjectController
{
    public Section section;
    public float position = 4.5f;
    public SectionGuiController sectionGui;
    public Text sectionText;

    public override void UpdateSongObject()
    {
        transform.position = new Vector3(CHART_CENTER_POS + position, section.song.ChartPositionToWorldYPosition(section.position), 0);

        sectionText.text = section.title;
    }

    public override void Delete()
    {
        if (sectionGui)
            Destroy(sectionGui.gameObject);

        section.song.Remove(section);
  
        Destroy(gameObject);
    }

    public void Init(Section _section, TimelineHandler _timelineHandler, GameObject bpmGuiParent)
    {
        base.Init(_section);
        section = _section;
        section.controller = this;

        if (sectionGui)
            sectionGui.Init(_section, _timelineHandler, bpmGuiParent);
    }
}
