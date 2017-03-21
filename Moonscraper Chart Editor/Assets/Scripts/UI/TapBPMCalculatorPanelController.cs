using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapBPMCalculatorPanelController : MonoBehaviour {
    public Text averageBPMText;
    public Text roundedBPMText;
    public Text numOfTapsText;
    public InputField tapInputField;
    public Button activationButton;

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
        gameObject.SetActive(false);
        activationButton.gameObject.SetActive(true);
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        activationButton.gameObject.SetActive(false);
    }
}
