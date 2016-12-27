using UnityEngine;
using System.Collections.Generic;

public class PlaceNoteController : ObjectlessTool {

    public PlaceNote[] notes = new PlaceNote[7];        // Starts at multi-note before heading into green (1), red (2) through to open (6)

    protected override void Awake()
    {
        base.Awake();
        // Initialise the notes
        foreach (PlaceNote note in notes)
        {
            note.gameObject.SetActive(true);
            note.gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        // Update flags in the note panel
        if (editor.currentSelectedObject != null && editor.currentSelectedObject.GetType() == typeof(Note))
        {
            foreach (PlaceNote note in notes)
            {
                note.note.flags = ((Note)editor.currentSelectedObject).flags;
                //note.gameObject.SetActive(false);
            }
        }
    }

    void OnDisable()
    {
        
    }

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;
        Debug.Log("Disable");
        foreach (PlaceNote placeableNotes in notes)
        {
            placeableNotes.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    protected override void Update () {
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
            }
            else
                notes[6].gameObject.SetActive(true);
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
        }
        else
        {
            // Multi-note
            notes[0].gameObject.SetActive(true);
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
	}
}
