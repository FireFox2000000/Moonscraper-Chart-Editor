using UnityEngine;
using System.Collections;

public class ClipboardObjectController : Snapable {

    public GroupSelect groupSelectTool;
    public Transform strikeline;
    public static Clipboard clipboard = new Clipboard();
    Renderer ren;

    uint pastePos = 0;

    protected override void Awake()
    {
        base.Awake();
        ren = GetComponent<Renderer>();
    }

    new void LateUpdate()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
            ren.enabled = true;
        else
            ren.enabled = false;

        if (Mouse.world2DPosition != null && Input.mousePosition.y < Camera.main.WorldToScreenPoint(editor.mouseYMaxLimit.position).y)
        {
            pastePos = objectSnappedChartPos;
        }
        else
        {
            pastePos = editor.currentSong.WorldPositionToSnappedChartPosition(strikeline.position.y, Globals.step);
        }

        transform.position = new Vector3(transform.position.x, editor.currentSong.ChartPositionToWorldYPosition(pastePos), transform.position.z);

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand))
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                Paste(pastePos);
                groupSelectTool.reset();
            }
        }
    }

    // Paste the clipboard data into the chart, overwriting anything else in the process
    public void Paste(uint chartLocationToPaste)
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && clipboard.data.Length > 0)
        {
            Rect collisionRect = clipboard.GetCollisionRect(chartLocationToPaste, editor.currentSong);
            uint colliderChartDistance = clipboard.areaChartPosMax - clipboard.areaChartPosMin;

            // Overwrite any objects in the clipboard space
            foreach (ChartObject chartObject in editor.currentChart.chartObjects)
            {
                if (chartObject.controller && chartObject.position >= chartLocationToPaste && chartObject.position < chartLocationToPaste + colliderChartDistance && chartObject.controller.AABBcheck(collisionRect))
                {
                    chartObject.controller.Delete(false);
                }
            }

            // Paste the new objects in
            foreach (ChartObject clipboardChartObject in clipboard.data)
            {
                ChartObject objectToAdd;

                switch (clipboardChartObject.classID)
                {
                    case ((int)SongObject.ID.Note):
                        objectToAdd = new Note((Note)clipboardChartObject);
                        break;
                    case ((int)SongObject.ID.Starpower):
                        objectToAdd = new StarPower((StarPower)clipboardChartObject);
                        break;
                    case ((int)SongObject.ID.ChartEvent):
                        objectToAdd = new ChartEvent((ChartEvent)clipboardChartObject);
                        break;
                    default:
                        continue;
                }

                objectToAdd.position = chartLocationToPaste + clipboardChartObject.position - clipboard.areaChartPosMin;
                editor.currentChart.Add(objectToAdd, false);

                switch (clipboardChartObject.classID)
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
