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
                accentToggle.interactable = NoteFunctions.AllowedToBeAccentOrGhost(currentNote);
                ghostToggle.interactable = NoteFunctions.AllowedToBeAccentOrGhost(currentNote);
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

    void Controls()
    {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteTap) && tapToggle.interactable)
        {
            tapToggle.isOn = !tapToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteForced) && forcedToggle.interactable)
        {
            forcedToggle.isOn = !forcedToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteCymbal) && cymbalToggle.interactable)
        {
            cymbalToggle.isOn = !cymbalToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteDoubleKick) && doubleKickToggle.interactable)
        {
            doubleKickToggle.isOn = !doubleKickToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteAccent) && accentToggle.interactable)
        {
            accentToggle.isOn = !accentToggle.isOn;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteGhost) && ghostToggle.interactable)
        {
            ghostToggle.isOn = !ghostToggle.isOn;
        }
    }

    new void OnDisable()
    {
        currentNote = null;
    }
	
    public void setTap()
    {
        SetNoteFlag(tapToggle, Note.Flags.Tap);
    }

    public void setForced()
    {
        SetNoteFlag(forcedToggle, Note.Flags.Forced);
    }

    public void setCymbal()
    {
        SetNoteFlag(cymbalToggle, Note.Flags.ProDrums_Cymbal);
    }

    public void setDoubleKick()
    {
        SetNoteFlag(doubleKickToggle, Note.Flags.DoubleKick);
    }

    public void setAccent()
    {
        SetNoteFlag(accentToggle, Note.Flags.ProDrums_Accent);
        // TODO: This doesn't seem to work correctly
        // A note can only be either an accent or a ghost, not both
        // if (accentToggle.isOn && ghostToggle.isOn)
        // {
        //     ghostToggle.isOn = false;
        // }
    }

    public void setGhost()
    {
        SetNoteFlag(ghostToggle, Note.Flags.ProDrums_Ghost);
        // TODO: This doesn't seem to work correctly
        // A note can only be either an accent or a ghost, not both
        // if (ghostToggle.isOn && accentToggle.isOn)
        // {
        //     accentToggle.isOn = false;
        // }
    }

    public void SetNoteFlag(Toggle toggle, Note.Flags flag)
    {
        if (toggleBlockingActive)
            return;

        if (IsInNoteTool())
        {
            if (toggle.interactable)
                SetNoteToolFlag(ref noteToolController.desiredFlags, flag);
        }
        else
        {
            if (currentNote == prevNote)
            {
                var newFlags = currentNote.flags;

                if (currentNote != null)
                {
                    if (toggle.isOn)
                        newFlags |= flag;
                    else
                        newFlags &= ~flag;
                }

                SetNewFlags(currentNote, newFlags);
            }
        }
    }

    void SetNoteToolFlag(ref Note.Flags flags, Note.Flags flagsToToggle)
    {
        if ((flags & flagsToToggle) == 0)
            flags |= flagsToToggle;
        else
            flags &= ~flagsToToggle;
    }

    void SetNewFlags(Note note, Note.Flags newFlags)
    {
        if (note.flags == newFlags)
            return;

        if (editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Cursor)
        {
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
