// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

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
    public Toggle autoValidateSongOnSave;

    public Slider musicSourceSlider;
    public Slider guitarSourceSlider;
    public Slider bassSourceSlider;
    public Slider rhythmSourceSlider;
	public Slider keysSourceSlider;
    public Slider drumSourceSlider;
    public Slider clapSourceSlider;
    public Slider sfxSlider;
	public Slider vocalSourceSlider;
	public Slider crowdSourceSlider;

    public Slider masterVolumeSlider;
    public Slider musicPanSlider;

    public InputField sustainGapInput;
    public Dropdown gameplayStartDelayDropdown;
    public Dropdown fpsSelectDropdown;
    public Dropdown bgSwapTimeDropdown;

    public Indicators strikelineFretPlacement;

    protected override void Awake()
    {
        base.Awake();      
    }

    void Start()
    {
        sustainGapInput.onValidateInput = Step.validateStepVal;
        sustainGapInput.text = GameSettings.sustainGap.ToString(); 
    }

    protected override void Update()
    {
        base.Update();

        if (sustainGapInput.text != string.Empty)
            sustainGapInput.text = GameSettings.sustainGap.ToString();

        // Set all variables' values based on the UI
        GameSettings.sustainGapEnabled = sustainGapEnabledToggle.isOn;

        GameSettings.vol_song = musicSourceSlider.value;
        GameSettings.vol_guitar = guitarSourceSlider.value;
        GameSettings.vol_bass = bassSourceSlider.value;
        GameSettings.vol_rhythm = rhythmSourceSlider.value;
        GameSettings.vol_keys = keysSourceSlider.value;
        GameSettings.vol_drums = drumSourceSlider.value;
        GameSettings.vol_vocals = vocalSourceSlider.value;
        GameSettings.vol_crowd = crowdSourceSlider.value;
        GameSettings.sfxVolume = sfxSlider.value;

        //editor.clapSource.volume = clapSourceSlider.value;

        GameSettings.vol_master = masterVolumeSlider.value / 10.0f;
        AudioListener.volume = GameSettings.vol_master;
        GameSettings.audio_pan = musicPanSlider.value / 10.0f;

        GameSettings.gameplayStartDelayTime = gameplayStartDelayDropdown.value * 0.5f;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Initialise GUI
        sustainGapEnabledToggle.isOn = GameSettings.sustainGapEnabled;

        initClapToggle(clapStrum, GameSettings.ClapToggle.STRUM);
        initClapToggle(clapHopo, GameSettings.ClapToggle.HOPO);
        initClapToggle(clapTap, GameSettings.ClapToggle.TAP);

        if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
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

        switch (GameSettings.customBgSwapTime)
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
        masterVolumeSlider.value = GameSettings.vol_master * 10.0f;
        musicSourceSlider.value = GameSettings.vol_song;
        guitarSourceSlider.value = GameSettings.vol_guitar;
        bassSourceSlider.value = GameSettings.vol_bass;
        rhythmSourceSlider.value = GameSettings.vol_rhythm;
        drumSourceSlider.value = GameSettings.vol_drums;
        sfxSlider.value = GameSettings.sfxVolume;
		keysSourceSlider.value = GameSettings.vol_keys;
		vocalSourceSlider.value = GameSettings.vol_vocals;
		crowdSourceSlider.value = GameSettings.vol_crowd;

        //clapSourceSlider.value = editor.clapSource.volume;
        musicPanSlider.value = GameSettings.audio_pan * 10.0f;

        if (GameSettings.extendedSustainsEnabled)
            extendedSustainsToggle.isOn = true;
        else
            extendedSustainsToggle.isOn = false;

        resetAfterPlay.isOn = GameSettings.resetAfterPlay;
        resetAfterGameplay.isOn = GameSettings.resetAfterGameplay;
        autoValidateSongOnSave.isOn = GameSettings.autoValidateSongOnSave;

        gameplayStartDelayDropdown.value = (int)(GameSettings.gameplayStartDelayTime * 2.0f);

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
        SetClapProperties(value, GameSettings.ClapToggle.STRUM);
    }

    public void SetClapHopo(bool value)
    {
        SetClapProperties(value, GameSettings.ClapToggle.HOPO);
    }

    public void SetClapTap(bool value)
    {
        SetClapProperties(value, GameSettings.ClapToggle.TAP);
    }

    public void SetLeftyFlip(bool value)
    {
        if (value == true)
            GameSettings.notePlacementMode = GameSettings.NotePlacementMode.LeftyFlip;
        else
            GameSettings.notePlacementMode = GameSettings.NotePlacementMode.Default;
        
        editor.events.leftyFlipToggledEvent.Fire();
    }

    public void SetResetAfterPlay(bool value)
    {
        GameSettings.resetAfterPlay = value;
    }

    public void SetResetAfterGameplay(bool value)
    {
        GameSettings.resetAfterGameplay = value;
    }

    public void SetExtendedSustains(bool value)
    {
        GameSettings.extendedSustainsEnabled = value;
    }

    public void SetAutoValidateSongOnSave(bool value)
    {
        GameSettings.autoValidateSongOnSave = value;
    }

    public void IncrementSustainsGapStep()
    {
        GameSettings.sustainGapStep.Increment();
    }

    public void DecrementSustainsGapStep()
    {
        GameSettings.sustainGapStep.Decrement();
    }

    void initClapToggle(Toggle toggle, GameSettings.ClapToggle setting)
    {
        if ((GameSettings.clapProperties & setting) != 0)
            toggle.isOn = true;
        else
            toggle.isOn = false;  
    }

    void SetClapProperties(bool value, GameSettings.ClapToggle setting)
    {
        if (value)
            GameSettings.clapProperties |= setting;
        else
            GameSettings.clapProperties &= ~setting;
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

        GameSettings.sustainGap = stepVal;
        sustainGapInput.text = GameSettings.sustainGap.ToString();
    }

    public void SetBgSwapTime(int value)
    {
        switch (value)
        {
            case 0:
                GameSettings.customBgSwapTime = 10;
                break;
            case 1:
                GameSettings.customBgSwapTime = 30;
                break;
            case 2:
                GameSettings.customBgSwapTime = 60;
                break;
            case 3:
                GameSettings.customBgSwapTime = 180;
                break;
            case 4:
                GameSettings.customBgSwapTime = 300;
                break;
            case 5:
                GameSettings.customBgSwapTime = 600;
                break;
            default:
                break;
        }
    }
}
