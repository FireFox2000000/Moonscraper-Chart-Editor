// Copyright (c) 2016-2017 Alexander Ong
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

    // Use this for initialization
    void Start () {
        editor = ChartEditor.Instance;
	}

    void Update()
    {
        fretSelectDropdown.gameObject.SetActive(!Globals.ghLiveMode);
        ghlFretSelectDropdown.gameObject.SetActive(!fretSelectDropdown.gameObject.activeSelf);

        if (!Services.IsTyping && !Globals.modifierInputActive)
            Shortcuts();
    }

    void Shortcuts()
    {
        if (ShortcutInput.GetInputDown(Shortcut.NoteSetNatural))
            setNoteNatural.onClick.Invoke();
        else if (ShortcutInput.GetInputDown(Shortcut.NoteSetStrum))
            setNoteStrum.onClick.Invoke();
        else if (ShortcutInput.GetInputDown(Shortcut.NoteSetHopo))
            setNoteHopo.onClick.Invoke();
        else if (ShortcutInput.GetInputDown(Shortcut.NoteSetTap))
            setNoteTap.onClick.Invoke();
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
        List<ChartObject> selected = new List<ChartObject>();

        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();
        
        foreach (ChartObject chartObject in editor.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note && chartObject.song != null) // check null in case note was already deleted when overwritten by changing a note before it
            {
                Note note = chartObject as Note;
                if (note.rawNote != noteNumber)
                {
                    Note newNote = new Note(note);
                    newNote.rawNote = noteNumber;

                    songEditCommands.Add(new SongEditModifyValidated(note, newNote));
                    selected.Add(newNote);
                }
            }
            else
                selected.Add(chartObject);
        }

        editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));

        List<ChartObject> actuallySelected = new List<ChartObject>();
        foreach (ChartObject chartObject in selected)
        {
            if (chartObject.classID == (int)SongObject.ID.Note && chartObject.song != null) // check null in case note was already deleted when overwritten by changing a note before it
            {
                Note note = chartObject as Note;
                int insertionIndex = SongObjectHelper.FindObjectPosition(note, editor.currentChart.notes);
                Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Song event failed to be inserted?");
                actuallySelected.Add(editor.currentChart.notes[insertionIndex]);
            }
            else
                actuallySelected.Add(chartObject);
        }
        editor.SetCurrentSelectedObjects(actuallySelected);
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
        uint songEndTick = editor.currentSong.TimeToTick(editor.currentSong.length, editor.currentSong.resolution);

        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();

        foreach (ChartObject chartObject in editor.currentSelectedObjects)
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

    public void SetNoteType(Note.NoteType type)
    {
        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();

        foreach (ChartObject chartObject in editor.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note)
            {
                Note note = chartObject as Note;
                Note newNote = new Note(note);
                newNote.flags = note.GetFlagsToSetType(type);
                songEditCommands.Add(new SongEditModify<Note>(note, newNote));
            }
        }

        if (songEditCommands.Count > 0)
            editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));
    }
}
