using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimesignaturePropertiesPanelController : PropertiesPanelController {
    public TimeSignature currentTS;
    public InputField tsValue;

    void Start()
    {
        tsValue.onValidateInput = validatePositiveInteger;
    }

    void OnEnable()
    {
        bool edit = ChartEditor.editOccurred;

        if (currentTS != null)
            tsValue.text = currentTS.value.ToString();

        ChartEditor.editOccurred = edit;
    }

    void Update()
    {
        if (currentTS != null)
        {
            positionText.text = "Position: " + currentTS.position.ToString();

            if (tsValue.text != string.Empty)
                tsValue.text = currentTS.value.ToString();
        }
    }

    void OnDisable()
    {
        currentTS = null;
    }

    public void UpdateTSValue(string value)
    {
        if (value != string.Empty && currentTS != null)
            currentTS.value = uint.Parse(value);

        ChartEditor.editOccurred = true;
    }

    public void EndEdit(string value)
    {
        if (value == string.Empty || currentTS.value < 1)
        {
            currentTS.value = 4;
        }

        tsValue.text = currentTS.value.ToString();
    }

    public char validatePositiveInteger(string text, int charIndex, char addedChar)
    {
        if (addedChar >= '0' && addedChar <= '9')
            return addedChar;
        else
            return '\0';
    }
}
