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
            tsValue.text = currentTS.numerator.ToString();

        ChartEditor.editOccurred = edit;
    }

    protected override void Update()
    {
        base.Update();
        if (currentTS != null)
        {
            positionText.text = "Position: " + currentTS.position.ToString();

            if (tsValue.text != string.Empty)
                tsValue.text = currentTS.numerator.ToString();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        currentTS = null;
    }

    public void UpdateTSValue(string value)
    {
        float prevValue = currentTS.numerator;

        if (value != string.Empty && currentTS != null)
        {
            currentTS.numerator = uint.Parse(value);
            UpdateInputFieldRecord();
        }

        if (prevValue != currentTS.numerator)
            ChartEditor.editOccurred = true;
    }

    public void EndEdit(string value)
    {
        if (value == string.Empty || currentTS.numerator < 1)
        {
            currentTS.numerator = 4;
            UpdateInputFieldRecord();
        }

        tsValue.text = currentTS.numerator.ToString();
    }

    public char validatePositiveInteger(string text, int charIndex, char addedChar)
    {
        if (addedChar >= '0' && addedChar <= '9')
            return addedChar;
        else
            return '\0';
    }
}
