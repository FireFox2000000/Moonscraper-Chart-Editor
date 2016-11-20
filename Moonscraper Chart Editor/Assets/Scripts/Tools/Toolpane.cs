using UnityEngine;
using System.Collections;
//using System.Windows.Forms;
//using System.Threading;
using System.Runtime.InteropServices;

public class Toolpane : MonoBehaviour {
    public static Tools currentTool = Tools.Cursor;
    ToolObject currentToolObject;
    [Header("Tool Use Screen Area")]
    public RectTransform x_min;
    public RectTransform x_max;
    public RectTransform y_min;
    public RectTransform y_max;

    void Start()
    {
        if (currentToolObject)
            SetTool(currentToolObject);
    }

    void Update()
    {
        if (currentToolObject)
        {
            if (Input.GetMouseButtonDown(1))
                currentToolObject.gameObject.SetActive(false);
            else if (Input.GetMouseButtonUp(1))
                currentToolObject.gameObject.SetActive(true);

            // Range check
            if( Input.mousePosition.x < x_min.GetScreenPosition().x ||
                Input.mousePosition.x > x_max.GetScreenPosition().x ||
                Input.mousePosition.y < y_min.GetScreenPosition().y ||
                Input.mousePosition.y > y_max.GetScreenPosition().y)
            {
                currentToolObject.gameObject.SetActive(false);
            }
            else
            {
                currentToolObject.gameObject.SetActive(true);
            }
            
        }
    }

    public void SetTool(ToolObject toolObject)
    {
        if (currentToolObject)
            currentToolObject.gameObject.SetActive(false);

        currentToolObject = toolObject;

        if (currentToolObject)
        {
            currentTool = currentToolObject.GetTool();
            currentToolObject.gameObject.SetActive(true);
        }
        else
            currentTool = Tools.Cursor;
    }

    public enum Tools
    {
        Cursor, Eraser, Note
    }
}
