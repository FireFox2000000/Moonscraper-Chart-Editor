using UnityEngine;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class DrumRollPropertiesPanelController : PropertiesPanelController
{
    public DrumRoll currentDrumRoll { get { return (DrumRoll)currentSongObject; } set { currentSongObject = value; } }

    [SerializeField]
    Text lengthText;
    [SerializeField]
    Dropdown drumRollTypeDropdown;

    bool m_dropdownBlockingActive = false;
    bool m_queueClearUiSelected = false;

    protected override void Update()
    {
        UpdateStringsInfo();

        m_dropdownBlockingActive = true;
        drumRollTypeDropdown.value = (int)currentDrumRoll.type;
        m_dropdownBlockingActive = false;

        // Dropdown is stealing focus from our scrolling input, clear after selecting the dropdown
        if (m_queueClearUiSelected)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void UpdateStringsInfo()
    {
        positionText.text = "Position: " + currentDrumRoll.tick.ToString();
        lengthText.text = "Length: " + currentDrumRoll.length.ToString();
    }

    public void ApplyRollTypeDropdownSelection(int option)
    {
        if (m_dropdownBlockingActive)
            return;

        DrumRoll.Type type = (DrumRoll.Type)option;
        if (type != currentDrumRoll.type)
        {
            var clone = currentDrumRoll.CloneAs<DrumRoll>();
            clone.type = type;
            editor.commandStack.Push(new SongEditModify<DrumRoll>(currentDrumRoll, clone));
        }

        m_queueClearUiSelected = true;
    }
}
