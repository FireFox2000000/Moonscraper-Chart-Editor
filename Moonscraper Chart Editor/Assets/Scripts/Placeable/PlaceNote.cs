using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(NoteController))]
public class PlaceNote : Snapable {
    protected Note note;
    NoteController controller;

    protected override void Awake()
    {
        base.Awake();
        note = new Note(editor.currentSong, editor.currentChart, 0, Note.Fret_Type.GREEN);
        controller = GetComponent<NoteController>();
        controller.note = note;
    }

	// Update is called once per frame
	protected override void Update () {
        base.Update();

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

    protected virtual void UpdateFretType()
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

    protected override void AddObject()
    {
        Debug.Log("Add");
        // Check if the mouse is in the correct position to add in the first place
        editor.currentChart.Add(note);
        editor.CreateNoteObject(note);

        Awake();
    }
}
