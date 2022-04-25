// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class PlaceChartEvent : PlaceSongObject
{
    public ChartEvent chartEvent { get { return (ChartEvent)songObject; } set { songObject = value; } }
    new public ChartEventController controller { get { return (ChartEventController)base.controller; } set { base.controller = value; } }

    protected override void SetSongObjectAndController()
    {
        chartEvent = new ChartEvent(0, "Default");

        controller = GetComponent<ChartEventController>();
        controller.chartEvent = chartEvent;
    }

    protected override void Update()
    {
        base.Update();
        chartEvent.chart = editor.currentChart;
    }

    protected new void LateUpdate()
    {
        // Re-do the controller's position setting
        base.LateUpdate();

        var events = editor.currentChart.events;

        float offset = ChartEventController.BASE_OFFSET;
        int index, length;
        SongObjectHelper.GetRange(events, chartEvent.tick, chartEvent.tick, out index, out length);

        // Determine the offset for the object
        for (int i = index; i < index + length; ++i)
        {
            if (events[i].GetType() != chartEvent.GetType())
                continue;

            offset += ChartEventController.OFFSET_SPACING;
        }

        transform.position = new Vector3(SongObjectController.CHART_CENTER_POS + ChartEventController.position, ChartEditor.WorldYPosition(chartEvent), offset);
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(new ChartEvent(this.chartEvent)));
        editor.selectedObjectsManager.SelectSongObject(chartEvent, editor.currentChart.chartObjects);
    }

    protected override void Controls()
    {
        if (!Globals.gameSettings.keysModeEnabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                int pos = SongObjectHelper.FindObjectPosition(chartEvent, editor.currentChart.events);
                if (pos == SongObjectHelper.NOTFOUND)
                {
                    AddObject();
                }
                // Link to the event already in
                else
                    editor.selectedObjectsManager.currentSelectedObject = editor.currentChart.events[pos];
            }
        }
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.AddSongObject))
        {
            var searchArray = editor.currentChart.events;
            int pos = SongObjectHelper.FindObjectPosition(chartEvent, searchArray);
            if (pos == SongObjectHelper.NOTFOUND)
            {
                AddObject();
            }
            else
            {
                editor.commandStack.Push(new SongEditDelete(searchArray[pos]));
                editor.selectedObjectsManager.currentSelectedObject = null;
            }
        }
    }
}
