using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsController : DisplayMenu
{
    public Toggle clapOnOff;
    public Toggle clapStrum;
    public Toggle clapHopo;
    public Toggle clapTap;
    public Toggle leftyFlipToggle;
    public Toggle extendedSustainsToggle;
    public Toggle sustainGapEnabledToggle;
    public Toggle resetAfterPlay;
    public Toggle resetAfterGameplay;

    public Slider musicSourceSlider;
    public Slider guitarSourceSlider;
    public Slider rhythmSourceSlider;
    public Slider clapSourceSlider;
    public Slider sfxSlider;

    public Slider masterVolumeSlider;
    public Slider musicPanSlider;

    public InputField sustainGapInput;
    public Dropdown gameplayStartDelayDropdown;

    protected override void Awake()
    {
        base.Awake();      
    }

    void Start()
    {
        sustainGapInput.onValidateInput = Step.validateStepVal;
        sustainGapInput.text = Globals.sustainGap.ToString(); 
    }

    protected override void Update()
    {
        base.Update();

        if (sustainGapInput.text != string.Empty)
            sustainGapInput.text = Globals.sustainGap.ToString();

        // Set all variables' values based on the UI
        Globals.sustainGapEnabled = sustainGapEnabledToggle.isOn;

        Globals.vol_song = musicSourceSlider.value;
        Globals.vol_guitar = guitarSourceSlider.value;
        Globals.vol_rhythm = rhythmSourceSlider.value;

        Globals.sfxVolume = sfxSlider.value;

        editor.clapSource.volume = clapSourceSlider.value;

        Globals.vol_master = masterVolumeSlider.value / 10.0f;
        Globals.audio_pan = musicPanSlider.value / 10.0f;

        editor.SetVolume();
        Globals.gameplayStartDelayTime = gameplayStartDelayDropdown.value * 0.5f;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Initialise GUI
        sustainGapEnabledToggle.isOn = Globals.sustainGapEnabled;

        initClapToggle(clapStrum, Globals.ClapToggle.STRUM);
        initClapToggle(clapHopo, Globals.ClapToggle.HOPO);
        initClapToggle(clapTap, Globals.ClapToggle.TAP);

        if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
            leftyFlipToggle.isOn = true;
        else
            leftyFlipToggle.isOn = false;

        // Set volume sliders
        masterVolumeSlider.value = Globals.vol_master * 10.0f;
        musicSourceSlider.value = Globals.vol_song;
        guitarSourceSlider.value = Globals.vol_guitar;
        rhythmSourceSlider.value = Globals.vol_rhythm;
        sfxSlider.value = Globals.sfxVolume;

        clapSourceSlider.value = editor.clapSource.volume;
        musicPanSlider.value = Globals.audio_pan * 10.0f;

        if (Globals.extendedSustainsEnabled)
            extendedSustainsToggle.isOn = true;
        else
            extendedSustainsToggle.isOn = false;

        resetAfterPlay.isOn = Globals.resetAfterPlay;
        resetAfterGameplay.isOn = Globals.resetAfterGameplay;

        gameplayStartDelayDropdown.value = (int)(Globals.gameplayStartDelayTime * 2.0f);

        editor.SetVolume();
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

    public void SetLeftyFlip(bool value)
    {
        if (value == true)
            Globals.notePlacementMode = Globals.NotePlacementMode.LeftyFlip;
        else
            Globals.notePlacementMode = Globals.NotePlacementMode.Default;
    }

    public void SetResetAfterPlay(bool value)
    {
        Globals.resetAfterPlay = value;
    }

    public void SetResetAfterGameplay(bool value)
    {
        Globals.resetAfterGameplay = value;
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

    public void SetStep(string value)
    {
        if (value != string.Empty)
        {
            StepInputEndEdit(value);
        }
    }

    public void StepInputEndEdit(string value)
    {
        int stepVal;
        const int defaultControlsStepVal = 16;

        if (value == string.Empty)
            stepVal = defaultControlsStepVal;
        else
        {
            try
            {
                stepVal = int.Parse(value);

                if (stepVal < Step.MIN_STEP)
                    stepVal = Step.MIN_STEP;
                else if (stepVal > Step.FULL_STEP)
                    stepVal = Step.FULL_STEP;
            }
            catch
            {
                stepVal = defaultControlsStepVal;
            }
        }

        Globals.sustainGap = stepVal;
        sustainGapInput.text = Globals.sustainGap.ToString();
    }
}
