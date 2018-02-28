using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Services : MonoBehaviour
{
    [Header("Area range")]
    public RectTransform area;
    public DropdownNotification notificationBar;
    public ToolPanelController toolpanelController;

    Rect toolScreenArea;
    public static bool IsInDropDown = false;
    static Vector2 prevScreenSize;

    public static bool HasScreenResized
    {
        get
        {
            return (prevScreenSize.x != Screen.width || prevScreenSize.y != Screen.height);
        }
    }

    public bool InToolArea
    {
        get
        {
            if (Input.mousePosition.x < toolScreenArea.xMin ||
                    Input.mousePosition.x > toolScreenArea.xMax ||
                    Input.mousePosition.y < toolScreenArea.yMin ||
                    Input.mousePosition.y > toolScreenArea.yMax)
                return false;
            else
                return true;
        }
    }

    public void OnScreenResize()
    {
        toolScreenArea = area.GetScreenCorners();
    }

    static bool _IsInDropDown
    {
        get
        {
            GameObject currentUIUnderPointer = Mouse.GetUIRaycastableUnderPointer();
            if (currentUIUnderPointer != null && (currentUIUnderPointer.GetComponentInChildren<ScrollRect>() || currentUIUnderPointer.GetComponentInParent<ScrollRect>()))
                return true;

            if ((EventSystem.current.currentSelectedGameObject == null ||
                EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>() == null) && !Mouse.GetUIUnderPointer<Dropdown>())
            {
                return false;
            }
            else
                return true;
        }
    }

    public static bool IsTyping
    {
        get
        {
            if (EventSystem.current.currentSelectedGameObject == null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
                return false;
            else
                return true;
        }
    }

    public void ResetAspectRatio()
    {
        int height = Screen.height;
        int width = (int)(16.0f / 9.0f * height);

        Screen.SetResolution(width, height, false);
    }

    public static string BoolToStrOnOff(bool val)
    {
        string toggleStr = val ? "ON" : "OFF";

        return toggleStr;
    }

    public void ToggleMouseLockMode()
    {
        GameSettings.keysModeEnabled = !GameSettings.keysModeEnabled;
        Debug.Log("Keys mode toggled " + GameSettings.keysModeEnabled);
    }

    public void ToggleExtendedSustains()
    {
        GameSettings.extendedSustainsEnabled = !GameSettings.extendedSustainsEnabled;
        Debug.Log("Extended sustains toggled " + GameSettings.extendedSustainsEnabled);
    }

    public void ToggleMetronome()
    {
        GameSettings.metronomeActive = !GameSettings.metronomeActive;
        Debug.Log("Metronome toggled " + GameSettings.metronomeActive);
    }

    ///////////////////////////////////////////////////////////////////////////////////////

    // Use this for initialization
    void Start()
    {
        toolScreenArea = area.GetScreenCorners();
        prevScreenSize.x = Screen.width;
        prevScreenSize.y = Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        IsInDropDown = _IsInDropDown;

        if (HasScreenResized)
            OnScreenResize();
    }

    void LateUpdate()
    {
        prevScreenSize.x = Screen.width;
        prevScreenSize.y = Screen.height;
    }
}
