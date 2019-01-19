using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditAdd : SongEditCommand
{
    List<SongObject> overwrittenSongObjects = new List<SongObject>();
    bool extendedSustainsEnabled;

    public SongEditAdd(IList<SongObject> songObjects) : base(songObjects)
    {
        SnapshotGameSettings();
        foreach (SongObject songObject in songObjects)
        {
            Debug.Assert(songObject.song == null, "Must add a new song object!");
        }
    }

    public SongEditAdd(SongObject songObject) : base(songObject)
    {
        SnapshotGameSettings();
        Debug.Assert(songObject.song == null, "Must add a new song object!");
    }

    public override void Invoke()
    {
        if (hasValidatedSongObjects)
        {
            ApplyPostValidatedAction(validatedSongObjects, overwrittenSongObjects);
        }
        else
        {
            ApplyActionAndFillValidation(extendedSustainsEnabled);
            songObjects.Clear();
            for (int i = validatedSongObjects.Count - 1; i >= 0; --i)
            {
                SongObject so = validatedSongObjects[i];
                if (so.song == null)    
                {
                    // Song object was probably removed during the initial add process, thus was never added at all
                    validatedSongObjects.RemoveAt(i);
                    overwrittenSongObjects.Remove(so);
                }
                else
                    validatedSongObjects[i] = so.Clone();
            }

            hasValidatedSongObjects = true;
        }

        PostExecuteUpdate();
    }

    public override void Revoke()
    {
        Debug.Assert(hasValidatedSongObjects, "Trying to revoke add action which has not made it's initial validation pass!");
        ApplyPostValidatedAction(overwrittenSongObjects, validatedSongObjects);

        PostExecuteUpdate();
    }

    void ApplyPostValidatedAction(IList<SongObject> songObjectsToAdd, IList<SongObject> songObjectsToDelete)
    {
        SongEditDelete.ApplyAction(songObjectsToDelete);

        foreach (SongObject songObject in songObjectsToAdd)
        {
            ApplyPostValidatedAction(songObject);
        }
    }

    public static void ApplyAction(IList<SongObject> songObjects, IList<SongObject> overwriteList, bool extendedSustainsEnabled)
    {
        List<SongObject> dummy = new List<SongObject>();
        foreach (SongObject songObject in songObjects)
        {
            ApplyAction(songObject, overwriteList, extendedSustainsEnabled, dummy);
        }
    }

    void ApplyActionAndFillValidation(bool extendedSustainsEnabled)
    {
        foreach (SongObject songObject in songObjects)
        {
            ApplyAction(songObject, overwrittenSongObjects, extendedSustainsEnabled, validatedSongObjects);
        }
    }

    static void ApplyAction(SongObject songObject, IList<SongObject> overwriteList, bool extendedSustainsEnabled, List<SongObject> validatedNotes)
    {
        SongObject validatedSo = null;

        switch (songObject.classID)
        {
            case ((int)SongObject.ID.Note):
                AddNote((Note)songObject, overwriteList, extendedSustainsEnabled, validatedNotes);
                break;

            case ((int)SongObject.ID.Starpower):
                throw new System.NotImplementedException();
                break;

            case ((int)SongObject.ID.ChartEvent):
                validatedSo = AddChartEvent((ChartEvent)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.BPM):
                validatedSo = AddBPM((BPM)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.Section):
                validatedSo = AddSection((Section)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.TimeSignature):
                validatedSo = AddTimeSignature((TimeSignature)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.Event):
                validatedSo = AddEvent((Event)songObject, overwriteList);
                break;

            default:
                Debug.LogError("Unhandled songobject!");
                break;
        }

        if (validatedSo != null)
            validatedNotes.Add(validatedSo);
    }

    public static void ApplyPostValidatedAction(SongObject songObject)
    {
        switch (songObject.classID)
        {
            case ((int)SongObject.ID.Note):
            case ((int)SongObject.ID.Starpower):
            case ((int)SongObject.ID.ChartEvent):
                {
                    ChartEditor editor = ChartEditor.Instance;
                    Chart chart = editor.currentChart;
                    chart.Add(songObject as ChartObject);
                }
                break;

            case ((int)SongObject.ID.BPM):
            case ((int)SongObject.ID.TimeSignature):
                {
                    ChartEditor editor = ChartEditor.Instance;
                    Song song = editor.currentSong;
                    song.Add(songObject as SyncTrack);
                }
                break;

            case ((int)SongObject.ID.Section):          
            case ((int)SongObject.ID.Event):
                {
                    ChartEditor editor = ChartEditor.Instance;
                    Song song = editor.currentSong;
                    song.Add(songObject as Event);
                }
                break;

            default:
                Debug.LogError("Unhandled songobject!");
                break;
        }
    }

    void SnapshotGameSettings()
    {
        extendedSustainsEnabled = GameSettings.extendedSustainsEnabled;
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

    static void AddNote(Note note, IList<SongObject> overwrittenList, bool extendedSustainsEnabled, List<SongObject> validatedNotes)
    {
        ChartEditor editor = ChartEditor.Instance;
        Chart chart = editor.currentChart;
        Song song = editor.currentSong;

        int index, length;
        SongObjectHelper.GetRange(chart.chartObjects, note.tick, note.tick, out index, out length);

        // Account for when adding an exact note as what's already in   
        if (length > 0)
        {
            for (int i = index + length - 1; i >= index; --i)
            {
                Note overwriteNote = chart.chartObjects[i] as Note;
                bool sameFret = note.guitarFret == overwriteNote.guitarFret;
                bool isOverwritableOpenNote = (note.IsOpenNote() || overwriteNote.IsOpenNote()) && !Globals.drumMode;
                if (overwriteNote != null && (isOverwritableOpenNote || sameFret))
                {
                    overwriteNote.Delete(false);
                    overwrittenList.Add(overwriteNote);
                }
            }
        }

        Note noteToAdd = new Note(note);
        List<Note> replacementNotes = new List<Note>();

        // Apply post-insert note corrections
        {
            if (noteToAdd.IsOpenNote())
                noteToAdd.flags &= ~Note.Flags.Tap;

            chart.Add(noteToAdd, false);
            if (noteToAdd.cannotBeForced)
                noteToAdd.flags &= ~Note.Flags.Forced;

            // Apply flags to chord
            foreach (Note chordNote in noteToAdd.chord)
            {
                // Overwrite note flags
                if (chordNote.flags != noteToAdd.flags)
                {
                    Note newChordNote = new Note(chordNote.tick, chordNote.rawNote, chordNote.length, note.flags);
                    AddOrReplaceNote(chart, chordNote, newChordNote, overwrittenList, replacementNotes);
                }
            }

            CapNoteCheck(chart, noteToAdd, overwrittenList, replacementNotes, song, extendedSustainsEnabled);
            ForwardCap(chart, noteToAdd, overwrittenList, replacementNotes, song);

            AutoForcedCheck(chart, noteToAdd, overwrittenList, replacementNotes);
        }

        // Queue visual refresh
        {
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

        validatedNotes.Add(noteToAdd);
        foreach(Note rNote in replacementNotes)
        {
            validatedNotes.Add(rNote);
        }
    }

    static ChartEvent AddChartEvent(ChartEvent chartEvent, IList<SongObject> overwrittenList)
    {      
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(chartEvent, editor.currentChart.chartObjects, overwrittenList);

        ChartEvent eventToAdd = new ChartEvent(chartEvent);

        editor.currentChart.Add(eventToAdd, false);
        Debug.Log("Added new chart event");

        return eventToAdd;
    }

    static BPM AddBPM(BPM bpm, IList<SongObject> overwrittenList)
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

        return bpmToAdd;
    }

    static TimeSignature AddTimeSignature(TimeSignature timeSignature, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(timeSignature, editor.currentSong.timeSignatures, overwrittenList);

        TimeSignature tsToAdd = new TimeSignature(timeSignature);
        editor.currentSong.Add(tsToAdd, false);
        Debug.Log("Added new timesignature");

        return tsToAdd;
    }

    static Event AddEvent(Event songEvent, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(songEvent, editor.currentSong.events, overwrittenList);

        Event eventToAdd = new Event(songEvent);
        editor.currentSong.Add(eventToAdd, false);

        Debug.Log("Added new song event");

        return eventToAdd;
    }

    static Section AddSection(Section section, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(section, editor.currentSong.sections, overwrittenList);

        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd, false);

        Debug.Log("Added new section");

        return sectionToAdd;
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

    protected static void ForwardCap(Chart chart, Note note, IList<SongObject> overwrittenList, IList<Note> replacementNotes, Song song)
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
                    uint newLength = noteToCap.GetCappedLength(next, song);
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
                uint newLength = note.GetCappedLength(next, song);
                if (note.length != newLength)
                {
                    Note newNote = new Note(note.tick, note.rawNote, newLength, note.flags);
                    AddOrReplaceNote(chart, note, newNote, overwrittenList, replacementNotes);
                }
            }
        }
    }

    static void CapNoteCheck(Chart chart, Note noteToAdd, IList<SongObject> overwrittenList, IList<Note> replacementNotes, Song song, bool extendedSustainsEnabled)
    {
        Note[] previousNotes = NoteFunctions.GetPreviousOfSustains(noteToAdd, extendedSustainsEnabled);
        if (!GameSettings.extendedSustainsEnabled)
        {
            // Cap all the notes
            foreach (Note prevNote in previousNotes)
            {
                uint newLength = prevNote.GetCappedLength(noteToAdd, song);
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
                    uint newLength = prevNote.GetCappedLength(noteToAdd, song);
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
