using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimesignaturePropertiesPanelController : PropertiesPanelController {
    public TimeSignature currentTS { get { return (TimeSignature)currentSongObject; } set { currentSongObject = value; } }
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

    protected override void Update()
    {
        base.Update();
        if (currentTS != null)
        {
            positionText.text = "Position: " + currentTS.position.ToString();

            if (tsValue.text != string.Empty)
                tsValue.text = currentTS.value.ToString();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        currentTS = null;
    }

    public void UpdateTSValue(string value)
    {
        float prevValue = currentTS.value;

        if (value != string.Empty && currentTS != null)
        {
            currentTS.value = uint.Parse(value);
            UpdateInputFieldRecord();
        }

        if (prevValue != currentTS.value)
            ChartEditor.editOccurred = true;
    }

    public void EndEdit(string value)
    {
        if (value == string.Empty || currentTS.value < 1)
        {
            currentTS.value = 4;
            UpdateInputFieldRecord();
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
