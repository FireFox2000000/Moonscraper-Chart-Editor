using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotePropertiesPanelController : MonoBehaviour {

    public Note currentNote;

    public Text fretText;
    public Text positionText;

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

        prevNote = currentNote;
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
        Note previous = currentNote.previous;
        while (previous != null && previous.position == currentNote.position)
        {
            previous.flags = currentNote.flags;
            previous = previous.previous;
        }

        Note next = currentNote.next;
        while (next != null && next.position == currentNote.position)
        {
            next.flags = currentNote.flags;
            next = next.next;
        }

        ChartEditor.editOccurred = true;
    }
}
