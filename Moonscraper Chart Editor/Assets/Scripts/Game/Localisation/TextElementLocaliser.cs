// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextElementLocaliser : MonoBehaviour
{
    struct StringElement
    {
        public string stringId;
        public object element;
    }

    List<StringElement> stringsToLocalise = new List<StringElement>();

    // Use this for initialization
    void Start()
    {
        Dropdown dropdown = GetComponent<Dropdown>();
        if (dropdown)
        {
            foreach (Dropdown.OptionData data in dropdown.options)
            {
                stringsToLocalise.Add(new StringElement() { stringId = data.text, element = data });
            }
        }

        CustomUnityDropdown instantDropdown = GetComponent<CustomUnityDropdown>();
        if (instantDropdown)
        {
            foreach (CustomUnityDropdown.OptionData data in instantDropdown.options)
            {
                stringsToLocalise.Add(new StringElement() { stringId = data.text, element = data });
            }
        }

        var text = GetComponent<Text>();
        if (text)
        {
            stringsToLocalise.Add(new StringElement() { stringId = text.text, element = text });
        }

        OnLocalise();
    }

    void OnLocalise()
    {
        foreach (var stringElement in stringsToLocalise)
        {
            string localisedString = Localiser.Instance.Localise(stringElement.stringId);

            var property = stringElement.element.GetType().GetProperty("text");
            if (property != null)
            {
                try
                {
                    property.SetValue(stringElement.element, localisedString);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Unable to localise string. Error: " + e.Message);
                }
            }
            else
            {
                Debug.Assert(false, "No \"text\" member found on stringElement");
            }
        }
    }
}
