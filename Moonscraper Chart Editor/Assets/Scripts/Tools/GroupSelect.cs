using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupSelect : ToolObject {
    List<ChartObject> chartObjects = new List<ChartObject>();

    public override void ToolDisable()
    {
        chartObjects.Clear();
    }

    public void SetNoteType(Note.Note_Type type)
    {
        Note[] notes = chartObjects.OfType<Note>().ToArray();

        foreach (Note note in notes)
            note.SetNoteType(type);
    }
}
