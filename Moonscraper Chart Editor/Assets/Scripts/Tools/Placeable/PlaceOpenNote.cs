// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class PlaceOpenNote : PlaceNote {

    protected void UpdateOpenPrevAndNext(int closestNoteArrayPos)
    {
        if (editor.currentChart.notes[closestNoteArrayPos] < note)
        {
            Note previous = GetPreviousOfOpen(note.position, editor.currentChart.notes[closestNoteArrayPos]);

            note.previous = previous;
            note.next = GetNextOfOpen(note.position, previous.next);
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
        if (previousNote == null || previousNote.position != openNotePos || (!previousNote.isChord && previousNote.position != openNotePos))
            return previousNote;
        else
            return GetPreviousOfOpen(openNotePos, previousNote.previous);
    }

    Note GetNextOfOpen(uint openNotePos, Note nextNote)
    {
        if (nextNote == null || nextNote.position != openNotePos || (!nextNote.isChord && nextNote.position != openNotePos))
            return nextNote;
        else
            return GetNextOfOpen(openNotePos, nextNote.next);
    }
}
