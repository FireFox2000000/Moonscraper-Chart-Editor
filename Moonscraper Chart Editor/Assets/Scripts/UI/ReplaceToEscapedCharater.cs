using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class ReplaceToEscapedCharater : MonoBehaviour {
    public Dropdown dropdown;

	// Use this for initialization
	void Start () {
        
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
    }

}
