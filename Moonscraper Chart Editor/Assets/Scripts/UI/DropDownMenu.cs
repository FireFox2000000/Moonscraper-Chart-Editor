using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

[ExecuteInEditMode]
//[RequireComponent(typeof(Dropdown))]
public class DropDownMenu : MonoBehaviour {
    public string title;
    public Text titleText;
    public UnityEvent[] menuEvent;

    Dropdown dropdown;
    CustomUnityDropdown instantDropdown;

    public void Start()
    {
        dropdown = GetComponent<Dropdown>();
        instantDropdown = GetComponent<CustomUnityDropdown>();

        resetDropdown();
    }

    public void Update()
    {
        titleText.text = title;
    }

    public void InvokeFuction(int pos)
    {
        StartCoroutine(selectFunction(pos));
        /*
        if (pos < menuEvent.Length && pos >= 0)
        {
            if (menuEvent[pos] != null)
            {
                menuEvent[pos].Invoke();
                
            }
            titleText.text = title;

            resetDropdown();
            StartCoroutine(deselectDropdown());
        }*/
    }

    IEnumerator deselectDropdown()
    {
        yield return null;
        Globals.DeselectCurrentUI();
    }

    IEnumerator selectFunction(int pos)
    {
        if (pos < menuEvent.Length && pos >= 0)
        {
            if (menuEvent[pos] != null)
            {
                yield return null;
                menuEvent[pos].Invoke();
            }
            titleText.text = title;

            resetDropdown();
            yield return StartCoroutine(deselectDropdown());
        }

        /*
        if (pos < menuEvent.Length && pos >= 0)
        {
            if (dropdown)
                dropdown.Hide();

            if (instantDropdown)
                instantDropdown.Hide();
            
            yield return null;
            
            menuEvent[pos].Invoke();
            titleText.text = title;

            resetDropdown();
        }
        */
    }

    void resetDropdown()
    {
        if (dropdown)
        {
            // Add a blank dropdown option you will then remove at the end of the options list
            dropdown.options.Add(new Dropdown.OptionData() { text = "" });
            // Select it
            dropdown.value = dropdown.GetComponent<Dropdown>().options.Count - 1;
            // Remove it
            dropdown.options.RemoveAt(dropdown.GetComponent<Dropdown>().options.Count - 1);
        }

        if (instantDropdown)
        {
            // Add a blank dropdown option you will then remove at the end of the options list
            instantDropdown.options.Add(new CustomUnityDropdown.OptionData() { text = "" });
            // Select it
            instantDropdown.value = instantDropdown.GetComponent<CustomUnityDropdown>().options.Count - 1;
            // Remove it
            instantDropdown.options.RemoveAt(instantDropdown.GetComponent<CustomUnityDropdown>().options.Count - 1);
        }
    }
}
