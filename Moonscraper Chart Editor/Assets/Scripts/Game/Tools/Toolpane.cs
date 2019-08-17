// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Toolpane : MonoBehaviour {
    public static Tools currentTool = Tools.Cursor;
    [SerializeField]
    ToolObject initiToolObject;
    ToolObject currentToolObject;

    Globals globals;
    public static bool mouseDownInArea { get; private set; }

    void Start()
    {
        mouseDownInArea = false;
        if (currentToolObject)
            SetTool(currentToolObject);

        globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<Globals>();

        currentToolObject = initiToolObject;
    }

    bool deleteModeCancel = false;
    public static bool menuCancel = false;

    void Update()
    {
        if (((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))) && !globals.services.InToolArea)
            mouseDownInArea = false;
        else if (!mouseDownInArea && (!Input.GetMouseButton(0) && !Input.GetMouseButton(1)))
            mouseDownInArea = true;

        // Handle delete mode cancel
        if (Input.GetMouseButton(1))
        {
            deleteModeCancel = true;
        }
        else if (deleteModeCancel && !Input.GetMouseButton(0))
            deleteModeCancel = false;

        // Handle exiting out of a menu cancel
        if (Input.GetMouseButtonUp(0))
            menuCancel = false;
        else if (ChartEditor.Instance.currentState != ChartEditor.State.Editor && Input.GetMouseButtonDown(0))
        {
            menuCancel = true;
        }

        // Update current tool object's visibility/activeness
        if (currentToolObject)
        {
            bool toolIsActive = currentToolObject.gameObject.activeSelf;

            if (GameSettings.keysModeEnabled)
                toolIsActive = true;
            else
            {
                bool blockedByUI = menuCancel || Mouse.IsUIUnderPointer();
                if (deleteModeCancel || blockedByUI)
                {
                    toolIsActive = false;
                }
                else if (ChartEditor.Instance.currentState == ChartEditor.State.Editor && mouseDownInArea)
                {
                    // Range check
                    if (!globals.services.InToolArea)
                    {
                        toolIsActive = false;
                    }
                    else
                    {
                        toolIsActive = true;

                        if (!Input.GetMouseButton(0) && Input.GetMouseButton(1))    // Hide the tool object when right-click hold left-click deleting
                        {
                            toolIsActive = false;
                        }
                        else if (!Input.GetMouseButton(1))
                            toolIsActive = true;

                        if (currentTool == Tools.BPM && ShortcutInput.modifierInput)    // Hide when ctrl-dragging bpm
                            toolIsActive = false;
                    }
                }
                else
                {
                    toolIsActive = false;
                }
            }

            currentToolObject.gameObject.SetActive(toolIsActive);
        }
    }

    public void SetTool(ToolObject toolObject)
    {
        if (currentToolObject)
        {
            currentToolObject.ToolDisable();
            currentToolObject.gameObject.SetActive(false);
        }

        currentToolObject = toolObject;
            
        if (currentToolObject)
        {            
            currentTool = currentToolObject.GetTool();
            currentToolObject.gameObject.SetActive(true);

            currentToolObject.ToolEnable();
        }
        else
            currentTool = Tools.Cursor;
    }

    public enum Tools
    {
        Cursor, Eraser, Note, Starpower, ChartEvent, BPM, Timesignature, Section, SongEvent
    }
}
