using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Dropdown))]
public class DropDownMenu : MonoBehaviour {
    public string title;
    public Text titleText;
    public UnityEvent[] menuEvent;

    Dropdown dropdown;   

    public void Start()
    {
        dropdown = GetComponent<Dropdown>();

        resetDropdown();
    }

    public void Update()
    {
        titleText.text = title;
    }

    public void InvokeFuction(int pos)
    {
        if (pos < menuEvent.Length && pos >= 0)
        {
            menuEvent[pos].Invoke();
            titleText.text = title;

            resetDropdown();
            StartCoroutine(deselectDropdown());
        }
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
            dropdown.Hide();

            yield return null;

            menuEvent[pos].Invoke();
            titleText.text = title;

            resetDropdown();
        }
    }

    void resetDropdown()
    {
        // Add a blank dropdown option you will then remove at the end of the options list
        dropdown.options.Add(new Dropdown.OptionData() { text = "" });
        // Select it
        dropdown.value = dropdown.GetComponent<Dropdown>().options.Count - 1;
        // Remove it
        dropdown.options.RemoveAt(dropdown.GetComponent<Dropdown>().options.Count - 1);
    }
}
