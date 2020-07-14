// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using MoonscraperChartEditor.Song;

public class PlaceNoteController : ObjectlessTool {
    public PlaceNote[] standardPlaceableNotes = new PlaceNote[7];        // Starts at multi-note before heading into green (1), red (2) through to open (6)

    List<PlaceNote> allPlaceableNotes = new List<PlaceNote>();

    public PlaceNote multiNote;
    public PlaceNote openNote;

    // Mouse mode burst mode
    List<SongObject> currentlyAddingNotes = new List<SongObject>();

    // Keys sustain mode
    List<SongEditCommand> keyControlsCommands = new List<SongEditCommand>();

    [HideInInspector]
    public Note.Flags desiredFlags;
    public bool forcedInteractable { get; private set; }
    public bool tapInteractable { get; private set; }
    public bool cymbalInteractable { get; private set; }

    // Keyboard mode sustain dragging
    Note[] heldNotes;

    // Keyboard mode burst mode
    bool[] inputBlock;        // Prevents controls from ocilating between placing and removing notes

    public delegate void NotePlacementUpdate();

    public NotePlacementUpdate CurrentNotePlacementUpdate;
    enum KeysPlacementMode
    {
        None,
        Adding,
        Deleting,
    }
    KeysPlacementMode currentPlacementMode = KeysPlacementMode.None;

    readonly MSChartEditorInputActions[] NumToLaneActionLUT = new MSChartEditorInputActions[]
    {
        MSChartEditorInputActions.ToolNoteLane1,
        MSChartEditorInputActions.ToolNoteLane2,
        MSChartEditorInputActions.ToolNoteLane3,
        MSChartEditorInputActions.ToolNoteLane4,
        MSChartEditorInputActions.ToolNoteLane5,
        MSChartEditorInputActions.ToolNoteLane6,
    };

    MSChartEditorInputActions GetInputForNoteIndex(int index, int laneCount)
    {
        if (index >= laneCount)
        {
            return MSChartEditorInputActions.ToolNoteLaneOpen;
        }

        return NumToLaneActionLUT[index];
    }

    protected override void Awake()
    {
        base.Awake();

        desiredFlags = Note.Flags.None;
        forcedInteractable = true;
        tapInteractable = true;
        cymbalInteractable = true;

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

        editor.events.toolChangedEvent.Register(OnModeSwitch);
        editor.events.keyboardModeToggledEvent.Register(OnKeysModeChanged);
        editor.events.notePlacementModeChangedEvent.Register(OnModeSwitch);
        editor.events.editorStateChangedEvent.Register(OnApplicationModeSwitch);
    }

    public override void ToolEnable()
    {
        ResetNoteAdding();
        editor.selectedObjectsManager.currentSelectedObject = multiNote.note;
        OnModeSwitch();
    }

    public override void ToolDisable()
    {
        editor.selectedObjectsManager.currentSelectedObject = null;

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
        keyControlsCommands.Clear();
    }

    // Update is called once per frame
    protected override void Update () {
        // Needs to be in a specific order from NotePropertiesPanelController        
        CurrentNotePlacementUpdate();
        SetAllFlags(GetDisplayFlags());

        if (editor.selectedObjectsManager.currentSelectedObject == null)
            editor.selectedObjectsManager.currentSelectedObject = multiNote.note;
    }

    void OnKeysModeChanged(in bool keyboardModeEnabled)
    {
        OnModeSwitch();
    }

    void OnModeSwitch()
    {
        KeysDraggedSustainRecordingCheck();
        ResetNoteAdding();

        KeysControlsInit();

        if (Globals.gameSettings.keysModeEnabled)
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

    void OnApplicationModeSwitch(in ChartEditor.State editorState)
    {
        KeysDraggedSustainRecordingCheck();
        ResetNoteAdding();
    }

    public void SetAllFlags(Note.Flags flags)
    {
        foreach (PlaceNote note in allPlaceableNotes)
        {
            note.note.flags = flags;
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
        forcedInteractable = true;
        tapInteractable = true;
        cymbalInteractable = true;

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
        forcedInteractable = true;
        tapInteractable = true;
        cymbalInteractable = true;

        LaneInfo laneInfo = editor.laneInfo;
        UpdateSnappedPos();
        bool wantCommandPop = keyControlsCommands.Count > 0;
        int currentCommandCount = keyControlsCommands.Count;
        bool refreshActions = false;

        FillNotesKeyboardControlsSustainMode(laneInfo);
        bool extendedSustainsEnabled = Globals.gameSettings.extendedSustainsEnabled;

        // Update sustain lengths of notes that are already in
        for (int i = 0; i < heldNotes.Length; ++i)
        {
            if (heldNotes[i] != null)  // Check if already inserted and no longer being held
            {
                foreach (Note chordNote in heldNotes[i].chord)
                {
                    if (chordNote.tick + chordNote.length < objectSnappedChartPos || (objectSnappedChartPos < chordNote.tick + chordNote.length && chordNote.length > 0))
                    {
                        int pos = SongObjectHelper.FindObjectPosition(chordNote, editor.currentChart.chartObjects);
                        Debug.Assert(pos != SongObjectHelper.NOTFOUND);

                        Note oldNote = editor.currentChart.chartObjects[pos] as Note;
                        chordNote.SetSustainByPos(objectSnappedChartPos, editor.currentSong, extendedSustainsEnabled);
                        keyControlsCommands.Add(new SongEditModifyValidated(oldNote, chordNote));

                        Debug.Assert(chordNote.tick + chordNote.length == objectSnappedChartPos, "Sustain was set to an incorrect length");
                        refreshActions = true;
                    }
                }
            }
        }

        refreshActions |= keyControlsCommands.Count != currentCommandCount;

        if (keyControlsCommands.Count > 0 && refreshActions)
        {
            if (wantCommandPop)
                editor.commandStack.Pop();

            editor.commandStack.Push(new BatchedSongEditCommand(keyControlsCommands));
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
                ClearHeldNotes(i);
            }
        }
    }

    void ClearHeldNotes(int i)
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
    }

    void FillNotesKeyboardControlsSustainMode(LaneInfo laneInfo)
    {
        int laneCount = laneInfo.laneCount;
        bool isTyping = Services.IsTyping;

        // Tell the system to stop updating the sustain length
        for (int i = 0; i < heldNotes.Length; ++i)
        {
            if (isTyping || MSChartEditorInput.GetInputUp(GetInputForNoteIndex(i, laneCount)))
            {
                ClearHeldNotes(i);
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
            if (MSChartEditorInput.GetInputDown(GetInputForNoteIndex(i, laneCount)))
            {
                int notePos = i;

                if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolNoteLaneOpen))
                {
                    if (openNotesBanned)     // Ban conflicting inputs as the command stack REALLY doesn't like this.
                        continue;

                    notePos = allPlaceableNotes.IndexOf(openNote);
                }

                LeftyFlipReflectionCheck(ref notePos, laneCount);

                allPlaceableNotes[notePos].ExplicitUpdate();
                int pos = SongObjectHelper.FindObjectPosition(allPlaceableNotes[notePos].note, editor.currentChart.chartObjects);

                if (pos == SongObjectHelper.NOTFOUND)
                {
                    Note newNote = allPlaceableNotes[notePos].note.CloneAs<Note>();            
                    newNote.length = 0;

                    keyControlsCommands.Add(new SongEditAdd(newNote));
                    heldNotes[i] = newNote;
                }
                else
                {
                    Note note = editor.currentChart.chartObjects[pos] as Note;
                    keyControlsCommands.Add(new SongEditDelete(note));
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

            int inputOnKeyboard = index;
            if (MSChartEditorInput.GetInput(GetInputForNoteIndex(inputOnKeyboard, laneCount)) && !inputBlock[index])
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
                else if (MSChartEditorInput.GetInputDown(GetInputForNoteIndex(inputOnKeyboard, laneCount)) && currentPlacementMode == KeysPlacementMode.Deleting)
                {
                    Debug.Log("Removed " + editor.currentChart.notes[pos].rawNote + " note at position " + editor.currentChart.notes[pos].tick + " using keyboard controls");
                    currentlyAddingNotes.Add(editor.currentChart.notes[pos]);
                    inputBlock[index] = true;
                }
            }
            else if (!MSChartEditorInput.GetInput(GetInputForNoteIndex(i, laneCount)))
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

        bool anyStandardKeyInput = false;
        for (int i = 0; i < maxLanes; ++i)
        {
            if (MSChartEditorInput.GetInput(NumToLaneActionLUT[i]))
            {
                anyStandardKeyInput = true;
                break;
            }
        }

        // Select which notes to run based on keyboard input
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolNoteLaneOpen))  // Open note takes priority
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
        else if (!MSChartEditorInput.GetInput(MSChartEditorInputActions.ToolNoteLaneOpen) && (anyStandardKeyInput))
        {
            for (int i = 0; i < maxLanes; ++i)
            {
                int leftyPos = maxLanes - (i + 1);

                if (MSChartEditorInput.GetInput(NumToLaneActionLUT[i]))
                {
                    if (Globals.gameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
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

        foreach (PlaceNote placeableNotes in allPlaceableNotes)
        {
            if (!activeNotes.Contains(placeableNotes) && placeableNotes.isActiveAndEnabled)
            {
                placeableNotes.gameObject.SetActive(false);
            }
        }

        // Update prev and next
        UpdateNoteLinkedListRefs(activeNotes);

        Note primaryActiveNote = activeNotes[0].note;
        editor.selectedObjectsManager.currentSelectedObject = primaryActiveNote;

        // Update flags in the note panel
        foreach (PlaceNote note in standardPlaceableNotes)
        {
            note.note.flags = primaryActiveNote.flags;
        }

        UpdateFlagsInteractable(primaryActiveNote);

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

            // Hackfix, re-update linked-list bindings so that it doesn't screw with the note properties panel
            // Otherwise previous note references on note tool and in-chart can get screwed up. No idea how the in-chart ones get affected which scares me. 
            UpdateNoteLinkedListRefs(activeNotes);  

            editor.selectedObjectsManager.currentSelectedObject = primaryActiveNote;
        }
    }

    static void UpdateNoteLinkedListRefs(IList<PlaceNote> notes)
    {
        foreach (PlaceNote placeNote in notes)
        {
            placeNote.UpdatePrevAndNext();
        }

        if (notes.Count > 1)
        {
            for (int i = 0; i < notes.Count; ++i)
            {
                Note note = notes[i].controller.note;

                if (i == 0)     // Start
                {
                    note.next = notes[i + 1].note;
                }
                else if (i >= (notes.Count - 1))      // End
                {
                    note.previous = notes[i - 1].note;
                }
                else
                {
                    note.previous = notes[i - 1].note;
                    note.next = notes[i + 1].note;
                }

                // Visuals for some reason aren't being updated in this cycle
                var noteObjectSelector = notes[i].controller.noteObjectSelector;
                noteObjectSelector.UpdateSelectedGameObject();            
            }
        }
    }

    void LeftyFlipReflectionCheck(ref int noteNumber, int laneCount)
    {
        if (Globals.gameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip && noteNumber >= 0 && noteNumber < laneCount)
            noteNumber = laneCount - (noteNumber + 1);
    }

    bool HasKeysInput(LaneInfo laneInfo)
    {
        int laneCount = laneInfo.laneCount;
        for (int i = 0; i < laneCount + 1; ++i)      // Start at 1 to ignore the multinote
        {
            // Need to make sure the note is at it's correct tick position
            if (MSChartEditorInput.GetInput(GetInputForNoteIndex(i, laneCount)))
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

    void UpdateFlagsInteractable(Note note)
    {
        // Prevent users from forcing notes when they shouldn't be forcable but retain the previous user-set forced property when using the note tool
        bool drumsMode = Globals.drumMode;

        if (!drumsMode)
        {
            forcedInteractable = !(note.cannotBeForced && !Globals.gameSettings.keysModeEnabled);

            // Disable tap note box for open notes
            tapInteractable = !note.IsOpenNote();
        }
        else
        {
            cymbalInteractable = NoteFunctions.AllowedToBeCymbal(note);
        }
    }

    public Note.Flags GetDisplayFlags()
    {
        Note.Flags flags = Note.Flags.None;

        flags = desiredFlags;

        if (!forcedInteractable && gameObject.activeSelf)
        {
            flags &= ~Note.Flags.Forced;
        }

        if (!tapInteractable && gameObject.activeSelf)
        {
            flags &= ~Note.Flags.Tap;
        }

        if (!cymbalInteractable && gameObject.activeSelf)
        {
            flags &= ~Note.Flags.ProDrums_Cymbal;
        }

        return flags;
    }
}
