using UnityEngine;
using System.Collections;

public class SectionGuiController : MonoBehaviour {
    Section section;
    TimelineHandler timelineHandler;

    void Update()
    {
        transform.localPosition = GetLocalPos();
    }

	public void Init(Section _section, TimelineHandler _timelineHandler, GameObject bpmGuiParent)
    {
        section = _section;
        timelineHandler = _timelineHandler;

        transform.parent = bpmGuiParent.transform;
        transform.localPosition = Vector3.zero;
    }

    public Vector3 GetLocalPos()
    {
        float time = section.song.ChartPositionToTime(section.position);
        float endTime = section.song.length;

        if (endTime > 0)
            return timelineHandler.handlePosToLocal(time / endTime);
        else
            return Vector3.zero;
    }
}
