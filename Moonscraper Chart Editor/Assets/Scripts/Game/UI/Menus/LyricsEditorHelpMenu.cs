using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LyricsEditorHelpMenu : MonoBehaviour
{
    [SerializeField]
    TextAsset tutorialText = null;
    Text textField;

    // Start is called before the first frame update
    void Start()
    {
        textField = GetComponent<Text>();

        // Populate text field
        textField.text = tutorialText.text;
    }
}
