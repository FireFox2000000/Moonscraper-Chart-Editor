//#define GAMEPAD

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

[RequireComponent(typeof(AudioSource))]
public class GameplayManager : MonoBehaviour {
    public AudioClip comboBreak;

    AudioSource audioSource;

    const float FREESTRUM_TIME = 0.1f;
    const int NUM_OF_FREESTRUMS = 1;
    int freestrum = 0;
    float freestrumTimer = 0;

    const float STRUM_WINDOW_TIME = 0.06f;
    bool strumWindowOpen = false;
    float strumWindowTimer = 0;

    public UnityEngine.UI.Text noteStreakText;
    public UnityEngine.UI.Text percentHitText;
    public UnityEngine.UI.Text debugHitText;

    uint noteStreak = 0;
    uint notesHit = 0;
    uint totalNotes = 0;

    List<NoteController> physicsWindow = new List<NoteController>();
    List<NoteController> notesInWindow = new List<NoteController>();
    List<NoteController> currentSustains = new List<NoteController>();
    ChartEditor editor;

    float hitWindowHeight = 0.17f;
    float initWindowSize;
    float initSize;
    float previousStrumValue;
    int previousInputMask;

    bool strum = false;
    bool canTap;
#if GAMEPAD
    public static GamePadState? gamepad;
#endif

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        previousStrumValue = Input.GetAxisRaw("Strum");
        previousInputMask = GetFretInputMask();
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        initWindowSize = hitWindowHeight;

        initSize = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
    }

    void Update()
    {
        uint startNS = noteStreak;

#if GAMEPAD
        gamepad = null;
        for (int i = 0; i < 4; ++i)
        {
            PlayerIndex playerIndex = (PlayerIndex)i;
            GamePadState testState = GamePad.GetState(playerIndex);
            if (testState.IsConnected)
            {
                gamepad = GamePad.GetState(playerIndex);
                break;
            }
        }

        //gamepad = GamePad.GetState(0);
#endif
        // Configure the timing window to take into account hyperspeed changes
        //transform.localScale = new Vector3(transform.localScale.x, initSize * Globals.hyperspeed, transform.localScale.z);
        hitWindowHeight = initWindowSize * Globals.hyperspeed;// * Time.timeScale;

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
                ++totalNotes;
            }
        }

        bool noStrumInWindow = false;
        foreach (NoteController note in notesInWindow)
        {
            if (note.note.type != Note.Note_Type.STRUM)
            {
                noStrumInWindow = true;
                break;
            }
        }

        // Handle freestrum timer
        if (noStrumInWindow)
            freestrumTimer = 0;

        if (!noStrumInWindow && freestrumTimer > FREESTRUM_TIME)
        {
            freestrum = 0;
        }

        if (freestrum > 0)
            freestrumTimer += Time.deltaTime;
        else
            freestrumTimer = 0;

        // Handle strum window
        if (strumWindowTimer > STRUM_WINDOW_TIME)
        {
            if (strumWindowOpen)        // Overstrum
            {
                noteStreak = 0;
                Debug.Log("Overstrum");
            }
            strumWindowOpen = false;
        }

        if (strumWindowOpen)
            strumWindowTimer += Time.deltaTime;
        else
            strumWindowTimer = 0;

        // Configure current strum input
        float strumValue;
#if GAMEPAD
        if (gamepad != null && ((GamePadState)gamepad).DPad.Down == ButtonState.Pressed)
            strumValue = -1;
        else if (gamepad != null && ((GamePadState)gamepad).DPad.Up == ButtonState.Pressed)
            strumValue = 1;
        else
            strumValue = 0;
#else
        strumValue = Input.GetAxisRaw("Strum");
#endif

        // Get player input
        if (strumValue != 0 && strumValue != previousStrumValue)
            //if (Input.GetAxisRaw("Strum") != 0 && Input.GetAxisRaw("Strum") != previousStrumValue)
            strum = true;
        else
            strum = false;

        // Gameplay
        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !Globals.bot)
        {
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
                if (strumWindowOpen)            // Allows for the strum buffer
                {
                    if (ValidateFrets(notesInWindow[0].note))
                    {
                        hitNote(notesInWindow[0]);

                        strumWindowOpen = false;
                    }
                    else if (strum)
                    {
                        if (overstrumCheck())
                        {
                            Debug.Log("Strummed incorrect frets");
                        }
                    }
                }
                else if ((notesInWindow[0].note.type != Note.Note_Type.STRUM && noteStreak > 0) || noteStreak > 10)       // Gives time for strumming to get back on beat
                { 
                    if (ValidateFrets(notesInWindow[0].note) && ValidateStrum(notesInWindow[0].note, canTap))
                    {
                        hitNote(notesInWindow[0]);
                    }
                    else if (strum)
                    {
                        if (overstrumCheck())
                            Debug.Log("Strummed incorrect note 2, " + "Frets: " + System.Convert.ToString(inputMask, 2) + ", Strum: " + strum);
                    }
                }
                else
                {
                    // Search to see if user is hitting a note ahead
                    List<NoteController> validatedNotes = new List<NoteController>();
                    foreach (NoteController note in notesInWindow)
                    {
                        if (ValidateFrets(note.note) && ValidateStrum(note.note, canTap))
                            validatedNotes.Add(note);
                    }

                    if (validatedNotes.Count > 0)
                    {
                        // Select the note closest to the strikeline
                        float aimYPos = editor.visibleStrikeline.transform.position.y + 0.25f;  // Added offset from the note controller

                        NoteController selectedNote = validatedNotes[0];
                        float dis = Mathf.Abs(aimYPos - selectedNote.transform.position.y);
                            
                        foreach (NoteController validatedNote in validatedNotes)
                        {
                            float distance = Mathf.Abs(aimYPos - validatedNote.transform.position.y);
                            if (distance < dis)
                            {
                                selectedNote = validatedNote;
                                dis = distance;
                            }                    
                        }

                        int index = notesInWindow.IndexOf(selectedNote);
                        NoteController note = notesInWindow[notesInWindow.IndexOf(selectedNote)];
                        if (index > 0)
                            noteStreak = 0;

                        hitNote(note);

                        canTap = false;

                        // Remove all previous notes
                        NoteController[] nConArray = notesInWindow.ToArray();
                        for (int j = index - 1; j >= 0; --j)
                        {
                            ++totalNotes;
                            notesInWindow.Remove(nConArray[j]);
                        }
                    }
                    else
                    {
                        // Will not reach here if user hit a note
                        if (strum)
                        {
                            if (overstrumCheck())
                                Debug.Log("Strummed when no note");
                        }
                    }     
                }
            }
            else
            {
                if (strum)
                {
                    if (overstrumCheck())
                        Debug.Log("Strummed when no note v2");
                }
            }

            // Handle sustain breaking
            foreach (NoteController note in currentSustains.ToArray())
            {
                if (!note.gameObject.activeSelf || note.note == null)
                {
                    currentSustains.Remove(note);
                    continue;
                }
                if (!note.isActivated && (noteStreak == 0 || (!note.note.IsChord && !ValidateFrets(note.note)) || (note.note.IsChord && note.note.mask != inputMask)))
                {
                    foreach (Note chordNote in note.note.GetChord())
                        chordNote.controller.sustainBroken = true;
                    currentSustains.Remove(note);
                }
            }

            noteStreakText.text = "Note streak: " + noteStreak.ToString();
            if (totalNotes > 0)
                percentHitText.text = ((float)notesHit / (float)totalNotes * 100).Round(2).ToString() + "%";
            else
                percentHitText.text = "0.00%";

            debugHitText.text = notesHit.ToString() + " / " + totalNotes.ToString();

            previousInputMask = inputMask;

            if (startNS >= 10 && noteStreak < startNS)
            {
                audioSource.PlayOneShot(comboBreak, Globals.sfxVolume);
            }
            
        }
        else if (Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            reset();
        }
        else
        {
            reset();
            //noteStreakText.text = string.Empty;
        }

        previousStrumValue = strumValue;
        if (freestrum > NUM_OF_FREESTRUMS)
            freestrum = NUM_OF_FREESTRUMS;
    }

    void reset()
    {
        noteStreakText.text = string.Empty;
        percentHitText.text = string.Empty;
        debugHitText.text = string.Empty;
        noteStreak = 0;
        notesHit = 0;
        totalNotes = 0;
        notesInWindow.Clear();
        physicsWindow.Clear();
    }

    bool overstrumCheck()
    {
        if (freestrum > 0)
        {
            --freestrum;
        }
        else
        {
            if (strumWindowOpen)
            {
                noteStreak = 0;
                return true;
            }
            else
                strumWindowOpen = true;
        }

        return false;
    }

    void hitNote(NoteController note)
    {
        ++noteStreak;
        ++notesHit;
        ++totalNotes;

        foreach (Note chordNote in note.note.GetChord())
        {
            chordNote.controller.hit = true;
            chordNote.controller.PlayIndicatorAnim();

            //if (strum)
                //chordNote.controller.DeactivateNote();
        }

        if (note.note.sustain_length > 0)
            currentSustains.Add(note);

        if (note.note.type != Note.Note_Type.STRUM)     // Allow a strum to happen after the note has already been hit
            freestrum = NUM_OF_FREESTRUMS;

        notesInWindow.Remove(note);
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
        if (note.hit || note.transform.position.y < editor.visibleStrikeline.position.y - (hitWindowHeight / 2))
        {
            notesInWindow.Remove(note);
            return true;
        }

        return false;
    }

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
#if GAMEPAD
        if (GameplayManager.gamepad != null)
        {
            GamePadState gamepad = (GamePadState)GameplayManager.gamepad;

            if (gamepad.Buttons.A == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.GREEN;

            if (gamepad.Buttons.B == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.RED;

            if (gamepad.Buttons.Y == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.YELLOW;

            if (gamepad.Buttons.X == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.BLUE;

            if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed)
                inputMask |= 1 << (int)Note.Fret_Type.ORANGE;
        }
#else
        
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
            
#endif
        return inputMask;
    }
}
