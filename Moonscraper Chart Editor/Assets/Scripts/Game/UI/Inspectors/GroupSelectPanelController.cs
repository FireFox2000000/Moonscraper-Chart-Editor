// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class GroupSelectPanelController : MonoBehaviour
{
    ChartEditor editor;
    [SerializeField]
    Dropdown fretSelectDropdown;
    [SerializeField]
    Dropdown drumsFretSelectDropdown;
    [SerializeField]
    Dropdown ghlFretSelectDropdown;
    [SerializeField]
    Dropdown drums4LaneSelectDropdown;
    [SerializeField]
    Button setNoteNatural;
    [SerializeField]
    Button setNoteStrum;
    [SerializeField]
    Button setNoteHopo;
    [SerializeField]
    Button setNoteTap;
    [SerializeField]
    Button setNoteCymbal;
    [SerializeField]
    Button setDoubleKick;
    [SerializeField]
    Button altDoubleKick;

    Dictionary<Chart.GameMode, Dropdown> laneSelectForGamemodeLookup = new Dictionary<Chart.GameMode, Dropdown>();
    Dictionary<Chart.GameMode, Dictionary<int, Dropdown>> laneSelectLaneCountOverrideLookup = new Dictionary<Chart.GameMode, Dictionary<int, Dropdown>>();
    Dropdown currentFretSelector = null;

    // Use this for initialization
    void Start () {
        //fretSelectDropdown = transform.Find("Fret Select").GetComponent<Dropdown>();

        // Setup lane selector dictionaries and hide all selector varients
        {
            laneSelectForGamemodeLookup[Chart.GameMode.Guitar] = fretSelectDropdown;
            laneSelectForGamemodeLookup[Chart.GameMode.Drums] = drumsFretSelectDropdown;
            laneSelectForGamemodeLookup[Chart.GameMode.GHLGuitar] = ghlFretSelectDropdown;

            var drumsOverrideLaneSelectDict = new Dictionary<int, Dropdown>();
            drumsOverrideLaneSelectDict[4] = drums4LaneSelectDropdown;
            laneSelectLaneCountOverrideLookup[Chart.GameMode.Drums] = drumsOverrideLaneSelectDict;

            currentFretSelector = laneSelectForGamemodeLookup[Chart.GameMode.Guitar];

            foreach (var dropKeyVal in laneSelectForGamemodeLookup)
            {
                dropKeyVal.Value.gameObject.SetActive(false);
            }

            foreach (var overrideKeyVal in laneSelectLaneCountOverrideLookup)
            {
                foreach (var dropKeyVal in overrideKeyVal.Value)
                {
                    dropKeyVal.Value.gameObject.SetActive(false);
                }
            }
        }

        editor = ChartEditor.Instance;
        editor.events.chartReloadedEvent.Register(UpdateUIActiveness);
        editor.events.lanesChangedEvent.Register(OnLanesChanged);
        editor.events.drumsModeOptionChangedEvent.Register(UpdateUIActiveness);

        UpdateUIActiveness();

    }

    Dropdown GetCurrentFretSelector(Chart.GameMode gameMode, int laneCount)
    {
        Dropdown dropdown = fretSelectDropdown;

        Dictionary<int, Dropdown> overrideLookup;
        if (!(laneSelectLaneCountOverrideLookup.TryGetValue(gameMode, out overrideLookup) && overrideLookup.TryGetValue(laneCount, out dropdown)))
        {
            // No overrides present, go with the defaults
            laneSelectForGamemodeLookup.TryGetValue(gameMode, out dropdown);
        }

        return dropdown;
    }

    void Update()
    {
        if (!Services.IsTyping && !Globals.modifierInputActive)
            Shortcuts();
    }

    void OnLanesChanged(in int laneCount)
    {
        UpdateUIActiveness();
    }

    void UpdateUIActiveness()
    {
        currentFretSelector.gameObject.SetActive(false);
        currentFretSelector = GetCurrentFretSelector(editor.currentGameMode, editor.laneInfo.laneCount);
        currentFretSelector.gameObject.SetActive(true);

        bool drumsMode = Globals.drumMode;
        bool proDrumsMode = drumsMode && Globals.gameSettings.drumsModeOptions == GameSettings.DrumModeOptions.ProDrums;
        bool doubleKickActive = proDrumsMode && ChartEditor.Instance.currentDifficulty == Song.Difficulty.Expert;
        setNoteStrum.gameObject.SetActive(!drumsMode);
        setNoteHopo.gameObject.SetActive(!drumsMode);
        setNoteTap.gameObject.SetActive(!drumsMode);
        setNoteCymbal.gameObject.SetActive(proDrumsMode);
        setDoubleKick.gameObject.SetActive(doubleKickActive);
        altDoubleKick.gameObject.SetActive(doubleKickActive);
    }

    void Shortcuts()
    {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.NoteSetNatural))
            setNoteNatural.onClick.Invoke();
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.NoteSetStrum))
            setNoteStrum.onClick.Invoke();
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.NoteSetHopo))
            setNoteHopo.onClick.Invoke();
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.NoteSetTap))
            setNoteTap.onClick.Invoke();
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.NoteSetCymbal))
            setNoteCymbal.onClick.Invoke();
    }

    int GetOpenNoteForGameMode(Chart.GameMode gameMode)
    {
        int rawNoteValue = 0;
        switch (gameMode)
        {
            case Chart.GameMode.Guitar:
                {
                    rawNoteValue = (int)Note.GuitarFret.Open;
                    break;
                }
            case Chart.GameMode.Drums:
                {
                    rawNoteValue = (int)Note.DrumPad.Kick;
                    break;
                }
            case Chart.GameMode.GHLGuitar:
                {
                    rawNoteValue = (int)Note.GHLiveGuitarFret.Open;
                    break;
                }
            default:
                {
                    Debug.Assert(false, "Unhandled open note selection for gamemode " + editor.currentChart.gameMode);
                    break;
                }
        }

        return rawNoteValue;
    }

    public void ApplyFretDropdownSelection()
    {
        Dropdown activeDropDown = currentFretSelector;

        int totalLanesPlusOpen = editor.laneInfo.laneCount + 1;
        if (activeDropDown && activeDropDown.value >= 0 && activeDropDown.value < totalLanesPlusOpen)
        {
            int rawNoteValue = activeDropDown.value;
            if (activeDropDown.value == editor.laneInfo.laneCount)
            {
                // Set to be the open note
                rawNoteValue = GetOpenNoteForGameMode(editor.currentGameMode);
            }

            SetFretType(rawNoteValue);
        }
    }

    public void SetFretType(int noteNumber)
    {
        List<SongObject> selected = new List<SongObject>();

        List<SongEditCommand> deleteCommands = new List<SongEditCommand>();
        List<SongEditCommand> addCommands = new List<SongEditCommand>();
        
        foreach (ChartObject chartObject in editor.selectedObjectsManager.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note && chartObject.song != null) // check null in case note was already deleted when overwritten by changing a note before it
            {
                Note note = chartObject as Note;
                if (note.rawNote != noteNumber)
                {
                    Note newNote = new Note(note);
                    newNote.rawNote = noteNumber;

                    deleteCommands.Add(new SongEditDelete(note));
                    addCommands.Add(new SongEditAdd(newNote));
                    selected.Add(newNote);
                }
            }
            else
                selected.Add(chartObject);
        }

        // Delete commands must come first, as add commands can overwrite notes we might try to delete later
        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();
        songEditCommands.AddRange(deleteCommands);
        songEditCommands.AddRange(addCommands);

        editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));
        editor.selectedObjectsManager.TryFindAndSelectSongObjects(selected);
    }

    public void SetZeroSustain()
    {
        SetSustain(0);
    }

    public void SetMaxSustain()
    {
        SetSustain(uint.MaxValue);
    }

    void SetSustain(uint length)
    {
        uint songEndTick = editor.currentSong.TimeToTick(editor.currentSongLength, editor.currentSong.resolution);

        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();

        foreach (ChartObject chartObject in editor.selectedObjectsManager.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note)
            {
                Note note = chartObject as Note;
                uint assignedLength = length;
                if (length == uint.MaxValue)
                    assignedLength = songEndTick - note.tick;

                songEditCommands.Add(new SongEditModifyValidated(note, new Note(note.tick, note.rawNote, assignedLength, note.flags)));
            }
        }

        if (songEditCommands.Count > 0)
            editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));
    }

    public void SetNatural()
    {
        SetNoteType(Note.NoteType.Natural);
    }

    public void SetStrum()
    {
        SetNoteType(Note.NoteType.Strum);
    }

    public void SetHopo()
    {
        SetNoteType(Note.NoteType.Hopo);
    }

    public void SetTap()
    {
        SetNoteType(Note.NoteType.Tap);
    }

    public void SetCymbal()
    {
        SetNoteType(Note.NoteType.Cymbal);
    }

    public void SetNoteType(Note.NoteType type)
    {
        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();
        List<ChartObject> objectsToSelect = new List<ChartObject>();

        foreach (ChartObject chartObject in editor.selectedObjectsManager.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note)
            {
                Note note = chartObject as Note;
                Note newNote = new Note(note);
                newNote.flags = note.GetFlagsToSetType(type);
                songEditCommands.Add(new SongEditModifyValidated(note, newNote));
                objectsToSelect.Add(newNote);
            }
        }

        if (songEditCommands.Count > 0)
        {
            editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));
        }
    }

    public void SetDoubleKick()
    {
        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();

        foreach (ChartObject chartObject in editor.selectedObjectsManager.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note)
            {
                Note note = chartObject as Note;
                if (note.IsOpenNote() && (note.flags & Note.Flags.DoubleKick) == 0)
                {
                    Note newNote = new Note(note);
                    newNote.flags |= Note.Flags.DoubleKick;

                    songEditCommands.Add(new SongEditModifyValidated(note, newNote));
                }
            }
        }

        if (songEditCommands.Count > 0)
        {
            editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));
        }
    }

    public void AlternateDoubleKick()
    {
        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();

        bool applyDoubleKick = false;
        foreach (ChartObject chartObject in editor.selectedObjectsManager.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note)
            {
                Note note = chartObject as Note;
                if (note.IsOpenNote())
                {
                    Note newNote = new Note(note);

                    if (applyDoubleKick)
                    {
                        newNote.flags |= Note.Flags.DoubleKick;
                    }
                    else
                    {
                        newNote.flags &= ~Note.Flags.DoubleKick;
                    }

                    songEditCommands.Add(new SongEditModifyValidated(note, newNote));
                    applyDoubleKick = !applyDoubleKick;
                }
            }
        }

        if (songEditCommands.Count > 0)
        {
            editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));
        }
    }
}
