using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimesignaturePropertiesPanelController : PropertiesPanelController {
    public BPM currentTS;
    public InputField tsValue;

    void OnEnable()
    {
        if (currentTS != null)
            tsValue.text = currentTS.value.ToString();
    }

    void Update()
    {
        if (currentTS != null)
        {
            positionText.text = "Position: " + currentTS.position.ToString();
            tsValue.text = currentTS.value.ToString();
        }
    }

    void OnDisable()
    {
        currentTS = null;
    }

    public void UpdateTSValue(string value)
    {
        if (currentTS != null)
            currentTS.value = uint.Parse(value);

        ChartEditor.editOccurred = true;
    }
}
