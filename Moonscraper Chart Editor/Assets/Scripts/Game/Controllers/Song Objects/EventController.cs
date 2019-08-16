// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventController : SongObjectController
{
    public Event songEvent { get { return (Event)songObject; } set { Init(value, this); } }
    public const float position = -3.0f;
    public Text songEventText;
    public const int OFFSET_SPACING = -1;

    public override void UpdateSongObject()
    {
        if (songEvent.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position + GetOffset(editor, this.songEvent), songEvent.worldYPosition, 0);

            songEventText.text = songEvent.title;
        }
    }

    public static float GetOffset(ChartEditor editor, Event songEvent)
    {
        var events = editor.currentSong.events;

        int offset = 0;
        int index, length;
        SongObjectHelper.GetRange(events, songEvent.tick, songEvent.tick, out index, out length);

        // Determine the offset for the object
        for (int i = index; i < index + length; ++i)
        {
            if (events[i].GetType() != songEvent.GetType())
                continue;

            if (events[i] < songEvent)
            {
                offset += OFFSET_SPACING;
            }
        }

        return offset;
    }
}
