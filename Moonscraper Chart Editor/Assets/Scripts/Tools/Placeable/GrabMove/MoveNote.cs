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
        Note noteToAdd = new Note(note);
        editor.currentChart.Add(noteToAdd);
        editor.CreateNoteObject(noteToAdd).standardOverwriteOpen();
        editor.currentSelectedObject = noteToAdd;

        CapNoteCheck(noteToAdd);

        editor.actionHistory.Insert(new ActionHistory.Action[] { new ActionHistory.Delete(initObject), new ActionHistory.Add(noteToAdd) });
    }
}
