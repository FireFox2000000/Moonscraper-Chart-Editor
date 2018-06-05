// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public class PlaceNoteController : ObjectlessTool {
    public NotePropertiesPanelController panel;
    public PlaceNote[] standardPlaceableNotes = new PlaceNote[7];        // Starts at multi-note before heading into green (1), red (2) through to open (6)
    [HideInInspector]
    public PlaceNote[] allPlaceableNotes;
    [HideInInspector]
    public PlaceNote[] allKeyboardPlaceableNotes;
    [HideInInspector]
    public PlaceNote[] standardKeyboardPlaceableNotes;
    [HideInInspector]
    public PlaceNote[] ghlKeyboardPlaceableNotes;

    public PlaceNote multiNote;
    public PlaceNote openNote;

    // Mouse mode burst mode
    List<ActionHistory.Action> mouseBurstAddHistory = new List<ActionHistory.Action>();

    // Keyboard mode sustain dragging
    Note[] heldNotes;
    ActionHistory.Action[][] heldInitialOverwriteActions;

    // Keyboard mode burst mode
    bool[] inputBlock;        // Prevents controls from ocilating between placing and removing notes
    List<ActionHistory.Action> keysBurstAddHistory = new List<ActionHistory.Action>();

    //const int MULTI_NOTE_POS = 0;
    //const int OPEN_NOTE_POS = 5;
    int standardNoteLimit { get { return (Globals.ghLiveMode ? (int)Note.GHLiveGuitarFret.OPEN : (int)Note.GuitarFret.OPEN); } }
    string GetOpenNoteInputKey()
    {
        int key = standardNoteLimit;
        return (key + 1).ToString();
    }

    protected override void Awake()
    {
        base.Awake();
        // Initialise the notes
        foreach (PlaceNote note in standardPlaceableNotes)
        {
            note.gameObject.SetActive(true);
            note.gameObject.SetActive(false);
        }

        List<PlaceNote> notes = new List<PlaceNote>();
        {
            notes.AddRange(standardPlaceableNotes);
            notes.Add(openNote);
            notes.Add(multiNote);
            allPlaceableNotes = notes.ToArray();
            notes.Clear();
        }

        {
            notes.AddRange(standardPlaceableNotes);
            notes.Add(openNote);
            allKeyboardPlaceableNotes = notes.ToArray();

            int numberOfNotes = allKeyboardPlaceableNotes.Length;
            heldNotes = new Note[numberOfNotes];
            heldInitialOverwriteActions = new ActionHistory.Action[numberOfNotes][];
            inputBlock = new bool[numberOfNotes];
            notes.Clear();
        }

        {
            for (int i = 0; i < (int)Note.GuitarFret.OPEN; ++i)
                notes.Add(standardPlaceableNotes[i]);
            notes.Add(openNote);
            standardKeyboardPlaceableNotes = notes.ToArray();
            notes.Clear();
        }

        {
            for (int i = 0; i < (int)Note.GHLiveGuitarFret.OPEN; ++i)
                notes.Add(standardPlaceableNotes[i]);
            notes.Add(openNote);
            ghlKeyboardPlaceableNotes = notes.ToArray();
            notes.Clear();
        }
    }

    public override void ToolEnable()
    {
        editor.currentSelectedObject = multiNote.note;
    }

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;

        foreach (PlaceNote placeableNotes in allPlaceableNotes)
        {
            placeableNotes.gameObject.SetActive(false);
        }

        BurstRecordingInsertCheck(keysBurstAddHistory);
        BurstRecordingInsertCheck(mouseBurstAddHistory);
        KeysDraggedSustainRecordingCheck();
    }

    // Update is called once per frame
    protected override void Update () {
        if (!GameSettings.keysModeEnabled)
        {
            BurstRecordingInsertCheck(keysBurstAddHistory);
            KeysDraggedSustainRecordingCheck();
            MouseControlsBurstMode();
        }
        else
        {
            BurstRecordingInsertCheck(mouseBurstAddHistory);
            UpdateSnappedPos();
            KeysControlsInit();

            if (KeysNotePlacementModePanelController.currentPlacementMode == KeysNotePlacementModePanelController.PlacementMode.Sustain)
            {
                BurstRecordingInsertCheck(keysBurstAddHistory);

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
                            KeySustainActionHistoryInsert(i);
                        }
                    }
                }

                KeyboardControlsSustainMode();
            }
            else
            {
                KeysDraggedSustainRecordingCheck();

                KeyboardControlsBurstMode();
            }
        }
    }

    void KeysDraggedSustainRecordingCheck()
    {
        for (int i = 0; i < heldNotes.Length; ++i)
        {
            if (heldNotes[i] != null)
            {
                KeySustainActionHistoryInsert(i);
            }
        }
    }

    void BurstRecordingInsertCheck(List<ActionHistory.Action> burstHistory)
    {
        if (burstHistory.Count > 0)
        {
            editor.actionHistory.Insert(burstHistory.ToArray());
            burstHistory.Clear();
        }
    }

    void KeySustainActionHistoryInsert(int i)
    {
        if (heldNotes[i] != null && heldInitialOverwriteActions[i] != null)
        {
            editor.actionHistory.Insert(heldInitialOverwriteActions[i]);
            
            Note initialNote = new Note(heldNotes[i]);
            initialNote.length = 0;
            editor.actionHistory.Insert(new ActionHistory.Modify(initialNote, heldNotes[i]));
        }

        heldNotes[i] = null;
        heldInitialOverwriteActions[i] = null;
    }

    void KeysControlsInit()
    {
        foreach (PlaceNote placeableNotes in allKeyboardPlaceableNotes)
        {
            placeableNotes.gameObject.SetActive(true);
        }

        foreach (PlaceNote placeableNotes in allPlaceableNotes)
        {
            placeableNotes.gameObject.SetActive(false);
        }

        // Update flags in the note panel
        if (editor.currentSelectedObject != null && editor.currentSelectedObject.GetType() == typeof(Note))
        {
            foreach (PlaceNote note in allKeyboardPlaceableNotes)
            {
                note.note.flags = ((Note)editor.currentSelectedObject).flags;
            }
        }
    }

    void KeyboardControlsSustainMode()
    {
        PlaceNote[] keyboardPlaceableNotes = Globals.ghLiveMode ? ghlKeyboardPlaceableNotes : standardKeyboardPlaceableNotes; //allKeyboardPlaceableNotes;

        for (int i = 0; i < heldNotes.Length; ++i)
        {
            // Add in the held note history when user lifts off the keys
            if (Input.GetKeyUp((i + 1).ToString()))
            {
                KeySustainActionHistoryInsert(i);
            }
        }

        // Guard to prevent users from pressing keys while dragging out sustains
        if (!GameSettings.extendedSustainsEnabled)
        {
            foreach (Note heldNote in heldNotes)
            {
                if (heldNote != null && heldNote.length > 0)
                    return;
            }
        }

        for (int i = 0; i < keyboardPlaceableNotes.Length; ++i)      // Start at 1 to ignore the multinote
        {                     
            // Need to make sure the note is at it's correct tick position
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                int notePos = i;

                LeftyFlipReflectionCheck(ref notePos);

                keyboardPlaceableNotes[notePos].ExplicitUpdate();
                int pos = SongObjectHelper.FindObjectPosition(keyboardPlaceableNotes[notePos].note, editor.currentChart.notes);

                if (pos == SongObjectHelper.NOTFOUND)
                {
                    Debug.Log("Added " + keyboardPlaceableNotes[notePos].note.rawNote + " note at position " + keyboardPlaceableNotes[notePos].note.position + " using keyboard controls");
                    heldInitialOverwriteActions[i] = PlaceNote.AddObjectToCurrentChart((Note)keyboardPlaceableNotes[notePos].note.Clone(), editor, out heldNotes[i]);
                    
                    //editor.actionHistory.Insert(PlaceNote.AddObjectToCurrentChart((Note)notes[notePos].note.Clone(), editor, out heldNotes[i - 1]));
                }
                else
                {
                    editor.actionHistory.Insert(new ActionHistory.Delete(editor.currentChart.notes[pos]));
                    Debug.Log("Removed " + editor.currentChart.notes[pos].rawNote + " note at position " + editor.currentChart.notes[pos].position + " using keyboard controls");
                    editor.currentChart.notes[pos].Delete();
                }
            }
        }
    }

    void KeyboardControlsBurstMode()
    {
        int keysPressed = 0;

        PlaceNote[] keyboardPlaceableNotes = Globals.ghLiveMode ? ghlKeyboardPlaceableNotes : standardKeyboardPlaceableNotes; //allKeyboardPlaceableNotes;

        for (int i = 0; i < keyboardPlaceableNotes.Length; ++i)
        {
            if (i + 1 >= keyboardPlaceableNotes.Length && keysPressed > 0)           // Prevents open notes while holding other keys
                continue;
          
            if (Input.GetKey((i + 1).ToString()) && !inputBlock[i])
            {
                ++keysPressed;
                int notePos = i;

                LeftyFlipReflectionCheck(ref notePos);

                keyboardPlaceableNotes[notePos].ExplicitUpdate();

                int pos = SongObjectHelper.FindObjectPosition(keyboardPlaceableNotes[notePos].note, editor.currentChart.notes);

                if (pos == SongObjectHelper.NOTFOUND)
                {
                    Debug.Log("Not found");
                    keysBurstAddHistory.AddRange(PlaceNote.AddObjectToCurrentChart((Note)keyboardPlaceableNotes[notePos].note.Clone(), editor));
                }
                else if (Input.GetKeyDown(i.ToString()))
                {
                    editor.actionHistory.Insert(new ActionHistory.Delete(editor.currentChart.notes[pos]));
                    Debug.Log("Removed " + editor.currentChart.notes[pos].rawNote + " note at position " + editor.currentChart.notes[pos].position + " using keyboard controls");
                    editor.currentChart.notes[pos].Delete();
                    inputBlock[i] = true;
                }
            }
            else if (!Input.GetKey((i + 1).ToString()))
            {
                inputBlock[i] = false;
            }
        }

        if (keysPressed == 0)
            BurstRecordingInsertCheck(keysBurstAddHistory);
    }

    void MouseControlsBurstMode()
    {
        bool openActive = false;
        if (openNote.gameObject.activeSelf)
            openActive = true;

        foreach (PlaceNote placeableNotes in allPlaceableNotes)
        {
            placeableNotes.gameObject.SetActive(false);
        }

        bool anyStandardKeyInput = false;
        for (int i = 0; i < standardNoteLimit; ++i)
        {
            if (Input.GetKey((i + 1).ToString()))
            {
                anyStandardKeyInput = true;
                break;
            }
        }

        List<PlaceNote> activeNotes = new List<PlaceNote>();

        // Select which notes to run based on keyboard input
        if (Input.GetKeyDown(GetOpenNoteInputKey()))  // Open note takes priority
        {
            if (openActive)
            {
                multiNote.gameObject.SetActive(true);
                activeNotes.Add(multiNote);
            }
            else
            {
                openNote.gameObject.SetActive(true);
                activeNotes.Add(openNote);
            }
        }
        else if (!Input.GetKey(GetOpenNoteInputKey()) && (anyStandardKeyInput))
        {
            for (int i = 0; i < standardNoteLimit; ++i)
            {
                int leftyPos = standardNoteLimit - (i + 1);

                if (Input.GetKey((i + 1).ToString()))
                {
                    if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
                    {
                        standardPlaceableNotes[leftyPos].gameObject.SetActive(true);
                        activeNotes.Add(standardPlaceableNotes[leftyPos]);
                    }
                    else
                    {
                        standardPlaceableNotes[i].gameObject.SetActive(true);
                        activeNotes.Add(standardPlaceableNotes[i]);
                    }
                }
            }
        }
        else if (openActive)
        {
            openNote.gameObject.SetActive(true);
            activeNotes.Add(openNote);
        }
        else
        {
            // Multi-note
            multiNote.gameObject.SetActive(true);
            activeNotes.Add(multiNote);
        }

        // Update prev and next if chord
        if (activeNotes.Count > 1)
        {
            for (int i = 0; i < activeNotes.Count; ++i)
            {
                if (i == 0)     // Start
                {
                    //activeNotes[i].controller.note.previous = null;
                    activeNotes[i].controller.note.next = activeNotes[i + 1].note;
                }
                else if (i >= (activeNotes.Count - 1))      // End
                {
                    activeNotes[i].controller.note.previous = activeNotes[i - 1].note;
                    //activeNotes[i].controller.note.next = null;
                }
                else
                {
                    activeNotes[i].controller.note.previous = activeNotes[i - 1].note;
                    activeNotes[i].controller.note.next = activeNotes[i + 1].note;
                }

                // Visuals for some reason aren't being updated in this cycle
                activeNotes[i].visuals.UpdateVisuals();
            }

            //panel.currentNote = activeNotes[0].note;
        }
        else
        {
            //panel.currentNote = multiNote.note;
        }

        // Update flags in the note panel
        if (editor.currentSelectedObject.GetType() == typeof(Note))
        {
            foreach (PlaceNote note in standardPlaceableNotes)
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
                mouseBurstAddHistory.AddRange(placeNote.AddNoteWithRecord());
            }
        }
        else
        {
            BurstRecordingInsertCheck(mouseBurstAddHistory);
        }
    }

    void LeftyFlipReflectionCheck(ref int noteNumber)
    {
        if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip && noteNumber >= 0 && noteNumber < standardNoteLimit)
            noteNumber = standardNoteLimit - (noteNumber + 1);
    }
}
