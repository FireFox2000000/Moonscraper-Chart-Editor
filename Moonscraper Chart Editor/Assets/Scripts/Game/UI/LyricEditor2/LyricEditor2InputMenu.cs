using UnityEngine;
using UnityEngine.UI;

public class LyricEditor2InputMenu : MonoBehaviour
{
    [SerializeField]
    InputField inputField;

    public string text {get {return inputField.text;}}

    public void Display () {
        gameObject.SetActive(true);
        inputField.text = null;
    }

    public void Display (string prefillText) {
        gameObject.SetActive(true);
        inputField.text = prefillText;
    }
}
