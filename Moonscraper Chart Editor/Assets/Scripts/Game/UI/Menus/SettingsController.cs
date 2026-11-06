// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsController : TabMenu
{
    class AudioTrackControl
    {
        public Slider slider;
        public Button muteButton;
        public Button soloButton;
    }

    readonly List<AudioTrackControl> audioTrackControls = new List<AudioTrackControl>();

    [SerializeField]
    RectTransform settingsMenuContentArea;

    [SerializeField]
    Button lyricEditorButton;

    public Toggle clapStrum;
    public Toggle clapHopo;
    public Toggle clapTap;
    public Toggle leftyFlipToggle;
    public Toggle extendedSustainsToggle;
    public Toggle openTapChordsToggle;
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

    void Start()
    {
        CreateAudioTrackControls();

        sustainGapInput.onValidateInput = Step.validateStepVal;
        sustainGapInput.text = Globals.gameSettings.sustainGap.ToString();
        sustainGapTimeInput.text = Globals.gameSettings.sustainGapTimeMs.ToString();
        lyricEditorPhaseEndTimeInput.text = Globals.gameSettings.lyricEditorSettings.phaseEndThreashold.ToString();
        lyricEditorPhaseEndTimeInput.onValidateInput = LocalesManager.ValidateDecimalInput;
    }

    public void SetSettingsGroup(RectTransform content)
    {
        SetTabGroup(content);
    }

    protected override void Awake()
    {
        base.Awake();

        menuContextArea = settingsMenuContentArea;
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

        {
            float phaseEndTimeInput = 0;
            bool parseSuccess = float.TryParse(lyricEditorPhaseEndTimeInput.text, out phaseEndTimeInput);
            if (!string.IsNullOrEmpty(lyricEditorPhaseEndTimeInput.text) && (phaseEndTimeInput != Globals.gameSettings.lyricEditorSettings.phaseEndThreashold || lyricEditorPhaseEndTimeInput.text.StartsWith(".")))
            {
                lyricEditorPhaseEndTimeInput.text = Globals.gameSettings.lyricEditorSettings.phaseEndThreashold.ToString();
            }
        }

        // Set all variables' values based on the UI
        Globals.gameSettings.sustainGapEnabled.value = sustainGapEnabledToggle.isOn;

        for (int i = 0; i < audioTrackControls.Count; ++i)
        {
            if (!IsAudioTrackMuted(i))
            {
                SetAudioTrackVolume(i, audioTrackControls[i].slider.value);
            }
        }

        //editor.clapSource.volume = clapSourceSlider.value;

        Globals.gameSettings.vol_master.value = masterVolumeSlider.value / 10.0f;
        AudioListener.volume = Globals.gameSettings.vol_master;
        Globals.gameSettings.audio_pan.value = musicPanSlider.value / 10.0f;

        Globals.gameSettings.gameplayStartDelayTime.value = gameplayStartDelayDropdown.value * 0.5f;

        UpdateAudioTrackControlVisuals();
        editor.ApplyAudioTrackVolumes();
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
        openTapChordsToggle.isOn = Globals.gameSettings.openChordsEnabled || Globals.gameSettings.tapOpensEnabled;

        // Make sure both open chords and tap open chords are set to the same value
        // This is because the game settings are separate but controlled by the same UI toggle
        Globals.gameSettings.openChordsEnabled.value = Globals.gameSettings.tapOpensEnabled.value = openTapChordsToggle.isOn;

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

    void CreateAudioTrackControls()
    {
        Slider[] trackSliders =
        {
            musicSourceSlider,
            guitarSourceSlider,
            bassSourceSlider,
            rhythmSourceSlider,
            drumSourceSlider,
            drum2SourceSlider,
            drum3SourceSlider,
            drum4SourceSlider,
            vocalSourceSlider,
            keysSourceSlider,
            crowdSourceSlider,
            sfxSlider
        };

        for (int i = 0; i < trackSliders.Length; ++i)
        {
            RectTransform sliderTransform = trackSliders[i].GetComponent<RectTransform>();
            RectTransform parent = sliderTransform.parent as RectTransform;
            float controlsStartX = sliderTransform.anchoredPosition.x + sliderTransform.rect.width * 0.5f + 32;

            AudioTrackControl control = new AudioTrackControl();
            control.slider = trackSliders[i];
            control.muteButton = CreateAudioButton(parent, "Mute", controlsStartX, sliderTransform.anchoredPosition.y, 52, 26);
            control.soloButton = CreateAudioButton(parent, "Solo", controlsStartX + 56, sliderTransform.anchoredPosition.y, 52, 26);

            int trackIndex = i;
            control.muteButton.onClick.AddListener(delegate { ToggleAudioTrackMute(trackIndex); });
            control.soloButton.onClick.AddListener(delegate { ToggleAudioTrackSolo(trackIndex); });
            audioTrackControls.Add(control);
        }

        RectTransform firstSliderTransform = musicSourceSlider.GetComponent<RectTransform>();
        RectTransform controlsParent = firstSliderTransform.parent as RectTransform;
        RectTransform lastSliderTransform = trackSliders[trackSliders.Length - 1].GetComponent<RectTransform>();
        float resetButtonX = lastSliderTransform.anchoredPosition.x;
        float resetButtonY = lastSliderTransform.anchoredPosition.y - 36;
        CreateAudioButton(controlsParent, "Reset Volumes", resetButtonX, resetButtonY, 112, 26)
            .onClick.AddListener(ResetAudioTrackVolumes);
        controlsParent.sizeDelta = new Vector2(
            controlsParent.sizeDelta.x,
            Mathf.Max(controlsParent.sizeDelta.y, -resetButtonY + 26));

        UpdateAudioTrackControlVisuals();
    }

    Button CreateAudioButton(RectTransform parent, string label, float x, float y, float width, float height)
    {
        GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
        buttonTransform.anchorMin = new Vector2(0, 1);
        buttonTransform.anchorMax = new Vector2(0, 1);
        buttonTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonTransform.anchoredPosition = new Vector2(x, y);
        buttonTransform.sizeDelta = new Vector2(width, height);

        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.28f, 0.4f, 1);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelTransform = labelObject.GetComponent<RectTransform>();
        labelTransform.anchorMin = Vector2.zero;
        labelTransform.anchorMax = Vector2.one;
        labelTransform.offsetMin = Vector2.zero;
        labelTransform.offsetMax = Vector2.zero;

        Text text = labelObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        text.raycastTarget = false;

        return button;
    }

    void ToggleAudioTrackMute(int trackIndex)
    {
        Globals.gameSettings.mutedAudioTracks[trackIndex] = !Globals.gameSettings.mutedAudioTracks[trackIndex];
        UpdateAudioTrackControlVisuals();
        editor.ApplyAudioTrackVolumes();
    }

    void ToggleAudioTrackSolo(int trackIndex)
    {
        Globals.gameSettings.soloAudioTracks[trackIndex] = !Globals.gameSettings.soloAudioTracks[trackIndex];
        UpdateAudioTrackControlVisuals();
        editor.ApplyAudioTrackVolumes();
    }

    void ResetAudioTrackVolumes()
    {
        for (int i = 0; i < audioTrackControls.Count; ++i)
        {
            Globals.gameSettings.mutedAudioTracks[i] = false;
            SetAudioTrackVolume(i, 1);
        }

        for (int i = 0; i < Globals.gameSettings.soloAudioTracks.Length; ++i)
        {
            Globals.gameSettings.soloAudioTracks[i] = false;
        }

        UpdateAudioTrackControlVisuals();
        editor.ApplyAudioTrackVolumes();
    }

    bool IsAudioTrackMuted(int trackIndex)
    {
        return Globals.gameSettings.IsAudioTrackMuted(trackIndex);
    }

    void UpdateAudioTrackControlVisuals()
    {
        for (int i = 0; i < audioTrackControls.Count; ++i)
        {
            AudioTrackControl control = audioTrackControls[i];
            bool muted = IsAudioTrackMuted(i);
            bool soloed = Globals.gameSettings.soloAudioTracks[i];

            control.slider.value = muted ? 0 : GetAudioTrackVolume(i);
            control.slider.interactable = !muted;
            SetAudioButtonState(control.muteButton, Globals.gameSettings.mutedAudioTracks[i], "Mute", "Unmute");
            SetAudioButtonState(control.soloButton, soloed, "Solo", "Unsolo");
        }
    }

    void SetAudioButtonState(Button button, bool active, string inactiveLabel, string activeLabel)
    {
        Text label = button.GetComponentInChildren<Text>();
        label.text = active ? activeLabel : inactiveLabel;
        button.GetComponent<Image>().color = active
            ? new Color(0.08f, 0.5f, 0.68f, 1)
            : new Color(0.08f, 0.28f, 0.4f, 1);
    }

    float GetAudioTrackVolume(int trackIndex)
    {
        switch (trackIndex)
        {
            case 0: return Globals.gameSettings.vol_song;
            case 1: return Globals.gameSettings.vol_guitar;
            case 2: return Globals.gameSettings.vol_bass;
            case 3: return Globals.gameSettings.vol_rhythm;
            case 4: return Globals.gameSettings.vol_drums;
            case 5: return Globals.gameSettings.vol_drums2;
            case 6: return Globals.gameSettings.vol_drums3;
            case 7: return Globals.gameSettings.vol_drums4;
            case 8: return Globals.gameSettings.vol_vocals;
            case 9: return Globals.gameSettings.vol_keys;
            case 10: return Globals.gameSettings.vol_crowd;
            case GameSettings.SfxAudioTrackIndex: return Globals.gameSettings.sfxVolume;
            default: throw new System.ArgumentOutOfRangeException("trackIndex");
        }
    }

    void SetAudioTrackVolume(int trackIndex, float volume)
    {
        switch (trackIndex)
        {
            case 0: Globals.gameSettings.vol_song.value = volume; break;
            case 1: Globals.gameSettings.vol_guitar.value = volume; break;
            case 2: Globals.gameSettings.vol_bass.value = volume; break;
            case 3: Globals.gameSettings.vol_rhythm.value = volume; break;
            case 4: Globals.gameSettings.vol_drums.value = volume; break;
            case 5: Globals.gameSettings.vol_drums2.value = volume; break;
            case 6: Globals.gameSettings.vol_drums3.value = volume; break;
            case 7: Globals.gameSettings.vol_drums4.value = volume; break;
            case 8: Globals.gameSettings.vol_vocals.value = volume; break;
            case 9: Globals.gameSettings.vol_keys.value = volume; break;
            case 10: Globals.gameSettings.vol_crowd.value = volume; break;
            case GameSettings.SfxAudioTrackIndex: Globals.gameSettings.sfxVolume = volume; break;
            default: throw new System.ArgumentOutOfRangeException("trackIndex");
        }
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

    public void SetOpenTapChords(bool value)
    {
        Globals.gameSettings.openChordsEnabled.value = value;
        Globals.gameSettings.tapOpensEnabled.value = value;
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
    }

    public void SetLyricEditorStepSnappingEnabled(bool value)
    {
        Globals.gameSettings.lyricEditorSettings.stepSnappingEnabled = value;
    }

    public void OnLyricEditorPhaseEndTimeInputUpdated(string value)
    {
        float timeVal = 0;
        if (!string.IsNullOrEmpty(value) && !float.TryParse(value, out timeVal))
        {
            return;
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
