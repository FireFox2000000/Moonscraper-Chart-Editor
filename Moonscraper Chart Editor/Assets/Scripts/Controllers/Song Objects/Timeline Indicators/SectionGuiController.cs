// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SectionGuiController : TimelineIndicator
{
    public Section section { get { return (Section)songObject; } set { songObject = value; } }
    MovementController movement;
    UnityEngine.UI.Text timelineText;
    string prevName = string.Empty;

    protected override void Awake()
    {
        base.Awake();

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();      
        timelineText = GetComponentInChildren<UnityEngine.UI.Text>();
    }

    void LateUpdate()
    {
        if ((songObject != null && songObject.song != null && prevName != section.title))
        {
            ExplicitUpdate();
        }

        prevName = section.title;
    }

    public override void ExplicitUpdate()
    {
        base.ExplicitUpdate();
        if (section != null)
            timelineText.text = section.title;
    }

    public void JumpToPos()
    {
        editor.Stop();
        movement.SetPosition(section.tick); 
    }
}
