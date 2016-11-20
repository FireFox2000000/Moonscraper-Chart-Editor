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
    }

	// Update is called once per frame
	void Update () {
        if (Toolpane.currentTool == disableOnTool)
            button.interactable = false;
        else
            button.interactable = true;
	}
}
