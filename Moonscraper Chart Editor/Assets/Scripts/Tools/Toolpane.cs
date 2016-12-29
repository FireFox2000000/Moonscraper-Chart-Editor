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

   
    void Update()
    {
        if (((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))) && !globals.InToolArea)
            mouseDownInArea = false;
        else if (!mouseDownInArea && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
            mouseDownInArea = true;

        if (currentToolObject)
        {
            if (Globals.applicationMode == Globals.ApplicationMode.Editor && mouseDownInArea)
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
                        currentToolObject.gameObject.SetActive(false);
                    else if (!Input.GetMouseButton(1))
                        currentToolObject.gameObject.SetActive(true);
                }
            }
            else if (currentTool != Tools.GroupSelect)
                currentToolObject.gameObject.SetActive(false);
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
