using UnityEngine;
using System.Collections;

public class MoveNote : PlaceNote {

    protected override void Controls()
    {
        if (Input.GetMouseButtonUp(0))
        {
            AddObject();

            Destroy(gameObject);
        }
    }

    public void Init(Note note)
    {
        this.note = note;
        GetComponent<NoteController>().Init(note);
    }

    protected override void AddObject()
    {
        Note noteToAdd = new Note(note);
        editor.currentChart.Add(noteToAdd);
        editor.CreateNoteObject(noteToAdd);
        editor.currentSelectedNote = noteToAdd;
    }
}
