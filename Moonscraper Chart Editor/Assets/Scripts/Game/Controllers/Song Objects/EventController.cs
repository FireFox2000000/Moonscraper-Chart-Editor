// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using TMPro;
using MoonscraperChartEditor.Song;

public class EventController : SongObjectController
{
    public Event songEvent { get { return (Event)songObject; } set { Init(value, this); } }
    public const float position = -3.5f;
    public TextMeshPro songEventText;
    public const float OFFSET_SPACING = -0.7f;
    public const float BASE_OFFSET = -0.5f;

    public override void UpdateSongObject()
    {
        if (songEvent.song != null)
        {
            transform.position = new UnityEngine.Vector3(CHART_CENTER_POS + position, desiredWorldYPosition, GetOffset(editor, this.songEvent));

            songEventText.text = songEvent.title;
        }
    }

    public static float GetOffset(ChartEditor editor, Event songEvent)
    {
        var events = editor.currentSong.events;

        int index, length;
        SongObjectHelper.GetRange(events, songEvent.tick, songEvent.tick, out index, out length);

        // Determine the offset for the object
        for (int i = index; i < index + length; ++i)
        {
            if (events[i].GetType() != songEvent.GetType())
                continue;

            if (events[i] == songEvent)
            {
                return BASE_OFFSET + (length - (i - index) - 1) * OFFSET_SPACING;
            }
        }

        return BASE_OFFSET;
    }
}
