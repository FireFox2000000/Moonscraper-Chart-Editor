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
        editor = ChartEditor.Instance;
        hyperspeedSlider.value = GameSettings.hyperspeed;
        highwayLengthSlider.value = GameSettings.highwayLength;

        UpdateGameSpeedText();

        snappingStep.onValidateInput = Step.validateStepVal;
        UpdateSnappingStepText();

        OnEnable();

        editor.events.chartReloadedEvent.Register(OnChartReload);
        editor.events.editorStateChangedEvent.Register(OnApplicationModeChanged);

        OnChartReload();

        base.Start();
    }

    void OnEnable()
    {
        clapToggle.isOn = GameSettings.clapEnabled;
        metronomeToggle.isOn = GameSettings.metronomeActive;
    }

    public override void OnServiceUpdate()
    {
        if (editor.currentChart.note_count != prevNoteCount)
            noteCount.text = "Notes: " + editor.currentChart.note_count.ToString();

        if (GameSettings.step != prevSnappingStep)
            UpdateSnappingStepText();

        // Shortcuts
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleClap))
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

    void OnApplicationModeChanged(in ChartEditor.State editorState)
    {
        bool interactable = (editorState != ChartEditor.State.Playing);
        hyperspeedSlider.interactable = interactable;
        gameSpeedSlider.interactable = interactable;
        highwayLengthSlider.interactable = interactable;
    }

    public void SetHyperspeed(float value)
    {
        GameSettings.hyperspeed = value;
        editor.events.hyperspeedChangeEvent.Fire();
    }

    public void SetGameSpeed(float value)
    {
        value = Mathf.Round(value / 5.0f) * 5;
        GameSettings.gameSpeed = value / 100.0f;
        UpdateGameSpeedText();

        editor.events.hyperspeedChangeEvent.Fire();
    }

    void UpdateGameSpeedText()
    {
        gameSpeed.text = string.Format("Speed- x{0:0.00}", GameSettings.gameSpeed);
    }

    public void SetHighwayLength(float value)
    {
        GameSettings.highwayLength = value;

        Vector3 pos = Vector3.zero;
        pos.y = value * 5 + minHighwayLength;
        maxHighwayLength.transform.localPosition = pos;

        bgFade.AdjustHeight();

        editor.events.hyperspeedChangeEvent.Fire();
    }

    public void ToggleClap(bool value)
    {
        GameSettings.clapEnabled = value;

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
