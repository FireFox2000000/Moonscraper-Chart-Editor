// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;
using System.Text;

public class NotePropertiesPanelController : PropertiesPanelController {
    public Note currentNote { get { return (Note)currentSongObject; } set { currentSongObject = value; } }

    public Text sustainText;
    public Text fretText;
    
    public Toggle tapToggle;
    public Toggle forcedToggle;
    public Toggle cymbalToggle;
    public Toggle doubleKickToggle;
    public Toggle accentToggle;
    public Toggle ghostToggle;

    public GameObject noteToolObject;
    PlaceNoteController noteToolController;

    Note prevNote = null;
    Note prevClonedNote = new Note(0, 0);

    bool toggleBlockingActive = false;
    bool initialised = false;

    private void Start()
    {
        if (!initialised)
        {
            noteToolController = noteToolObject.GetComponent<PlaceNoteController>();
            editor.events.toolChangedEvent.Register(OnToolChanged);
            initialised = true;
        }

        ChartEditor.Instance.events.drumsModeOptionChangedEvent.Register(UpdateTogglesInteractable);
    }

    void OnToolChanged()
    {
    }

    void OnEnable()
    {
        if (!initialised)
        {
            Start();
        }

        Update();
    }

    protected override void Update()
    {
        UpdateTogglesInteractable();
        UpdateTogglesDisplay();

        UpdateNoteStringsInfo();
        Controls();

        prevNote = currentNote;
    }

    uint lastKnownKeysModePos = uint.MaxValue;
    void UpdateNoteStringsInfo()
    {
        bool hasCurrentNote = currentNote != null;
        bool hasPreviousNote = prevClonedNote != null;
        bool valuesAreTheSame = hasCurrentNote && hasPreviousNote && prevClonedNote.AllValuesCompare(currentNote);

        if (IsInNoteTool() && Globals.gameSettings.keysModeEnabled)
        {
            // Don't update the string unless the position has actually changed. Results in per-frame garbage otherwise
            if (lastKnownKeysModePos != editor.currentTickPos)
            {
                positionText.text = "Position: " + editor.currentTickPos;
                lastKnownKeysModePos = editor.currentTickPos;
            }

            fretText.text = "Fret: N/A";
            sustainText.text = "Length: N/A";
        }
        else if (currentNote != null && (prevClonedNote != currentNote || !valuesAreTheSame))
        {
            string noteTypeString = string.Empty;
            if (Globals.drumMode)
            {
                noteTypeString = currentNote.GetDrumString(editor.laneInfo);
            }
            else if (Globals.ghLiveMode)
                noteTypeString = currentNote.ghliveGuitarFret.ToString();
            else
                noteTypeString = currentNote.guitarFret.ToString();

            fretText.text = "Fret: " + noteTypeString;
            positionText.text = "Position: " + currentNote.tick.ToString();
            sustainText.text = "Length: " + currentNote.length.ToString();

            prevClonedNote.CopyFrom(currentNote);
            lastKnownKeysModePos = uint.MaxValue;
        }
    }

    bool IsInNoteTool()
    {
        return editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Note;
    }

    public Note.Flags GetDisplayFlags()
    {
        Note.Flags flags = Note.Flags.None;
        bool inNoteTool = IsInNoteTool();

        if (inNoteTool)
        {
            flags = noteToolController.GetDisplayFlags();
        }
        else if (currentNote != null)
        {
            flags = currentNote.flags;
        }

        return flags;
    }

    void UpdateTogglesDisplay()
    {
        toggleBlockingActive = true;

        Note.Flags flags = GetDisplayFlags();
        bool inNoteTool = IsInNoteTool();

        if (!inNoteTool && currentNote == null)
        {
            gameObject.SetActive(false);
            Debug.LogError("No note loaded into note inspector");
        }

        forcedToggle.isOn = (flags & Note.Flags.Forced) != 0;
        tapToggle.isOn = (flags & Note.Flags.Tap) != 0;
        cymbalToggle.isOn = (flags & Note.Flags.ProDrums_Cymbal) != 0;
        doubleKickToggle.isOn = (flags & Note.Flags.DoubleKick) != 0;
        accentToggle.isOn = (flags & Note.Flags.ProDrums_Accent) != 0;
        ghostToggle.isOn = (flags & Note.Flags.ProDrums_Ghost) != 0;

        toggleBlockingActive = false;
    }

    void UpdateTogglesInteractable()
    {
        // Prevent users from forcing notes when they shouldn't be forcable but retain the previous user-set forced property when using the note tool
        bool drumsMode = Globals.drumMode;
        bool proDrumsMode = drumsMode && Globals.gameSettings.drumsModeOptions == GameSettings.DrumModeOptions.ProDrums;

        forcedToggle.gameObject.SetActive(!drumsMode);
        tapToggle.gameObject.SetActive(!drumsMode);
        cymbalToggle.gameObject.SetActive(proDrumsMode);
        doubleKickToggle.gameObject.SetActive(proDrumsMode);
        accentToggle.gameObject.SetActive(proDrumsMode);
        ghostToggle.gameObject.SetActive(proDrumsMode);

        if (!drumsMode)
        {
            if (IsInNoteTool() && (noteToolObject.activeSelf || Globals.gameSettings.keysModeEnabled))
            {
                forcedToggle.interactable = noteToolController.forcedInteractable;
                tapToggle.interactable = noteToolController.tapInteractable;
            }
            else if (!IsInNoteTool())
            {
                forcedToggle.interactable = !(currentNote.cannotBeForced && !Globals.gameSettings.keysModeEnabled);
                tapToggle.interactable = !currentNote.IsOpenNote();
            }
            else
            {
                forcedToggle.interactable = true;
                tapToggle.interactable = true;
            }
        }
        else
        {
            if (IsInNoteTool() && noteToolObject.activeSelf)
            {
                cymbalToggle.interactable = noteToolController.cymbalInteractable;
                doubleKickToggle.interactable = noteToolController.doubleKickInteractable;
                accentToggle.interactable = noteToolController.accentInteractable;
                ghostToggle.interactable = noteToolController.ghostInteractable;
            }
            else if (!IsInNoteTool())
            {
                cymbalToggle.interactable = NoteFunctions.AllowedToBeCymbal(currentNote);
                doubleKickToggle.interactable = NoteFunctions.AllowedToBeDoubleKick(currentNote, editor.currentDifficulty);
                accentToggle.interactable = NoteFunctions.AllowedToBeAccent(currentNote);
                ghostToggle.interactable = NoteFunctions.AllowedToBeGhost(currentNote);
            }
            else
            {
                cymbalToggle.interactable = true;
                doubleKickToggle.interactable = true;
                accentToggle.interactable = true;
                ghostToggle.interactable = true;
            }
        }
    }

    static bool ToggleIsValid(Toggle toggle)
    {
        return toggle.interactable && toggle.isActiveAndEnabled;
    }

    void Controls()
    {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteTap) && ToggleIsValid(tapToggle))
        {
            Debug.Log("ToggleNoteTap");
            tapToggle.isOn = !tapToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteForced) && ToggleIsValid(forcedToggle))
        {
            Debug.Log("ToggleNoteForced");
            forcedToggle.isOn = !forcedToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteCymbal) && ToggleIsValid(cymbalToggle))
        {
            Debug.Log("ToggleNoteCymbal");
            cymbalToggle.isOn = !cymbalToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteDoubleKick) && ToggleIsValid(doubleKickToggle))
        {
            Debug.Log("ToggleNoteDoubleKick");
            doubleKickToggle.isOn = !doubleKickToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteAccent) && ToggleIsValid(accentToggle))
        {
            Debug.Log("ToggleNoteAccent");
            accentToggle.isOn = !accentToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteGhost) && ToggleIsValid(ghostToggle))
        {
            Debug.Log("ToggleNoteGhost");
            ghostToggle.isOn = !ghostToggle.isOn;
        }
    }

    new void OnDisable()
    {
        currentNote = null;
    }
	
    public void setTap()
    {
        // A note can only be forced or a tap, not both
        OnNoteFlagToggleChanged(tapToggle, Note.Flags.Tap, Note.Flags.Forced);
    }

    public void setForced()
    {
        // A note can only be forced or a tap, not both
        OnNoteFlagToggleChanged(forcedToggle, Note.Flags.Forced, Note.Flags.Tap);
    }

    public void setCymbal()
    {
        OnNoteFlagToggleChanged(cymbalToggle, Note.Flags.ProDrums_Cymbal);
    }

    public void setDoubleKick()
    {
        OnNoteFlagToggleChanged(doubleKickToggle, Note.Flags.DoubleKick);
    }

    public void setAccent()
    {
        // A note can only be either an accent or a ghost, not both
        OnNoteFlagToggleChanged(accentToggle, Note.Flags.ProDrums_Accent, Note.Flags.ProDrums_Ghost);
    }

    public void setGhost()
    {
        // A note can only be either an accent or a ghost, not both
        OnNoteFlagToggleChanged(ghostToggle, Note.Flags.ProDrums_Ghost, Note.Flags.ProDrums_Accent);
    }

    void OnNoteFlagToggleChanged(Toggle toggle, Note.Flags flag, Note.Flags flagToExclude = Note.Flags.None)
    {
        if (toggleBlockingActive)
            return;

        Note.Flags newFlags;
        if (IsInNoteTool())
        {
            if (toggle.interactable)
                newFlags = noteToolController.desiredFlags;
            else
                return;
        }
        else
        {
            if (currentNote == prevNote && currentNote != null)
                newFlags = currentNote.flags;
            else
                return;
        }

        newFlags = ToggleFlags(newFlags, flag, toggle.isOn);
        if ((newFlags & flagToExclude) != Note.Flags.None)
        {
            newFlags = ToggleFlags(newFlags, flagToExclude, false);
        }

        SetNewFlags(currentNote, newFlags);
    }

    Note.Flags ToggleFlags(Note.Flags flags, Note.Flags flagsToToggle, bool enabled)
    {
        if (enabled)
            flags |= flagsToToggle;
        else
            flags &= ~flagsToToggle;

        return flags;
    }

    void SetNewFlags(Note note, Note.Flags newFlags)
    {
        if (editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Cursor)
        {
            if (note.flags == newFlags)
            {
                return;
            }

            Note newNote = new Note(note.tick, note.rawNote, note.length, newFlags);
            SongEditModifyValidated command = new SongEditModifyValidated(note, newNote);
            editor.commandStack.Push(command);
        }
        else
        {
            // Updating note tool parameters and visuals
            noteToolController.desiredFlags = newFlags;
        }
    }
}
