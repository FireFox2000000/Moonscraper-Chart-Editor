// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class ReplaceToEscapedCharater : MonoBehaviour {
	// Use this for initialization
	void Start () {
        Dropdown dropdown = GetComponent<Dropdown>();
        if (dropdown)
            foreach (Dropdown.OptionData data in dropdown.options)
            {
                for (int i = 10; i >= 0; --i)
                {
                    string oldValue = "\\t" + i;
                    string newValue = string.Empty;

                    for (int j = 0; j < i; ++j)
                        newValue += "\t";

                    data.text = data.text.Replace(oldValue, newValue);
                }
            }

        CustomUnityDropdown instantDropdown = GetComponent<CustomUnityDropdown>();
        if (instantDropdown)
            foreach (CustomUnityDropdown.OptionData data in instantDropdown.options)
            {
                for (int i = 10; i >= 0; --i)
                {
                    string oldValue = "\\t" + i;
                    string newValue = string.Empty;

                    for (int j = 0; j < i; ++j)
                        newValue += "\t";

                    data.text = data.text.Replace(oldValue, newValue);
                }
            }
    }

}
