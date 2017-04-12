using UnityEngine;
using System.Collections.Generic;

public class PlaceNoteController : ObjectlessTool {

    public PlaceNote[] notes = new PlaceNote[7];        // Starts at multi-note before heading into green (1), red (2) through to open (6)

    List<ActionHistory.Action> draggedBurstNotesRecord;

    protected override void Awake()
    {
        base.Awake();
        // Initialise the notes
        foreach (PlaceNote note in notes)
        {
            note.gameObject.SetActive(true);
            note.gameObject.SetActive(false);
        }

        draggedBurstNotesRecord = new List<ActionHistory.Action>();
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

    Note[] heldNotes = new Note[6];
    ActionHistory.Action[][] heldInitialOverwriteActions = new ActionHistory.Action[6][];

    // Update is called once per frame
    protected override void Update () {
        if (!Globals.lockToStrikeline)
        {
            burstRecordingInsertCheck();
            draggedSustainRecordingCheck();
            MouseControls();
        }
        else
        {
            UpdateSnappedPos();
            KeysControlsInit();

            if (KeysNotePlacementModePanelController.currentPlacementMode == KeysNotePlacementModePanelController.PlacementMode.Sustain)
            {
                burstRecordingInsertCheck();

                for (int i = 0; i < heldNotes.Length; ++i)
                {
                    if (heldNotes[i] != null)
                    {
                        if (heldNotes[i].song != null)
                        {
                            foreach (Note chordNote in heldNotes[i].GetChord())
                                chordNote.SetSustainByPos(objectSnappedChartPos);
                        }
                        else
                        {
                            // Controls sustain recording
                            keySustainActionHistoryInsert(i);
                        }
                    }
                }

                KeyboardControlsSustainMode();
            }
            else
            {
                draggedSustainRecordingCheck();

                KeyboardControlsBurstMode();
            }
        }
    }

    void draggedSustainRecordingCheck()
    {
        for (int i = 0; i < heldNotes.Length; ++i)
        {
            if (heldNotes[i] != null)
            {
                keySustainActionHistoryInsert(i);
            }
        }
    }

    void burstRecordingInsertCheck()
    {
        if (burstAddHistory.Count > 0)
        {
            editor.actionHistory.Insert(burstAddHistory.ToArray());
            burstAddHistory.Clear();
        }
    }

    void keySustainActionHistoryInsert(int i)
    {
        if (heldNotes[i] != null && heldInitialOverwriteActions[i] != null)
        {
            editor.actionHistory.Insert(heldInitialOverwriteActions[i]);
            
            Note initialNote = new Note(heldNotes[i]);
            initialNote.sustain_length = 0;
            editor.actionHistory.Insert(new ActionHistory.Modify(initialNote, heldNotes[i]));
        }

        heldNotes[i] = null;
        heldInitialOverwriteActions[i] = null;
    }

    void KeysControlsInit()
    {
        foreach (PlaceNote placeableNotes in notes)
        {
            placeableNotes.gameObject.SetActive(false);
        }

        // Update flags in the note panel
        if (editor.currentSelectedObject != null && editor.currentSelectedObject.GetType() == typeof(Note))
        {
            foreach (PlaceNote note in notes)
            {
                note.note.flags = ((Note)editor.currentSelectedObject).flags;
            }
        }
    }

    void KeyboardControlsSustainMode()
    {
        for (int i = 0; i < heldNotes.Length; ++i)
        {
            // Add in the held note history when user lifts off the keys
            if (Input.GetKeyUp((i + 1).ToString()))
            {
                keySustainActionHistoryInsert(i);
            }
        }

        // Guard to prevent users from pressing keys while dragging out sustains
        if (!Globals.extendedSustainsEnabled)
        {
            foreach (Note heldNote in heldNotes)
            {
                if (heldNote != null && heldNote.sustain_length > 0)
                    return;
            }
        }

        for (int i = 1; i < notes.Length; ++i)      // Start at 1 to ignore the multinote
        {                     
            // Need to make sure the note is at it's correct tick position
            if (Input.GetKeyDown(i.ToString()))
            {
                int notePos = i;

                if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip && notePos > 0 && notePos < 6)
                    notePos = 6 - notePos;

                notes[notePos].ExplicitUpdate();
                int pos = SongObject.FindObjectPosition(notes[notePos].note, editor.currentChart.notes);

                if (pos == SongObject.NOTFOUND)
                {
                    Debug.Log("Added " + notes[notePos].note.fret_type + " note at position " + notes[notePos].note.position + " using keyboard controls");
                    heldInitialOverwriteActions[i - 1] = PlaceNote.AddObjectToCurrentChart((Note)notes[notePos].note.Clone(), editor, out heldNotes[i - 1]);
                    
                    //editor.actionHistory.Insert(PlaceNote.AddObjectToCurrentChart((Note)notes[notePos].note.Clone(), editor, out heldNotes[i - 1]));
                }
                else
                {
                    editor.actionHistory.Insert(new ActionHistory.Delete(editor.currentChart.notes[pos]));
                    Debug.Log("Removed " + editor.currentChart.notes[pos].fret_type + " note at position " + editor.currentChart.notes[pos].position + " using keyboard controls");
                    editor.currentChart.notes[pos].Delete();
                }
            }
        }
    }

    bool[] inputBlock = new bool[6];        // Prevents controls from ocilating between placing and removing notes
    List<ActionHistory.Action> burstAddHistory = new List<ActionHistory.Action>();

    void KeyboardControlsBurstMode()
    {
        int keysPressed = 0;
        for (int i = 1; i < notes.Length; ++i)      // Start at 1 to ignore the multinote
        {
            if (i + 1 >= notes.Length && keysPressed > 0)           // Prevents open notes while holding other keys
                continue;
          
            if (Input.GetKey(i.ToString()) && !inputBlock[i - 1])
            {
                ++keysPressed;
                int notePos = i;

                if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip && notePos > 0 && notePos < 6)
                    notePos = 6 - notePos;

                notes[notePos].ExplicitUpdate();

                int pos = SongObject.FindObjectPosition(notes[notePos].note, editor.currentChart.notes);

                if (pos == SongObject.NOTFOUND)
                {
                    burstAddHistory.AddRange(PlaceNote.AddObjectToCurrentChart((Note)notes[notePos].note.Clone(), editor));
                }
                else if (Input.GetKeyDown(i.ToString()))
                {
                    editor.actionHistory.Insert(new ActionHistory.Delete(editor.currentChart.notes[pos]));
                    Debug.Log("Removed " + editor.currentChart.notes[pos].fret_type + " note at position " + editor.currentChart.notes[pos].position + " using keyboard controls");
                    editor.currentChart.notes[pos].Delete();
                    inputBlock[i - 1] = true;
                }
            }
            else if (!Input.GetKey(i.ToString()))
            {
                inputBlock[i - 1] = false;
            }
        }

        if (keysPressed == 0)
            burstRecordingInsertCheck();
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
                if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
                {
                    notes[5].gameObject.SetActive(true);
                    activeNotes.Add(notes[5]);
                }
                else
                {
                    notes[1].gameObject.SetActive(true);
                    activeNotes.Add(notes[1]);
                }
            }
            if (Input.GetKey("2"))
            {
                if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
                {
                    notes[4].gameObject.SetActive(true);
                    activeNotes.Add(notes[4]);
                }
                else
                {
                    notes[2].gameObject.SetActive(true);
                    activeNotes.Add(notes[2]);
                }
            }
            if (Input.GetKey("3"))
            {
                notes[3].gameObject.SetActive(true);
                activeNotes.Add(notes[3]);
            }
            if (Input.GetKey("4"))
            {
                if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
                {
                    notes[2].gameObject.SetActive(true);
                    activeNotes.Add(notes[2]);
                }
                else
                {
                    notes[4].gameObject.SetActive(true);
                    activeNotes.Add(notes[4]);
                }
            }
            if (Input.GetKey("5"))
            {
                if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
                {
                    notes[1].gameObject.SetActive(true);
                    activeNotes.Add(notes[1]);
                }
                else
                {
                    notes[5].gameObject.SetActive(true);
                    activeNotes.Add(notes[5]);
                }
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
                    activeNotes[i].controller.note.previous = null;
                    activeNotes[i].controller.note.next = activeNotes[i + 1].note;
                }
                else if (i >= (activeNotes.Count - 1))      // End
                {
                    activeNotes[i].controller.note.previous = activeNotes[i - 1].note;
                    activeNotes[i].controller.note.next = null;
                }
                else
                {
                    activeNotes[i].controller.note.previous = activeNotes[i - 1].note;
                    activeNotes[i].controller.note.next = activeNotes[i + 1].note;
                }

                // Visuals for some reason aren't being updated in this cycle
                activeNotes[i].visuals.UpdateVisuals();
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

        // Place the notes down manually then determine action history
        if (PlaceNote.addNoteCheck)
        {
            foreach (PlaceNote placeNote in activeNotes)
            {
                // Find if there's already note in that position. If the notes match exactly, add it to the list, but if it's the same, don't bother.
                draggedBurstNotesRecord.AddRange(placeNote.AddNoteWithRecord());
            }
        }
        else
        {
            if (draggedBurstNotesRecord.Count > 0)
            {
                editor.actionHistory.Insert(draggedBurstNotesRecord.ToArray());
                draggedBurstNotesRecord.Clear();
            }
        }
    }
}
