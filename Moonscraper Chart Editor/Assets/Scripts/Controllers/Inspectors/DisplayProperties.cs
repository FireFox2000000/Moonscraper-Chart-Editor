using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayProperties : MonoBehaviour {
    public Text songNameText;
    public Slider hyperspeedSlider;
    public InputField snappingStep;
    public Text noteCount;
    public Text gameSpeed;
    public Slider gameSpeedSlider;
    public Toggle clapToggle; 
    public Toggle metronomeToggle;

    ChartEditor editor;

    void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        hyperspeedSlider.value = Globals.hyperspeed;

        snappingStep.onValidateInput = Step.validateStepVal;
        snappingStep.text = Globals.step.ToString();

        OnEnable();
    }

    void OnEnable()
    {
        clapToggle.isOn = (Globals.clapSetting != Globals.ClapToggle.NONE);
        metronomeToggle.isOn = Globals.metronomeActive;
    }

    void Update()
    {
        songNameText.text = editor.currentSong.name + " - " + editor.currentChart.name;

        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            hyperspeedSlider.interactable = false;
            gameSpeedSlider.interactable = false;
        }
        else
        {
            hyperspeedSlider.interactable = true;
            gameSpeedSlider.interactable = true;
        }

        if (snappingStep.text != string.Empty)
            snappingStep.text = Globals.step.ToString();

        noteCount.text = "Notes: " + editor.currentChart.note_count.ToString();

        // Shortcuts
        if (!Globals.modifierInputActive && !Globals.IsTyping)
        {
            if (Input.GetButtonDown("ToggleClap"))
                clapToggle.isOn = !clapToggle.isOn;

            if (Input.GetButtonDown("Toggle Metronome"))
                metronomeToggle.isOn = !metronomeToggle.isOn;
        }
    }

    public void SetHyperspeed(float value)
    {
        Globals.hyperspeed = value;
    }

    public void SetGameSpeed(float value)
    {
        value = Mathf.Round(value / 5.0f) * 5;
        Globals.gameSpeed = value / 100.0f;
        gameSpeed.text = "Speed- x" + Globals.gameSpeed.ToString();
    }

    public void ToggleClap(bool value)
    {
        if (value)
            Globals.clapSetting = Globals.clapProperties;
        else
            Globals.clapSetting = Globals.ClapToggle.NONE;

        Debug.Log("Clap toggled: " + value);
    }

    public void ToggleMetronome(bool value)
    {
        Globals.metronomeActive = value;

        Debug.Log("Metronome toggled: " + value);
    }

    public void IncrementSnappingStep()
    {
        Globals.snappingStep.Increment();
    }

    public void DecrementSnappingStep()
    {
        Globals.snappingStep.Decrement();
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

        Globals.step = stepVal;
        snappingStep.text = Globals.step.ToString();
    }
}
