#define GAMEPAD
//#define MISS_DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;
using Un4seen.Bass;

[RequireComponent(typeof(AudioSource))]
public class GameplayManager : MonoBehaviour {
    public AudioClip comboBreak;
    public CameraShake camShake;

    AudioSource audioSource;
    int sample;
    int channel;

    const float FREESTRUM_TIME = 0.2f;

    public GameObject statsPanel;
    public UnityEngine.UI.Text noteStreakText;
    public UnityEngine.UI.Text percentHitText;
    public UnityEngine.UI.Text totalHitText;

    static uint noteStreak = 0;
    public static uint ns { get { return noteStreak; } }
    uint notesHit = 0;
    uint totalNotes = 0;

    List<NoteController> physicsWindow = new List<NoteController>();
    List<NoteController> notesInWindow = new List<NoteController>();
    List<NoteController> currentSustains = new List<NoteController>();
    ChartEditor editor;

    const float BACKEND_HIT_WINDOW_TIME = 0.2f;
    const float FRONTEND_HIT_WINDOW_TIME = 0.116f;
    float initSize;

    float previousStrumValue;
    int previousInputMask;

    Note lastNoteHit = null;
    float? lastStrumTime = null;

    const float SLOP_WINDOW_SIZE = 0.3f;
    float slopWindowTimer = 0;

    bool strum = false;
    bool canTap;
#if GAMEPAD
    public static GamePadState? gamepad;
    public static GamePadState? previousGamepad;
#endif

    void Start()
    {
        //audioSource = GetComponent<AudioSource>();
        //audioSource.clip = comboBreak;

        byte[] comboBreakBytes = comboBreak.GetWavBytes();
        sample = Bass.BASS_SampleLoad(comboBreakBytes, 0, comboBreakBytes.Length, 1, BASSFlag.BASS_DEFAULT);
        channel = Bass.BASS_SampleGetChannel(sample, false);

        previousStrumValue = Input.GetAxisRaw("Strum");
        previousInputMask = GetFretInputMask();
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        initSize = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
    }

    void Update()
    {
        statsPanel.SetActive(Globals.applicationMode == Globals.ApplicationMode.Playing && !Globals.bot);

        uint startNS = noteStreak;

        // Update the Xbox 360 state
#if GAMEPAD
        previousGamepad = gamepad;
        gamepad = null;

        for (int i = 0; i < 1; ++i)
        {
            GamePadState testState = GamePad.GetState((PlayerIndex)i);
            if (testState.IsConnected)
            {
                gamepad = testState;
                break;
            }
        }
#endif
        // Configure collisions and choose to update the hit window or not
        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !Globals.bot)
        {
            transform.localScale = new Vector3(transform.localScale.x, initSize, transform.localScale.z);
            UpdateHitWindow();
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        }

        // Strum buffer stuff
        if (slopWindowTimer > SLOP_WINDOW_SIZE)
        {
            if (lastStrumTime == null || !(NoteInHitWindow(lastNoteHit, lastNoteHit.nextSeperateNote, (float)lastStrumTime) && lastNoteHit.type != Note.Note_Type.Strum))
            {
                noteStreak = 0;
#if MISS_DEBUG
                Debug.Log("Slop timer");
#endif
            }

            lastStrumTime = null;
        }
        
        if (lastStrumTime != null)
        {
            slopWindowTimer += Time.deltaTime;
        }
        else
            slopWindowTimer = 0;

        // Configure current strum input
        float strumValue = 0;
#if GAMEPAD
        if (gamepad != null)
        {
            if (((GamePadState)gamepad).DPad.Down == ButtonState.Pressed)
                strumValue = -1;
            else if (((GamePadState)gamepad).DPad.Up == ButtonState.Pressed)
                strumValue = 1;
        }
        else
        {
            // Keyboard controls
            if (Input.GetButtonDown("Strum Up"))
                strumValue = 1;
            else if (Input.GetButtonDown("Strum Down"))
                strumValue = -1;
        }
#else
        strumValue = Input.GetAxisRaw("Strum");    
#endif

        // Finalise if a strum has occured or not
        if (strumValue != 0 && strumValue != previousStrumValue)
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
            foreach (NoteController nCon in notesInWindow)
            {
                if (nCon.hit)
                    notesInWindow.Remove(nCon);
            }

            // What note is the player trying to hit next?
            Note nextNote = null;
            if (notesInWindow.Count > 0)
                nextNote = notesInWindow[0].note;

            if (nextNote != null)
            {
                Note nextSeperate = nextNote.nextSeperateNote;

                if (noteStreak > 0)
                {
                    bool fretsCorrect = ValidateFrets(notesInWindow[0].note);
                    bool strummingCorrect = ValidateStrum(notesInWindow[0].note, canTap);

                    if (fretsCorrect && (strummingCorrect || (lastStrumTime != null && NoteInHitWindow(nextNote, nextSeperate, (float)lastStrumTime))))
                    {
                        NoteController next = null;
                        if (notesInWindow.Count > 1)
                            next = notesInWindow[1];

                        hitNote(notesInWindow[0], next, strum);

                        if (!strummingCorrect)  // Using a previous strum
                        {
                            lastStrumTime = null;
                        }
                    }
                    else if (strum)
                    {
                        if (lastStrumTime != null)
                        {
                            noteStreak = 0;
                            lastStrumTime = null;
#if MISS_DEBUG
                            Debug.Log("Overstrum v4");
#endif
                        }
                        else
                        {
                            if (lastNoteHit != null && lastNoteHit.mask != nextNote.mask && NoteInHitWindow(nextNote, nextSeperate, Song.WorldYPositionToTime(editor.visibleStrikeline.position.y)))
                                lastStrumTime = Song.WorldYPositionToTime(editor.visibleStrikeline.position.y);
                            else
                            {
                                noteStreak = 0;
                                lastStrumTime = null;
#if MISS_DEBUG
                                Debug.Log("Overstrum v6");
#endif

                                // Possible false-positive here when strumming a HOPO, and then strumming the next HOPO but the user hasn't moved to the correct fret just yet
                            }
                        }
                    }
                }
                else
                {
                    HitNoteCheckRecovery();
                }
            }
            // No note in window
            else if (strum)
            {
                if (lastNoteHit != null && lastNoteHit.type != Note.Note_Type.Strum && NoteInHitWindow(lastNoteHit, lastNoteHit.nextSeperateNote, Song.WorldYPositionToTime(editor.visibleStrikeline.position.y)))
                {
                    lastStrumTime = null;
                    lastNoteHit = null;
                }
                else
                {
                    noteStreak = 0;
                    lastStrumTime = null;
#if MISS_DEBUG
                    Debug.Log("Strummed when no note v3");
#endif
                }
                /*
                if (lastStrumTime == null)
                    lastStrumTime = Song.WorldYPositionToTime(editor.visibleStrikeline.position.y);
                else
                {
                    noteStreak = 0;
                    lastStrumTime = null;
                    Debug.Log("Strummed when no note v3");
                }*/
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

            // Update UI
            noteStreakText.text = noteStreak.ToString();
            if (totalNotes > 0)
                percentHitText.text = ((float)notesHit / (float)totalNotes * 100).Round(2).ToString() + "%";
            else
                percentHitText.text = "0.00%";

            totalHitText.text = notesHit.ToString() + " / " + totalNotes.ToString();

            previousInputMask = inputMask;

            // Lost combo auditorial feedback
            if (startNS >= 10 && noteStreak < startNS && 
                    (
                        Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_STOPPED || Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_PAUSED
                    )
                )// !audioSource.isPlaying)
            {
                Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, Globals.sfxVolume * Globals.vol_master);
                Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_PAN, Globals.audio_pan);
                Bass.BASS_ChannelPlay(channel, false); // play it
                camShake.ShakeCamera();
                //Bass.BASS_ChannelPlay(sample, false);
                // audioSource.Play();
            }   
        }
        else
        {
            physicsWindow.Clear();
        }

        previousStrumValue = strumValue;
    }

    bool NoteInHitWindow (Note note, Note next, float currentTime)
    {  
        return !(note.time < currentTime - BACKEND_HIT_WINDOW_TIME
            || (next != null && note.time <= currentTime - (next.time - note.time) / 2.0f));        // half the distance to the next note5
    }

    public void Reset()
    {
        noteStreakText.text = "0";
        percentHitText.text = "0%";
        totalHitText.text = "0/0";
        noteStreak = 0;
        notesHit = 0;
        totalNotes = 0;
        notesInWindow.Clear();
        physicsWindow.Clear();
        lastNoteHit = null;
    }

    void HitNoteCheckRecovery()
    {
        // Search to see if user is hitting a note ahead
        List<NoteController> validatedNotes = new List<NoteController>();
        foreach (NoteController note in notesInWindow)
        {
            // Collect all notes the user is possibly hitting
            if (ValidateFrets(note.note) && ValidateStrum(note.note, canTap))
                validatedNotes.Add(note);
        }

        if (validatedNotes.Count > 0)
        {
            // Recovery algorithm
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
            NoteController note = notesInWindow[index];
            NoteController next = null;
            if (index < notesInWindow.Count - 1)
                next = notesInWindow[index + 1];

            if (index > 0)
                noteStreak = 0;

            hitNote(note, next, strum);

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
#if MISS_DEBUG
                Debug.Log("Strummed when no note");
#endif
            }
        }
    }

    void hitNote(NoteController note, NoteController next, bool strummed)
    {
        ++noteStreak;
        ++notesHit;
        ++totalNotes;
        lastStrumTime = null;

        foreach (Note chordNote in note.note.GetChord())
        {
            chordNote.controller.hit = true;
            chordNote.controller.PlayIndicatorAnim();
        }

        if (note.note.sustain_length > 0)
            currentSustains.Add(note);

        lastNoteHit = note.note;
        notesInWindow.Remove(note);
        canTap = false;
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
        if (!note.hit && note.transform.position.y < editor.visibleStrikeline.position.y + Song.TimeToWorldYPosition(FRONTEND_HIT_WINDOW_TIME))
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

    bool ExitWindow(NoteController note, NoteController nextNote)
    {
        Note next = null;
        if (nextNote != null)
            next = nextNote.note;

        if (!NoteInHitWindow(note.note, next, Song.WorldYPositionToTime(editor.visibleStrikeline.position.y)))
       // if (note.hit || note.transform.position.y < editor.visibleStrikeline.position.y - (Song.TimeToWorldYPosition(hitWindowTime / 2))
       //     || (nextNote != null && nextNote.transform.position.y <= editor.visibleStrikeline.position.y + Song.TimeToWorldYPosition(0.02f)))
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
            case (Note.Note_Type.Tap):
                if (canTap)
                    return true;
                else if (strum)
                    return true;
                else
                    return false;
            case (Note.Note_Type.Hopo):
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
                if (noteStreak == 0 || note.type == Note.Note_Type.Strum)
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
        else
        {
            // Keyboard controls
            for (int i = 0; i < 5; ++i)
            {
                if (Input.GetKey((i + 1).ToString()))
                    inputMask |= 1 << i;
            }
        }
#else
        
        if (Input.GetButton("Fret0"))
            inputMask |= 1 << (int)Note.Fret_Type.GREEN;

        if (Input.GetButton("Fret1"))
            inputMask |= 1 << (int)Note.Fret_Type.RED;

        if (Input.GetButton("Fret2"))
            inputMask |= 1 << (int)Note.Fret_Type.YELLOW;

        if (Input.GetButton("Fret3"))
            inputMask |= 1 << (int)Note.Fret_Type.BLUE;

        if (Input.GetButton("Fret4"))
            inputMask |= 1 << (int)Note.Fret_Type.ORANGE;
            
#endif
        return inputMask;
    }

    void UpdateHitWindow()
    {
        // Update the hit window
        foreach (NoteController note in physicsWindow.ToArray())
        {
            if (EnterWindow(note))
                physicsWindow.Remove(note);
        }

        for (int i = 0; i < notesInWindow.Count; ++i)
        {
            NoteController noteToTest = notesInWindow[i];
            NoteController next = null;
            if ((i + 1) < notesInWindow.Count)
                next = notesInWindow[i + 1];

            if (ExitWindow(noteToTest, next) && !noteToTest.hit)
            {
                foreach (Note chordNote in noteToTest.note.GetChord())
                {
                    chordNote.controller.sustainBroken = true;

                    if (noteStreak > 0)
                        chordNote.controller.DeactivateNote();
                }

                noteStreak = 0;
#if MISS_DEBUG
                Debug.Log("Missed note: " + noteToTest.note.fret_type + ", " + noteToTest.note.position);
#endif
                ++totalNotes;
            }
        }
    }

    ~GameplayManager()
    {
        Bass.BASS_SampleFree(sample);
    }
}
