using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BPMPropertiesPanelController : PropertiesPanelController {
    public BPM currentBPM;
    public InputField bpmValue;

    void OnEnable()
    {
        if (currentBPM != null)
            bpmValue.text = ((float)currentBPM.value / 1000.0f).ToString();
    }

    void Update()
    {
        if (currentBPM != null)
        {
            positionText.text = "Position: " + currentBPM.position.ToString();
            bpmValue.text = ((float)currentBPM.value / 1000.0f).ToString();
        }
    }

    void OnDisable()
    {
        currentBPM = null;
    }

    public void UpdateBPMValue(string value)
    {
        if (currentBPM != null)
            currentBPM.value = (uint)(float.Parse(value) * 1000);

        ChartEditor.editOccurred = true;
    }
}
