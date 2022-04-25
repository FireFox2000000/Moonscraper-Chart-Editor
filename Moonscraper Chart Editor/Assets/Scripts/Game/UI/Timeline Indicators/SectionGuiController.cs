// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using MoonscraperChartEditor.Song;

public class SectionGuiController : TimelineIndicator
{
    public Section section
    {
        get
        {
            ChartEditor editor = ChartEditor.Instance;
            Song song = editor.currentSong;
            if (index < song.sections.Count)
                return song.sections[index];
            else
                return null;
        }
    }
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
        if (section != null)
        {
            if (section.song != null && prevName != section.title)
            {
                ExplicitUpdate();
            }

            prevName = section.title;
        }
    }

    public override void ExplicitUpdate()
    {
        if (section != null)
        {
            transform.localPosition = GetLocalPos(section.tick, section.song);
            timelineText.text = section.title;
        }
    }

    public void JumpToPos()
    {
        editor.Stop();
        movement.SetPosition(section.tick); 
    }
}
