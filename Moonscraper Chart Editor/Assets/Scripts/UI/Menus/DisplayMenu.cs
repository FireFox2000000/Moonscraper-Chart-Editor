using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class DisplayMenu : MonoBehaviour {
    protected ChartEditor editor;
    public RectTransform mouseArea;
    public Selectable defaultSelectable;

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
        UITabbing.defaultSelectable = defaultSelectable;

        StartCoroutine(SetUIDelayed());
    }

    IEnumerator SetUIDelayed()
    {
        yield return null;
        if (defaultSelectable)
        {
            InputField inputfield = defaultSelectable.GetComponent<InputField>();
            if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(EventSystem.current));  //if it's an input field, also set the text caret

            EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);
        }
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
        UITabbing.defaultSelectable = null;
    }

}
