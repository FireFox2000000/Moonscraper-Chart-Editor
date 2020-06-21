// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorManager : MonoBehaviour {
    ErrorMessage errorMenu;
    string queuedErrorMessage = string.Empty;

    private void Start()
    {
        errorMenu = ChartEditor.Instance.uiServices.gameObject.GetComponentInChildren<ErrorMessage>(true);
        Debug.Assert(errorMenu, "Unable to find error menu component");
    }

    // Update is called once per frame
    void Update () {
        if (ChartEditor.Instance.currentState == ChartEditor.State.Editor && HasErrorToDisplay())
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
