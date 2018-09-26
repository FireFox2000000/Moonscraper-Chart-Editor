using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorManager : MonoBehaviour {
    ErrorMessage errorMenu;
    string queuedErrorMessage = string.Empty;

    private void Start()
    {
        errorMenu = ChartEditor.GetInstance().uiServices.gameObject.GetComponentInChildren<ErrorMessage>(true);
        Debug.Assert(errorMenu, "Unable to find error menu component");
    }

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
