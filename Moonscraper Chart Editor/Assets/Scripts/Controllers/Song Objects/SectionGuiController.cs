using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SectionGuiController : MonoBehaviour
{
    Section section;
    public TimelineHandler timelineHandler;
    ChartEditor editor;
    MovementController movement;
    UnityEngine.UI.Text timelineText;

    void Awake()
    {
        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        timelineText = GetComponentInChildren<UnityEngine.UI.Text>();
    }

    void LateUpdate()
    {
        transform.localPosition = GetLocalPos();
        if (section != null)
            timelineText.text = section.title;
    }

	public void Init(Section _section)
    {
        section = _section;

        transform.localPosition = GetLocalPos();
        transform.localScale = new Vector3(1, 1, 1);
    }

    public Vector3 GetLocalPos()
    {
        if (section != null && section.song != null)
        {
            float time = section.song.ChartPositionToTime(section.position, section.song.resolution);
            float endTime = section.song.length;

            if (endTime > 0)
                return timelineHandler.handlePosToLocal(time / endTime);
            else
                return Vector3.zero;
        }

        return Vector3.zero;
    }

    public void JumpToPos()
    {
        editor.Stop();
        movement.SetPosition(section.position); 
    }
}
