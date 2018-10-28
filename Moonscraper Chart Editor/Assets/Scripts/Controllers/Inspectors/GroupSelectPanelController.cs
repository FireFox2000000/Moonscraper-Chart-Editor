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
        List<ActionHistory.Action> actions = new List<ActionHistory.Action>();
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

                    songEditCommands.Add(new SongEditModify<Note>(note, newNote));
                    selected.Add(newNote);
                }
            }
            else
                selected.Add(chartObject);
        }

        editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));

        ChartEditor.isDirty = true;

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
        List<ActionHistory.Action> actions = new List<ActionHistory.Action>();
        uint songEndTick = editor.currentSong.TimeToTick(editor.currentSong.length, editor.currentSong.resolution);

        foreach (ChartObject chartObject in editor.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note)
            {
                Note note = chartObject as Note;
                uint assignedLength = length;
                if (length == uint.MaxValue)
                    assignedLength = songEndTick - note.tick;

                if (GameSettings.extendedSustainsEnabled)
                {
                    Note original = (Note)note.Clone();
                    note.length = assignedLength;          
                    note.CapSustain(note.FindNextSameFretWithinSustainExtendedCheck());

                    if (original.length != note.length)
                        actions.Add(new ActionHistory.Modify(original, note));
                }
                else
                {
                    // Needs to handle chords
                    Note[] chordNotes = note.GetChord();
                    Note[] chordNotesCopy = new Note[chordNotes.Length];
                    Note capNote = note.nextSeperateNote;

                    for (int i = 0; i < chordNotes.Length; ++i)
                    {
                        chordNotesCopy[i] = (Note)chordNotes[i].Clone();
                        chordNotes[i].length = assignedLength;
                        chordNotes[i].CapSustain(capNote);

                        if (chordNotesCopy[i].length != chordNotes[i].length)
                            actions.Add(new ActionHistory.Modify(chordNotesCopy[i], chordNotes[i]));
                    }
                }
            }
        }

        if (actions.Count > 0)
            editor.actionHistory.Insert(actions.ToArray());

        ChartEditor.isDirty = true;
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
        List<ActionHistory.Action> actions = new List<ActionHistory.Action>();

        foreach (ChartObject chartObject in editor.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note)
            {
                Note note = chartObject as Note;

                // Need to record the whole chord
                Note unmodified = (Note)note.Clone();
                Note[] chord = note.GetChord();

                ActionHistory.Action[] deleteRecord = new ActionHistory.Action[chord.Length];
                for (int i = 0; i < deleteRecord.Length; ++i)
                    deleteRecord[i] = new ActionHistory.Delete(chord[i]);

                note.SetType(type);
                //SetNoteType(note as Note, type);

                chord = note.GetChord();

                ActionHistory.Action[] addRecord = new ActionHistory.Action[chord.Length];
                for (int i = 0; i < addRecord.Length; ++i)
                    addRecord[i] = new ActionHistory.Add(chord[i]);

                if (note.flags != unmodified.flags)
                {
                    actions.AddRange(deleteRecord);
                    actions.AddRange(addRecord);
                }

                foreach (Note chordNote in note.chord)
                {
                    if (chordNote.controller)
                    {
                        chordNote.controller.SetDirty();
                    }
                }
            }

            if (chartObject.controller)
                chartObject.controller.SetDirty();
        }

        if (actions.Count > 0)
            editor.actionHistory.Insert(actions.ToArray());

        ChartEditor.isDirty = true;
    }
    /*
    public static void SetNoteType(Note note, AppliedNoteType noteType)
    {
        note.flags = Note.Flags.NONE;
        switch (noteType)
        {
            case (AppliedNoteType.Strum):
                if (note.IsChord)
                    note.flags &= ~Note.Flags.FORCED;
                else
                {
                    if (note.IsNaturalHopo)
                        note.flags |= Note.Flags.FORCED;
                    else
                        note.flags &= ~Note.Flags.FORCED;
                }

                break;

            case (AppliedNoteType.Hopo):
                if (!note.CannotBeForcedCheck)
                {
                    if (note.IsChord)
                        note.flags |= Note.Flags.FORCED;
                    else
                    {
                        if (!note.IsNaturalHopo)
                            note.flags |= Note.Flags.FORCED;
                        else
                            note.flags &= ~Note.Flags.FORCED;
                    }
                }
                break;

            case (AppliedNoteType.Tap):
                note.flags |= Note.Flags.TAP;
                break;

            default:
                break;
        }

        note.applyFlagsToChord();
    }

    public enum AppliedNoteType
    {
        Natural, Strum, Hopo, Tap
    }*/
}
