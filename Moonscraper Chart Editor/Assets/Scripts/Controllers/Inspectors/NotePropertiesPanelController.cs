// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotePropertiesPanelController : PropertiesPanelController {
    public Note currentNote { get { return (Note)currentSongObject; } set { currentSongObject = value; } }

    public Text sustainText;
    public Text fretText;
    
    public Toggle tapToggle;
    public Toggle forcedToggle;

    public GameObject noteToolObject;

    Note prevNote = null;

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
        if (Globals.drumMode)
        {
            forcedToggle.gameObject.SetActive(false);
            tapToggle.gameObject.SetActive(false);
        }
        else
        {
            if (Toolpane.currentTool != Toolpane.Tools.Note || (Toolpane.currentTool == Toolpane.Tools.Note && noteToolObject.activeSelf))
            {
                if (currentNote.CannotBeForcedCheck && !GameSettings.keysModeEnabled)
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

        if (currentNote != null)
        {
            string noteTypeString = string.Empty;
            if (Globals.drumMode)
                noteTypeString = currentNote.drumPad.ToString();
            else if (Globals.ghLiveMode)
                noteTypeString = currentNote.ghliveGuitarFret.ToString();
            else
                noteTypeString = currentNote.guitarFret.ToString();

            fretText.text = "Fret: " + noteTypeString;
            positionText.text = "Position: " + currentNote.position.ToString();
            sustainText.text = "Length: " + currentNote.length.ToString();
        }

        Controls();

        prevNote = currentNote;
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
            System.Collections.Generic.List<ActionHistory.Action> record = new System.Collections.Generic.List<ActionHistory.Action>();
            foreach (Note chordNote in currentNote.GetChord())
                record.Add(new ActionHistory.Delete(chordNote));

            if (currentNote != null)
            {
                if (tapToggle.isOn)
                    currentNote.flags = currentNote.flags | Note.Flags.Tap;
                else
                    currentNote.flags = currentNote.flags & ~Note.Flags.Tap;
            }

            setFlags(currentNote);          

            foreach (Note chordNote in currentNote.GetChord())
                record.Add(new ActionHistory.Add(chordNote));

            if (Toolpane.currentTool == Toolpane.Tools.Cursor)
                editor.actionHistory.Insert(record.ToArray());
        }
    }

    public void setForced()
    {
        //if (currentNote == prevNote)
        //{
        System.Collections.Generic.List<ActionHistory.Action> record = new System.Collections.Generic.List<ActionHistory.Action>();
        foreach (Note chordNote in currentNote.GetChord())
            record.Add(new ActionHistory.Delete(chordNote));

        if (currentNote != null)
        {
            if (forcedToggle.isOn)
                currentNote.flags = currentNote.flags | Note.Flags.Forced;
            else
                currentNote.flags = currentNote.flags & ~Note.Flags.Forced;
        }

        setFlags(currentNote);

        foreach (Note chordNote in currentNote.GetChord())
            record.Add(new ActionHistory.Add(chordNote));

        if (currentNote == prevNote && Toolpane.currentTool == Toolpane.Tools.Cursor)
            editor.actionHistory.Insert(record.ToArray());
        //}
    }

    void setFlags(Note note)
    {
        if (Toolpane.currentTool != Toolpane.Tools.Note)
        {
            note.applyFlagsToChord();

            ChartEditor.isDirty = true;
        }

        foreach (Note chordNote in note.GetChord())
        {
            if (chordNote.controller)
                chordNote.controller.SetDirty();
        }
    }

    Note[] CloneChord(Note note)
    {
        Note[] chord = note.GetChord();
        Note[] original = new Note[chord.Length];
        for (int i = 0; i < chord.Length; ++i)
        {
            original[i] = (Note)chord[i].Clone();
        }

        return original;
    }
}
