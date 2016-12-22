using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayMenu : MonoBehaviour {
    protected ChartEditor editor;
    public RectTransform mouseArea;
    public Texture2D mouseNotWorkingTex;

    protected virtual void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    protected virtual void Update()
    {
        if(!RectTransformUtility.RectangleContainsScreenPoint(mouseArea, Input.mousePosition))
        {
            Cursor.SetCursor(mouseNotWorkingTex, Vector2.zero, CursorMode.ForceSoftware);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    protected virtual void OnEnable()
    {
        editor.Stop();
        Globals.applicationMode = Globals.ApplicationMode.Menu;
    }

    protected virtual void OnDisable()
    {
        Globals.applicationMode = Globals.ApplicationMode.Editor;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

}
