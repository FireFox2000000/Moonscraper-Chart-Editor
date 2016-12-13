using UnityEngine;
using System.Collections;

public class MoveNote : PlaceNote {

    protected override void Awake()
    {
        base.Awake();
        //Debug.Log(note.flags);
    }
    protected override void Controls()
    {
        if (Input.GetMouseButtonUp(0))
        {
            AddObject();

            Destroy(gameObject);
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Update()
    {
        base.Update();
    }

    public void Init(Note note)
    {
        this.note = new Note (note);
        controller.Init(this.note);
        //Debug.Log(note.flags);
    }

    protected override void AddObject()
    {
        Note noteToAdd = new Note(note);
        editor.currentChart.Add(noteToAdd);
        editor.CreateNoteObject(noteToAdd);
        editor.currentSelectedObject = noteToAdd;
    }
}
