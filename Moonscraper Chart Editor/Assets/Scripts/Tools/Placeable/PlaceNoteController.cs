using UnityEngine;
using System.Collections.Generic;

public class PlaceNoteController : ObjectlessTool {

    public PlaceNote[] notes = new PlaceNote[7];        // Starts at multi-note before heading into green (1), red (2) through to open (6)

    List<ActionHistory.Action> draggedNotesRecord;

    protected override void Awake()
    {
        base.Awake();
        // Initialise the notes
        foreach (PlaceNote note in notes)
        {
            note.gameObject.SetActive(true);
            note.gameObject.SetActive(false);
        }

        draggedNotesRecord = new List<ActionHistory.Action>();
    }

    public override void ToolEnable()
    {
        editor.currentSelectedObject = notes[0].note;
    }

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;

        foreach (PlaceNote placeableNotes in notes)
        {
            placeableNotes.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    protected override void Update () {
        if (!Globals.lockToStrikeline)
            MouseControls();
        else
            KeyboardControls();
    }

    void KeyboardControls()
    {
        foreach (PlaceNote placeableNotes in notes)
        {
            placeableNotes.gameObject.SetActive(false);
        }

        for (int i = 1; i < notes.Length; ++i)
        {
            // Need to make sure the note is at it's correct tick position
            if (Input.GetKeyDown(i.ToString()))
            {
                notes[i].ExplicitUpdate();
                int pos = SongObject.FindObjectPosition(notes[i].note, editor.currentChart.notes);

                if (pos == Globals.NOTFOUND)
                    PlaceNote.AddObjectToCurrentChart((Note)notes[i].note.Clone(), editor);
                else
                {
                    editor.currentChart.notes[pos].Delete();
                }
            }
        }
    }

    void MouseControls()
    {
        bool openActive = false;
        if (notes[6].gameObject.activeSelf)
            openActive = true;

        foreach (PlaceNote placeableNotes in notes)
        {
            placeableNotes.gameObject.SetActive(false);
        }

        List<PlaceNote> activeNotes = new List<PlaceNote>();

        // Select which notes to run based on keyboard input
        if (Input.GetKeyDown("6"))  // Open note takes priority
        {
            if (openActive)
            {
                notes[0].gameObject.SetActive(true);
                activeNotes.Add(notes[0]);
            }
            else
            {
                notes[6].gameObject.SetActive(true);
                activeNotes.Add(notes[6]);
            }
        }
        else if (!Input.GetKey("6") && (Input.GetKey("1") || Input.GetKey("2") || Input.GetKey("3") || Input.GetKey("4") || Input.GetKey("5")))
        {
            if (Input.GetKey("1"))
            {
                notes[1].gameObject.SetActive(true);
                activeNotes.Add(notes[1]);
            }
            if (Input.GetKey("2"))
            {
                notes[2].gameObject.SetActive(true);
                activeNotes.Add(notes[2]);
            }
            if (Input.GetKey("3"))
            {
                notes[3].gameObject.SetActive(true);
                activeNotes.Add(notes[3]);
            }
            if (Input.GetKey("4"))
            {
                notes[4].gameObject.SetActive(true);
                activeNotes.Add(notes[4]);
            }
            if (Input.GetKey("5"))
            {
                notes[5].gameObject.SetActive(true);
                activeNotes.Add(notes[5]);
            }
        }
        else if (openActive)
        {
            notes[6].gameObject.SetActive(true);
            activeNotes.Add(notes[6]);
        }
        else
        {
            // Multi-note
            notes[0].gameObject.SetActive(true);
            activeNotes.Add(notes[0]);
        }

        // Update prev and next if chord
        if (activeNotes.Count > 1)
        {
            for (int i = 0; i < activeNotes.Count; ++i)
            {
                if (i == 0)     // Start
                {
                    activeNotes[i].note.next = activeNotes[i + 1].note;
                }
                else if (i >= (activeNotes.Count - 1))      // End
                {
                    activeNotes[i].note.previous = activeNotes[i - 1].note;
                }
                else
                {
                    activeNotes[i].note.previous = activeNotes[i - 1].note;
                    activeNotes[i].note.next = activeNotes[i + 1].note;
                }
            }
        }

        // Update flags in the note panel
        if (editor.currentSelectedObject.GetType() == typeof(Note))
        {
            foreach (PlaceNote note in notes)
            {
                note.note.flags = ((Note)editor.currentSelectedObject).flags;
            }
        }

        // Place the notes down manuall then determine action history
        if (PlaceNote.addNoteCheck)
        {
            foreach (PlaceNote placeNote in activeNotes)
            {
                // Find if there's already note in that position. If the notes match exactly, add it to the list, but if it's the same, don't bother.
                draggedNotesRecord.AddRange(placeNote.AddNoteWithRecord());
            }
        }
        else
        {
            if (draggedNotesRecord.Count > 0)
            {
                editor.actionHistory.Insert(draggedNotesRecord.ToArray());
                draggedNotesRecord.Clear();
            }
        }
    }
}
