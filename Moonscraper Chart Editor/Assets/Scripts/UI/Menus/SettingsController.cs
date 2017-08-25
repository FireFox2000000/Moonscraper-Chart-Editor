using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsController : DisplayMenu
{
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
    public Slider drumSourceSlider;
    //public Slider clapSourceSlider;
    public Slider sfxSlider;

    public Slider masterVolumeSlider;
    public Slider musicPanSlider;

    public InputField sustainGapInput;
    public Dropdown gameplayStartDelayDropdown;
    public Dropdown fpsSelectDropdown;
    public Dropdown bgSwapTimeDropdown;

    public StrikelineFretPlacement strikelineFretPlacement;

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
        Globals.vol_drum = drumSourceSlider.value;

        Globals.sfxVolume = sfxSlider.value;

        //editor.clapSource.volume = clapSourceSlider.value;

        Globals.vol_master = masterVolumeSlider.value / 10.0f;
        AudioListener.volume = Globals.vol_master;
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

        switch(Application.targetFrameRate)
        {
            case (60):
                fpsSelectDropdown.value = 0;
                break;
            case (120):
                fpsSelectDropdown.value = 1;
                break;
            case (240):
                fpsSelectDropdown.value = 2;
                break;
            default:
                fpsSelectDropdown.value = 3;
                break;
        }

        switch (Globals.customBgSwapTime)
        {
            case 10:
                bgSwapTimeDropdown.value = 0;
                break;
            case 30:
                bgSwapTimeDropdown.value = 1;
                break;
            case 60:
                bgSwapTimeDropdown.value = 2;
                break;
            case 180:
                bgSwapTimeDropdown.value = 3;
                break;
            case 300:
                bgSwapTimeDropdown.value = 4;
                break;
            case 600:
                bgSwapTimeDropdown.value = 5;
                break;
            default:
                bgSwapTimeDropdown.value = 0;
                break;
        }

        // Set volume sliders
        masterVolumeSlider.value = Globals.vol_master * 10.0f;
        musicSourceSlider.value = Globals.vol_song;
        guitarSourceSlider.value = Globals.vol_guitar;
        rhythmSourceSlider.value = Globals.vol_rhythm;
        drumSourceSlider.value = Globals.vol_drum;
        sfxSlider.value = Globals.sfxVolume;

        //clapSourceSlider.value = editor.clapSource.volume;
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

    public void SetFPS(int dropdownValue)
    {
        int fps = 60 * (int)(Mathf.Pow(2, dropdownValue));
        if (fps > 240)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = fps;
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

        strikelineFretPlacement.SetFretPlacement();
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

    public void SetBgSwapTime(int value)
    {
        switch (value)
        {
            case 0:
                Globals.customBgSwapTime = 10;
                break;
            case 1:
                Globals.customBgSwapTime = 30;
                break;
            case 2:
                Globals.customBgSwapTime = 60;
                break;
            case 3:
                Globals.customBgSwapTime = 180;
                break;
            case 4:
                Globals.customBgSwapTime = 300;
                break;
            case 5:
                Globals.customBgSwapTime = 600;
                break;
            default:
                break;
        }
    }
}
