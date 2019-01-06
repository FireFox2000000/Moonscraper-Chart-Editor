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
    List<SongObject> currentlyAddingNotes = new List<SongObject>();

    // Keyboard mode sustain dragging
    Note[] heldNotes;

    // Keyboard mode burst mode
    bool[] inputBlock;        // Prevents controls from ocilating between placing and removing notes

    delegate void NotePlacementUpdate();

    NotePlacementUpdate CurrentNotePlacementUpdate;
    enum KeysPlacementMode
    {
        None,
        Adding,
        Deleting,
    }
    KeysPlacementMode currentPlacementMode = KeysPlacementMode.None;

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

        CurrentNotePlacementUpdate = UpdateMouseBurstMode;

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
        heldNotes = new Note[totalNotes];
        inputBlock = new bool[totalNotes];

        EventsManager.onToolChangedEventList.Add(OnModeSwitch);
        EventsManager.onKeyboardModeToggledEvent.Add(OnKeysModeChanged);
        EventsManager.onNotePlacementModeChangedEvent.Add(OnModeSwitch);
    }

    public override void ToolEnable()
    {
        ResetNoteAdding();
        editor.currentSelectedObject = multiNote.note;
        OnModeSwitch();
    }

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;

        foreach (PlaceNote placeableNotes in allPlaceableNotes)
        {
            placeableNotes.gameObject.SetActive(false);
        }

        KeysDraggedSustainRecordingCheck();

        ResetNoteAdding();
    }

    void ResetNoteAdding()
    {
        currentlyAddingNotes.Clear();
        currentPlacementMode = KeysPlacementMode.None;
    }

    // Update is called once per frame
    protected override void Update () {
        CurrentNotePlacementUpdate();
    }

    void OnKeysModeChanged(bool keyboardModeEnabled)
    {
        OnModeSwitch();
    }

    void OnModeSwitch()
    {
        KeysDraggedSustainRecordingCheck();
        ResetNoteAdding();

        KeysControlsInit();

        if (GameSettings.keysModeEnabled)
        {
            if (KeysNotePlacementModePanelController.currentPlacementMode == KeysNotePlacementModePanelController.PlacementMode.Sustain)
                CurrentNotePlacementUpdate = UpdateKeysSustainMode;
            else
                CurrentNotePlacementUpdate = UpdateKeysBurstMode;
        }
        else
        {
            CurrentNotePlacementUpdate = UpdateMouseBurstMode;
        }
    }

    void UpdateMouseBurstMode()
    {
        LaneInfo laneInfo = editor.laneInfo;

        KeysDraggedSustainRecordingCheck();

        if (Input.GetMouseButtonUp(0))
            ResetNoteAdding();
        MouseControlsBurstMode(laneInfo);
    }

    void UpdateKeysBurstMode()
    {
        LaneInfo laneInfo = editor.laneInfo;
        UpdateSnappedPos();
        KeysDraggedSustainRecordingCheck();

        bool wantCommandPop = currentlyAddingNotes.Count > 0;
        int currentNoteCount = currentlyAddingNotes.Count;
        bool refreshActions = false;

        FillNotesKeyboardControlsBurstMode(laneInfo);

        refreshActions |= currentlyAddingNotes.Count != currentNoteCount;

        if (currentlyAddingNotes.Count > 0 && refreshActions)
        {
            if (wantCommandPop)
                editor.commandStack.Pop();

            if (currentPlacementMode == KeysPlacementMode.Adding)
            {
                editor.commandStack.Push(new SongEditAdd(currentlyAddingNotes));
            }
            else if (currentPlacementMode == KeysPlacementMode.Deleting)
            {
                editor.commandStack.Push(new SongEditDelete(currentlyAddingNotes));
            }
        }

        if (!HasKeysInput(laneInfo))
            ResetNoteAdding();
    }

    void UpdateKeysSustainMode()
    {
        LaneInfo laneInfo = editor.laneInfo;
        UpdateSnappedPos();
        bool wantCommandPop = currentlyAddingNotes.Count > 0;
        int currentNoteCount = currentlyAddingNotes.Count;
        bool refreshActions = false;

        FillNotesKeyboardControlsSustainMode(laneInfo);
        bool extendedSustainsEnabled = GameSettings.extendedSustainsEnabled;

        // Update sustain lengths of notes that are already in
        for (int i = 0; i < heldNotes.Length; ++i)
        {
            if (heldNotes[i] != null)  // Check if already inserted and no longer being held
            {
                foreach (Note chordNote in heldNotes[i].chord)
                {
                    if (chordNote.tick + chordNote.length < objectSnappedChartPos || (objectSnappedChartPos < chordNote.tick + chordNote.length && chordNote.length > 0))
                    {
                        chordNote.SetSustainByPos(objectSnappedChartPos, editor.currentSong, extendedSustainsEnabled);
                        Debug.Assert(chordNote.tick + chordNote.length == objectSnappedChartPos, "Sustain was set to an incorrect length");
                        refreshActions = true;
                    }
                }
            }
        }

        refreshActions |= currentlyAddingNotes.Count != currentNoteCount;

        if (currentlyAddingNotes.Count > 0 && refreshActions)
        {
            if (wantCommandPop)
                editor.commandStack.Pop();

            if (currentPlacementMode == KeysPlacementMode.Adding)
            {
                editor.commandStack.Push(new SongEditAdd(currentlyAddingNotes));
            }
            else if (currentPlacementMode == KeysPlacementMode.Deleting)
            {
                editor.commandStack.Push(new SongEditDelete(currentlyAddingNotes));
            }
        }

        if (!HasKeysInput(laneInfo))
            ResetNoteAdding();
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

    void KeySustainActionHistoryInsert(int i)
    {
        heldNotes[i] = null;
    }

    void KeysControlsInit()
    {
        currentPlacementMode = KeysPlacementMode.None;

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

    void FillNotesKeyboardControlsSustainMode(LaneInfo laneInfo)
    {
        int laneCount = laneInfo.laneCount;
        bool isTyping = Services.IsTyping;

        // Tell the system to stop updating the sustain length
        for (int i = 0; i < heldNotes.Length; ++i)
        {
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

        bool openNotesBanned, nonOpenNotesBanned;
        CheckBannedInputsForSustainHolds(out openNotesBanned, out nonOpenNotesBanned);
        if (nonOpenNotesBanned)
            return;

        for (int i = 0; i < laneCount + 1; ++i)      // Start at 1 to ignore the multinote
        {                     
            // Need to make sure the note is at it's correct tick position
            if (Input.GetKeyDown(NumToStringLUT[(i + 1)]))
            {
                int notePos = i;

                if (Input.GetKeyDown(GetOpenNoteInputKey(laneCount)))
                {
                    if (openNotesBanned)     // Ban conflicting inputs as the command stack REALLY doesn't like this.
                        continue;

                    notePos = allPlaceableNotes.IndexOf(openNote);
                }

                LeftyFlipReflectionCheck(ref notePos, laneCount);

                allPlaceableNotes[notePos].ExplicitUpdate();
                int pos = SongObjectHelper.FindObjectPosition(allPlaceableNotes[notePos].note, editor.currentChart.notes);

                if (currentPlacementMode == KeysPlacementMode.None)
                {
                    currentPlacementMode = pos == SongObjectHelper.NOTFOUND ? KeysPlacementMode.Adding : KeysPlacementMode.Deleting;
                }

                if (currentPlacementMode == KeysPlacementMode.Adding && pos == SongObjectHelper.NOTFOUND)
                {
                    heldNotes[i] = allPlaceableNotes[notePos].note.CloneAs<Note>();
                    heldNotes[i].length = 0;
                    currentlyAddingNotes.Add(heldNotes[i]);
                    Debug.Log("Added " + allPlaceableNotes[notePos].note.rawNote + " note at position " + allPlaceableNotes[notePos].note.tick + " using keyboard controls");
                }
                else if (currentPlacementMode == KeysPlacementMode.Deleting)
                {
                    currentlyAddingNotes.Add(editor.currentChart.notes[pos]);
                    Debug.Log("Removed " + editor.currentChart.notes[pos].rawNote + " note at position " + editor.currentChart.notes[pos].tick + " using keyboard controls");
                }
            }
        }
    }

    void FillNotesKeyboardControlsBurstMode(LaneInfo laneInfo)
    {
        int keysPressed = 0;
        int laneCount = laneInfo.laneCount;
        bool isTyping = Services.IsTyping;
        if (isTyping)
            return;

        for (int i = 0; i < laneCount + 1; ++i)
        {
            int index = i;

            bool isOpenInput = index >= laneCount;
            if (isOpenInput && keysPressed > 0)           // Prevents open notes while holding other keys
                continue;

            int inputOnKeyboard = index + 1;
            if (Input.GetKey(NumToStringLUT[inputOnKeyboard]) && !inputBlock[index])
            {
                ++keysPressed;
                int notePos = index;

                if (isOpenInput)
                {
                    notePos = allPlaceableNotes.IndexOf(openNote);
                }

                LeftyFlipReflectionCheck(ref notePos, laneCount);

                allPlaceableNotes[notePos].ExplicitUpdate();
                int pos = SongObjectHelper.FindObjectPosition(allPlaceableNotes[notePos].note, editor.currentChart.notes);

                if (currentPlacementMode == KeysPlacementMode.None)
                {
                    currentPlacementMode = pos == SongObjectHelper.NOTFOUND ? KeysPlacementMode.Adding : KeysPlacementMode.Deleting;
                }

                if (currentPlacementMode == KeysPlacementMode.Adding && pos == SongObjectHelper.NOTFOUND)
                {
                    Debug.Log("Adding note");
                    currentlyAddingNotes.Add(allPlaceableNotes[notePos].note.Clone());
                }
                else if (Input.GetKeyDown(NumToStringLUT[inputOnKeyboard]) && currentPlacementMode == KeysPlacementMode.Deleting)
                {
                    Debug.Log("Removed " + editor.currentChart.notes[pos].rawNote + " note at position " + editor.currentChart.notes[pos].tick + " using keyboard controls");
                    currentlyAddingNotes.Add(editor.currentChart.notes[pos]);
                    inputBlock[index] = true;
                }
            }
            else if (!Input.GetKey(NumToStringLUT[(index + 1)]))
            {
                inputBlock[index] = false;
            }
        }
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

        bool wantCommandPop = currentlyAddingNotes.Count > 0;
        int currentNoteCount = currentlyAddingNotes.Count;

        // Place the notes down manually then determine action history
        if (PlaceNote.addNoteCheck)
        {
            foreach (PlaceNote placeNote in activeNotes)
            {
                // Find if there's already note in that position. If the notes match exactly, add it to the list, but if it's the same, don't bother.
                bool isNewNote = true;
                foreach(Note note in currentlyAddingNotes)
                {
                    if (note.AllValuesCompare(placeNote.note))
                    {
                        isNewNote = false;
                    }
                }
                if (isNewNote)
                    currentlyAddingNotes.Add(placeNote.note.CloneAs<Note>());
            }
        }

        if (currentlyAddingNotes.Count > 0 && currentlyAddingNotes.Count != currentNoteCount)
        {
            if (wantCommandPop)
                editor.commandStack.Pop();

            editor.commandStack.Push(new SongEditAdd(currentlyAddingNotes));
        }
    }

    void LeftyFlipReflectionCheck(ref int noteNumber, int laneCount)
    {
        if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip && noteNumber >= 0 && noteNumber < laneCount)
            noteNumber = laneCount - (noteNumber + 1);
    }

    bool HasKeysInput(LaneInfo laneInfo)
    {
        int laneCount = laneInfo.laneCount;
        for (int i = 0; i < laneCount + 1; ++i)      // Start at 1 to ignore the multinote
        {
            // Need to make sure the note is at it's correct tick position
            if (Input.GetKey(NumToStringLUT[(i + 1)]))
                return true;
        }

        return false;
    }

    void CheckBannedInputsForSustainHolds(out bool openNotesBanned, out bool nonOpenNotesBanned)
    {
        openNotesBanned = false;
        nonOpenNotesBanned = false;

        bool isHoldingNotes = false;
        foreach (var note in heldNotes)
        {
            isHoldingNotes |= note != null;
            if (isHoldingNotes && note.IsOpenNote())
            {
                // Ban conflicting inputs as the command stack REALLY doesn't like this.
                nonOpenNotesBanned = true;
                return;
            }

            if (isHoldingNotes)
            {
                openNotesBanned = true;
                break;
            }
        }
    }
}
