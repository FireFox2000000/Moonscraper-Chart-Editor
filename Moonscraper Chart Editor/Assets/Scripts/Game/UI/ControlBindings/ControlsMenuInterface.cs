using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ControlsMenuInterface : MonoBehaviour
{
    [SerializeField]
    ActionBindingsMenu actionBindingsMenu;
    [SerializeField]
    Text actionCategoryText;
    [SerializeField]
    Text deviceText;

    [Header("Default Controls Editor")]
    [SerializeField]
    bool isDefaultControlsEditor = false;
    [SerializeField]
    TextAsset defaultControlsFile;

    int actionCategoryIndex = 0;
    int deviceIndex = 0;

    MSChartEditorInput.MSChartEditorActionContainer actionsToEdit = null;

    enum ActionCategory
    {
        Editor,
        Gameplay,
    }

    private void Start()
    {
        InputManager.Instance.disconnectEvent.Register(OnDeviceDisconnect);

        SetActionsToEdit();
    }

    void SetActionsToEdit()
    {
        if (actionsToEdit == null)
        {
            if (isDefaultControlsEditor)
            {
                actionsToEdit = JsonUtility.FromJson<MSChartEditorInput.MSChartEditorActionContainer>(defaultControlsFile.text);
            }
            else
            {
                actionsToEdit = GameSettings.controls;
            }
        }
    }

    void OnDeviceDisconnect(in MSE.Input.IInputDevice device)
    {
        deviceIndex = 0;
        RefreshActionBindingsMenu();
    }

    private void OnEnable()
    {
        SetActionsToEdit();
        RefreshActionBindingsMenu();
    }

    public void SetDefaultControls()
    {
        SetDefaultControls(GetCurrentInputDevice().Type, GetCurrentActionCategoryMask());
    }

    void SetDefaultControls(MSE.Input.DeviceType device, int categoryMask)
    {
        MSE.Input.InputRebinder.SetToDefault(actionsToEdit, InputManager.Instance.defaultControls, categoryMask, device);

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
        actionBindingsMenu.Setup(GetCurrentInputDevice(), actionsToEdit, GetCurrentActionCategoryMask());
    }

    public void SaveActionsToDefaultControlsFile()
    {
#if UNITY_EDITOR
        actionsToEdit.UpdateSaveData(true);

        string filepath = AssetDatabase.GetAssetPath(defaultControlsFile);
        System.IO.File.WriteAllText(filepath, JsonUtility.ToJson(actionsToEdit, true));
#endif
    }
}
