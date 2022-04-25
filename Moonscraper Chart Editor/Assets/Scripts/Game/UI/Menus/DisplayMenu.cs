// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

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
        editor = ChartEditor.Instance;
        if (!editor)
        {
            enabled = false;
        }
    }

    protected virtual void Update()
    {
        MovementController.cancel = true;

        bool exitInput = MSChartEditorInput.GetInputDown(MSChartEditorInputActions.CloseMenu) && !editor.uiServices.popupBlockerEnabled;
        bool clickedOutSideWindow = Input.GetMouseButtonDown(0) && !RectTransformUtility.RectangleContainsScreenPoint(mouseArea, editor.uiServices.GetUIMousePosition());

        if (clickedOutSideWindow)
        {
            var mouseMonitor = editor.services.mouseMonitorSystem;
            var uiUnderMouse = mouseMonitor.GetUIRaycastableUnderPointer();
            DisplayMenu menu = uiUnderMouse ? uiUnderMouse.GetComponentInParent<DisplayMenu>() : null;
            bool clickingObjectInMenu = menu && menu == this;

            clickedOutSideWindow &= !clickingObjectInMenu;
        }

        if (exitInput || clickedOutSideWindow || editor.errorManager.HasErrorToDisplay())
        {
            Disable();
        }
    }

    protected virtual void OnEnable()
    {
        editor.Stop();
        editor.ChangeStateToMenu();
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
        if (editor)
            editor.ChangeStateToEditor();
    }

    public void Disable()
    {
        UITabbing.defaultSelectable = null;
        gameObject.SetActive(false);       
    }

}
