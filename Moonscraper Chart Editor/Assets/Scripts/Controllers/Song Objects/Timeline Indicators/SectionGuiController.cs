using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SectionGuiController : TimelineIndicator
{
    public Section section { get { return (Section)songObject; } set { songObject = value; } }
    MovementController movement;
    UnityEngine.UI.Text timelineText;

    protected override void Awake()
    {
        base.Awake();

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();      
        timelineText = GetComponentInChildren<UnityEngine.UI.Text>();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (section != null)
            timelineText.text = section.title;
    }

    public void JumpToPos()
    {
        editor.Stop();
        movement.SetPosition(section.position); 
    }
}
