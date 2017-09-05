using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(NoteController))]
public class PlaceNote : PlaceSongObject {
    public Note note { get { return (Note)songObject; } set { songObject = value; } }
    new public NoteController controller { get { return (NoteController)base.controller; } set { base.controller = value; } }

    [HideInInspector]
    public NoteVisualsManager visuals;

    [HideInInspector]
    public float horizontalMouseOffset = 0;

    public static bool addNoteCheck
    {
        get
        {
            return (Toolpane.currentTool == Toolpane.Tools.Note && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0));
        }
    }

    protected override void SetSongObjectAndController()
    {
        visuals = GetComponentInChildren<NoteVisualsManager>();
        note = new Note(0, Note.Fret_Type.GREEN);

        controller = GetComponent<NoteController>();
        controller.note = note;
        note.controller = controller;
    }

    protected override void Controls()
    {
        if (addNoteCheck)   // Now handled by the PlaceNoteController
        {
            //AddObject();
        }
    }

    protected override void OnEnable()
    {
        editor.currentSelectedObject = note;
        
        Update();
    }

    void OnDisable()
    {
        note.previous = null;
        note.next = null;
    }

    public void ExplicitUpdate()
    {
        Update();
    }

    // Update is called once per frame
    protected override void Update () {
        note.chart = editor.currentChart;
        base.Update();

        // Get previous and next note
        int pos = SongObject.FindClosestPosition(note.position, editor.currentChart.notes);
        //Debug.Log(pos);
        if (pos == SongObject.NOTFOUND)
        {
            note.previous = null;
            note.next = null;
        }
        else
        {
            if (note.fret_type == Note.Fret_Type.OPEN)
                UpdateOpenPrevAndNext(pos);
            else
                UpdatePrevAndNext(pos);
        }

        UpdateFretType();
    }

    void UpdatePrevAndNext(int closestNoteArrayPos)
    {
        if (editor.currentChart.notes[closestNoteArrayPos] < note)
        {
            note.previous = editor.currentChart.notes[closestNoteArrayPos];
            note.next = editor.currentChart.notes[closestNoteArrayPos].next;
        }
        else if (editor.currentChart.notes[closestNoteArrayPos] > note)
        {
            note.next = editor.currentChart.notes[closestNoteArrayPos];
            note.previous = editor.currentChart.notes[closestNoteArrayPos].previous;
        }
        else
        {
            // Found own note
            note.previous = editor.currentChart.notes[closestNoteArrayPos].previous;
            note.next = editor.currentChart.notes[closestNoteArrayPos].next;
        }
    }

    void UpdateOpenPrevAndNext(int closestNoteArrayPos)
    {
        if (editor.currentChart.notes[closestNoteArrayPos] < note)
        {
            Note previous = GetPreviousOfOpen(note.position, editor.currentChart.notes[closestNoteArrayPos]);

            note.previous = previous;
            if (previous != null)
                note.next = GetNextOfOpen(note.position, previous.next);
            else
                note.next = GetNextOfOpen(note.position, editor.currentChart.notes[closestNoteArrayPos]);
        }
        else if (editor.currentChart.notes[closestNoteArrayPos] > note)
        {
            Note next = GetNextOfOpen(note.position, editor.currentChart.notes[closestNoteArrayPos]);

            note.next = next;
            note.previous = GetPreviousOfOpen(note.position, next.previous);
        }
        else
        {
            // Found own note
            note.previous = editor.currentChart.notes[closestNoteArrayPos].previous;
            note.next = editor.currentChart.notes[closestNoteArrayPos].next;
        }
    }

    Note GetPreviousOfOpen(uint openNotePos, Note previousNote)
    {
        if (previousNote == null || previousNote.position != openNotePos || (!previousNote.IsChord && previousNote.position != openNotePos))
            return previousNote;
        else
            return GetPreviousOfOpen(openNotePos, previousNote.previous);
    }

    Note GetNextOfOpen(uint openNotePos, Note nextNote)
    {
        if (nextNote == null || nextNote.position != openNotePos || (!nextNote.IsChord && nextNote.position != openNotePos))
            return nextNote;
        else
            return GetNextOfOpen(openNotePos, nextNote.next);
    }

    protected virtual void UpdateFretType()
    {
        if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
        {
            if (Input.GetKey("1"))
                note.fret_type = Note.Fret_Type.ORANGE;
            else if (Input.GetKey("2"))
                note.fret_type = Note.Fret_Type.BLUE;
            else if (Input.GetKey("3"))
                note.fret_type = Note.Fret_Type.YELLOW;
            else if (Input.GetKey("4"))
                note.fret_type = Note.Fret_Type.RED;
            else if (Input.GetKey("5"))
                note.fret_type = Note.Fret_Type.GREEN;
            //else if (Input.GetKey("6"))
            //note.fret_type = Note.Fret_Type.OPEN;
            else if (note.fret_type != Note.Fret_Type.OPEN && Mouse.world2DPosition != null)
            {
                Vector2 mousePosition = (Vector2)Mouse.world2DPosition;
                mousePosition.x += horizontalMouseOffset;
                note.fret_type = XPosToFretType(mousePosition.x);
            }
        }
        else
        {
            if (Input.GetKey("1"))
                note.fret_type = Note.Fret_Type.GREEN;
            else if (Input.GetKey("2"))
                note.fret_type = Note.Fret_Type.RED;
            else if (Input.GetKey("3"))
                note.fret_type = Note.Fret_Type.YELLOW;
            else if (Input.GetKey("4"))
                note.fret_type = Note.Fret_Type.BLUE;
            else if (Input.GetKey("5"))
                note.fret_type = Note.Fret_Type.ORANGE;
            //else if (Input.GetKey("6"))
            //note.fret_type = Note.Fret_Type.OPEN;
            else if (note.fret_type != Note.Fret_Type.OPEN && Mouse.world2DPosition != null)
            {
                Vector2 mousePosition = (Vector2)Mouse.world2DPosition;
                mousePosition.x += horizontalMouseOffset;
                note.fret_type = XPosToFretType(mousePosition.x);
            }
        }
    }

    public static Note.Fret_Type XPosToFretType(float xPos)
    {
        if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
            xPos *= -1;

        if (xPos > -0.5f)
        {
            if (xPos < 0.5f)
                return Note.Fret_Type.YELLOW;
            else if (xPos < 1.5f)
                return Note.Fret_Type.BLUE;
            else
                return Note.Fret_Type.ORANGE;
        }
        else
        {
            if (xPos > -1.5f)
                return Note.Fret_Type.RED;
            else
                return Note.Fret_Type.GREEN;
        }
    }

    public ActionHistory.Action[] AddNoteWithRecord()
    {
        return AddObjectToCurrentChart(note, editor);
    }

    protected override void AddObject()
    {
        AddObjectToCurrentChart(note, editor);
    }

    public static ActionHistory.Action[] AddObjectToCurrentChart(Note note, ChartEditor editor, bool update = true, bool copy = true)
    {
        Note throwaway;
        return AddObjectToCurrentChart(note, editor, out throwaway, update, copy);
    }

    public static ActionHistory.Action[] AddObjectToCurrentChart(Note note, ChartEditor editor, out Note addedNote, bool update = true, bool copy = true)
    {
        List<ActionHistory.Action> noteRecord = new List<ActionHistory.Action>();

        Note[] notesToCheckOverwrite = SongObject.GetRangeCopy(editor.currentChart.notes, note.position, note.position);
        
        // Account for when adding an exact note as what's already in   
        if (notesToCheckOverwrite.Length > 0)
        {
            bool cancelAdd = false;
            foreach (Note overwriteNote in notesToCheckOverwrite)
            {              
                if (note.AllValuesCompare(overwriteNote))
                {
                    cancelAdd = true;
                    break;
                }
                if ((((note.fret_type == Note.Fret_Type.OPEN || overwriteNote.fret_type == Note.Fret_Type.OPEN) && !Globals.drumMode) || note.fret_type == overwriteNote.fret_type) && !note.AllValuesCompare(overwriteNote))
                {
                    noteRecord.Add(new ActionHistory.Delete(overwriteNote));
                }
            }
            if (!cancelAdd)
                noteRecord.Add(new ActionHistory.Add(note));
        }
        else
            noteRecord.Add(new ActionHistory.Add(note));
                
        Note noteToAdd;
        if (copy)
            noteToAdd = new Note(note);
        else
            noteToAdd = note;

        if (noteToAdd.fret_type == Note.Fret_Type.OPEN)
            noteToAdd.flags &= ~Note.Flags.TAP;

        editor.currentChart.Add(noteToAdd, update);
        if (noteToAdd.CannotBeForcedCheck)
            noteToAdd.flags &= ~Note.Flags.FORCED;

        noteToAdd.applyFlagsToChord();

        //NoteController nCon = editor.CreateNoteObject(noteToAdd);
        standardOverwriteOpen(noteToAdd);

        noteRecord.InsertRange(0, CapNoteCheck(noteToAdd));
        noteRecord.InsertRange(0, ForwardCap(noteToAdd));     // Do this due to pasting from the clipboard

        // Check if the automatic un-force will kick in
        ActionHistory.Action forceCheck = AutoForcedCheck(noteToAdd);

        addedNote = noteToAdd;

        if (forceCheck != null)
            noteRecord.Insert(0, forceCheck);           // Insert at the start so that the modification happens at the end of the undo function, otherwise the natural force check prevents it from being forced

        return noteRecord.ToArray();
    }

    protected static void standardOverwriteOpen(Note note)
    {
        if (note.fret_type != Note.Fret_Type.OPEN && MenuBar.currentInstrument != Song.Instrument.Drums)
        {
            Note[] chordNotes = SongObject.FindObjectsAtPosition(note.position, note.chart.notes);

            // Check for open notes and delete
            foreach (Note chordNote in chordNotes)
            {
                if (chordNote.fret_type == Note.Fret_Type.OPEN)
                {
                    chordNote.Delete();
                }
            }
        }
    }

    protected static ActionHistory.Action AutoForcedCheck(Note note)
    {
        Note next = note.nextSeperateNote;
        if (next != null && (next.flags & Note.Flags.FORCED) == Note.Flags.FORCED && next.CannotBeForcedCheck)
        {           
            Note originalNext = (Note)next.Clone();
            next.flags &= ~Note.Flags.FORCED;
            next.applyFlagsToChord();

            return new ActionHistory.Modify(originalNext, next);
        }
        else
            return null;
    }

    protected static ActionHistory.Action[] ForwardCap(Note note)
    {
        List<ActionHistory.Action> actionRecord = new List<ActionHistory.Action>();
        Note[] notesToCap;
        Note next;
        next = note.nextSeperateNote;      
        
        if (!Globals.extendedSustainsEnabled)
        {
            // Get chord  
            next = note.nextSeperateNote;
            notesToCap = note.GetChord();          
        }
        else
        {
            notesToCap = new Note[] { note };

            // Find the next note of the same fret type or open
            next = note.next;
            while (next != null && next.fret_type != note.fret_type && next.fret_type != Note.Fret_Type.OPEN )
                next = next.next;

            // If it's an open note it won't be capped
        }

        if (next != null)
        {
            foreach (Note noteToCap in notesToCap)
            {
                ActionHistory.Action action = noteToCap.CapSustain(next);
                if (action != null)
                    actionRecord.Add(action);
            }
        }

        return actionRecord.ToArray();
    }

    protected static ActionHistory.Action[] CapNoteCheck(Note noteToAdd)
    {
        List<ActionHistory.Action> actionRecord = new List<ActionHistory.Action>();

        Note[] previousNotes = Note.GetPreviousOfSustains(noteToAdd);
        if (!Globals.extendedSustainsEnabled)
        {
            // Cap all the notes
            foreach (Note prevNote in previousNotes)
            {
                if (prevNote.controller != null)
                {
                    ActionHistory.Action action = prevNote.CapSustain(noteToAdd);
                    if (action != null)
                        actionRecord.Add(action);
                }
            }

            foreach(Note chordNote in noteToAdd.GetChord())
            {
                if (chordNote.controller != null)
                    chordNote.controller.note.sustain_length = noteToAdd.sustain_length; 
            }
        }
        else
        {
            // Cap only the sustain of the same fret type and open notes
            foreach (Note prevNote in previousNotes)
            {
                if (prevNote.controller != null && (noteToAdd.fret_type == Note.Fret_Type.OPEN || prevNote.fret_type == noteToAdd.fret_type /*|| prevNote.fret_type == Note.Fret_Type.OPEN*/))
                {
                    ActionHistory.Action action = prevNote.CapSustain(noteToAdd);
                    if (action != null)
                        actionRecord.Add(action);
                }
            }
        }

        return actionRecord.ToArray();
    }
}
