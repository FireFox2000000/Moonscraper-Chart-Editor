using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotePropertiesPanelController : PropertiesPanelController {

    public Note currentNote;

    public Text fretText;

    public Toggle tapToggle;
    public Toggle forcedToggle;

    Note prevNote = null;
    void Update()
    {      
        if (currentNote != null)
        {
            fretText.text = "Fret: " + currentNote.fret_type.ToString();
            positionText.text = "Position: " + currentNote.position.ToString();
        }

        if (currentNote != null)
        {
            if ((currentNote.flags & Note.Flags.TAP) == Note.Flags.TAP)
                tapToggle.isOn = true;
            else
                tapToggle.isOn = false;

            if ((currentNote.flags & Note.Flags.FORCED) == Note.Flags.FORCED)
                forcedToggle.isOn = true;
            else
                forcedToggle.isOn = false;
        }
        else
        {
            gameObject.SetActive(false);
            Debug.LogError("No note loaded into note inspector");
        }

        if (!Globals.IsTyping)
            controls();

        prevNote = currentNote;
    }

    void controls()
    {
        if (Input.GetButtonDown("ToggleTap"))
            if (tapToggle.isOn)
                tapToggle.isOn = false;
            else
                tapToggle.isOn = true;

        if (Input.GetButtonDown("ToggleForced"))
            if (forcedToggle.isOn)
                forcedToggle.isOn = false;
            else
                forcedToggle.isOn = true;
    }
    
    void OnDisable()
    {
        currentNote = null;
    }
	
    public void setTap()
    {
        if (currentNote == prevNote)
        {
            if (currentNote != null)
            {
                if (tapToggle.isOn)
                    currentNote.flags = currentNote.flags | Note.Flags.TAP;
                else
                    currentNote.flags = currentNote.flags & ~Note.Flags.TAP;
            }

            setFlags(currentNote);
        }
    }

    public void setForced()
    {
        if (currentNote == prevNote)
        {
            if (currentNote != null)
            {
                if (forcedToggle.isOn)
                    currentNote.flags = currentNote.flags | Note.Flags.FORCED;
                else
                    currentNote.flags = currentNote.flags & ~Note.Flags.FORCED;
            }

            setFlags(currentNote);
        }
    }

    void setFlags(Note note)
    {
        Note[] chordNotes = note.GetChord();

        foreach (Note chordNote in chordNotes)
        {
            chordNote.flags = note.flags;
        }

        ChartEditor.editOccurred = true;
    }
}
