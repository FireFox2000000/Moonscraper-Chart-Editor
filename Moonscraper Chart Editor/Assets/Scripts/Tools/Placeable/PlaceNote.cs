using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(NoteController))]
public class PlaceNote : ToolObject {
    protected Note note;
    NoteController controller;

    protected override void Awake()
    {
        base.Awake();
        note = new Note(editor.currentSong, editor.currentChart, 0, Note.Fret_Type.GREEN);

        controller = GetComponent<NoteController>();
        controller.note = note;
    }

    protected override void Controls()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Note && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            AddObject();
        }
    }

    // Update is called once per frame
    protected override void Update () {
        base.Update();

        editor.currentSelectedNote = note;

        note.song = editor.currentSong;
        note.chart = editor.currentChart;
        note.position = objectSnappedChartPos;

        // Get previous and next note
        int pos = SongObject.FindClosestPosition(note.position, editor.currentChart.notes);
        if (pos == Globals.NOTFOUND)
        {
            note.previous = null;
            note.next = null;
        }
        else
        {
            UpdatePrevAndNext(pos);
        }

        UpdateFretType();
    }

    protected virtual void UpdatePrevAndNext(int closestNoteArrayPos)
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

    void UpdateFretType()
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
        else if (Input.GetKey("6"))
            note.fret_type = Note.Fret_Type.OPEN;
        else if (note.fret_type != Note.Fret_Type.OPEN)
        {
            // Snap to either -2, -1, 0, 1 or 2
            if (mousePos.x > -0.5f)
            {
                if (mousePos.x < 0.5f)
                    note.fret_type = Note.Fret_Type.YELLOW;
                else if (mousePos.x < 1.5f)
                    note.fret_type = Note.Fret_Type.BLUE;
                else
                    note.fret_type = Note.Fret_Type.ORANGE;
            }
            else
            {
                if (mousePos.x > -1.5f)
                    note.fret_type = Note.Fret_Type.RED;
                else
                    note.fret_type = Note.Fret_Type.GREEN;
            }
        }
    }

    protected override void AddObject()
    {
        Debug.Log("Add");
        // TODO- Check if the mouse is in the correct position to add in the first place
        Note noteToAdd = new Note(note);
        editor.currentChart.Add(noteToAdd);
        editor.CreateNoteObject(noteToAdd);
    }
}
