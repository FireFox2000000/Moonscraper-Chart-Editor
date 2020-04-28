// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChartEventController : SongObjectController
{
    public ChartEvent chartEvent { get { return (ChartEvent)songObject; } set { Init(value, this); } }
    public const float position = 3.0f;
    public TextMeshPro chartEventText;
    public const int OFFSET_SPACING = 1;

    public override void UpdateSongObject()
    {
        if (chartEvent.chart != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position + GetOffset(editor, chartEvent), chartEvent.worldYPosition, 0);

            chartEventText.text = chartEvent.eventName;
        }
    }

    public static float GetOffset (ChartEditor editor, ChartEvent chartEvent)
    {
        var events = editor.currentChart.events;

        int offset = 0;
        int index, length;
        SongObjectHelper.GetRange(events, chartEvent.tick, chartEvent.tick, out index, out length);

        // Determine the offset for the object
        for (int i = index; i < index + length; ++i)
        {
            if (events[i].GetType() != chartEvent.GetType())
                continue;

            if (events[i] < chartEvent)
            {
                offset += OFFSET_SPACING;
            }
        }

        return offset;
    }
}
