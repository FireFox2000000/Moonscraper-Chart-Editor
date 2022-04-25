// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapBPMCalculatorPanelController : MonoBehaviour {
    public Text averageBPMText;
    public Text roundedBPMText;
    public Text numOfTapsText;
    public InputField tapInputField;

    TapBPMCalculator bpmCalculator = new TapBPMCalculator();
	
	// Update is called once per frame
	void Update () {
        averageBPMText.text = "Average BPM: " + bpmCalculator.bpm.Round(2);
        roundedBPMText.text = "Nearest Whole: " + Mathf.Round(bpmCalculator.bpm);
        numOfTapsText.text = "Taps: " + bpmCalculator.taps;

        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == tapInputField.gameObject && Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
        {
            bpmCalculator.Tap();
        }
    }

    public void Reset()
    {
        bpmCalculator.Reset();
    }

    public void Disable()
    {
        ChartEditor.Instance.interactionMethodManager.ChangeInteraction(EditorInteractionManager.InteractionType.HighwayObjectEdit);
    }

    public void Enable()
    {
        ChartEditor.Instance.interactionMethodManager.ChangeInteraction(EditorInteractionManager.InteractionType.BpmCalculator);
    }
}
