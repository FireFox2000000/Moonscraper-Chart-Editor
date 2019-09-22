using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsMenuInterface : MonoBehaviour
{
    [SerializeField]
    ActionBindingsMenu actionBindingsMenu;

    public void SetDefaultControls()
    {
        // Todo, enum for current DISPLAYED device
        SetDefaultControls(MSE.Input.DeviceType.Keyboard);
    }

    void SetDefaultControls(MSE.Input.DeviceType device)
    {
        switch (device)
        {
            case MSE.Input.DeviceType.Keyboard:
                {
                    GameSettings.SetDefaultKeysControls(GameSettings.controls);
                    break;
                }
            default:
                Debug.LogError("Unhandled device type " + device.ToString());
                break;
        }

        actionBindingsMenu.OnRebindComplete();
    }
}
