using UnityEngine;
using System.Collections;

public class MoveNote : PlaceNote {

    protected override void Controls()
    {
        MovementControls();
    }

    public void Init(Note note)
    {
        this.note = new Note (note);
        controller.Init(this.note);
        editor.currentSelectedObject = this.note;
    }

    protected override void AddObject()
    {
        Note noteToAdd = new Note(note);
        editor.currentChart.Add(noteToAdd);
        editor.CreateNoteObject(noteToAdd);
        editor.currentSelectedObject = noteToAdd;
    }
}
