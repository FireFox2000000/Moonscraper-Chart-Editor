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

    public GameObject noteToolObject;

    Note prevNote = null;
    Note prevClonedNote = new Note(0, 0);

    bool prevForcedProperty = false;

    void OnEnable()
    {
        if ((currentNote.flags & Note.Flags.Tap) == Note.Flags.Tap)
            tapToggle.isOn = true;
        else
            tapToggle.isOn = false;

        if ((currentNote.flags & Note.Flags.Forced) == Note.Flags.Forced)
            forcedToggle.isOn = true;
        else
            forcedToggle.isOn = false;

        prevForcedProperty = forcedToggle.isOn;
    }

    protected override void Update()
    {
        // Prevent users from forcing notes when they shouldn't be forcable but retain the previous user-set forced property when using the note tool
        bool drumsMode = Globals.drumMode;
        forcedToggle.gameObject.SetActive(!drumsMode);
        tapToggle.gameObject.SetActive(!drumsMode);

        if (!drumsMode)
        {
            if (Toolpane.currentTool != Toolpane.Tools.Note || (Toolpane.currentTool == Toolpane.Tools.Note && noteToolObject.activeSelf))
            {
                if (currentNote.cannotBeForced && !GameSettings.keysModeEnabled)
                {
                    forcedToggle.interactable = false;
                    currentNote.flags &= ~Note.Flags.Forced;
                }
                else
                {
                    if (!forcedToggle.interactable && Toolpane.currentTool == Toolpane.Tools.Note)
                    {
                        forcedToggle.isOn = prevForcedProperty;
                        setForced();
                    }
                    forcedToggle.interactable = true;
                }
            }
            else
            {
                if (!forcedToggle.interactable)
                {
                    forcedToggle.interactable = true;
                    forcedToggle.isOn = prevForcedProperty;
                }
            }

            if (forcedToggle.interactable)
            {
                prevForcedProperty = forcedToggle.isOn;
            }

            if (currentNote != null)
            {
                tapToggle.isOn = ((currentNote.flags & Note.Flags.Tap) == Note.Flags.Tap);

                forcedToggle.isOn = ((currentNote.flags & Note.Flags.Forced) == Note.Flags.Forced);
            }
            else
            {
                gameObject.SetActive(false);
                Debug.LogError("No note loaded into note inspector");
            }

            // Disable tap note box for open notes
            tapToggle.interactable = !(currentNote.IsOpenNote() && Toolpane.currentTool != Toolpane.Tools.Note);
        }

        UpdateNoteStringsInfo();
        Controls();

        prevNote = currentNote;
    }

    void UpdateNoteStringsInfo()
    {
        if (currentNote != null && prevClonedNote != currentNote)
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

    void Controls()
    {
        if (ShortcutInput.GetInputDown(Shortcut.ToggleNoteTap) && tapToggle.interactable)
        {
            if (tapToggle.isOn)
                tapToggle.isOn = false;
            else
                tapToggle.isOn = true;
        }

        if (ShortcutInput.GetInputDown(Shortcut.ToggleNoteForced) && forcedToggle.interactable)
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

    public void setForced()
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

    static List<SongObject> currentNotes = new List<SongObject>();
    static List<SongObject> newNotes = new List<SongObject>();
    void SetNewFlags(Note note, Note.Flags newFlags)
    {
        if (note.flags == newFlags)
            return;

        if (Toolpane.currentTool == Toolpane.Tools.Cursor)
        {
            currentNotes.Clear();
            newNotes.Clear();

            foreach (Note chordNote in note.chord)
            {
                currentNotes.Add(chordNote);
                newNotes.Add(new Note(chordNote.tick, chordNote.rawNote, chordNote.length, newFlags));
            }

            SongEditCommand[] commands = new SongEditCommand[] 
            {
                new SongEditDelete(currentNotes),
                new SongEditAdd(newNotes)
            };

            editor.commandStack.Push(new BatchedSongEditCommand(commands));

            currentNotes.Clear();
            newNotes.Clear();
        }
        else
        {
            // Updating note tool parameters and visuals
            note.flags = newFlags;
            note.ApplyFlagsToChord();
            foreach (Note chordNote in note.chord)
            {
                if (chordNote.controller)
                    chordNote.controller.SetDirty();
            }
        }
    }
}
