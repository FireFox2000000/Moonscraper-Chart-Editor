using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventController : SongObjectController
{
    public Event songEvent { get { return (Event)songObject; } set { Init(value, this); } }
    public const float position = -3.0f;
    public Text songEventText;

    public override void UpdateSongObject()
    {
        if (songEvent.song != null)
        {
            Event[] events = editor.currentSong.events;

            int offset = 0;
            int index, length;
            SongObject.GetRange(events, songEvent.position, songEvent.position, out index, out length);

            // Determine the offset for the object
            for (int i = index; i < index + length; ++i)
            {
                if (events[i].GetType() != songEvent.GetType())
                    continue;

                if (events[i] < songEvent)
                {
                    ++offset;
                }
            }

            transform.position = new Vector3(CHART_CENTER_POS + position - offset, songEvent.worldYPosition, 0);

            songEventText.text = songEvent.title;
        }
    }
}
