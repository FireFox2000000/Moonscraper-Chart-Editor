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
        toggle.isOn = (GameSettings.clapProperties & setting) != 0;
    }

    void SetClapSetting(GameSettings.ClapToggle setting, bool value)
    {
        if (value)
            GameSettings.clapProperties |= setting;
        else
            GameSettings.clapProperties &= ~setting;

        if (GameSettings.clapSetting != GameSettings.ClapToggle.NONE)
        {
            GameSettings.clapSetting = GameSettings.clapProperties;
        }
    }

    public void ApplyCurrentToggleProperties()
    {
        if (toggleValueChangedBlockingEnabled)
            return;

        foreach (ClapToggleSetting clapToggleSetting in clapToggles)
        {
            SetClapSetting(clapToggleSetting.clapToggleType, clapToggleSetting.toggleUI.isOn);
        }
    }
}
