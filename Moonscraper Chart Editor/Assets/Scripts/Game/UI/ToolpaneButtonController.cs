// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ToolpaneButtonController : MonoBehaviour {
    public EditorObjectToolManager.ToolID toolId;
    private Button button;
    bool aliveCheck = false;

    void Awake()
    {
        aliveCheck = true;
    }

    void Start()
    {
        ChartEditor.Instance.events.toolChangedEvent.Register(RefreshInteractability);
        ChartEditor.Instance.events.applicationShutdown.Register(OnShutdown);

        button = GetComponent<Button>();
        if (ChartEditor.Instance.toolManager.currentToolId == toolId)
        {
            Press();

            // We're not registered to the event in time when this is first fired. Make sure our icon is correct on startup.
            RefreshInteractability();
        }
    }

    void OnShutdown()
    {
        aliveCheck = false;
    }

    void OnDisable()
    {
        if (aliveCheck)
        {
            bool isMyTool = ChartEditor.Instance.toolManager.currentToolId == toolId;
            if (isMyTool)
            {
                ChartEditor.Instance.toolManager.ChangeTool(EditorObjectToolManager.ToolID.Cursor);
            }
        }
    }

    void RefreshInteractability() 
    {
        bool isMyTool = ChartEditor.Instance.toolManager.currentToolId == toolId;
        button.interactable = !isMyTool;
        enabled = isMyTool;
    }

    // if the tool is this one, enable the script, else disable
    public void Press()
    {
        ChartEditor.Instance.toolManager.ChangeTool(toolId);
    }
}
