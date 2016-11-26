using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Toolpane : MonoBehaviour {
    public static Tools currentTool = Tools.Cursor;
    ToolObject currentToolObject;

    Globals globals;

    void Start()
    {
        if (currentToolObject)
            SetTool(currentToolObject);

        globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<Globals>();
    }

    void Update()
    {
        if (currentToolObject)
        {
            if (Globals.applicationMode == Globals.ApplicationMode.Editor)
            {
                // Range check
                if (Mouse.world2DPosition == null ||
                    !globals.InToolArea)
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
            else
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
        Cursor, Eraser, Note, Starpower
    }
}
