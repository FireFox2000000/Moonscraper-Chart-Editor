// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectorSwitching : MonoBehaviour {

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

        editor = ChartEditor.FindCurrentEditor();
    }
	
	// Update is called once per frame
	void Update () {
        if ((Toolpane.currentTool == Toolpane.Tools.GroupSelect || Toolpane.currentTool == Toolpane.Tools.Cursor) && editor.currentSelectedObjects.Length > 1)
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

        else if (editor.currentSelectedObject != null)
        {
            GameObject previousPanel = currentPropertiesPanel;

            switch (editor.currentSelectedObjects[0].classID)
            {
                case ((int)SongObject.ID.Note):
                    noteInspector.currentNote = (Note)editor.currentSelectedObject;
                    currentPropertiesPanel = noteInspector.gameObject;
                    break;
                case ((int)SongObject.ID.Section):
                    sectionInspector.currentSection = (Section)editor.currentSelectedObject;
                    currentPropertiesPanel = sectionInspector.gameObject;
                    break;
                case ((int)SongObject.ID.BPM):
                    bpmInspector.currentBPM = (BPM)editor.currentSelectedObject;
                    currentPropertiesPanel = bpmInspector.gameObject;
                    break;
                case ((int)SongObject.ID.TimeSignature):
                    tsInspector.currentTS = (TimeSignature)editor.currentSelectedObject;
                    currentPropertiesPanel = tsInspector.gameObject;
                    break;
                case ((int)SongObject.ID.Event):
                    eventInspector.currentEvent = (Event)editor.currentSelectedObject;
                    currentPropertiesPanel = eventInspector.gameObject;
                    break;
                case ((int)SongObject.ID.ChartEvent):
                    eventInspector.currentChartEvent = (ChartEvent)editor.currentSelectedObject;
                    currentPropertiesPanel = eventInspector.gameObject;
                    break;
                default:
                    currentPropertiesPanel = null;
                    //editor.currentSelectedObject = null;
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
        }
    }
}
