// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ShortcutFormatting : MonoBehaviour {
    [SerializeField]
    TextAsset shortcutText = null;
    [SerializeField]
    Text headingTextField = null;
    [SerializeField]
    Text keysTextField = null;
    [SerializeField]
    Text valuesTextField = null;

    const char HEADER_ID = '\t';
    const char KEYVALUE_SPLIT_ID = '|';

    // Use this for initialization
    void Start () {
        SetStrings();
    }
	
	// Update is called once per frame
	void Update () {
        if (!Application.isPlaying)
            SetStrings();
    }

    void SetStrings()
    {
        StringBuilder headerSb = new StringBuilder();
        StringBuilder keysSb = new StringBuilder();
        StringBuilder valuesSb = new StringBuilder();

        string[] lines = shortcutText.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        foreach (string line in lines)
        {
            if (line.Length > 1)
            {
                if (line[0] != HEADER_ID)
                    headerSb.Append(line.Trim());
                else
                {
                    string[] keyValueSplit = line.Split(KEYVALUE_SPLIT_ID);
                    if (keyValueSplit.Length != 2)
                        continue;

                    string key = keyValueSplit[0].Trim();
                    string value = keyValueSplit[1].Trim();

                    keysSb.AppendFormat("{0} - ", key);
                    valuesSb.Append(value);
                }
            }

            const char LINE_BREAK = '\n';
            headerSb.Append(LINE_BREAK);
            keysSb.Append(LINE_BREAK);
            valuesSb.Append(LINE_BREAK);
        }

        headingTextField.text = headerSb.ToString();
        keysTextField.text = keysSb.ToString();
        valuesTextField.text = valuesSb.ToString();
    }
}
