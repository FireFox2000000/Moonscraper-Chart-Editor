using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Toolpane : MonoBehaviour {
    public static Tools currentTool = Tools.Cursor;
    ToolObject currentToolObject;

    Globals globals;
    public static bool mouseDownInArea { get; private set; }

    void Start()
    {
        mouseDownInArea = false;
        if (currentToolObject)
            SetTool(currentToolObject);

        globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<Globals>();
    }

    bool deleteModeCancel = false;
    public static bool menuCancel = false;

    void Update()
    {
        if (((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))) && !globals.InToolArea)
            mouseDownInArea = false;
        else if (!mouseDownInArea && (!Input.GetMouseButton(0) && !Input.GetMouseButton(1)))//(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
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
        else if (Globals.applicationMode != Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            menuCancel = true;
        }


        if (currentToolObject)
        {
            if (Globals.lockToStrikeline)
                currentToolObject.gameObject.SetActive(true);
            else
            {
                if ((deleteModeCancel && currentTool != Tools.GroupSelect) || ((menuCancel || Mouse.IsUIUnderPointer()) && currentTool != Tools.GroupSelect))
                {
                    currentToolObject.gameObject.SetActive(false);
                }
                else if (Globals.applicationMode == Globals.ApplicationMode.Editor && mouseDownInArea)
                {
                    // Range check
                    if (!globals.InToolArea && currentTool != Tools.GroupSelect)
                    {
                        currentToolObject.gameObject.SetActive(false);
                    }
                    else
                    {
                        currentToolObject.gameObject.SetActive(true);

                        if (Input.GetMouseButton(1))
                        {
                            currentToolObject.gameObject.SetActive(false);
                        }
                        else if (!Input.GetMouseButton(1))
                            currentToolObject.gameObject.SetActive(true);
                    }
                }
                else if (currentTool != Tools.GroupSelect)
                {
                    currentToolObject.gameObject.SetActive(false);
                }
            }          
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
        Cursor, Eraser, Note, Starpower, ChartEvent, BPM, Timesignature, Section, SongEvent, GroupSelect
    }
}
