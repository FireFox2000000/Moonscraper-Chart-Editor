// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ClapSettingsMenu : DisplayMenu
{
    [System.Serializable]
    public struct ClapToggleSetting
    {
        public Toggle toggleUI;
        public GameSettings.ClapToggle clapToggleType;
    }

    public ClapToggleSetting[] clapToggles;
    bool toggleValueChangedBlockingEnabled = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        toggleValueChangedBlockingEnabled = true;

        foreach (ClapToggleSetting clapToggleSetting in clapToggles)
        {
            InitClapToggle(clapToggleSetting.toggleUI, clapToggleSetting.clapToggleType);
        }

        toggleValueChangedBlockingEnabled = false;
    }

    void InitClapToggle(Toggle toggle, GameSettings.ClapToggle setting)
    {
        if (setting.HasFlag(GameSettings.ClapToggle.HOPO) 
            || setting.HasFlag(GameSettings.ClapToggle.STRUM)
            || setting.HasFlag(GameSettings.ClapToggle.TAP)
            )
        {
            bool forceAllNoteClaps = (Globals.gameSettings.clapProperties & GameSettings.ClapToggle.ALL_NOTES) != 0;
            toggle.interactable = !forceAllNoteClaps;
        }
        
        toggle.isOn = (Globals.gameSettings.clapProperties & setting) != 0;
    }

    void SetClapSetting(GameSettings.ClapToggle setting, bool value)
    {
        if (value)
            Globals.gameSettings.clapProperties.value |= setting;
        else
            Globals.gameSettings.clapProperties.value &= ~setting;
    }

    public void ApplyCurrentToggleProperties()
    {
        if (toggleValueChangedBlockingEnabled)
            return;

        foreach (ClapToggleSetting clapToggleSetting in clapToggles)
        {
            SetClapSetting(clapToggleSetting.clapToggleType, clapToggleSetting.toggleUI.isOn);
        }

        foreach (ClapToggleSetting clapToggleSetting in clapToggles)
        {
            InitClapToggle(clapToggleSetting.toggleUI, clapToggleSetting.clapToggleType);
        }
    }
}
