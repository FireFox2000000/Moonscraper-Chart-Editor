using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBar : MonoBehaviour {
    ChartEditor editor;

    [Header("Disableable UI")]
    [SerializeField]
    Button undoButton;
    [SerializeField]
    Button redoButton;

    [Header("Shortcuts")]
    [SerializeField]
    Toggle mouseModeToggle;

    [Header("Preview Mode")]
    [SerializeField]
    GameObject[] disableDuringPreview;

    public static Song.Instrument currentInstrument = Song.Instrument.Guitar;
    public static Song.Difficulty currentDifficulty = Song.Difficulty.Expert;

    // Use this for initialization
    void Start () {
        editor = ChartEditor.FindCurrentEditor();
    }
	
	// Update is called once per frame
	void Update () {
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
        try
        {
            currentInstrument = (Song.Instrument)System.Enum.Parse(typeof(Song.Instrument), value, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Invalid instrument set: " + value);
        }
    }

    public void SetDifficulty(string value)
    {
        try
        {
            currentDifficulty = (Song.Difficulty)System.Enum.Parse(typeof(Song.Difficulty), value, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Invalid difficulty set: " + value);
        }
    }

    public void LoadCurrentInstumentAndDifficulty()
    {
        editor.LoadChart(editor.currentSong.GetChart(currentInstrument, currentDifficulty));
    }

    public static bool previewing { get { return _previewing; } }
    static bool _previewing = false;
    float initialPosition = 0;
    public void PreviewToggle()
    {
        _previewing = !_previewing;

        // Disable UI
        foreach(GameObject go in disableDuringPreview)
        {
            go.SetActive(!_previewing);
        }

        // Set chart view mode
        Globals.viewMode = Globals.ViewMode.Chart;

        if (_previewing)
        {
            // Record the current position
            initialPosition = editor.visibleStrikeline.position.y;

            // Move to the start of the chart
            editor.movement.SetTime(-3);

            // Play
            editor.Play();
        }
        else
        {
            // Stop
            editor.Stop();

            // Restore to initial position
            editor.movement.SetTime(Song.WorldYPositionToTime(initialPosition));
        }
    }
}
