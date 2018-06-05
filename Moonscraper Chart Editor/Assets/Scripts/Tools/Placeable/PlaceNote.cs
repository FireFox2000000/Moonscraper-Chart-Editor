// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

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
        note = new Note(0, Note.GuitarFret.Green);

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
        int pos = SongObjectHelper.FindClosestPosition(note.position, editor.currentChart.notes);
        //Debug.Log(pos);
        if (pos == SongObjectHelper.NOTFOUND)
        {
            note.previous = null;
            note.next = null;
        }
        else
        {
            if (note.IsOpenNote())
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
        if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
        {
            if (Input.GetKey("1"))
                note.guitarFret = Note.GuitarFret.Orange;
            else if (Input.GetKey("2"))
                note.guitarFret = Note.GuitarFret.Blue;
            else if (Input.GetKey("3"))
                note.guitarFret = Note.GuitarFret.Yellow;
            else if (Input.GetKey("4"))
                note.guitarFret = Note.GuitarFret.Red;
            else if (Input.GetKey("5"))
                note.guitarFret = Note.GuitarFret.Green;
            //else if (Input.GetKey("6"))
            else if (!note.IsOpenNote() && Mouse.world2DPosition != null)
            {
                Vector2 mousePosition = (Vector2)Mouse.world2DPosition;
                mousePosition.x += horizontalMouseOffset;
                note.rawNote = XPosToNoteNumber(mousePosition.x);
            }
        }
        else
        {
            if (Input.GetKey("1"))
                note.guitarFret = Note.GuitarFret.Green;
            else if (Input.GetKey("2"))
                note.guitarFret = Note.GuitarFret.Red;
            else if (Input.GetKey("3"))
                note.guitarFret = Note.GuitarFret.Yellow;
            else if (Input.GetKey("4"))
                note.guitarFret = Note.GuitarFret.Blue;
            else if (Input.GetKey("5"))
                note.guitarFret = Note.GuitarFret.Orange;
            //else if (Input.GetKey("6"))

            else if (!note.IsOpenNote() && Mouse.world2DPosition != null)
            {
                Vector2 mousePosition = (Vector2)Mouse.world2DPosition;
                mousePosition.x += horizontalMouseOffset;
                note.rawNote = XPosToNoteNumber(mousePosition.x);
            }
        }
    }

    public static int XPosToNoteNumber(float xPos)
    {
        if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
            xPos *= -1;

        float startPos = -2.0f;
        float endPos = 2.0f;

        int max = Globals.ghLiveMode ? (int)Note.GHLiveGuitarFret.White3 : (int)Note.GuitarFret.Orange;
        float factor = (endPos - startPos) / (max);

        for (int i = 0; i < max; ++i)
        {
            float currentPosCheck = startPos + i * factor + factor / 2.0f;
            if (xPos < currentPosCheck)
                return i;
        }

        return max;
        /*
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
        }*/
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

        Note[] notesToCheckOverwrite = SongObjectHelper.GetRangeCopy(editor.currentChart.notes, note.position, note.position);
        
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
                if ((((note.IsOpenNote() || overwriteNote.IsOpenNote()) && !Globals.drumMode) || note.guitarFret == overwriteNote.guitarFret) && !note.AllValuesCompare(overwriteNote))
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

        if (noteToAdd.IsOpenNote())
            noteToAdd.flags &= ~Note.Flags.Tap;

        editor.currentChart.Add(noteToAdd, update);
        if (noteToAdd.CannotBeForcedCheck)
            noteToAdd.flags &= ~Note.Flags.Forced;

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

        foreach (Note chordNote in addedNote.GetChord())
        {
            if (chordNote.controller)
                chordNote.controller.SetDirty();
        }

        Note next = addedNote.nextSeperateNote;
        if (next != null)
        {
            foreach (Note chordNote in next.GetChord())
            {
                if (chordNote.controller)
                    chordNote.controller.SetDirty();
            }
        }

        return noteRecord.ToArray();
    }

    protected static void standardOverwriteOpen(Note note)
    {
        if (!note.IsOpenNote() && MenuBar.currentInstrument != Song.Instrument.Drums)
        {
            Note[] chordNotes = SongObjectHelper.FindObjectsAtPosition(note.position, note.chart.notes);

            // Check for open notes and delete
            foreach (Note chordNote in chordNotes)
            {
                if (chordNote.IsOpenNote())
                {
                    chordNote.Delete();
                }
            }
        }
    }

    protected static ActionHistory.Action AutoForcedCheck(Note note)
    {
        Note next = note.nextSeperateNote;
        if (next != null && (next.flags & Note.Flags.Forced) == Note.Flags.Forced && next.CannotBeForcedCheck)
        {           
            Note originalNext = (Note)next.Clone();
            next.flags &= ~Note.Flags.Forced;
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
        
        if (!GameSettings.extendedSustainsEnabled)
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
            while (next != null && next.guitarFret != note.guitarFret && !next.IsOpenNote())
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
        if (!GameSettings.extendedSustainsEnabled)
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
                    chordNote.controller.note.length = noteToAdd.length; 
            }
        }
        else
        {
            // Cap only the sustain of the same fret type and open notes
            foreach (Note prevNote in previousNotes)
            {
                if (prevNote.controller != null && (noteToAdd.IsOpenNote() || prevNote.guitarFret == noteToAdd.guitarFret))
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
