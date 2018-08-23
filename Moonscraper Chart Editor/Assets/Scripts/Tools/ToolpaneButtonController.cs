// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ToolpaneButtonController : MonoBehaviour {
    public Toolpane.Tools disableOnTool;
    private Button button;

    void Start()
    {
        EventsManager.onToolChangedEventList.Add(OnToolChangedEvent);

        button = GetComponent<Button>();
        if (Toolpane.currentTool == disableOnTool)
            Press();
    }

	// Update is called once per frame
	void OnToolChangedEvent () {
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

        EventsManager.FireToolChangedEvent();
    }
}
