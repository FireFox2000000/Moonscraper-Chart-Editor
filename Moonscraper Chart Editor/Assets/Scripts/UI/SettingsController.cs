using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsController : DisplayMenu
{
    public Toggle clapOnOff;
    public Toggle clapStrum;
    public Toggle clapHopo;
    public Toggle clapTap;
    public Toggle extendedSustainsToggle;
    public Toggle sustainGapEnabledToggle;

    public Slider musicSourceSlider;
    public Slider guitarSourceSlider;
    public Slider rhythmSourceSlider;
    public Slider clapSourceSlider;

    public InputField sustainGapInput;

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

        //sustainGapText.text = "1/" + Globals.sustainGap.ToString();
        if (sustainGapInput.text != string.Empty)
            sustainGapInput.text = Globals.sustainGap.ToString();

        Globals.sustainGapEnabled = sustainGapEnabledToggle.isOn;

        editor.musicSources[ChartEditor.MUSIC_STREAM_ARRAY_POS].volume = musicSourceSlider.value;
        editor.musicSources[ChartEditor.GUITAR_STREAM_ARRAY_POS].volume = guitarSourceSlider.value;
        editor.musicSources[ChartEditor.RHYTHM_STREAM_ARRAY_POS].volume = rhythmSourceSlider.value;

        editor.clapSource.volume = clapSourceSlider.value;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Initialise GUI
        sustainGapEnabledToggle.isOn = Globals.sustainGapEnabled;

        initClapToggle(clapStrum, Globals.ClapToggle.STRUM);
        initClapToggle(clapHopo, Globals.ClapToggle.HOPO);
        initClapToggle(clapTap, Globals.ClapToggle.TAP);

        musicSourceSlider.value = editor.musicSources[ChartEditor.MUSIC_STREAM_ARRAY_POS].volume;
        guitarSourceSlider.value = editor.musicSources[ChartEditor.GUITAR_STREAM_ARRAY_POS].volume;
        rhythmSourceSlider.value = editor.musicSources[ChartEditor.RHYTHM_STREAM_ARRAY_POS].volume;

        clapSourceSlider.value = editor.clapSource.volume;

        if (Globals.extendedSustainsEnabled)
            extendedSustainsToggle.isOn = true;
        else
            extendedSustainsToggle.isOn = false;

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
