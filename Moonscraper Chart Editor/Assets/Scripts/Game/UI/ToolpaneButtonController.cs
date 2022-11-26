// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ToolpaneButtonController : MonoBehaviour {
    public EditorObjectToolManager.ToolID toolId;
    private Button button;

    [SerializeField]
    bool m_drumsOnlyTool = false;

    void Start()
    {
        ChartEditor.Instance.events.toolChangedEvent.Register(RefreshInteractability);
        ChartEditor.Instance.events.chartReloadedEvent.Register(RefreshInteractability);

        button = GetComponent<Button>();
        if (ChartEditor.Instance.toolManager.currentToolId == toolId)
        {
            Press();

            // We're not registered to the event in time when this is first fired. Make sure our icon is correct on startup.
            RefreshInteractability();
        }
    }

	void RefreshInteractability() 
    {
        bool isMyTool = ChartEditor.Instance.toolManager.currentToolId == toolId;
        if (m_drumsOnlyTool)
        {
            button.interactable = Globals.drumMode && !isMyTool;
            if (!Globals.drumMode && isMyTool)
            {
                ChartEditor.Instance.toolManager.ChangeTool(EditorObjectToolManager.ToolID.Cursor);
            }
            else
            {
                enabled = isMyTool;
            }
        }
        else
        {
            button.interactable = !isMyTool;
            enabled = isMyTool;
        }
	}

    // if the tool is this one, enable the script, else disable
    public void Press()
    {
        ChartEditor.Instance.toolManager.ChangeTool(toolId);
    }
}
