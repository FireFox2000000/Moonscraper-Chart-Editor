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
    Button setNoteNatural;
    [SerializeField]
    Button setNoteStrum;
    [SerializeField]
    Button setNoteHopo;
    [SerializeField]
    Button setNoteTap;

    // Use this for initialization
    void Start () {
        editor = ChartEditor.FindCurrentEditor();
	}

    void Update()
    {
        if (!Globals.IsTyping && !Globals.modifierInputActive)
            Shortcuts();
    }

    void Shortcuts()
    {
        if (Input.GetKeyDown(KeyCode.X))
            setNoteNatural.onClick.Invoke();
        else if (Input.GetKeyDown(KeyCode.S))
            setNoteStrum.onClick.Invoke();
        else if (Input.GetKeyDown(KeyCode.H))
            setNoteHopo.onClick.Invoke();
        else if (Input.GetKeyDown(KeyCode.T))
            setNoteTap.onClick.Invoke();
    }

    public void ApplyFretDropdownSelection()
    {
        if (fretSelectDropdown.value >= 0 && fretSelectDropdown.value < 6)
        {
            SetFretType((Note.Fret_Type)fretSelectDropdown.value);
        }
    }

    public void SetFretType(Note.Fret_Type type)
    {
        List<ActionHistory.Action> actions = new List<ActionHistory.Action>();

        foreach (ChartObject chartObject in editor.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note && chartObject.song != null) // check null in case note was already deleted when overwritten by changing a note before it
            {  
                Note note = chartObject as Note;
                if (note.fret_type != type)
                {
                    // Delete original then re-add to let notes be overwritten, chaing a note into an open note that was already part of a chord
                    actions.Add(new ActionHistory.Delete(note));
                    note.Delete();
                    note.fret_type = type;
                    actions.AddRange(PlaceNote.AddObjectToCurrentChart(note, editor, false, true));
                }
            }
        }

        editor.currentChart.UpdateCache();

        if (actions.Count > 0)
            editor.actionHistory.Insert(actions.ToArray());

        ChartEditor.isDirty = true;
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
        uint songEndTick = editor.currentSong.TimeToChartPosition(editor.currentSong.length, editor.currentSong.resolution);

        foreach (ChartObject chartObject in editor.currentSelectedObjects)
        {
            if (chartObject.classID == (int)SongObject.ID.Note)
            {
                Note note = chartObject as Note;
                uint assignedLength = length;
                if (length == uint.MaxValue)
                    assignedLength = songEndTick - note.position;

                if (Globals.extendedSustainsEnabled)
                {
                    Note original = (Note)note.Clone();
                    note.sustain_length = assignedLength;          
                    note.CapSustain(note.FindNextSameFretWithinSustainExtendedCheck());

                    if (original.sustain_length != note.sustain_length)
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
                        chordNotes[i].sustain_length = assignedLength;
                        chordNotes[i].CapSustain(capNote);

                        if (chordNotesCopy[i].sustain_length != chordNotes[i].sustain_length)
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
        SetNoteType(Note.Note_Type.Natural);
    }

    public void SetStrum()
    {
        SetNoteType(Note.Note_Type.Strum);
    }

    public void SetHopo()
    {
        SetNoteType(Note.Note_Type.Hopo);
    }

    public void SetTap()
    {
        SetNoteType(Note.Note_Type.Tap);
    }

    public void SetNoteType(Note.Note_Type type)
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
            }
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
