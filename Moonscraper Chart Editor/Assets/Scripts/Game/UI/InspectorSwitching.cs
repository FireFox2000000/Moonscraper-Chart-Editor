// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectorSwitching : MonoBehaviour {
    [SerializeField]
    GameObject canvas;
    [SerializeField]
    NotePropertiesPanelController noteInspector;
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
        sectionInspector.gameObject.SetActive(false);
        bpmInspector.gameObject.SetActive(false);
        tsInspector.gameObject.SetActive(false);
        eventInspector.gameObject.SetActive(false);

        editor = ChartEditor.Instance;
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
                    eventInspector.currentEvent = (Event)editor.selectedObjectsManager.currentSelectedObject;
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
