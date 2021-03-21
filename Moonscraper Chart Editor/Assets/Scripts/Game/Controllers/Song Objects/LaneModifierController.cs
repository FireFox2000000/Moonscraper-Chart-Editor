// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class LaneModifierController : SongObjectController
{
    public LaneModifier laneModifier { get { return (LaneModifier)songObject; } set { Init(value, this); } }

    public override void UpdateSongObject()
    {
        if (laneModifier.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS, desiredWorldYPosition, 0);

            if (isDirty)
            {
                // TODO, update visuals for all the notes that will be affected by this lane
                // Should also draw a sustain looking thing to display length
                //visualsManager.UpdateVisuals();
            }
        }
    }

    protected override void UpdateCheck()
    {
        if (laneModifier != null)
        {
            uint endPosition = laneModifier.tick + laneModifier.length;

            if ((laneModifier.tick >= editor.minPos && laneModifier.tick < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (laneModifier.tick < editor.minPos && endPosition >= editor.maxPos))
            {
                ChartEditor.State currentState = editor.currentState;
                if (currentState == ChartEditor.State.Editor)
                {
                    UpdateSongObject();
                }
            }
            else
                gameObject.SetActive(false);
        }
        else
            gameObject.SetActive(false);
    }

    // TODO, interactive controls for dragging and adjustments
}
