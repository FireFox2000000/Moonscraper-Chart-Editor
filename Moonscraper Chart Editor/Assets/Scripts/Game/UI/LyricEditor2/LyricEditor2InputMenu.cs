using UnityEngine;
using UnityEngine.UI;

public class LyricEditor2InputMenu : MonoBehaviour
{
    [SerializeField]
    InputField inputField;
    [SerializeField]
    Text title;

    public string text {get {return inputField.text;}}

    public void Display (string prefillText) {
        gameObject.SetActive(true);
        inputField.text = prefillText;
        if (prefillText == null || prefillText.Length == 0) {
            inputField.text = null;
        }
    }

    public void SetTitle(string newTitle) {
        title.text = newTitle;
    }
}
