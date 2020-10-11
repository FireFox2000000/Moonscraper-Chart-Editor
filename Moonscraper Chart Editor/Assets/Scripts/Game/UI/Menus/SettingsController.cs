// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsController : TabMenu
{
    [SerializeField]
    RectTransform settingsMenuContentArea;

    public Toggle clapStrum;
    public Toggle clapHopo;
    public Toggle clapTap;
    public Toggle leftyFlipToggle;
    public Toggle extendedSustainsToggle;
    public Toggle sustainGapEnabledToggle;
    public Toggle sustainGapTimeBasedToggle;
    public Toggle resetAfterPlay;
    public Toggle resetAfterGameplay;
    public Toggle autoValidateSongOnSave;
    public Toggle slowdownPitchCorrectionEnabled;

    public Slider musicSourceSlider;
    public Slider guitarSourceSlider;
    public Slider bassSourceSlider;
    public Slider rhythmSourceSlider;
	public Slider keysSourceSlider;
    public Slider drumSourceSlider;
    public Slider drum2SourceSlider;
    public Slider drum3SourceSlider;
    public Slider drum4SourceSlider;
    public Slider clapSourceSlider;
    public Slider sfxSlider;
	public Slider vocalSourceSlider;
	public Slider crowdSourceSlider;

    public Slider masterVolumeSlider;
    public Slider musicPanSlider;

    public InputField sustainGapInput;
    public InputField sustainGapTimeInput;
    public Dropdown gameplayStartDelayDropdown;
    public Dropdown fpsSelectDropdown;
    public Dropdown bgSwapTimeDropdown;

    public Dropdown antiAliasingLevel; 

    public void SetSettingsGroup(RectTransform content)
    {
        SetTabGroup(content);
    }

    protected override void Awake()
    {
        base.Awake();

        menuContextArea = settingsMenuContentArea;
    }

    protected override void Start()
    {
        base.Start();

        sustainGapInput.onValidateInput = Step.validateStepVal;
        sustainGapInput.text = Globals.gameSettings.sustainGap.ToString();
        sustainGapTimeInput.text = Globals.gameSettings.sustainGapTimeMs.ToString();
    }

    protected override void Update()
    {
        base.Update();

        if (!string.IsNullOrEmpty(sustainGapInput.text))
        {
            sustainGapInput.text = Globals.gameSettings.sustainGap.ToString();
        }

        if (!string.IsNullOrEmpty(sustainGapTimeInput.text) && int.Parse(sustainGapTimeInput.text) != Globals.gameSettings.sustainGapTimeMs)
        {
            sustainGapTimeInput.text = Globals.gameSettings.sustainGapTimeMs.ToString();
        }

        // Set all variables' values based on the UI
        Globals.gameSettings.sustainGapEnabled = sustainGapEnabledToggle.isOn;

        Globals.gameSettings.vol_song = musicSourceSlider.value;
        Globals.gameSettings.vol_guitar = guitarSourceSlider.value;
        Globals.gameSettings.vol_bass = bassSourceSlider.value;
        Globals.gameSettings.vol_rhythm = rhythmSourceSlider.value;
        Globals.gameSettings.vol_keys = keysSourceSlider.value;
        Globals.gameSettings.vol_drums = drumSourceSlider.value;
        Globals.gameSettings.vol_drums2 = drum2SourceSlider.value;
        Globals.gameSettings.vol_drums3 = drum3SourceSlider.value;
        Globals.gameSettings.vol_drums4 = drum4SourceSlider.value;
        Globals.gameSettings.vol_vocals = vocalSourceSlider.value;
        Globals.gameSettings.vol_crowd = crowdSourceSlider.value;
        Globals.gameSettings.sfxVolume = sfxSlider.value;

        //editor.clapSource.volume = clapSourceSlider.value;

        Globals.gameSettings.vol_master = masterVolumeSlider.value / 10.0f;
        AudioListener.volume = Globals.gameSettings.vol_master;
        Globals.gameSettings.audio_pan = musicPanSlider.value / 10.0f;

        Globals.gameSettings.gameplayStartDelayTime = gameplayStartDelayDropdown.value * 0.5f;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Initialise GUI
        sustainGapEnabledToggle.isOn = Globals.gameSettings.sustainGapEnabled;
        sustainGapTimeBasedToggle.isOn = Globals.gameSettings.sustainGapIsTimeBased;
        sustainGapTimeInput.text = Globals.gameSettings.sustainGapTimeMs.ToString();

        UpdateSustainGapInteractability();

        initClapToggle(clapStrum, GameSettings.ClapToggle.STRUM);
        initClapToggle(clapHopo, GameSettings.ClapToggle.HOPO);
        initClapToggle(clapTap, GameSettings.ClapToggle.TAP);

        if (Globals.gameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
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

        switch (Globals.gameSettings.customBgSwapTime)
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
        masterVolumeSlider.value = Globals.gameSettings.vol_master * 10.0f;
        musicSourceSlider.value = Globals.gameSettings.vol_song;
        guitarSourceSlider.value = Globals.gameSettings.vol_guitar;
        bassSourceSlider.value = Globals.gameSettings.vol_bass;
        rhythmSourceSlider.value = Globals.gameSettings.vol_rhythm;
        drumSourceSlider.value = Globals.gameSettings.vol_drums;
        drum2SourceSlider.value = Globals.gameSettings.vol_drums2;
        drum3SourceSlider.value = Globals.gameSettings.vol_drums3;
        drum4SourceSlider.value = Globals.gameSettings.vol_drums4;
        sfxSlider.value = Globals.gameSettings.sfxVolume;
		keysSourceSlider.value = Globals.gameSettings.vol_keys;
		vocalSourceSlider.value = Globals.gameSettings.vol_vocals;
		crowdSourceSlider.value = Globals.gameSettings.vol_crowd;

        //clapSourceSlider.value = editor.clapSource.volume;
        musicPanSlider.value = Globals.gameSettings.audio_pan * 10.0f;

        if (Globals.gameSettings.extendedSustainsEnabled)
            extendedSustainsToggle.isOn = true;
        else
            extendedSustainsToggle.isOn = false;

        resetAfterPlay.isOn = Globals.gameSettings.resetAfterPlay;
        resetAfterGameplay.isOn = Globals.gameSettings.resetAfterGameplay;
        autoValidateSongOnSave.isOn = Globals.gameSettings.autoValidateSongOnSave;
        slowdownPitchCorrectionEnabled.isOn = Globals.gameSettings.slowdownPitchCorrectionEnabled;

        gameplayStartDelayDropdown.value = (int)(Globals.gameSettings.gameplayStartDelayTime * 2.0f);

        // Set antiAliasingLevel dropdown
        {
            int antiAliasingLevelDropdownValue = 3;
            switch (QualitySettings.antiAliasing)
            {
                case 0:
                    {
                        antiAliasingLevelDropdownValue = 0;
                        break;
                    }
                case 2:
                    {
                        antiAliasingLevelDropdownValue = 1;
                        break;
                    }
                case 4:
                    {
                        antiAliasingLevelDropdownValue = 2;
                        break;
                    }
                default: break;
            }

            antiAliasingLevel.value = antiAliasingLevelDropdownValue;
        }

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
            Globals.gameSettings.notePlacementMode = GameSettings.NotePlacementMode.LeftyFlip;
        else
            Globals.gameSettings.notePlacementMode = GameSettings.NotePlacementMode.Default;
        
        editor.events.leftyFlipToggledEvent.Fire();
    }

    public void SetResetAfterPlay(bool value)
    {
        Globals.gameSettings.resetAfterPlay = value;
    }

    public void SetResetAfterGameplay(bool value)
    {
        Globals.gameSettings.resetAfterGameplay = value;
    }

    public void SetExtendedSustains(bool value)
    {
        Globals.gameSettings.extendedSustainsEnabled = value;
    }

    public void SetAutoValidateSongOnSave(bool value)
    {
        Globals.gameSettings.autoValidateSongOnSave = value;
    }

    public void SetSlowdownPitchCorrectionEnabled(bool value)
    {
        Globals.gameSettings.slowdownPitchCorrectionEnabled = value;
    }

    public void IncrementSustainsGapStep()
    {
        Globals.gameSettings.sustainGapStep.Increment();
    }

    public void DecrementSustainsGapStep()
    {
        Globals.gameSettings.sustainGapStep.Decrement();
    }

    void initClapToggle(Toggle toggle, GameSettings.ClapToggle setting)
    {
        if ((Globals.gameSettings.clapProperties & setting) != 0)
            toggle.isOn = true;
        else
            toggle.isOn = false;  
    }

    void SetClapProperties(bool value, GameSettings.ClapToggle setting)
    {
        if (value)
            Globals.gameSettings.clapProperties |= setting;
        else
            Globals.gameSettings.clapProperties &= ~setting;
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

        Globals.gameSettings.sustainGap = stepVal;
        sustainGapInput.text = Globals.gameSettings.sustainGap.ToString();
    }

    public void OnSustainGapTimeBasedToggled(bool value)
    {
        Globals.gameSettings.sustainGapIsTimeBased = value;

        UpdateSustainGapInteractability();
    }

    private void UpdateSustainGapInteractability()
    {
        bool value = Globals.gameSettings.sustainGapIsTimeBased;

        sustainGapInput.interactable = !value;
        sustainGapTimeInput.interactable = value;
    }

    public void OnSustainGapTimeInputUpdated(string value)
    {
        int stepVal = 0;
        if (!string.IsNullOrEmpty(value))
        {
            stepVal = int.Parse(value);
        }

        Globals.gameSettings.sustainGapTimeMs = stepVal;
    }

    public void OnSustainGapTimeEndInput(string value)
    {
        OnSustainGapTimeInputUpdated(value);
        if (string.IsNullOrEmpty(value))
        {
            sustainGapTimeInput.text = "0";
        }
    }

    public void SetBgSwapTime(int value)
    {
        switch (value)
        {
            case 0:
                Globals.gameSettings.customBgSwapTime = 10;
                break;
            case 1:
                Globals.gameSettings.customBgSwapTime = 30;
                break;
            case 2:
                Globals.gameSettings.customBgSwapTime = 60;
                break;
            case 3:
                Globals.gameSettings.customBgSwapTime = 180;
                break;
            case 4:
                Globals.gameSettings.customBgSwapTime = 300;
                break;
            case 5:
                Globals.gameSettings.customBgSwapTime = 600;
                break;
            default:
                break;
        }
    }

    public void SetAntiAliasingLevel(int value)
    {
        switch (value)
        {
            case 0:
                {
                    QualitySettings.antiAliasing = 0;
                    break;
                }
            case 1:
                {
                    QualitySettings.antiAliasing = 2;
                    break;
                }
            case 2:
                {
                    QualitySettings.antiAliasing = 4;
                    break;
                }
            default:
                {
                    QualitySettings.antiAliasing = 8;
                    break;
                }
        }
    }

    void SetSynchronisedVolume(float volume)
    {
        musicSourceSlider.value = volume;
        guitarSourceSlider.value = volume;
        bassSourceSlider.value = volume;
        rhythmSourceSlider.value = volume;
        drumSourceSlider.value = volume;
        drum2SourceSlider.value = volume;
        drum3SourceSlider.value = volume;
        drum4SourceSlider.value = volume;
        keysSourceSlider.value = volume;
        vocalSourceSlider.value = volume;
        crowdSourceSlider.value = volume;
    }

    public void MuteAllStems()
    {
        SetSynchronisedVolume(0);
    }

    public void UnmuteAllStems()
    {
        SetSynchronisedVolume(1);
    }

}
