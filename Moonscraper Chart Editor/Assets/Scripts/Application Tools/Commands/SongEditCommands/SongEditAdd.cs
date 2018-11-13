using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditAdd : SongEditCommand
{
    List<SongObject> overwrittenSongObjects = new List<SongObject>();       // Todo properly

    public SongEditAdd(IList<SongObject> songObjects) : base(songObjects)
    {
        foreach(SongObject songObject in songObjects)
        {
            Debug.Assert(songObject.song == null, "Must add a new song object!");
        }
    }

    public SongEditAdd(SongObject songObject) : base(songObject)
    {
        Debug.Assert(songObject.song == null, "Must add a new song object!");
    }

    public override void Invoke()
    {
        ApplyAction(songObjects, overwrittenSongObjects);
        PostExecuteUpdate();
    }

    public override void Revoke()
    {
        SongEditDelete.ApplyAction(songObjects);
        ApplyAction(overwrittenSongObjects, new List<SongObject>());

        overwrittenSongObjects.Clear();

        PostExecuteUpdate();
    }

    public static void ApplyAction(IList<SongObject> songObjects, IList<SongObject> overwriteList)
    {
        foreach (SongObject songObject in songObjects)
        {
            ApplyAction(songObject, overwriteList);
        }
    }

    public static void ApplyAction(SongObject songObject, IList<SongObject> overwriteList)
    {
        // Todo, replace this, the functions contained within are horrible, especially for notes. 
        // Need to handle overwriting somehow?

        switch (songObject.classID)
        {
            case ((int)SongObject.ID.Note):
                AddNote((Note)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.Starpower):
                throw new System.NotImplementedException();
                break;

            case ((int)SongObject.ID.ChartEvent):
                AddChartEvent((ChartEvent)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.BPM):
                AddBPM((BPM)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.Section):
                AddSection((Section)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.TimeSignature):
                AddTimeSignature((TimeSignature)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.Event):
                AddEvent((Event)songObject, overwriteList);
                break;

            default:
                Debug.LogError("Unhandled songobject!");
                break;
        }
    }

    #region Object specific add functions

    static void TryRecordOverwrite<T>(T songObject, IList<T> searchObjects, IList<SongObject> overwrittenObjects) where T : SongObject
    {
        if (overwrittenObjects == null)
            return;

        ChartEditor editor = ChartEditor.Instance;
        int overwriteIndex = SongObjectHelper.FindObjectPosition(songObject.tick, editor.currentChart.chartObjects);

        if (overwriteIndex != SongObjectHelper.NOTFOUND)
        {
            overwrittenObjects.Add(editor.currentChart.chartObjects[overwriteIndex].Clone());
        }
    }

    static void AddNote(Note note, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        Chart chart = editor.currentChart;

        int index, length;
        SongObjectHelper.GetRange(chart.notes, note.tick, note.tick, out index, out length);

        // Account for when adding an exact note as what's already in   
        if (length > 0)
        {
            for (int i = index; i < index + length; ++i)
            {
                Note overwriteNote = chart.notes[i];
                if ((((note.IsOpenNote() || overwriteNote.IsOpenNote()) && !Globals.drumMode) || note.guitarFret == overwriteNote.guitarFret))
                {
                    overwriteNote.Delete();
                    overwrittenList.Add(overwriteNote);
                }
            }
        }

        Note noteToAdd = new Note(note);
        if (noteToAdd.IsOpenNote())
            noteToAdd.flags &= ~Note.Flags.Tap;

        chart.Add(noteToAdd, false);
        if (noteToAdd.cannotBeForced)
            noteToAdd.flags &= ~Note.Flags.Forced;

        List<Note> replacementNotes = new List<Note>();

        // Apply flags to chord
        foreach (Note chordNote in note.chord)
        {
            // Overwrite note flags
            if (chordNote.flags != note.flags)
            {  
                Note newChordNote = new Note(chordNote.tick, chordNote.rawNote, chordNote.length, note.flags);
                AddOrReplaceNote(chart, chordNote, newChordNote, overwrittenList, replacementNotes);
            }
        } 

        CapNoteCheck(chart, noteToAdd, overwrittenList, replacementNotes);
        ForwardCap(chart, noteToAdd, overwrittenList, replacementNotes);
        AutoForcedCheck(chart, noteToAdd, overwrittenList, replacementNotes);

        foreach (Note chordNote in noteToAdd.chord)
        {
            if (chordNote.controller)
                chordNote.controller.SetDirty();
        }

        Note next = noteToAdd.nextSeperateNote;
        if (next != null)
        {
            foreach (Note chordNote in next.chord)
            {
                if (chordNote.controller)
                    chordNote.controller.SetDirty();
            }
        }
    }

    static void AddChartEvent(ChartEvent chartEvent, IList<SongObject> overwrittenList)
    {      
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(chartEvent, editor.currentChart.chartObjects, overwrittenList);

        ChartEvent eventToAdd = new ChartEvent(chartEvent);

        editor.currentChart.Add(eventToAdd, false);
        Debug.Log("Added new chart event");
    }

    static void AddBPM(BPM bpm, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(bpm, editor.currentSong.bpms, overwrittenList);

        BPM bpmToAdd = new BPM(bpm);
        editor.currentSong.Add(bpmToAdd, false);
        Debug.Log("Added new bpm");

        if (bpmToAdd.anchor != null)
        {
            bpmToAdd.anchor = bpmToAdd.song.LiveTickToTime(bpmToAdd.tick, bpmToAdd.song.resolution);
        }

        ChartEditor.Instance.songObjectPoolManager.SetAllPoolsDirty();
    }

    static void AddTimeSignature(TimeSignature timeSignature, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(timeSignature, editor.currentSong.timeSignatures, overwrittenList);

        TimeSignature tsToAdd = new TimeSignature(timeSignature);
        editor.currentSong.Add(tsToAdd, false);
        Debug.Log("Added new timesignature");
    }

    static void AddEvent(Event songEvent, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(songEvent, editor.currentSong.events, overwrittenList);

        Event eventToAdd = new Event(songEvent);
        editor.currentSong.Add(eventToAdd, false);

        Debug.Log("Added new song event");
    }

    static void AddSection(Section section, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(section, editor.currentSong.sections, overwrittenList);

        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd, false);

        Debug.Log("Added new section");
    }

    #endregion

    #region Note Insertion Helper Functions

    static Note FindReplacementNote(Note originalNote, IList<Note> replacementNotes)
    {
        foreach(Note replacementNote in replacementNotes)
        {
            if (originalNote.tick == replacementNote.tick && originalNote.rawNote == replacementNote.rawNote)
                return replacementNote;
        }

        return null;
    }

    static void AddOrReplaceNote(Chart chart, Note note, Note newNote, IList<SongObject> overwrittenList, IList<Note> replacementNotes)
    {
        Note replacementNote = FindReplacementNote(note, replacementNotes);
        if (replacementNote == null)
        {
            overwrittenList.Add(note);
            replacementNotes.Add(newNote);
            chart.Add(newNote);
        }
        else
        {
            replacementNote.CopyFrom(newNote);
        }
    }

    protected static void ForwardCap(Chart chart, Note note, IList<SongObject> overwrittenList, IList<Note> replacementNotes)
    {
        Note next;
        next = note.nextSeperateNote;

        if (!GameSettings.extendedSustainsEnabled)
        {
            // Get chord  
            next = note.nextSeperateNote;

            if (next != null)
            {
                foreach (Note noteToCap in note.chord)
                {
                    uint newLength = noteToCap.GetCappedLength(next);
                    if (noteToCap.length != newLength)
                    {
                        Note newNote = new Note(noteToCap.tick, noteToCap.rawNote, newLength, noteToCap.flags);
                        AddOrReplaceNote(chart, noteToCap, newNote, overwrittenList, replacementNotes);
                    }
                }
            }
        }
        else
        {
            // Find the next note of the same fret type or open
            next = note.next;
            while (next != null && next.guitarFret != note.guitarFret && !next.IsOpenNote())
                next = next.next;

            // If it's an open note it won't be capped

            if (next != null)
            {
                uint newLength = note.GetCappedLength(next);
                if (note.length != newLength)
                {
                    Note newNote = new Note(note.tick, note.rawNote, newLength, note.flags);
                    AddOrReplaceNote(chart, note, newNote, overwrittenList, replacementNotes);
                }
            }
        }
    }

    static void CapNoteCheck(Chart chart, Note noteToAdd, IList<SongObject> overwrittenList, IList<Note> replacementNotes)
    {
        Note[] previousNotes = NoteFunctions.GetPreviousOfSustains(noteToAdd);
        if (!GameSettings.extendedSustainsEnabled)
        {
            // Cap all the notes
            foreach (Note prevNote in previousNotes)
            {
                uint newLength = prevNote.GetCappedLength(noteToAdd);
                if (prevNote.length != newLength)
                {
                    Note newNote = new Note(prevNote.tick, prevNote.rawNote, newLength, prevNote.flags);
                    AddOrReplaceNote(chart, prevNote, newNote, overwrittenList, replacementNotes);
                }
            }

            foreach (Note chordNote in noteToAdd.chord)
            {
                uint newLength = noteToAdd.length;
                if (chordNote.length != newLength)
                {
                    Note newNote = new Note(chordNote.tick, chordNote.rawNote, newLength, chordNote.flags);
                    AddOrReplaceNote(chart, chordNote, newNote, overwrittenList, replacementNotes);
                }
            }
        }
        else
        {
            // Cap only the sustain of the same fret type and open notes
            foreach (Note prevNote in previousNotes)
            {
                if (noteToAdd.IsOpenNote() || prevNote.guitarFret == noteToAdd.guitarFret)
                {
                    uint newLength = prevNote.GetCappedLength(noteToAdd);
                    if (prevNote.length != newLength)
                    {
                        overwrittenList.Add(prevNote);
                        chart.Add(new Note(prevNote.tick, prevNote.rawNote, newLength, prevNote.flags));
                    }
                }
            }
        }
    }

    static void AutoForcedCheck(Chart chart, Note note, IList<SongObject> overwrittenNotes, IList<Note> replacementNotes)
    {
        Note next = note.nextSeperateNote;
        if (next != null && (next.flags & Note.Flags.Forced) == Note.Flags.Forced && next.cannotBeForced)
        {
            Note.Flags flags = next.flags;
            flags &= ~Note.Flags.Forced;

            // Apply flags to chord
            foreach (Note chordNote in next.chord)
            {
                // Overwrite note flags
                if (chordNote.flags != flags)
                {
                    Note newChordNote = new Note(chordNote.tick, chordNote.rawNote, chordNote.length, note.flags);
                    AddOrReplaceNote(chart, chordNote, newChordNote, overwrittenNotes, replacementNotes);
                }
            }
        }
    }

    #endregion
}
