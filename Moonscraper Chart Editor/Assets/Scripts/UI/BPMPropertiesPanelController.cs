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
        bool edit = ChartEditor.editOccurred;

        if (currentBPM != null)
            bpmValue.text = ((float)currentBPM.value / 1000.0f).ToString();

        ChartEditor.editOccurred = edit;
    }

    void Update()
    {
        if (currentBPM != null)
        {
            positionText.text = "Position: " + currentBPM.position.ToString();
            if (bpmValue.text != string.Empty && bpmValue.text[bpmValue.text.Length - 1] != '.')
                bpmValue.text = (currentBPM.value / 1000.0f).ToString();
        }

        editor.currentSong.updateArrays();
    }

    void OnDisable()
    {
        currentBPM = null;
        editor.currentSong.updateArrays();
    }

    public void UpdateBPMValue(string value)
    {
        if (value != string.Empty && value[value.Length - 1] != '.' && currentBPM != null && float.Parse(value) != 0)
        {
            float floatVal = float.Parse(value) * 1000;     // Store it in another variable due to weird parsing-casting bug at decimal points of 2 or so. Seems to fix it for whatever reason.

            currentBPM.value = (uint)floatVal;
            
        }
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
        {
            if (addedChar != '.')
            {
                if (bpmValue.selectionAnchorPosition == text.Length && bpmValue.selectionFocusPosition == 0)
                    return addedChar;

                if (!text.Contains(".") && text.Length < 3)         // Adding a number, no decimal point
                    return addedChar;
                else if (text.Contains(".") && text.IndexOf('.') <= 3)
                    return addedChar;
            }
            else
                return addedChar;
        }

        return '\0';
    }
}
