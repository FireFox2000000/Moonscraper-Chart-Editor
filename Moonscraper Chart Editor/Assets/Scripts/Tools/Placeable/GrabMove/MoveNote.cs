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
        controller.Init(this.note);
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
        int arrayPos = SongObject.FindObjectPosition(note, editor.currentChart.notes);
        if (arrayPos != Globals.NOTFOUND)       // Found an object that matches
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
        NoteController nCon = editor.CreateNoteObject(noteToAdd);
        nCon.standardOverwriteOpen();

        noteRecord.AddRange(CapNoteCheck(noteToAdd));
        noteRecord.AddRange(ForwardCap(noteToAdd));     // Do this due to pasting from the clipboard

        editor.currentSelectedObject = noteToAdd;
        editor.actionHistory.Insert(noteRecord.ToArray());
    }
}
