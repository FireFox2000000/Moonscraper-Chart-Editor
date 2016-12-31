using UnityEngine;
using System.Collections;

public class ClipboardObjectController : Snapable {

    public GroupSelect groupSelectTool;
    public static Clipboard clipboard = new Clipboard();

    protected override void Update()
    {
        base.Update();

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand))
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                Paste(objectSnappedChartPos);
                groupSelectTool.reset();
            }
        }
    }

    // Paste the clipboard data into the chart, overwriting anything else in the process
    public void Paste(uint chartLocationToPaste)
    {
        if (clipboard.data.Length > 0)
        {
            Rect collisionRect = clipboard.GetCollisionRect(chartLocationToPaste, editor.currentSong);
            uint colliderChartDistance = clipboard.areaChartPosMax - clipboard.areaChartPosMin;

            // Overwrite any objects in the clipboard space
            foreach (ChartObject chartObject in editor.currentChart.chartObjects)
            {
                if (chartObject.controller && chartObject.position < chartLocationToPaste + colliderChartDistance && chartObject.controller.AABBcheck(collisionRect))
                {
                    chartObject.controller.Delete(false);
                }
            }

            // Paste the new objects in
            foreach (ChartObject chartObject in clipboard.data)
            {
                ChartObject objectToAdd;

                switch (chartObject.classID)
                {
                    case ((int)SongObject.ID.Note):
                        objectToAdd = new Note((Note)chartObject);
                        break;
                    case ((int)SongObject.ID.Starpower):
                        objectToAdd = new StarPower((StarPower)chartObject);
                        break;
                    case ((int)SongObject.ID.ChartEvent):
                        objectToAdd = new ChartEvent((ChartEvent)chartObject);
                        break;
                    default:
                        continue;
                }

                objectToAdd.position = chartLocationToPaste + chartObject.position - clipboard.areaChartPosMin;
                editor.currentChart.Add(objectToAdd, false);

                switch (chartObject.classID)
                {
                    case ((int)SongObject.ID.Note):
                        editor.CreateNoteObject((Note)objectToAdd);
                        break;
                    case ((int)SongObject.ID.Starpower):
                        editor.CreateStarpowerObject((StarPower)objectToAdd);
                        break;
                    case ((int)SongObject.ID.ChartEvent):
                        break;
                    default:
                        continue;
                }

                editor.currentChart.updateArrays();

            }
        }
        // else don't bother pasting
    }
}
