using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotePropertiesPanelController : PropertiesPanelController {

    public Note currentNote;

    public Text fretText;

    public Toggle tapToggle;
    public Toggle forcedToggle;

    public GameObject noteToolObject;

    Note prevNote = null;

    bool prevForcedProperty = false;

    void OnEnable()
    {
        if ((currentNote.flags & Note.Flags.TAP) == Note.Flags.TAP)
            tapToggle.isOn = true;
        else
            tapToggle.isOn = false;

        if ((currentNote.flags & Note.Flags.FORCED) == Note.Flags.FORCED)
            forcedToggle.isOn = true;
        else
            forcedToggle.isOn = false;

        prevForcedProperty = forcedToggle.isOn;
    }
    
    void Update()
    {
        // Prevent users from forcing notes when they shouldn't be forcable but retain the previous user-set forced property when using the note tool
        if (Toolpane.currentTool != Toolpane.Tools.Note || (Toolpane.currentTool == Toolpane.Tools.Note && noteToolObject.activeSelf))
        {
            if (currentNote.CannotBeForcedCheck)
            {
                forcedToggle.interactable = false;
                currentNote.flags &= ~Note.Flags.FORCED;
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
        if (Input.GetButtonDown("ToggleTap") && tapToggle.interactable)
        {
            if (tapToggle.isOn)
                tapToggle.isOn = false;
            else
                tapToggle.isOn = true;
        }

        if (Input.GetButtonDown("ToggleForced") && forcedToggle.interactable)
        {
            if (forcedToggle.isOn)
                forcedToggle.isOn = false;
            else
                forcedToggle.isOn = true;
        }
    }
    
    void OnDisable()
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
                    currentNote.flags = currentNote.flags | Note.Flags.TAP;
                else
                    currentNote.flags = currentNote.flags & ~Note.Flags.TAP;
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
                currentNote.flags = currentNote.flags | Note.Flags.FORCED;
            else
                currentNote.flags = currentNote.flags & ~Note.Flags.FORCED;
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
            note.applyFlagsToChord();

        ChartEditor.editOccurred = true;
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
