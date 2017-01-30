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

    ChartEditor editor;

    void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        hyperspeedSlider.value = Globals.hyperspeed;

        snappingStep.onValidateInput = Step.validateStepVal;
        snappingStep.text = Globals.step.ToString();
    }

    void Update()
    {
        songNameText.text = editor.currentSong.name;
        Globals.hyperspeed = hyperspeedSlider.value;

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

        // Speed slider snapping
        gameSpeedSlider.value = Mathf.Round(gameSpeedSlider.value / 5.0f) * 5;
        Globals.gameSpeed = gameSpeedSlider.value / 100.0f;

        // if (Time.timeScale < 1)
        gameSpeed.text = "Speed- x" + Globals.gameSpeed.ToString();//.Substring(0, 3);
      //  else
        //    gameSpeed.text = "Speed- x1.0";
    }

    public void ToggleClap(bool value)
    {
        if (value)
            Globals.clapSetting = Globals.clapProperties;
        else
            Globals.clapSetting = Globals.ClapToggle.NONE;
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
