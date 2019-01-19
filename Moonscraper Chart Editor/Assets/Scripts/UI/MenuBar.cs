// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBar : UpdateableService {
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

    [Header("Misc")]
    [SerializeField]
    Button gameplayButton;

    public static Song.Instrument currentInstrument = Song.Instrument.Guitar;
    public static Song.Difficulty currentDifficulty = Song.Difficulty.Expert;

    int desiredLaneCount = 0;

    // Use this for initialization
    protected override void Start () {
        editor = ChartEditor.Instance;

        EventsManager.onChartReloadEventList.Add(GameplayEnabledCheck);

        base.Start();
    }

    // Update is called once per frame
    public override void OnServiceUpdate()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            undoButton.interactable = !editor.commandStack.isAtStart;
            redoButton.interactable = !editor.commandStack.isAtEnd;
        }
        else
        {
            undoButton.interactable = false;
            redoButton.interactable = false;
        }

        if (!Services.IsTyping)
            Controls();
    }

    void Controls()
    {
        if (ShortcutInput.GetInputDown(Shortcut.ToggleExtendedSustains))
        {
            ToggleExtendedSustains();
            editor.globals.services.notificationBar.PushNotification("EXTENDED SUSTAINS TOGGLED " + Services.BoolToStrOnOff(GameSettings.extendedSustainsEnabled), 2, true);
        }

        else if (ShortcutInput.GetInputDown(Shortcut.ToggleMouseMode))
        {
            ToggleMouseLockMode();
            editor.globals.services.notificationBar.PushNotification("KEYS MODE TOGGLED " + Services.BoolToStrOnOff(GameSettings.keysModeEnabled), 2, true);
        }
    }

    public void ToggleMouseLockMode(bool value)
    {
        editor.services.SetKeysMode(value);
        Debug.Log("Keys mode toggled " + value);
    }

    public void ToggleMouseLockMode()
    {
        editor.services.SetKeysMode(!GameSettings.keysModeEnabled);
        Debug.Log("Keys mode toggled " + GameSettings.keysModeEnabled);
    }

    public void ToggleExtendedSustains()
    {
        GameSettings.extendedSustainsEnabled = !GameSettings.extendedSustainsEnabled;
        Debug.Log("Extended sustains toggled " + GameSettings.extendedSustainsEnabled);
    }

    public void ToggleMetronome()
    {
        GameSettings.metronomeActive = !GameSettings.metronomeActive;
        Debug.Log("Metronome toggled " + GameSettings.metronomeActive);
    }

    public void SetInstrument(string value)
    {
        try
        {
            currentInstrument = (Song.Instrument)System.Enum.Parse(typeof(Song.Instrument), value, true); 
        }
        catch
        {
            Debug.LogError("Invalid instrument set: " + value);
        }

        desiredLaneCount = 0;
    }

    public void SetDifficulty(string value)
    {
        try
        {
            currentDifficulty = (Song.Difficulty)System.Enum.Parse(typeof(Song.Difficulty), value, true);
        }
        catch
        {
            Debug.LogError("Invalid difficulty set: " + value);
        }
    }

    public void SetLaneCount(int value)
    {
        desiredLaneCount = value;
    }

    public void LoadCurrentInstumentAndDifficulty()
    {
        if (!editor)
            editor = ChartEditor.Instance;

        editor.LoadChart(editor.currentSong.GetChart(currentInstrument, currentDifficulty));
        editor.currentSelectedObject = null;

        EventsManager.FireChartReloadedEvent();

        if (desiredLaneCount > 0)
            editor.laneInfo.laneCount = desiredLaneCount;
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
            editor.movement.SetTime(TickFunctions.WorldYPositionToTime(initialPosition));
        }
    }

    void GameplayEnabledCheck()
    {
        gameplayButton.gameObject.SetActive(!(Globals.ghLiveMode));
    }
}
