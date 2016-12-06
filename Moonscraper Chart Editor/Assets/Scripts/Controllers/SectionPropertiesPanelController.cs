using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SectionPropertiesPanelController : MonoBehaviour {

    public Section currentSection;
    public Text positionText;
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
        if (currentSection != null)
            sectionName.text = currentSection.title;
    }

    void OnDisable()
    {
        currentSection = null;
    }

    public void UpdateSectionName (string name)
    {
        if (currentSection != null)
            currentSection.title = name;

        ChartEditor.editOccurred = true;
    }
}
