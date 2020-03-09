// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NotePropertiesPanelController : PropertiesPanelController {
    public Note currentNote { get { return (Note)currentSongObject; } set { currentSongObject = value; } }

    public Text sustainText;
    public Text fretText;
    
    public Toggle tapToggle;
    public Toggle forcedToggle;
    public Toggle cymbalToggle;

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

    void UpdateNoteStringsInfo()
    {
        bool hasCurrentNote = currentNote != null;
        bool hasPreviousNote = prevClonedNote != null;
        bool valuesAreTheSame = hasCurrentNote && hasPreviousNote && prevClonedNote.AllValuesCompare(currentNote);
        if (currentNote != null && (prevClonedNote != currentNote || !valuesAreTheSame))
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

        toggleBlockingActive = false;
    }

    void UpdateTogglesInteractable()
    {
        // Prevent users from forcing notes when they shouldn't be forcable but retain the previous user-set forced property when using the note tool
        bool drumsMode = Globals.drumMode;
        forcedToggle.gameObject.SetActive(!drumsMode);
        tapToggle.gameObject.SetActive(!drumsMode);
        cymbalToggle.gameObject.SetActive(drumsMode && ChartEditor.Instance.laneInfo.laneCount == 4);   // Todo, only if in pro drums mode and if in 4 lane

        if (!drumsMode)
        {
            if (IsInNoteTool() && noteToolObject.activeSelf)
            {
                forcedToggle.interactable = noteToolController.forcedInteractable;
                tapToggle.interactable = noteToolController.tapInteractable;
            }
            else if (!IsInNoteTool())
            {
                forcedToggle.interactable = !(currentNote.cannotBeForced && !GameSettings.keysModeEnabled);
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
            }
            else if (!IsInNoteTool())
            {
                cymbalToggle.interactable = NoteFunctions.AllowedToBeCymbal(currentNote);
            }
            else
            {
                cymbalToggle.interactable = true;
            }
        }
    }

    void Controls()
    {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteTap) && tapToggle.interactable)
        {
            if (tapToggle.isOn)
                tapToggle.isOn = false;
            else
                tapToggle.isOn = true;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleNoteForced) && forcedToggle.interactable)
        {
            if (forcedToggle.isOn)
                forcedToggle.isOn = false;
            else
                forcedToggle.isOn = true;
        }
    }
    
    new void OnDisable()
    {
        currentNote = null;
    }
	
    public void setTap()
    {
        if (toggleBlockingActive)
            return;

        if (IsInNoteTool())
        {
            SetTapNoteTool();
        }
        else
        {
            SetTapNote();
        }
    }

    void SetTapNoteTool()
    {
        if (tapToggle.interactable)
            SetNoteToolFlag(ref noteToolController.desiredFlags, tapToggle, Note.Flags.Tap);
    }

    void SetTapNote()
    {
        if (currentNote == prevNote)
        {
            var newFlags = currentNote.flags;

            if (currentNote != null)
            {
                if (tapToggle.isOn)
                    newFlags |= Note.Flags.Tap;
                else
                    newFlags &= ~Note.Flags.Tap;
            }

            SetNewFlags(currentNote, newFlags);
        }
    }

    void SetNoteToolFlag(ref Note.Flags flags, Toggle uiToggle, Note.Flags flagsToToggle)
    {
        if ((flags & flagsToToggle) == 0)
            flags |= flagsToToggle;
        else
            flags &= ~flagsToToggle;
    }

    public void setForced()
    {
        if (toggleBlockingActive)
            return;

        if (IsInNoteTool())
        {
            SetForcedNoteTool();
        }
        else
        {
            SetForcedNote();
        }
    }

    public void setCymbal()
    {
        if (toggleBlockingActive)
            return;

        if (IsInNoteTool())
        {
            SetCymbalNoteTool();
        }
        else
        {
            SetCymbalNote();
        }
    }

    void SetForcedNote()
    {
        if (currentNote == prevNote)
        {
            var newFlags = currentNote.flags;

            if (currentNote != null)
            {
                if (forcedToggle.isOn)
                    newFlags |= Note.Flags.Forced;
                else
                    newFlags &= ~Note.Flags.Forced;
            }

            SetNewFlags(currentNote, newFlags);
        }
    }

    void SetForcedNoteTool()
    {
        if (forcedToggle.interactable)
            SetNoteToolFlag(ref noteToolController.desiredFlags, forcedToggle, Note.Flags.Forced);
    }

    void SetCymbalNote()
    {
        if (currentNote == prevNote)
        {
            var newFlags = currentNote.flags;

            if (currentNote != null)
            {
                if (cymbalToggle.isOn)
                    newFlags |= Note.Flags.ProDrums_Cymbal;
                else
                    newFlags &= ~Note.Flags.ProDrums_Cymbal;
            }

            SetNewFlags(currentNote, newFlags);
        }

    }
    void SetCymbalNoteTool()
    {
        if (cymbalToggle.interactable)
            SetNoteToolFlag(ref noteToolController.desiredFlags, cymbalToggle, Note.Flags.ProDrums_Cymbal);
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
