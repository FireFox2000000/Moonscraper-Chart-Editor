using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsController : DisplayMenu
{
    public Toggle clapOnOff;
    public Toggle clapStrum;
    public Toggle clapHopo;
    public Toggle clapTap;
    public Toggle extendedSustainsToggle;
    public Toggle sustainGapEnabledToggle;

    public Text sustainGapText;

    void Start()
    {
        sustainGapEnabledToggle.isOn = Globals.sustainGapEnabled;
    }

    void Update()
    {
        sustainGapText.text = "1/" + Globals.sustainGap.ToString();
        Globals.sustainGapEnabled = sustainGapEnabledToggle.isOn;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        initClapToggle(clapStrum, Globals.ClapToggle.STRUM);
        initClapToggle(clapHopo, Globals.ClapToggle.HOPO);
        initClapToggle(clapTap, Globals.ClapToggle.TAP);

        if (Globals.extendedSustainsEnabled)
            extendedSustainsToggle.isOn = true;
        else
            extendedSustainsToggle.isOn = false;

        Start();
        Update();
    }  

    public void SetClapStrum(bool value)
    {
        SetClapProperties(value, Globals.ClapToggle.STRUM);
    }

    public void SetClapHopo(bool value)
    {
        SetClapProperties(value, Globals.ClapToggle.HOPO);
    }

    public void SetClapTap(bool value)
    {
        SetClapProperties(value, Globals.ClapToggle.TAP);
    }

    public void SetExtendedSustains(bool value)
    {
        Globals.extendedSustainsEnabled = value;
    }

    public void IncrementSustainsGapStep()
    {
        Globals.sustainGapStep.Increment();
    }

    public void DecrementSustainsGapStep()
    {
        Globals.sustainGapStep.Decrement();
    }

    void initClapToggle(Toggle toggle, Globals.ClapToggle setting)
    {
        if ((Globals.clapProperties & setting) != 0)
            toggle.isOn = true;
        else
            toggle.isOn = false;  
    }

    void SetClapProperties(bool value, Globals.ClapToggle setting)
    {
        if (value)
            Globals.clapProperties |= setting;
        else
            Globals.clapProperties &= ~setting;

        if (Globals.clapSetting != Globals.ClapToggle.NONE)
        {
            Globals.clapSetting = Globals.clapProperties;
        }
    }
}
