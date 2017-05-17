using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToolPanelController : MonoBehaviour { 
    public Toggle viewModeToggle;
    public KeysNotePlacementModePanelController keysModePanel;

    [SerializeField]
    Button cursorSelect;
    [SerializeField]
    Button eraserSelect;
    [SerializeField]
    Button groupSelect;
    [SerializeField]
    Button noteSelect;
    [SerializeField]
    Button starpowerSelect;
    [SerializeField]
    Button bpmSelect;
    [SerializeField]
    Button timeSignatureSelect;
    [SerializeField]
    Button sectionSelect;

    // Update is called once per frame
    void Update () {
        if (Input.GetButtonDown("Toggle View") && (Globals.applicationMode == Globals.ApplicationMode.Editor || Globals.applicationMode == Globals.ApplicationMode.Playing)
            && !Globals.IsTyping)
        {
            viewModeToggle.isOn = !viewModeToggle.isOn;
        }

        keysModePanel.gameObject.SetActive(Toolpane.currentTool == Toolpane.Tools.Note && Globals.lockToStrikeline);

        if (!Globals.IsTyping && !Globals.modifierInputActive)
            Shortcuts();
    }

    void Shortcuts()
    {
        if (Input.GetKeyDown(KeyCode.J))
            cursorSelect.onClick.Invoke();

        else if (Input.GetKeyDown(KeyCode.K))
            eraserSelect.onClick.Invoke();

        else if (Input.GetKeyDown(KeyCode.L))
            groupSelect.onClick.Invoke();

        else if (Input.GetKeyDown(KeyCode.Y))
            noteSelect.onClick.Invoke();

        else if (Input.GetKeyDown(KeyCode.U))
            starpowerSelect.onClick.Invoke();

        else if (Input.GetKeyDown(KeyCode.I))
            bpmSelect.onClick.Invoke();

        else if (Input.GetKeyDown(KeyCode.O))
            timeSignatureSelect.onClick.Invoke();

        else if (Input.GetKeyDown(KeyCode.P))
            sectionSelect.onClick.Invoke();
    }
}
