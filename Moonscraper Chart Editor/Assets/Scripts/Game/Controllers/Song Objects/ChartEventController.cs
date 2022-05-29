// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using TMPro;
using MoonscraperChartEditor.Song;

public class ChartEventController : SongObjectController
{
    public ChartEvent chartEvent { get { return (ChartEvent)songObject; } set { Init(value, this); } }
    public const float position = 3.5f;
    public TextMeshPro chartEventText;
    public const float OFFSET_SPACING = -0.7f;
    public const float BASE_OFFSET = -0.5f;

    public override void UpdateSongObject()
    {
        if (chartEvent.chart != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, desiredWorldYPosition, GetOffset(editor, chartEvent));

            chartEventText.text = chartEvent.eventName;
        }
    }

    public static float GetOffset (ChartEditor editor, ChartEvent chartEvent)
    {
        var events = editor.currentChart.events;
        int index, length;
        SongObjectHelper.GetRange(events, chartEvent.tick, chartEvent.tick, out index, out length);

        // Determine the offset for the object
        for (int i = index; i < index + length; ++i)
        {
            if (events[i].GetType() != chartEvent.GetType())
                continue;

            if (events[i] == chartEvent)
            {
                return BASE_OFFSET + (length - (i - index) - 1) * OFFSET_SPACING;
            }
        }

        return BASE_OFFSET;
    }
}
