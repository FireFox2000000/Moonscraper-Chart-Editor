using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameplayManager : MonoBehaviour {
    public UnityEngine.UI.Text noteStreakText;
    uint noteStreak = 0;
    List<NoteController> notesInWindow = new List<NoteController>();
    ChartEditor editor;

    float initSize;

    bool strum = false;

    void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        initSize = transform.localScale.y;
    }

    float previousStrumValue = 0;
    void Update()
    {
        if (Input.GetAxis("Strum") != 0 && Input.GetAxis("Strum") != previousStrumValue)
            strum = true;
        else
            strum = false;

        transform.localScale = new Vector3(transform.localScale.x, initSize * Globals.hyperspeed, transform.localScale.z);

        // Guard in case there are notes that shouldn't be in the window
        foreach (NoteController nCon in notesInWindow.ToArray())
        {
            if (!nCon.isActivated)
                notesInWindow.Remove(nCon);
        }

        if (notesInWindow.Count > 0)
        {
            if (noteStreak > 0)
            {
                if (ValidateFrets(notesInWindow[0].note) && ValidateStrum(notesInWindow[0].note))
                {
                    ++noteStreak;
                    foreach (Note note in notesInWindow[0].note.GetChord())
                    {
                        note.controller.Deactivate();
                    }

                    notesInWindow.RemoveAt(0);
                }
            }
            else
            {
                // Search to see if user is hitting a note ahead
                for (int i = 0; i < notesInWindow.Count; ++i)
                {
                    if (ValidateFrets(notesInWindow[i].note) && ValidateStrum(notesInWindow[i].note))
                    {
                        ++noteStreak;

                        // Remove all previous notes
                        for (int j = i; j >= 0; --j)
                        {
                            foreach (Note note in notesInWindow[j].note.GetChord())
                            {
                                note.controller.Deactivate();
                            }
                        }
                        return;
                    }
                }
            }
        }
        else
        {
            if (strum)
            {
                if (notesInWindow.Count > 0)
                {
                    foreach (Note note in notesInWindow[0].note.GetChord())
                    {
                        note.controller.Deactivate();
                    }

                    notesInWindow.RemoveAt(0);
                }

                noteStreak = 0;
                Debug.Log("Reset NS");
            }
        }
        /*
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown("joystick 1 button " + i))
            {
                Debug.Log("joystick 1 button " + i);
            }
        }*/

        /*
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(vKey))
            {
                //your code here
                Debug.Log(vKey);
            }
        }*/

        noteStreakText.text = "Note streak: " + noteStreak.ToString();

        previousStrumValue = Input.GetAxis("Strum");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        NoteController nCon = col.gameObject.GetComponentInParent<NoteController>();
        if (nCon)
        {
            // We only want 1 note per position so that we can compare using the note mask
            foreach (NoteController insertedNCon in notesInWindow)
            {
                if (nCon.note.position == insertedNCon.note.position)
                    return;
            }

            // Insert into sorted position
            for (int i = 0; i < notesInWindow.Count; ++i)
            {
                if (nCon.note < notesInWindow[i].note)
                {
                    notesInWindow.Insert(i, nCon);
                    return;
                }
            }

            notesInWindow.Add(nCon);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        NoteController nCon = col.gameObject.GetComponentInParent<NoteController>();
        if (nCon && notesInWindow.Contains(nCon) && nCon.isActivated)
        {
            // Missed note
            notesInWindow.Remove(nCon);
            noteStreak = 0;
        }
    }

    bool ValidateStrum(Note note)
    {
        switch (note.type)
        {
            case (Note.Note_Type.TAP):
                return true;
            case (Note.Note_Type.HOPO):
                if (noteStreak > 0)
                    return true;
                else if (strum)
                    return true;
                else
                    return false;
            default:    // Strum
                if (strum)
                    return true;
                else
                    return false;
        }
    }

	bool ValidateFrets(Note note)
    {
        int inputMask = 0;

        if (Input.GetButton("FretGreen"))
            inputMask |= 1 << (int)Note.Fret_Type.GREEN;

        if (Input.GetButton("FretRed"))
            inputMask |= 1 << (int)Note.Fret_Type.RED;

        if (Input.GetButton("FretYellow"))
            inputMask |= 1 << (int)Note.Fret_Type.YELLOW;

        if (Input.GetButton("FretBlue"))
            inputMask |= 1 << (int)Note.Fret_Type.BLUE;

        if (Input.GetButton("FretOrange"))
            inputMask |= 1 << (int)Note.Fret_Type.ORANGE;

        if (inputMask == 0)
        {
            if (note.fret_type == Note.Fret_Type.OPEN)
                return true;
            else
                return false;
        }
        else
        {
            // Chords
            if (note.IsChord)
            {
                // Regular chords
                if (noteStreak == 0 || note.type == Note.Note_Type.STRUM)
                {
                    if (inputMask == note.mask)
                        return true;
                    else
                        return false;
                }
                // HOPO or tap chords. Insert Exile chord anchor logic.
                else
                {
                    // Bit-shift to the right to compensate for anchor logic
                    int shiftedNoteMask = note.mask;
                    int shiftCount = 0;

                    while ((shiftedNoteMask & 1) != 1)
                    {
                        shiftedNoteMask >>= 1;
                        ++shiftCount;
                    }

                    int shiftedInputMask = inputMask;

                    shiftedInputMask >>= shiftCount;

                    if (shiftedInputMask == shiftedNoteMask)
                        return true;
                    else
                        return false;
                }
            }
            // Single notes
            else
            {
                int singleNoteInput = inputMask >> (int)note.fret_type;     // Anchor logic
                if (singleNoteInput == 1)
                    return true;
                else
                    return false;
            }
        }
    }
}
