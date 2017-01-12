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
        if (Globals.IsInDropDown)
        {
            Dropdown dropdownUnderMouse = Mouse.GetUIUnderPointer<Dropdown>();
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
        if (Input.GetMouseButtonUp(0) && EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>() == dropdown && !Mouse.GetUIUnderPointer<Dropdown>())
        {
            EventSystem.current.SetSelectedGameObject(null);   
        }
    }
}
