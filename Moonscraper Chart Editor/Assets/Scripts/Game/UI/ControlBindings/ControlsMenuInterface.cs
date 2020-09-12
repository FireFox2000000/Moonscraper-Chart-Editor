// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonscraperEngine.Input;
using MoonscraperEngine;

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
        Gameplay_Guitar,
        Gameplay_Drums,
        Gameplay_ProDrums,
    }

    private void Awake()
    {
        SetActionsToEdit();
        actionBindingsMenu.Setup(GetCurrentInputDevice(), actionsToEdit, GetCurrentActionCategoryMask());
    }

    private void Start()
    {
        InputManager.Instance.disconnectEvent.Register(OnDeviceDisconnect);
    }

    void SetActionsToEdit()
    {
        if (actionsToEdit == null)
        {
            if (isDefaultControlsEditor)
            {
                actionsToEdit = JsonUtility.FromJson<MSChartEditorInput.MSChartEditorActionContainer>(defaultControlsFile.text);
                actionsToEdit.LoadFromSaveData(actionsToEdit);
                Globals.LoadExternalControls(actionsToEdit);
            }
            else
            {
                actionsToEdit = Globals.gameSettings.controls;
            }
        }
    }

    void OnDeviceDisconnect(in IInputDevice device)
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
        SetDefaultControls(GetCurrentInputDevice(), GetCurrentActionCategoryMask());
    }

    void SetDefaultControls(IInputDevice device, int categoryMask)
    {
        InputRebinder.SetToDefault(actionsToEdit, InputManager.Instance.defaultControls, categoryMask, device);

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
        if (actionCategoryIndex < 0)
        {
            actionCategoryIndex = EnumX<ActionCategory>.Count - 1;
        }

        RefreshActionBindingsMenu();
    }

    IInputDevice GetCurrentInputDevice()
    {
        // Todo, enum for current DISPLAYED device
        return InputManager.Instance.devices[deviceIndex];
    }

    ActionCategory GetCurrentActionCategory()
    {
        return (ActionCategory)(actionCategoryIndex % EnumX<ActionCategory>.Count);
    }

    int GetCurrentActionCategoryMask()
    {
        switch (GetCurrentActionCategory())
        {
            case ActionCategory.Editor:
                return MSChartEditorInput.Category.kEditorCategoryMask;

            case ActionCategory.Gameplay_Guitar:
                return MSChartEditorInput.Category.kGameplayGuitarCategoryMask;

            case ActionCategory.Gameplay_Drums:
                return MSChartEditorInput.Category.kGameplayDrumsCategoryMask;

            case ActionCategory.Gameplay_ProDrums:
                return MSChartEditorInput.Category.kGameplayDrumsProCategoryMask;

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

            case ActionCategory.Gameplay_Guitar:
                return "Gameplay - Guitar";

            case ActionCategory.Gameplay_Drums:
                return "Gameplay - Drums";

            case ActionCategory.Gameplay_ProDrums:
                return "Gameplay - Pro Drums";

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
