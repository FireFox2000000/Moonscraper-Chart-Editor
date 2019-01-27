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
        int pos = SongObjectHelper.FindClosestPosition(note.tick, editor.currentChart.notes);
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
            Note previous = GetPreviousOfOpen(note.tick, editor.currentChart.notes[closestNoteArrayPos]);

            note.previous = previous;
            if (previous != null)
                note.next = GetNextOfOpen(note.tick, previous.next);
            else
                note.next = GetNextOfOpen(note.tick, editor.currentChart.notes[closestNoteArrayPos]);
        }
        else if (editor.currentChart.notes[closestNoteArrayPos] > note)
        {
            Note next = GetNextOfOpen(note.tick, editor.currentChart.notes[closestNoteArrayPos]);

            note.next = next;
            note.previous = GetPreviousOfOpen(note.tick, next.previous);
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
        if (previousNote == null || previousNote.tick != openNotePos || (!previousNote.isChord && previousNote.tick != openNotePos))
            return previousNote;
        else
            return GetPreviousOfOpen(openNotePos, previousNote.previous);
    }

    Note GetNextOfOpen(uint openNotePos, Note nextNote)
    {
        if (nextNote == null || nextNote.tick != openNotePos || (!nextNote.isChord && nextNote.tick != openNotePos))
            return nextNote;
        else
            return GetNextOfOpen(openNotePos, nextNote.next);
    }

    protected virtual void UpdateFretType()
    {
        if (!note.IsOpenNote() && Mouse.world2DPosition != null)
        {
            Vector2 mousePosition = (Vector2)Mouse.world2DPosition;
            mousePosition.x += horizontalMouseOffset;
            note.rawNote = XPosToNoteNumber(mousePosition.x, editor.laneInfo);
        }
    }

    public static int XPosToNoteNumber(float xPos, LaneInfo laneInfo)
    {
        if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
            xPos *= -1;

        float startPos = LaneInfo.positionRangeMin;
        float endPos = LaneInfo.positionRangeMax;

        int max = laneInfo.laneCount - 1;
        float factor = (endPos - startPos) / (max);

        for (int i = 0; i < max; ++i)
        {
            float currentPosCheck = startPos + i * factor + factor / 2.0f;
            if (xPos < currentPosCheck)
                return i;
        }

        return max;
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(this.note));
        editor.SelectSongObject(note, editor.currentChart.notes);
    }

    protected static void standardOverwriteOpen(Note note)
    {
        if (!note.IsOpenNote() && MenuBar.currentInstrument != Song.Instrument.Drums)
        {
            int index, length;
            SongObjectHelper.FindObjectsAtPosition(note.tick, note.chart.notes, out index, out length);

            // Check for open notes and delete
            for (int i = index; i < index + length; ++i)
            //foreach (Note chordNote in chordNotes)
            {
                Note chordNote = note.chart.notes[i];
                if (chordNote.IsOpenNote())
                {
                    chordNote.Delete();
                }
            }
        }
    }
}
