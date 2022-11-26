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

    protected override void Update()
    {
        UpdateStringsInfo();
    }

    void UpdateStringsInfo()
    {
        positionText.text = "Position: " + currentDrumRoll.tick.ToString();
        lengthText.text = "Length: " + currentDrumRoll.length.ToString();
    }

    public void ApplyRollTypeDropdownSelection(int option)
    {
        DrumRoll.Type type = (DrumRoll.Type)option;
        if (type != currentDrumRoll.type)
        {
            var clone = currentDrumRoll.CloneAs<DrumRoll>();
            clone.type = type;
            editor.commandStack.Push(new SongEditModify<DrumRoll>(currentDrumRoll, clone));
        }
    }
}
