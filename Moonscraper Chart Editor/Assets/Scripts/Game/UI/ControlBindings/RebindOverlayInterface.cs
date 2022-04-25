// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonscraperEngine.Input;

public class RebindOverlayInterface : MonoBehaviour
{
    InputRebinder rebinder;
    public MoonscraperEngine.Event rebindCompleteEvent = new MoonscraperEngine.Event();
    public MoonscraperEngine.Event<InputAction> inputConflictEvent = new MoonscraperEngine.Event<InputAction>();

    [SerializeField]
    Text conflictNotificationText;
    const string conflictFormatStr = "Cannot remap to {0} as it is already in use by {1}";

    IInputDevice device;

    private void OnEnable()
    {
        conflictNotificationText.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.CloseMenu))
        {
            Close(false);
        }
        else
        {

            InputAction conflict;
            IInputMap attemptedInput;
            if (!device.Connected || rebinder.TryMap(out conflict, out attemptedInput))
            {
                Close(true);
            }
            else if (conflict != null)
            {
                inputConflictEvent.Fire(conflict);
                conflictNotificationText.text = string.Format(conflictFormatStr, attemptedInput.GetInputStr(), conflict.properties.displayName);
                conflictNotificationText.enabled = true;
            }
        }
    }

    public void Open(InputAction actionToRebind, IInputMap mapToRebind, IEnumerable<InputAction> allActions, IInputDevice device)
    {
        if (ChartEditor.Instance)
            ChartEditor.Instance.uiServices.SetPopupBlockingEnabled(true);

        this.device = device;
        rebinder = new InputRebinder(actionToRebind, mapToRebind, allActions, device);
        gameObject.SetActive(true);
    }

    void Close(bool rebindSuccess)
    {
        this.device = null;
        if (!rebindSuccess && rebinder != null)
        {
            rebinder.RevertMapBeingRebound();
        }

        rebinder = null;
        gameObject.SetActive(false);
        rebindCompleteEvent.Fire();

        if (ChartEditor.Instance)
            ChartEditor.Instance.uiServices.SetPopupBlockingEnabled(false);

        Localiser.LocaliseScene();
    }

    void OnDisable()
    {
        if (rebinder != null)
        {
            Close(false);
        }
    }

    public void ClearInput()
    {
        Close(true);
    }
}
