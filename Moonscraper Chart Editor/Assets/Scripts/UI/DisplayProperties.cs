using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayProperties : MonoBehaviour {
    public Text stepText;
    public Text songNameText;
    public Slider hyperspeedSlider;

    ChartEditor editor;

    void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        hyperspeedSlider.value = Globals.hyperspeed;
    }

    void Update()
    {
        stepText.text = "1/" + Globals.step.ToString();
        songNameText.text = editor.currentSong.name;
        Globals.hyperspeed = hyperspeedSlider.value;

        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
            hyperspeedSlider.interactable = false;
        else
            hyperspeedSlider.interactable = true;
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
}
