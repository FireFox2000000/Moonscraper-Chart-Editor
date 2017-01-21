using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(NoteController))]
public class PlaceNote : PlaceSongObject {
    public Note note { get { return (Note)songObject; } set { songObject = value; } }
    new public NoteController controller { get { return (NoteController)base.controller; } set { base.controller = value; } }

    [HideInInspector]
    public float horizontalMouseOffset = 0;

    public static bool addNoteCheck
    {
        get
        {
            return (Toolpane.currentTool == Toolpane.Tools.Note && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0));
        }
    }

    protected override void Awake()
    {
        base.Awake();
        note = new Note(0, Note.Fret_Type.GREEN);

        controller = GetComponent<NoteController>();
        controller.note = note;
        note.controller = controller;
    }

    protected override void Controls()
    {
        if (addNoteCheck)
        {
            AddObject();
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

    // Update is called once per frame
    protected override void Update () {
        note.chart = editor.currentChart;
        base.Update();

        // Get previous and next note
        int pos = SongObject.FindClosestPosition(note.position, editor.currentChart.notes);
        if (pos == Globals.NOTFOUND)
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

    public static Note.Fret_Type XPosToFretType(float xPos)
    {
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

    protected override void AddObject()
    {
        AddObjectToCurrentChart(note, editor);
        /*
        Note noteToAdd = new Note(note);
        editor.currentChart.Add(noteToAdd);
        NoteController nCon = editor.CreateNoteObject(noteToAdd);
        nCon.standardOverwriteOpen();

        CapNoteCheck(noteToAdd);  */
    }

    public static void AddObjectToCurrentChart(Note note, ChartEditor editor, bool update = true)
    {
        Note noteToAdd = new Note(note);
        editor.currentChart.Add(noteToAdd, update);
        NoteController nCon = editor.CreateNoteObject(noteToAdd);
        nCon.standardOverwriteOpen();

        CapNoteCheck(noteToAdd);
    }

    protected static void CapNoteCheck(Note noteToAdd)
    {
        List<ActionHistory.Action> actionRecord = new List<ActionHistory.Action>();

        Note[] previousNotes = Note.GetPreviousOfSustains(noteToAdd);

        if (!Globals.extendedSustainsEnabled)
        {
            // Cap all the notes
            foreach (Note prevNote in previousNotes)
            {
                if (prevNote.controller != null)
                    prevNote.controller.sustain.CapSustain(noteToAdd);
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
                if (prevNote.controller != null && (prevNote.fret_type == noteToAdd.fret_type || prevNote.fret_type == Note.Fret_Type.OPEN))
                    prevNote.controller.sustain.CapSustain(noteToAdd);
            }
        }
    }
}
