using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameplayManager : MonoBehaviour {
    public UnityEngine.UI.Text noteStreakText;
    uint noteStreak = 0;
    List<NoteController> physicsWindow = new List<NoteController>();
    List<NoteController> notesInWindow = new List<NoteController>();
    List<NoteController> currentSustains = new List<NoteController>();
    ChartEditor editor;

    float hitWindowHeight = 0.19f;
    float initWindowSize;
    float initSize;
    float initYPos;
    float previousStrumValue;
    int previousInputMask;

    bool strum = false;
    bool canTap;

    void Start()
    {
        previousStrumValue = Input.GetAxisRaw("Strum");
        previousInputMask = GetFretInputMask();
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        initWindowSize = hitWindowHeight;
        initYPos = transform.localPosition.y;

        initSize = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
    }

    void Update()
    {
        // Configure the timing window to take into account hyperspeed changes
        //transform.localScale = new Vector3(transform.localScale.x, initSize * Globals.hyperspeed, transform.localScale.z);
        hitWindowHeight = initWindowSize * Globals.hyperspeed;
        //transform.localPosition = new Vector3(transform.localPosition.x, initYPos * Globals.hyperspeed, transform.localPosition.z);

        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            transform.localScale = new Vector3(transform.localScale.x, initSize, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        }

        // Update the hit window
        foreach (NoteController note in physicsWindow.ToArray())
        {
            if (EnterWindow(note))
                physicsWindow.Remove(note);
        }

        foreach (NoteController note in notesInWindow.ToArray())
        {
            if (ExitWindow(note) && !note.hit)
            {
                //Debug.Log("Missed note");
                foreach (Note chordNote in note.note.GetChord())
                    chordNote.controller.sustainBroken = true;

                noteStreak = 0;
            }
        }

        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            if (Globals.bot)
                noteStreakText.text = "BOT";
            else
            {
                // Get player input
                if (Input.GetAxisRaw("Strum") != 0 && Input.GetAxisRaw("Strum") != previousStrumValue)
                    strum = true;
                else
                    strum = false;

                int inputMask = GetFretInputMask();
                if (inputMask != previousInputMask)
                    canTap = true;

                // Guard in case there are notes that shouldn't be in the window
                foreach (NoteController nCon in notesInWindow.ToArray())
                {
                    if (nCon.hit)
                        notesInWindow.Remove(nCon);
                }

                if (notesInWindow.Count > 0)
                {
                    if (noteStreak > 0)
                    { 
                        if (ValidateFrets(notesInWindow[0].note) && ValidateStrum(notesInWindow[0].note, canTap))
                        {
                            ++noteStreak;
                            foreach (Note note in notesInWindow[0].note.GetChord())
                            {
                                note.controller.hit = true;
                            }

                            if (notesInWindow[0].note.sustain_length > 0)
                                currentSustains.Add(notesInWindow[0]);

                            notesInWindow.RemoveAt(0);
                        }
                        else if (strum)
                        {
                            Debug.Log("Strummed incorrect note 2");
                            noteStreak = 0;
                        }
                    }
                    else
                    {
                        bool hit = false;
                        // Search to see if user is hitting a note ahead
                        for (int i = 0; i < notesInWindow.Count; ++i)
                        {
                            if (ValidateFrets(notesInWindow[i].note) && ValidateStrum(notesInWindow[i].note, canTap))
                            {
                                if (i > 0)
                                    noteStreak = 0;
                                ++noteStreak;

                                canTap = false;

                                foreach (Note note in notesInWindow[i].note.GetChord())
                                {
                                    note.controller.hit = true;
                                }

                                if (notesInWindow[i].note.sustain_length > 0)
                                    currentSustains.Add(notesInWindow[i]);

                                // Remove all previous notes
                                NoteController[] nConArray = notesInWindow.ToArray();
                                for (int j = i; j >= 0; --j)
                                {
                                    notesInWindow.Remove(nConArray[j]);
                                }

                                hit = true;
                                break;
                            }
                        }

                        // Will not reach here if user hit a note
                        if (!hit && strum)
                        {
                            Debug.Log("Strummed incorrect note");
                            noteStreak = 0;
                        }
                    }
                }
                else
                {
                    if (strum)
                    {
                        Debug.Log("Strummed when no note");
                        noteStreak = 0;
                    }
                }

                // Handle sustain breaking
                foreach (NoteController note in currentSustains.ToArray())
                {
                    bool isChord = note.note.IsChord;

                    if (!note.isActivated && (noteStreak == 0 || (!note.note.IsChord && !ValidateFrets(note.note)) || (note.note.IsChord && note.note.mask != inputMask)))
                    {
                        foreach (Note chordNote in note.note.GetChord())
                            chordNote.controller.sustainBroken = true;
                        currentSustains.Remove(note);
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

                previousStrumValue = Input.GetAxisRaw("Strum");
                previousInputMask = inputMask;
            }
        }
        else if (Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            notesInWindow.Clear();
            noteStreakText.text = string.Empty;
        }
        else
        {
            noteStreakText.text = string.Empty;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
            NoteController nCon = col.gameObject.GetComponentInParent<NoteController>();
            if (nCon && !nCon.hit && !physicsWindow.Contains(nCon))
            {
                // We only want 1 note per position so that we can compare using the note mask
                foreach (NoteController insertedNCon in physicsWindow)
                {
                    if (nCon.note.position == insertedNCon.note.position)
                        return;
                }

                // Insert into sorted position
                for (int i = 0; i < physicsWindow.Count; ++i)
                {
                    if (nCon.note < physicsWindow[i].note)
                    {
                        physicsWindow.Insert(i, nCon);
                        return;
                    }
                }

                physicsWindow.Add(nCon);
            }
    }

    bool EnterWindow(NoteController note)
    {
        if (!note.hit && note.transform.position.y < editor.visibleStrikeline.position.y + (hitWindowHeight / 2))
        {
            // We only want 1 note per position so that we can compare using the note mask
            foreach (NoteController insertedNCon in notesInWindow)
            {
                if (note.note.position == insertedNCon.note.position)
                    return false;
            }

            // Insert into sorted position
            for (int i = 0; i < notesInWindow.Count; ++i)
            {
                if (note.note < notesInWindow[i].note)
                {
                    notesInWindow.Insert(i, note);
                    return true;
                }
            }

            notesInWindow.Add(note);

            return true;
        }

        return false;
    }

    bool ExitWindow(NoteController note)
    {
        if (note.transform.position.y < editor.visibleStrikeline.position.y - (hitWindowHeight / 2))
        {
            notesInWindow.Remove(note);
            return true;
        }

        return false;
    }
    /*
    void OnTriggerExit2D(Collider2D col)
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            NoteController nCon = col.gameObject.GetComponentInParent<NoteController>();
            if (nCon && notesInWindow.Contains(nCon) && !nCon.hit)
            {
                // Missed note
                //Debug.Log("Missed note");
                foreach (Note note in nCon.note.GetChord())
                    note.controller.sustainBroken = true;

                notesInWindow.Remove(nCon);
            }
        }
    }*/

    bool ValidateStrum(Note note, bool canTap)
    {
        switch (note.type)
        {
            case (Note.Note_Type.TAP):
                if (canTap)
                    return true;
                else if (strum)
                    return true;
                else
                    return false;
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
        int inputMask = GetFretInputMask();

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

    int GetFretInputMask()
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

        return inputMask;
    }
}
