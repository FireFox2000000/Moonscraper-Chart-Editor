using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class GuitarNoteHitAndMissDetect {

    public enum MissSubType
    {
        NoteMiss,
        Overstrum,
    }

    const float c_fretAndStrumSlopBuffer = 0.3f;

    public delegate void HitNoteFactory(float time, GuitarNoteHitKnowledge noteHitKnowledge);
    public delegate void MissNoteFactory(float time, MissSubType missSubType);

    HitNoteFactory m_hitNoteFactory;
    MissNoteFactory m_missNoteFactory;

    int previousInputMask;
    float previousStrumValue;
    bool canTap;
    GuitarNoteHitKnowledge lastNoteHit = null;

    public void Reset()
    {
        previousInputMask = 0;
        previousStrumValue = 0;
        canTap = true;
        lastNoteHit = null;
    }

    public GuitarNoteHitAndMissDetect(HitNoteFactory hitNoteFactory, MissNoteFactory missNoteFactory)
    {
        m_hitNoteFactory = hitNoteFactory;
        m_missNoteFactory = missNoteFactory;
    }
	
	public void Update (float time, HitWindow hitWindow, GamePadState? gamepad, uint noteStreak)
    {
        // Capture input
        bool strum = GameplayInputFunctions.GetStrumInput(gamepad, previousStrumValue, out previousStrumValue);
        int inputMask = GameplayInputFunctions.GetFretInputMask(gamepad);
        if (inputMask != previousInputMask)
            canTap = true;

        // What note is the player trying to hit next?
        GuitarNoteHitKnowledge nextNoteToHit = hitWindow.oldestUnhitNote;

        UpdateNoteKnowledge(time, hitWindow, inputMask, strum, noteStreak, nextNoteToHit);

        if (nextNoteToHit != null)
        {
            Note nextSeperate = nextNoteToHit.note.nextSeperateNote;

            if (noteStreak > 0)
            {
                PreserveStreakDetect(time, hitWindow, gamepad, strum, noteStreak, nextNoteToHit, inputMask);
            }
            else
            {
                RecoveryDetect(time, hitWindow, gamepad, strum, noteStreak);
            }
        }
        // No note in window
        else if (strum)
        {
            // Are we strumming late for a hopo/tap?
            // Todo, need to confirm with strum counter
            if (lastNoteHit != null && lastNoteHit.note.type != Note.Note_Type.Strum && hitWindow.IsWithinTimeWindow(lastNoteHit.note, lastNoteHit.note.nextSeperateNote, time))
            {
                lastNoteHit = null;
            }
            else
            {
                MissNote(time, MissSubType.Overstrum);
                Debug.Log("Missed due to strum input when no note was present in the window");
            }
        }

        previousInputMask = inputMask;
    }

    void UpdateNoteKnowledge(float time, HitWindow hitWindow, int inputMask, bool strummed, uint noteStreak, GuitarNoteHitKnowledge nextNoteToHit)
    {
        if (nextNoteToHit != null)
        {
            // Fill out note knowledge
            if (GameplayInputFunctions.ValidateFrets(nextNoteToHit.note, inputMask, noteStreak))
                nextNoteToHit.fretValidationTime = time;
            else
                nextNoteToHit.lastestFretInvalidationTime = time;

            if (GameplayInputFunctions.ValidateStrum(nextNoteToHit.note, canTap, strummed, noteStreak))
                nextNoteToHit.strumValidationTime = time;
            else
                nextNoteToHit.lastestStrumInvalidationTime = time;

            if (strummed)
                ++nextNoteToHit.strumCounter;
        }
    }

    void PreserveStreakDetect(float time, HitWindow hitWindow, GamePadState? gamepad, bool strummed, uint noteStreak, GuitarNoteHitKnowledge nextNoteToHit, int inputMask)
    {
        if (nextNoteToHit.strumCounter > 1)
        {
            MissNote(time, MissSubType.Overstrum);
            Debug.Log("Missed note due to double strumming on a single note");
        }
        else if (nextNoteToHit.fretsValidated && nextNoteToHit.strumValidated && Mathf.Abs(nextNoteToHit.fretValidationTime - nextNoteToHit.strumValidationTime) <= c_fretAndStrumSlopBuffer)
        {
            HitNote(time, nextNoteToHit);
        }
        else if (nextNoteToHit.strumValidated && Mathf.Abs(time - nextNoteToHit.strumValidationTime) > c_fretAndStrumSlopBuffer)
        {
            MissNote(time, MissSubType.Overstrum);
            Debug.Log("Missed note due to strum expiring on a note");

            nextNoteToHit.strumValidationTime = GuitarNoteHitKnowledge.NULL_TIME;
        }
    }

    void RecoveryDetect(float time, HitWindow hitWindow, GamePadState? gamepad, bool strummed, uint noteStreak)
    {
        var noteKnowledge = hitWindow.noteKnowledge;

        // Search to see if user is hitting a note ahead
        List<GuitarNoteHitKnowledge> validatedNotes = new List<GuitarNoteHitKnowledge>();
        foreach (GuitarNoteHitKnowledge note in noteKnowledge)
        {
            // Collect all notes the user is possibly hitting
            if (
                    GameplayInputFunctions.ValidateFrets(note.note, GameplayInputFunctions.GetFretInputMask(gamepad), noteStreak)
                    && GameplayInputFunctions.ValidateStrum(note.note, canTap, strummed, noteStreak)
                )
                validatedNotes.Add(note);
        }

        if (validatedNotes.Count > 0)
        {
            // Recovery algorithm
            // Select the note closest to the strikeline
            float aimYPos = ChartEditor.GetInstance().visibleStrikeline.transform.position.y + 0.25f;  // Added offset from the note controller

            GuitarNoteHitKnowledge selectedNote = validatedNotes[0];

            float dis = -1;

            foreach (GuitarNoteHitKnowledge validatedNote in validatedNotes)
            {
                if (!selectedNote.note.controller)
                    return;

                NoteController noteController = selectedNote.note.controller;

                float distance = Mathf.Abs(aimYPos - noteController.transform.position.y);
                if (distance < dis || dis < 0)
                {
                    selectedNote = validatedNote;
                    dis = distance;
                }
            }

            int index = noteKnowledge.IndexOf(selectedNote);
            GuitarNoteHitKnowledge note = noteKnowledge[index];
            GuitarNoteHitKnowledge next = null;
            if (index < noteKnowledge.Count - 1)
                next = noteKnowledge[index + 1];

            int notesMissed = index;

            // Recovery missed notes
            Debug.Log("Notestreak recovery. Notes skipped = " + notesMissed);
            for (int missedCounter = 0; missedCounter < notesMissed - 1; ++missedCounter)
            {
                MissNote(time, MissSubType.NoteMiss);
            }

            HitNote(time, note);

            // Remove all previous notes
            GuitarNoteHitKnowledge[] nConArray = noteKnowledge.ToArray();
            for (int j = notesMissed - 1; j >= 0; --j)
            {
                noteKnowledge[j].shouldExitWindow = true;
            }
        }
        else
        {
            // Will not reach here if user hit a note
            if (strummed)
            {
                Debug.Log("Missed due to strumming when there were no notes to strum during recovery");
            }
        }
    }

    void HitNote(float time, GuitarNoteHitKnowledge noteHitKnowledge)
    {
        m_hitNoteFactory(time, noteHitKnowledge);
        canTap = false;
        lastNoteHit = noteHitKnowledge;
    }

    void MissNote(float time, MissSubType missSubType)
    {
        m_missNoteFactory(time, missSubType);
    }
}
