using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BPMPropertiesPanelController : PropertiesPanelController {
    public BPM currentBPM;
    public InputField bpmValue;

    void Start()
    {
        bpmValue.onValidateInput = validatePositiveDecimal;
    }

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
        }
    }

    void OnDisable()
    {
        currentBPM = null;
    }

    public void UpdateBPMValue(string value)
    {
        if (value != string.Empty && value != "." && currentBPM != null && float.Parse(value) != 0)
            currentBPM.value = (uint)(float.Parse(value) * 1000);
        else if (value == ".")
            bpmValue.text = string.Empty;

        ChartEditor.editOccurred = true;
    }

    public void EndEdit(string value)
    {
        if (value == string.Empty || currentBPM.value <= 0)
        {
            currentBPM.value = 120000; 
        }

        bpmValue.text = ((float)currentBPM.value / 1000.0f).ToString();
    }

    public char validatePositiveDecimal(string text, int charIndex, char addedChar)
    {
        if ((addedChar == '.' && !text.Contains(".") && text.Length > 0) || (addedChar >= '0' && addedChar <= '9'))
            return addedChar;
        else
            return '\0';
    }
}
