using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToolPanelController : MonoBehaviour { 
    public Toggle viewModeToggle;
    public KeysNotePlacementModePanelController keysModePanel;

	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Toggle View") && (Globals.applicationMode == Globals.ApplicationMode.Editor || Globals.applicationMode == Globals.ApplicationMode.Playing)
            && !Globals.IsTyping)
        {
            viewModeToggle.isOn = !viewModeToggle.isOn;
        }

        keysModePanel.gameObject.SetActive(Toolpane.currentTool == Toolpane.Tools.Note && Globals.lockToStrikeline);
	}
}
