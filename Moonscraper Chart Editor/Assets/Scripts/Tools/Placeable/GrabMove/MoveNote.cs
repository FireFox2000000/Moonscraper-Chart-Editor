// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

// DEPRECATED IN FAVOUR OF GroupMove.cs
/*
using UnityEngine;
using System.Collections;

public class MoveNote : PlaceNote {
    public Note explicitPrevious = null, explicitNext = null;

    protected override void Controls()
    {
        MovementControls();
    }

    public void Init(Note note)
    {
        this.note = new Note (note);
        controller.note = this.note;
        editor.currentSelectedObject = this.note;
        initObject = this.note.Clone();
    }

    protected override void Update()
    {
        base.Update();

        if (explicitPrevious != null)
            note.previous = explicitPrevious;

        if (explicitNext != null)
            note.next = explicitNext;
    }

    protected override void AddObject()
    {
        System.Collections.Generic.List<ActionHistory.Action> noteRecord = new System.Collections.Generic.List<ActionHistory.Action>();
        noteRecord.Add(new ActionHistory.Delete(initObject));
        int arrayPos = SongObjectHelper.FindObjectPosition(note, editor.currentChart.notes);
        if (arrayPos != SongObjectHelper.NOTFOUND)       // Found an object that matches
        {
            if (!note.AllValuesCompare(editor.currentChart.notes[arrayPos]))
                // Object will changed, therefore record
                noteRecord.Add(new ActionHistory.Modify(editor.currentChart.notes[arrayPos], note));
        }
        else
        {
            noteRecord.Add(new ActionHistory.Add(note));
        }

        Note noteToAdd = new Note(note);

        editor.currentChart.Add(noteToAdd);
        //NoteController nCon = editor.CreateNoteObject(noteToAdd);
        standardOverwriteOpen(noteToAdd);

        noteRecord.AddRange(CapNoteCheck(noteToAdd));
        noteRecord.AddRange(ForwardCap(noteToAdd));     // Do this due to pasting from the clipboard

        // Check if the automatic un-force will kick in
        ActionHistory.Action forceCheck = AutoForcedCheck(noteToAdd);
        if (forceCheck != null)
            noteRecord.Insert(0, forceCheck);           // Insert at the start so that the modification happens at the end of the undo function, otherwise the natural force check prevents it from being forced

        editor.currentSelectedObject = noteToAdd;
        if (noteRecord.Count > 0 && !initObject.AllValuesCompare(noteToAdd))
            editor.actionHistory.Insert(noteRecord.ToArray());
    }
}
*/
