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

    const float SLOP_BUFFER_TIME = 0.3f;

    public delegate void HitNoteFactory(float time, GuitarNoteHitKnowledge noteHitKnowledge);
    public delegate void MissNoteFactory(float time, MissSubType missSubType, GuitarNoteHitKnowledge noteHitKnowledge);

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
	
	public void Update (float time, HitWindow<GuitarNoteHitKnowledge> hitWindow, GamePadState? gamepad, uint noteStreak)
    {
        // Capture input
        bool strum = GameplayInputFunctions.GetStrumInput(gamepad, previousStrumValue, out previousStrumValue);
        int inputMask = GameplayInputFunctions.GetFretInputMask(gamepad);
        if (inputMask != previousInputMask)
            canTap = true;

        // What note is the player trying to hit next?
        GuitarNoteHitKnowledge nextNoteToHit = hitWindow.oldestUnhitNote;

        // Check if it's valid to query the last hit note
        if (noteStreak <= 0 || lastNoteHit == null || !hitWindow.IsWithinTimeWindow(lastNoteHit.note, nextNoteToHit != null ? nextNoteToHit.note : null, time))
        {
            lastNoteHit = null;
        }

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
        else
        {
            BlankWindowDetect(time, strum);
        }

        previousInputMask = inputMask;
    }

    void UpdateNoteKnowledge(float time, HitWindow<GuitarNoteHitKnowledge> hitWindow, int inputMask, bool strummed, uint noteStreak, GuitarNoteHitKnowledge nextNoteToHit)
    {
        if (nextNoteToHit != null)
        {
            if (nextNoteToHit.strumCounter > 1)
                nextNoteToHit.strumCounter = 1;     // Make this still valid to hit because it's still in the hit window for a reason

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
            {
                if (lastNoteHit != null && lastNoteHit.strumCounter <= 0)// lastNoteHit.note.type != Note.Note_Type.Strum)
                    ++lastNoteHit.strumCounter;
                else
                    ++nextNoteToHit.strumCounter;
            }
        }
    }

    void PreserveStreakDetect(float time, HitWindow<GuitarNoteHitKnowledge> hitWindow, GamePadState? gamepad, bool strummed, uint noteStreak, GuitarNoteHitKnowledge nextNoteToHit, int inputMask)
    {
        if (nextNoteToHit.strumCounter > 1)
        {
            MissNote(time, MissSubType.Overstrum);
            Debug.Log("Missed note due to double strumming on a single note");
        }
        else if (nextNoteToHit.fretsValidated && nextNoteToHit.strumValidated && Mathf.Abs(nextNoteToHit.fretValidationTime - nextNoteToHit.strumValidationTime) <= SLOP_BUFFER_TIME)
        {
            HitNote(time, nextNoteToHit);
        }
        else if (nextNoteToHit.strumValidated && Mathf.Abs(time - nextNoteToHit.strumValidationTime) > SLOP_BUFFER_TIME && nextNoteToHit.strumCounter > 0)
        {
            MissNote(time, MissSubType.Overstrum);
            Debug.Log("Missed note due to strum expiration");

            nextNoteToHit.strumValidationTime = GuitarNoteHitKnowledge.NULL_TIME;
        }
    }

    void RecoveryDetect(float time, HitWindow<GuitarNoteHitKnowledge> hitWindow, GamePadState? gamepad, bool strummed, uint noteStreak)
    {
        var noteKnowledgeList = hitWindow.noteKnowledgeQueue;

        // Search to see if user is hitting a note ahead
        List<GuitarNoteHitKnowledge> validatedNotes = new List<GuitarNoteHitKnowledge>();
        foreach (GuitarNoteHitKnowledge noteKnowledge in noteKnowledgeList)
        {
            // Collect all notes the user is possibly hitting
            if (
                    GameplayInputFunctions.ValidateFrets(noteKnowledge.note, GameplayInputFunctions.GetFretInputMask(gamepad), noteStreak)
                    && GameplayInputFunctions.ValidateStrum(noteKnowledge.note, canTap, strummed, noteStreak)
                )
                validatedNotes.Add(noteKnowledge);
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

            int index = noteKnowledgeList.IndexOf(selectedNote);
            GuitarNoteHitKnowledge note = noteKnowledgeList[index];

            // Recovery missed notes
            if (index > 0)
                Debug.Log("Missed notes when performing recovery. Notes skipped = " + index);

            for (int missedCounter = 0; missedCounter < index; ++missedCounter)
            {
                MissNote(time, MissSubType.NoteMiss, noteKnowledgeList[missedCounter]);
            }

            HitNote(time, note);
        }
        else if (strummed)
        {
            MissNote(time, MissSubType.Overstrum);
            Debug.Log("Missed due to strumming when there were no notes to strum during recovery");
        }
    }

    void BlankWindowDetect(float time, bool strummed)
    {
        if (strummed)
        {
            // Are we strumming late for a hopo/tap?
            if (lastNoteHit != null && lastNoteHit.note.type != Note.Note_Type.Strum && lastNoteHit.strumCounter <= 1)
            {
                lastNoteHit = null;
            }
            else
            {
                MissNote(time, MissSubType.Overstrum);
                Debug.Log("Missed due to strum input when no note was present in the window");
            }
        }
    }

    void HitNote(float time, GuitarNoteHitKnowledge noteHitKnowledge)
    {
        m_hitNoteFactory(time, noteHitKnowledge);
        canTap = false;
        lastNoteHit = noteHitKnowledge;
    }

    void MissNote(float time, MissSubType missSubType, GuitarNoteHitKnowledge noteHitKnowledge = null)
    {
        m_missNoteFactory(time, missSubType, noteHitKnowledge);
    }
}
