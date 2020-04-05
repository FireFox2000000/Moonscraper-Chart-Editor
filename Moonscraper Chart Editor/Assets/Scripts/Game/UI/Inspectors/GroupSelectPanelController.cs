// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupSelectPanelController : MonoBehaviour
{
    ChartEditor editor;
    [SerializeField]
    Dropdown fretSelectDropdown;
    [SerializeField]
    Dropdown ghlFretSelectDropdown;
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

    // Use this for initialization
    void Start () {
        editor = ChartEditor.Instance;
        editor.events.chartReloadedEvent.Register(UpdateUIActiveness);
        editor.events.drumsModeOptionChangedEvent.Register(UpdateUIActiveness);

        UpdateUIActiveness();

    }

    void Update()
    {
        if (!Services.IsTyping && !Globals.modifierInputActive)
            Shortcuts();
    }

    void UpdateUIActiveness()
    {
        fretSelectDropdown.gameObject.SetActive(!Globals.ghLiveMode);
        ghlFretSelectDropdown.gameObject.SetActive(!fretSelectDropdown.gameObject.activeSelf);

        bool drumsMode = Globals.drumMode;
        setNoteStrum.gameObject.SetActive(!drumsMode);
        setNoteHopo.gameObject.SetActive(!drumsMode);
        setNoteTap.gameObject.SetActive(!drumsMode);
        setNoteCymbal.gameObject.SetActive(drumsMode && GameSettings.drumsModeOptions == GameSettings.DrumModeOptions.ProDrums);
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

    public void ApplyFretDropdownSelection()
    {
        if (fretSelectDropdown.gameObject.activeSelf && fretSelectDropdown.value >= 0 && fretSelectDropdown.value < 6)
        {
            SetFretType(fretSelectDropdown.value);
        }
        else if (ghlFretSelectDropdown.gameObject.activeSelf && ghlFretSelectDropdown.value >= 0 && ghlFretSelectDropdown.value < 7)
        {
            SetFretType(ghlFretSelectDropdown.value);
        }
    }

    public void SetFretType(int noteNumber)
    {
        List<SongObject> selected = new List<SongObject>();

        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();
        
        foreach (ChartObject chartObject in editor.selectedObjectsManager.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note && chartObject.song != null) // check null in case note was already deleted when overwritten by changing a note before it
            {
                Note note = chartObject as Note;
                if (note.rawNote != noteNumber)
                {
                    Note newNote = new Note(note);
                    newNote.rawNote = noteNumber;

                    songEditCommands.Add(new SongEditDelete(note));
                    songEditCommands.Add(new SongEditAdd(newNote));
                    selected.Add(newNote);
                }
            }
            else
                selected.Add(chartObject);
        }

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
}
