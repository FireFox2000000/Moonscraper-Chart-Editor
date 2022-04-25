// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsController : TabMenu
{
    [SerializeField]
    RectTransform settingsMenuContentArea;

    [SerializeField]
    Button lyricEditorButton;

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
    public Toggle lyricEditorStepSnappingEnabled;

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
    public InputField lyricEditorPhaseEndTimeInput;
    public InputField newSongResolutionInputField;

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
        lyricEditorPhaseEndTimeInput.text = Globals.gameSettings.lyricEditorSettings.phaseEndThreashold.ToString();
        lyricEditorPhaseEndTimeInput.onValidateInput = LocalesManager.ValidateDecimalInput;
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

        if (!string.IsNullOrEmpty(lyricEditorPhaseEndTimeInput.text) && float.Parse(lyricEditorPhaseEndTimeInput.text) != Globals.gameSettings.lyricEditorSettings.phaseEndThreashold)
        {
            lyricEditorPhaseEndTimeInput.text = Globals.gameSettings.lyricEditorSettings.phaseEndThreashold.ToString();
        }

        // Set all variables' values based on the UI
        Globals.gameSettings.sustainGapEnabled.value = sustainGapEnabledToggle.isOn;

        Globals.gameSettings.vol_song.value = musicSourceSlider.value;
        Globals.gameSettings.vol_guitar.value = guitarSourceSlider.value;
        Globals.gameSettings.vol_bass.value = bassSourceSlider.value;
        Globals.gameSettings.vol_rhythm.value = rhythmSourceSlider.value;
        Globals.gameSettings.vol_keys.value = keysSourceSlider.value;
        Globals.gameSettings.vol_drums.value = drumSourceSlider.value;
        Globals.gameSettings.vol_drums2.value = drum2SourceSlider.value;
        Globals.gameSettings.vol_drums3.value = drum3SourceSlider.value;
        Globals.gameSettings.vol_drums4.value = drum4SourceSlider.value;
        Globals.gameSettings.vol_vocals.value = vocalSourceSlider.value;
        Globals.gameSettings.vol_crowd.value = crowdSourceSlider.value;
        Globals.gameSettings.sfxVolume = sfxSlider.value;

        //editor.clapSource.volume = clapSourceSlider.value;

        Globals.gameSettings.vol_master.value = masterVolumeSlider.value / 10.0f;
        AudioListener.volume = Globals.gameSettings.vol_master;
        Globals.gameSettings.audio_pan.value = musicPanSlider.value / 10.0f;

        Globals.gameSettings.gameplayStartDelayTime.value = gameplayStartDelayDropdown.value * 0.5f;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Initialise GUI
        sustainGapEnabledToggle.isOn = Globals.gameSettings.sustainGapEnabled;
        sustainGapTimeBasedToggle.isOn = Globals.gameSettings.sustainGapIsTimeBased;
        sustainGapTimeInput.text = Globals.gameSettings.sustainGapTimeMs.ToString();
        lyricEditorPhaseEndTimeInput.text = Globals.gameSettings.lyricEditorSettings.phaseEndThreashold.ToString();
        newSongResolutionInputField.text = Globals.gameSettings.newSongResolution.ToString();

        UpdateSustainGapInteractability();

        initClapToggle(clapStrum, GameSettings.ClapToggle.STRUM);
        initClapToggle(clapHopo, GameSettings.ClapToggle.HOPO);
        initClapToggle(clapTap, GameSettings.ClapToggle.TAP);

        leftyFlipToggle.isOn = Globals.gameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip;

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

        extendedSustainsToggle.isOn = Globals.gameSettings.extendedSustainsEnabled;
        resetAfterPlay.isOn = Globals.gameSettings.resetAfterPlay;
        resetAfterGameplay.isOn = Globals.gameSettings.resetAfterGameplay;
        autoValidateSongOnSave.isOn = Globals.gameSettings.autoValidateSongOnSave;
        slowdownPitchCorrectionEnabled.isOn = Globals.gameSettings.slowdownPitchCorrectionEnabled;
        lyricEditorStepSnappingEnabled.isOn = Globals.gameSettings.lyricEditorSettings.stepSnappingEnabled;

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
        Globals.gameSettings.notePlacementMode.value = value ? GameSettings.NotePlacementMode.LeftyFlip : GameSettings.NotePlacementMode.Default;
        editor.events.leftyFlipToggledEvent.Fire();
    }

    public void SetResetAfterPlay(bool value)
    {
        Globals.gameSettings.resetAfterPlay.value = value;
    }

    public void SetResetAfterGameplay(bool value)
    {
        Globals.gameSettings.resetAfterGameplay.value = value;
    }

    public void SetExtendedSustains(bool value)
    {
        Globals.gameSettings.extendedSustainsEnabled.value = value;
    }

    public void SetAutoValidateSongOnSave(bool value)
    {
        Globals.gameSettings.autoValidateSongOnSave.value = value;
    }

    public void SetSlowdownPitchCorrectionEnabled(bool value)
    {
        Globals.gameSettings.slowdownPitchCorrectionEnabled.value = value;
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
            Globals.gameSettings.clapProperties.value |= setting;
        else
            Globals.gameSettings.clapProperties.value &= ~setting;
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
        Globals.gameSettings.sustainGapIsTimeBased.value = value;

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

        Globals.gameSettings.sustainGapTimeMs.value = stepVal;
    }

    public void OnSustainGapTimeEndInput(string value)
    {
        OnSustainGapTimeInputUpdated(value);
        if (string.IsNullOrEmpty(value))
        {
            sustainGapTimeInput.text = "0";
        }
    }

    public void OnNewSongResolutionInputUpdated(string value)
    {
        
    }

    public void OnNewSongResolutionEndInput(string value)
    {
        OnNewSongResolutionInputUpdated(value);

        int textValue = 0;
        int.TryParse(newSongResolutionInputField.text, out textValue);

        textValue = Mathf.Max(textValue, (int)MoonscraperChartEditor.Song.SongConfig.STANDARD_BEAT_RESOLUTION);

        Globals.gameSettings.newSongResolution.value = textValue;

        newSongResolutionInputField.text = Globals.gameSettings.newSongResolution.ToString();
    }

    public void SetBgSwapTime(int value)
    {
        int swapTime = Globals.gameSettings.customBgSwapTime;
        switch (value)
        {
            case 0:
                swapTime = 10;
                break;
            case 1:
                swapTime = 30;
                break;
            case 2:
                swapTime = 60;
                break;
            case 3:
                swapTime = 180;
                break;
            case 4:
                swapTime = 300;
                break;
            case 5:
                swapTime = 600;
                break;
            default:
                break;
        }

        Globals.gameSettings.customBgSwapTime.value = swapTime;
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

    public void OpenLyricEditorSettings()
    {
        lyricEditorButton.onClick.Invoke();
        initialMenuItemSet = true;
    }

    public void SetLyricEditorStepSnappingEnabled(bool value)
    {
        Globals.gameSettings.lyricEditorSettings.stepSnappingEnabled = value;
    }

    public void OnLyricEditorPhaseEndTimeInputUpdated(string value)
    {
        float timeVal = 0;
        if (!string.IsNullOrEmpty(value))
        {
            timeVal = float.Parse(value);
        }

        Globals.gameSettings.lyricEditorSettings.phaseEndThreashold = timeVal;
    }

    public void OnLyricEditorPhaseEndTimeEndInput(string value)
    {
        OnLyricEditorPhaseEndTimeInputUpdated(value);
        if (string.IsNullOrEmpty(value))
        {
            lyricEditorPhaseEndTimeInput.text = "0";
        }
    }
}
