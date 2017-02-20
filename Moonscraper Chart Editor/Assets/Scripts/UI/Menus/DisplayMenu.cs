using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayMenu : MonoBehaviour {
    protected ChartEditor editor;
    public RectTransform mouseArea;

    protected virtual void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    protected virtual void Update()
    {
        MovementController.cancel = true;

        if (Input.GetButtonDown("CloseMenu") || (Input.GetMouseButtonDown(0) && !RectTransformUtility.RectangleContainsScreenPoint(mouseArea, Input.mousePosition)))
            Disable();
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
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        gameObject.SetActive(false);
    }

}
