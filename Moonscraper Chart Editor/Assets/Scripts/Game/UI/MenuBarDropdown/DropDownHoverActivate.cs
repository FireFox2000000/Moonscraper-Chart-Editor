// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[RequireComponent(typeof(Dropdown))]
public class DropDownHoverActivate : MonoBehaviour
{
    Dropdown dropdown;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }
    
    void Update()
    {
        ChartEditor editor = ChartEditor.Instance;
        var mouseMonitor = editor.services.mouseMonitorSystem;

        if (Services.IsInDropDown && editor.currentState != ChartEditor.State.Loading)
        {
            Dropdown dropdownUnderMouse = mouseMonitor.GetUIUnderPointer<Dropdown>();
            Dropdown currentDropdown;
            if (EventSystem.current.currentSelectedGameObject == null)
                currentDropdown = null;
            else
                currentDropdown = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>();
            if (dropdownUnderMouse == dropdown && currentDropdown != null && currentDropdown != dropdown)
            {
                // Auto-switch dropdown on hover
                currentDropdown.Hide();
                EventSystem.current.SetSelectedGameObject(dropdown.gameObject);
                dropdown.Show();
            }

        }
        
        // Properly deselect dropdown
        if (Input.GetMouseButtonUp(0) && EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>() == dropdown && !mouseMonitor.GetUIUnderPointer<Dropdown>())
        {
            EventSystem.current.SetSelectedGameObject(null);   
        }
    }
}
