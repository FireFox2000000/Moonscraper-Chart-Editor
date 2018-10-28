// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public class PlaceNoteController : ObjectlessTool {
    public NotePropertiesPanelController panel;
    public PlaceNote[] standardPlaceableNotes = new PlaceNote[7];        // Starts at multi-note before heading into green (1), red (2) through to open (6)

    List<PlaceNote> allPlaceableNotes = new List<PlaceNote>();

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

    string GetOpenNoteInputKey(int laneCount)
    {
        int key = laneCount;
        return NumToStringLUT[(key + 1)];
    }

    readonly string[] NumToStringLUT = new string[]
    {
        0.ToString(),
        1.ToString(),
        2.ToString(),
        3.ToString(),
        4.ToString(),
        5.ToString(),
        6.ToString(),
        7.ToString(),
        8.ToString(),
        9.ToString(),
    };

    protected override void Awake()
    {
        base.Awake();
        // Initialise the notes
        foreach (PlaceNote note in standardPlaceableNotes)
        {
            note.gameObject.SetActive(true);
            note.gameObject.SetActive(false);
        }

        {
            allPlaceableNotes.AddRange(standardPlaceableNotes);
            allPlaceableNotes.Add(openNote);
            allPlaceableNotes.Add(multiNote);
        }

        int totalNotes = allPlaceableNotes.Count;
        heldInitialOverwriteActions = new ActionHistory.Action[totalNotes][];
        heldNotes = new Note[totalNotes];
        inputBlock = new bool[totalNotes];
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
        LaneInfo laneInfo = editor.laneInfo;

        if (!GameSettings.keysModeEnabled)
        {
            BurstRecordingInsertCheck(keysBurstAddHistory);
            KeysDraggedSustainRecordingCheck();
            MouseControlsBurstMode(laneInfo);
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
                            foreach (Note chordNote in heldNotes[i].chord)
                                chordNote.SetSustainByPos(objectSnappedChartPos);
                        }
                        else
                        {
                            // Controls sustain recording
                            KeySustainActionHistoryInsert(i);
                        }
                    }
                }

                KeyboardControlsSustainMode(laneInfo);
            }
            else
            {
                KeysDraggedSustainRecordingCheck();

                KeyboardControlsBurstMode(laneInfo);
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
        // Make sure the notes have been initialised
        foreach (PlaceNote placeableNotes in allPlaceableNotes)
        {
            placeableNotes.gameObject.SetActive(true);
            placeableNotes.gameObject.SetActive(false);
        }

        // Update flags in the note panel
        if (editor.currentSelectedObject != null && editor.currentSelectedObject.GetType() == typeof(Note))
        {
            foreach (PlaceNote note in allPlaceableNotes)
            {
                note.note.flags = ((Note)editor.currentSelectedObject).flags;
            }
        }
    }

    void KeyboardControlsSustainMode(LaneInfo laneInfo)
    {
        int laneCount = laneInfo.laneCount;
        bool isTyping = Services.IsTyping;

        for (int i = 0; i < heldNotes.Length; ++i)
        {
            // Add in the held note history when user lifts off the keys
            if (isTyping || Input.GetKeyUp(NumToStringLUT[(i + 1)]))
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

        if (isTyping)
            return;

        for (int i = 0; i < laneCount + 1; ++i)      // Start at 1 to ignore the multinote
        {                     
            // Need to make sure the note is at it's correct tick position
            if (Input.GetKeyDown(NumToStringLUT[(i + 1)]))
            {
                int notePos = i;

                if (Input.GetKeyDown(GetOpenNoteInputKey(laneCount)))
                {
                    notePos = allPlaceableNotes.IndexOf(openNote);
                }

                LeftyFlipReflectionCheck(ref notePos, laneCount);

                allPlaceableNotes[notePos].ExplicitUpdate();
                int pos = SongObjectHelper.FindObjectPosition(allPlaceableNotes[notePos].note, editor.currentChart.notes);

                if (pos == SongObjectHelper.NOTFOUND)
                {
                    Debug.Log("Added " + allPlaceableNotes[notePos].note.rawNote + " note at position " + allPlaceableNotes[notePos].note.tick + " using keyboard controls");
                    // #TODO
                    //heldInitialOverwriteActions[i] = PlaceNote.AddObjectToCurrentChart((Note)allPlaceableNotes[notePos].note.Clone(), editor, out heldNotes[i]);

                    //editor.actionHistory.Insert(PlaceNote.AddObjectToCurrentChart((Note)notes[notePos].note.Clone(), editor, out heldNotes[i - 1]));
                }
                else
                {
                    editor.actionHistory.Insert(new ActionHistory.Delete(editor.currentChart.notes[pos]));
                    Debug.Log("Removed " + editor.currentChart.notes[pos].rawNote + " note at position " + editor.currentChart.notes[pos].tick + " using keyboard controls");
                    editor.currentChart.notes[pos].Delete();
                }
            }
        }
    }

    void KeyboardControlsBurstMode(LaneInfo laneInfo)
    {
        int keysPressed = 0;
        int laneCount = laneInfo.laneCount;
        bool isTyping = Services.IsTyping;
        if (isTyping)
            return;

        for (int i = 0; i < laneCount + 1; ++i)
        {
            int index = i;
            if (index + 1 >= laneCount && keysPressed > 0)           // Prevents open notes while holding other keys
                continue;

            int inputOnKeyboard = index + 1;
            if (Input.GetKey(NumToStringLUT[inputOnKeyboard]) && !inputBlock[index])
            {
                ++keysPressed;
                int notePos = index;

                if (Input.GetKey(GetOpenNoteInputKey(laneCount)))
                {
                    notePos = allPlaceableNotes.IndexOf(openNote);
                }

                LeftyFlipReflectionCheck(ref notePos, laneCount);

                allPlaceableNotes[notePos].ExplicitUpdate();
                int pos = SongObjectHelper.FindObjectPosition(allPlaceableNotes[notePos].note, editor.currentChart.notes);

                if (pos == SongObjectHelper.NOTFOUND)
                {
                    Debug.Log("Not found");
                    // #TODO
                    //keysBurstAddHistory.AddRange(PlaceNote.AddObjectToCurrentChart((Note)allPlaceableNotes[notePos].note.Clone(), editor));
                }
                else if (Input.GetKeyDown(NumToStringLUT[inputOnKeyboard]))
                {
                    editor.actionHistory.Insert(new ActionHistory.Delete(editor.currentChart.notes[pos]));
                    Debug.Log("Removed " + editor.currentChart.notes[pos].rawNote + " note at position " + editor.currentChart.notes[pos].tick + " using keyboard controls");
                    editor.currentChart.notes[pos].Delete();
                    inputBlock[index] = true;
                }
            }
            else if (!Input.GetKey(NumToStringLUT[(index + 1)]))
            {
                inputBlock[index] = false;
            }
        }

        if (keysPressed == 0)
            BurstRecordingInsertCheck(keysBurstAddHistory);
    }

    List<PlaceNote> activeNotes = new List<PlaceNote>();
    void MouseControlsBurstMode(LaneInfo laneInfo)
    {
        activeNotes.Clear();
        bool openActive = false;
        if (openNote.gameObject.activeSelf)
            openActive = true;

        int maxLanes = laneInfo.laneCount;

        foreach (PlaceNote placeableNotes in allPlaceableNotes)
        {
            placeableNotes.gameObject.SetActive(false);
        }

        bool anyStandardKeyInput = false;
        for (int i = 0; i < maxLanes; ++i)
        {
            if (Input.GetKey(NumToStringLUT[(i + 1)]))
            {
                anyStandardKeyInput = true;
                break;
            }
        }

        // Select which notes to run based on keyboard input
        if (Input.GetKeyDown(GetOpenNoteInputKey(maxLanes)))  // Open note takes priority
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
        else if (!Input.GetKey(GetOpenNoteInputKey(maxLanes)) && (anyStandardKeyInput))
        {
            for (int i = 0; i < maxLanes; ++i)
            {
                int leftyPos = maxLanes - (i + 1);

                if (Input.GetKey(NumToStringLUT[(i + 1)]))
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
// #TODO
                //mouseBurstAddHistory.AddRange(placeNote.AddNoteWithRecord());
            }
        }
        else
        {
            BurstRecordingInsertCheck(mouseBurstAddHistory);
        }
    }

    void LeftyFlipReflectionCheck(ref int noteNumber, int laneCount)
    {
        if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip && noteNumber >= 0 && noteNumber < laneCount)
            noteNumber = laneCount - (noteNumber + 1);
    }
}
