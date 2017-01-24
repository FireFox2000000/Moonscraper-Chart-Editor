using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SectionPropertiesPanelController : PropertiesPanelController {
    public Section currentSection;
    public InputField sectionName;

    void Update()
    {
        if (currentSection != null)
        {
            positionText.text = "Position: " + currentSection.position.ToString();
            sectionName.text = currentSection.title;
        }
    }

    void OnEnable()
    {
        bool edit = ChartEditor.editOccurred;

        if (currentSection != null)
            sectionName.text = currentSection.title;

        ChartEditor.editOccurred = edit;
    }

    void OnDisable()
    {
        currentSection = null;
    }

    public void UpdateSectionName (string name)
    {
        string prevName = currentSection.title;
        if (currentSection != null)
            currentSection.title = name;

        if (prevName != currentSection.title)
            ChartEditor.editOccurred = true;
    }
}
