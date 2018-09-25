using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorManager : MonoBehaviour {
    [SerializeField]
    ErrorMessage errorMenu;
    string queuedErrorMessage = string.Empty;
	
	// Update is called once per frame
	void Update () {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && HasErrorToDisplay())
            DisplayErrorMenu();
    }

    public void QueueErrorMessage(string errorMessage)
    {
        queuedErrorMessage = errorMessage;
    }

    public bool HasErrorToDisplay()
    {
        return !string.IsNullOrEmpty(queuedErrorMessage);
    }

    void DisplayErrorMenu()
    {
        errorMenu.errorMessage = queuedErrorMessage;
        errorMenu.gameObject.SetActive(true);
        queuedErrorMessage = string.Empty;
    }
}
