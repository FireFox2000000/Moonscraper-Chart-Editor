// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeysNotePlacementModePanelController : MonoBehaviour {
    public Button[] buttons;

    [HideInInspector]
    public static PlacementMode currentPlacementMode = PlacementMode.Sustain;

    void OnEnable()
    {
        //if (buttons.Length > 0)
           // buttons[0].onClick.Invoke();
    }
	
	// Update is called once per frame
	void Update () {
        // Shortcuts
        if (!Services.IsTyping && !Globals.modifierInputActive)
        {
            if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolNoteHold))
                buttons[0].onClick.Invoke();
            else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolNoteBurst))
                buttons[1].onClick.Invoke();
        }
    }

    public void SelectButton(Button button)
    {
        foreach (Button b in buttons)
        {
            b.interactable = (b != button);
        }
    }

    public void SetSustainMode()
    {
        currentPlacementMode = PlacementMode.Sustain;
        ChartEditor.Instance.events.notePlacementModeChangedEvent.Fire();
    }

    public void SetBurstMode()
    {
        currentPlacementMode = PlacementMode.Burst;
        ChartEditor.Instance.events.notePlacementModeChangedEvent.Fire();
    }

    public enum PlacementMode
    {
        Sustain, Burst
    }
}
