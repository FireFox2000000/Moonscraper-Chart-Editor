// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;

public class LyricEditor2InputMenu : MonoBehaviour
{
    [SerializeField]
    InputField inputField = null;
    [SerializeField]
    Text title = null;

    public string text {get {return inputField.text;}}

    public void SetTitle(string newTitle) {
        title.text = newTitle;
    }

    public void Display (string prefillText) {
        gameObject.SetActive(true);
        inputField.text = prefillText;
        if (prefillText == null || prefillText.Length == 0) {
            inputField.text = null;
        }
    }
}
