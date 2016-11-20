using UnityEngine;
using System.Collections;

public class MoveNote : PlaceNote {

    protected override void Controls()
    {
        if (Input.GetButtonUp("Add Object"))
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
}
