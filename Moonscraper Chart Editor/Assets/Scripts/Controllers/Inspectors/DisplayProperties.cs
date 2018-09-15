// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DisplayProperties : UpdateableService
{
    public Text songNameText;
    public Slider hyperspeedSlider;
    public InputField snappingStep;
    public Text noteCount;
    public Text gameSpeed;
    public Slider gameSpeedSlider;
    public Toggle clapToggle; 
    public Toggle metronomeToggle;
    public Slider highwayLengthSlider;
    public Transform maxHighwayLength;
    public float minHighwayLength = 11.75f;
    [SerializeField]
    BGFadeHeightController bgFade;

    ChartEditor editor;
    int prevNoteCount = -1;
    int prevSnappingStep = 16;

    protected override void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        hyperspeedSlider.value = GameSettings.hyperspeed;
        highwayLengthSlider.value = GameSettings.highwayLength;

        snappingStep.onValidateInput = Step.validateStepVal;
        UpdateSnappingStepText();

        OnEnable();

        EventsManager.onChartReloadEventList.Add(OnChartReload);
        EventsManager.onApplicationModeChangedEventList.Add(OnApplicationModeChanged);

        OnChartReload();

        base.Start();
    }

    void OnEnable()
    {
        clapToggle.isOn = (GameSettings.clapSetting != GameSettings.ClapToggle.NONE);
        metronomeToggle.isOn = GameSettings.metronomeActive;
    }

    public override void OnServiceUpdate()
    {
        if (editor.currentChart.note_count != prevNoteCount)
            noteCount.text = "Notes: " + editor.currentChart.note_count.ToString();

        if (GameSettings.step != prevSnappingStep)
            UpdateSnappingStepText();

        // Shortcuts
        if (ShortcutInput.GetInputDown(Shortcut.ToggleClap))
            clapToggle.isOn = !clapToggle.isOn;

        prevNoteCount = editor.currentChart.note_count;
    }

    void OnChartReload()
    {
        UpdateSongNameText();
    }

    public void UpdateSongNameText()
    {
        songNameText.text = editor.currentSong.name + " - " + editor.currentChart.name;
    }

    void OnApplicationModeChanged(Globals.ApplicationMode applicationMode)
    {
        bool interactable = (applicationMode != Globals.ApplicationMode.Playing);
        hyperspeedSlider.interactable = interactable;
        gameSpeedSlider.interactable = interactable;
        highwayLengthSlider.interactable = interactable;
    }

    public void SetHyperspeed(float value)
    {
        GameSettings.hyperspeed = value;
        EventsManager.FireHyperspeedChangeEvent();
    }

    public void SetGameSpeed(float value)
    {
        value = Mathf.Round(value / 5.0f) * 5;
        GameSettings.gameSpeed = value / 100.0f;
        gameSpeed.text = "Speed- x" + GameSettings.gameSpeed.ToString();

        EventsManager.FireHyperspeedChangeEvent();
    }

    public void SetHighwayLength(float value)
    {
        GameSettings.highwayLength = value;

        Vector3 pos = Vector3.zero;
        pos.y = value * 5 + minHighwayLength;
        maxHighwayLength.transform.localPosition = pos;

        bgFade.AdjustHeight();

        EventsManager.FireHyperspeedChangeEvent();
    }

    public void ToggleClap(bool value)
    {
        if (value)
            GameSettings.clapSetting = GameSettings.clapProperties;
        else
            GameSettings.clapSetting = GameSettings.ClapToggle.NONE;

        Debug.Log("Clap toggled: " + value);
    }

    public void ToggleMetronome(bool value)
    {
        GameSettings.metronomeActive = value;

        Debug.Log("Metronome toggled: " + value);
    }

    public void IncrementSnappingStep()
    {
        GameSettings.snappingStep.Increment();
        UpdateSnappingStepText();
    }

    public void DecrementSnappingStep()
    {
        GameSettings.snappingStep.Decrement();
        UpdateSnappingStepText();
    }

    void UpdateSnappingStepText()
    {
        snappingStep.text = GameSettings.step.ToString();
        prevSnappingStep = GameSettings.step;
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

        GameSettings.step = stepVal;
        UpdateSnappingStepText();
    }
}
