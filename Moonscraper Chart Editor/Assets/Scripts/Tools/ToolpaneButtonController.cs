using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ToolpaneButtonController : MonoBehaviour {
    public Toolpane.Tools disableOnTool;
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (Toolpane.currentTool == disableOnTool)
            Press();
    }

	// Update is called once per frame
	void Update () {
        if (Toolpane.currentTool != disableOnTool)
        {
            button.interactable = true;
            enabled = false;
        }
	}

    // if the tool is this one, enable the script, else disable
    public void Press()
    {
        button.interactable = false;
        enabled = true;
    }
}
