// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class InspectorSwitching : MonoBehaviour {
    [SerializeField]
    GameObject canvas;
    [SerializeField]
    NotePropertiesPanelController noteInspector;
    [SerializeField]
    StarpowerPropertiesPanelController spInspector;
    [SerializeField]
    SectionPropertiesPanelController sectionInspector;
    [SerializeField]
    BPMPropertiesPanelController bpmInspector;
    [SerializeField]
    TimesignaturePropertiesPanelController tsInspector;
    [SerializeField]
    EventPropertiesPanelController eventInspector;
    [SerializeField]
    GameObject groupSelectInspector;

    ChartEditor editor;

    GameObject currentPropertiesPanel = null;

    // Use this for initialization
    void Start () {
        noteInspector.gameObject.SetActive(false);
        spInspector.gameObject.SetActive(false);
        sectionInspector.gameObject.SetActive(false);
        bpmInspector.gameObject.SetActive(false);
        tsInspector.gameObject.SetActive(false);
        eventInspector.gameObject.SetActive(false);

        editor = ChartEditor.Instance;

        editor.events.groupMoveStart.Register(OnGroupMoveStart);
    }

    void OnGroupMoveStart()
    {
        // Disable to the panel immediately. Inconsistent update order may make current panel get updated for 1 frame after group move has started.
        if (currentPropertiesPanel)
            currentPropertiesPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        if ((editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Cursor) && editor.selectedObjectsManager.currentSelectedObjects.Count > 1)
        {
            if (!currentPropertiesPanel || currentPropertiesPanel != groupSelectInspector)
            {
                if (currentPropertiesPanel)
                    currentPropertiesPanel.SetActive(false);
                currentPropertiesPanel = groupSelectInspector;
            }
            if (currentPropertiesPanel && !currentPropertiesPanel.gameObject.activeSelf)
                currentPropertiesPanel.gameObject.SetActive(true);

            if (currentPropertiesPanel == groupSelectInspector)
            {
                groupSelectInspector.SetActive(Globals.viewMode == Globals.ViewMode.Chart);
            }
        }

        else if (editor.selectedObjectsManager.currentSelectedObject != null)
        {
            GameObject previousPanel = currentPropertiesPanel;

            switch (editor.selectedObjectsManager.currentSelectedObjects[0].classID)
            {
                case ((int)SongObject.ID.Note):
                    noteInspector.currentNote = (Note)editor.selectedObjectsManager.currentSelectedObject;
                    currentPropertiesPanel = noteInspector.gameObject;
                    break;
                case ((int)SongObject.ID.Starpower):
                    spInspector.currentSp = (Starpower)editor.selectedObjectsManager.currentSelectedObject;
                    currentPropertiesPanel = spInspector.gameObject;
                    break;
                case ((int)SongObject.ID.Section):
                    sectionInspector.currentSection = (Section)editor.selectedObjectsManager.currentSelectedObject;
                    currentPropertiesPanel = sectionInspector.gameObject;
                    break;
                case ((int)SongObject.ID.BPM):
                    bpmInspector.currentBPM = (BPM)editor.selectedObjectsManager.currentSelectedObject;
                    currentPropertiesPanel = bpmInspector.gameObject;
                    break;
                case ((int)SongObject.ID.TimeSignature):
                    tsInspector.currentTS = (TimeSignature)editor.selectedObjectsManager.currentSelectedObject;
                    currentPropertiesPanel = tsInspector.gameObject;
                    break;
                case ((int)SongObject.ID.Event):
                    eventInspector.currentEvent = (MoonscraperChartEditor.Song.Event)editor.selectedObjectsManager.currentSelectedObject;
                    currentPropertiesPanel = eventInspector.gameObject;
                    break;
                case ((int)SongObject.ID.ChartEvent):
                    eventInspector.currentChartEvent = (ChartEvent)editor.selectedObjectsManager.currentSelectedObject;
                    currentPropertiesPanel = eventInspector.gameObject;
                    break;
                default:
                    currentPropertiesPanel = null;
                    break;
            }

            if (currentPropertiesPanel != previousPanel)
            {
                if (previousPanel)
                {
                    previousPanel.SetActive(false);
                }
            }

            if (currentPropertiesPanel != null && !currentPropertiesPanel.gameObject.activeSelf)
            {
                currentPropertiesPanel.gameObject.SetActive(true);
            }
        }
        else if (currentPropertiesPanel)
        {
            currentPropertiesPanel.gameObject.SetActive(false);
            currentPropertiesPanel = null;
        }

        if (!currentPropertiesPanel)
            canvas.SetActive(false);
        else
        {
            bool applicationModeNotPlaying = editor.currentState != ChartEditor.State.Playing;
            if (canvas.activeSelf != applicationModeNotPlaying)
                canvas.SetActive(applicationModeNotPlaying);
        }    
    }
}
