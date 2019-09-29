using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuInterface : MonoBehaviour
{
    [SerializeField]
    ActionBindingsMenu actionBindingsMenu;
    [SerializeField]
    Text actionCategoryText;
    [SerializeField]
    Text deviceText;

    int actionCategoryIndex = 0;
    int deviceIndex = 0;

    enum ActionCategory
    {
        Editor,
        Gameplay,
    }

    private void Start()
    {
        InputManager.Instance.disconnectEvent.Register(OnDeviceDisconnect);
    }

    void OnDeviceDisconnect(in MSE.Input.IInputDevice device)
    {
        deviceIndex = 0;
        RefreshActionBindingsMenu();
    }

    private void OnEnable()
    {
        RefreshActionBindingsMenu();
    }

    public void SetDefaultControls()
    {
        SetDefaultControls(GetCurrentInputDevice().Type, GetCurrentActionCategory());
    }

    void SetDefaultControls(MSE.Input.DeviceType device, ActionCategory category)
    {
        switch (device)
        {
            case MSE.Input.DeviceType.Keyboard:
                {
                    switch (category)
                    {
                        case ActionCategory.Editor:
                            GameSettings.SetDefaultKeysControls(GameSettings.controls);
                            break;

                        case ActionCategory.Gameplay:
                            GameSettings.SetDefaultGameplayControlsKeys(GameSettings.controls);
                            break;

                        default:
                            Debug.LogError("Unhandled category " + category.ToString());
                            break;
                    }
                    break;
                }
            case MSE.Input.DeviceType.Gamepad:
                {
                    switch (category)
                    {
                        case ActionCategory.Editor:
                            GameSettings.SetDefaultEditorControlsPad(GameSettings.controls);
                            break;

                        case ActionCategory.Gameplay:
                            GameSettings.SetDefaultGameplayControlsPad(GameSettings.controls);
                            break;

                        default:
                            Debug.LogError("Unhandled category " + category.ToString());
                            break;
                    }
                    break;
                }
            default:
                Debug.LogError("Unhandled device type " + device.ToString());
                break;
        }

        actionBindingsMenu.OnRebindComplete();
    }

    public void CycleDeviceForwards()
    {
        ++deviceIndex;
        if (deviceIndex >= InputManager.Instance.devices.Count)
        {
            deviceIndex = 0;
        }

        RefreshActionBindingsMenu();
    }

    public void CycleDeviceBackwards()
    {
        --deviceIndex;
        if (deviceIndex < 0)
        {
            deviceIndex = InputManager.Instance.devices.Count - 1;
        }

        RefreshActionBindingsMenu();
    }

    public void CycleActionCategoryForwards()
    {
        ++actionCategoryIndex;
        RefreshActionBindingsMenu();
    }

    public void CycleActionCategoryBackwards()
    {
        --actionCategoryIndex;
        RefreshActionBindingsMenu();
    }

    MSE.Input.IInputDevice GetCurrentInputDevice()
    {
        // Todo, enum for current DISPLAYED device
        return InputManager.Instance.devices[deviceIndex];
    }

    ActionCategory GetCurrentActionCategory()
    {
        return actionCategoryIndex % 2 == 0 ? ActionCategory.Editor : ActionCategory.Gameplay;
    }

    int GetCurrentActionCategoryMask()
    {
        switch (GetCurrentActionCategory())
        {
            case ActionCategory.Editor:
                return MSChartEditorInput.Category.kEditorCategoryMask;

            case ActionCategory.Gameplay:
                return MSChartEditorInput.Category.kGameplayCategoryMask;

            default:
                break;
        }

        return MSChartEditorInput.Category.kEditorCategoryMask;
    }

    string GetCurrentActionCategoryName()
    {
        switch (GetCurrentActionCategory())
        {
            case ActionCategory.Editor:
                return "Editor";

            case ActionCategory.Gameplay:
                return "Gameplay";

            default:
                break;
        }

        return "";
    }

    void RefreshActionBindingsMenu()
    {
        actionCategoryText.text = GetCurrentActionCategoryName();
        deviceText.text = GetCurrentInputDevice().GetDeviceName();
        actionBindingsMenu.Setup(GetCurrentInputDevice(), GameSettings.controls, GetCurrentActionCategoryMask());
    }
}
