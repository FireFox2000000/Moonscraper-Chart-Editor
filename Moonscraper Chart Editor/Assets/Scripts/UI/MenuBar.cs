using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBar : MonoBehaviour {
    ChartEditor editor;

    [SerializeField]
    Button playButton;
    [SerializeField]
    Button undoButton;
    [SerializeField]
    Button redoButton;
    [SerializeField]
    Toggle mouseModeToggle;

    public static Song.Instrument currentInstrument = Song.Instrument.Guitar;
    public static Song.Difficulty currentDifficulty = Song.Difficulty.Expert;

    // Use this for initialization
    void Start () {
        editor = ChartEditor.FindCurrentEditor();
    }
	
	// Update is called once per frame
	void Update () {
        playButton.interactable = (Globals.applicationMode != Globals.ApplicationMode.Playing);

        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            undoButton.interactable = editor.actionHistory.canUndo;
            redoButton.interactable = editor.actionHistory.canRedo;
        }
        else
        {
            undoButton.interactable = false;
            redoButton.interactable = false;
        }

        if (!Globals.IsTyping)
            Controls();
    }

    void Controls()
    {
        if (!Globals.modifierInputActive)
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                mouseModeToggle.isOn = !mouseModeToggle.isOn;
            }
        }
    }

    public void ToggleMouseLockMode(bool value)
    {
        Globals.lockToStrikeline = value;
        Debug.Log("Keys mode toggled " + value);
    }

    public void SetInstrument(string value)
    {
        switch (value.ToLower())
        {
            case ("guitar"):
                currentInstrument = Song.Instrument.Guitar;
                break;
            case ("guitarcoop"):
                currentInstrument = Song.Instrument.GuitarCoop;
                break;
            case ("bass"):
                currentInstrument = Song.Instrument.Bass;
                break;
            case ("keys"):
                currentInstrument = Song.Instrument.Keys;
                break;
            default:
                Debug.LogError("Invalid difficulty set: " + value);
                break;
        }
    }

    public void SetDifficulty(string value)
    {
        switch (value.ToLower())
        {
            case ("easy"):
                currentDifficulty = Song.Difficulty.Easy;
                break;
            case ("medium"):
                currentDifficulty = Song.Difficulty.Medium;
                break;
            case ("hard"):
                currentDifficulty = Song.Difficulty.Hard;
                break;
            case ("expert"):
                currentDifficulty = Song.Difficulty.Expert;
                break;
            default:
                Debug.LogError("Invalid difficulty set: " + value);
                break;
        }
    }

    public void LoadCurrentInstumentAndDifficulty()
    {
        editor.LoadChart(editor.currentSong.GetChart(currentInstrument, currentDifficulty));
    }
}
