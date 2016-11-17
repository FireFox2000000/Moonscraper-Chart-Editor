using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SectionGuiController : MonoBehaviour
{
    Section section;
    TimelineHandler timelineHandler;
    ChartEditor editor;
    MovementController movement;

    void Awake()
    {
        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    void Update()
    {
        transform.localPosition = GetLocalPos();
    }

	public void Init(Section _section, TimelineHandler _timelineHandler, GameObject bpmGuiParent)
    {
        section = _section;
        timelineHandler = _timelineHandler;

        transform.SetParent(bpmGuiParent.transform);
        transform.localPosition = Vector3.zero;
        transform.localScale = new Vector3(1, 1, 1);
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

    public void JumpToPos()
    {
        editor.Stop();
        movement.SetPosition(section.position); 
    }
}
