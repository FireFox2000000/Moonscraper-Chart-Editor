using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuitarGameplayRulestate : BaseGameplayRulestate {

    GuitarNoteHitAndMissDetect hitAndMissNoteDetect;
    GuitarSustainBreakDetect sustainBreakDetect;
    GuitarSustainHitKnowledge guitarSustainHitKnowledge;

    public GuitarGameplayRulestate(MissFeedback missFeedbackFn) : base(missFeedbackFn)
    {
        hitAndMissNoteDetect = new GuitarNoteHitAndMissDetect(HitNote, MissNote);
        sustainBreakDetect = new GuitarSustainBreakDetect(SustainBreak);
        guitarSustainHitKnowledge = new GuitarSustainHitKnowledge();
    }

    // Update is called once per frame
    public void Update (float time, HitWindow<GuitarNoteHitKnowledge> hitWindow, GamepadInput guitarInput) {
        uint noteStreak = stats.noteStreak;
        int missCount = 0;

        {
            var notesRemoved = hitWindow.DetectExit(time);
            
            foreach (var noteKnowledge in notesRemoved)
            {
                // Miss, exited window
                if (!noteKnowledge.hasBeenHit)
                {
                    foreach (Note chordNote in noteKnowledge.note.GetChord())
                    {
                        chordNote.controller.sustainBroken = true;

                        if (noteStreak > 0)
                            chordNote.controller.DeactivateNote();
                    }

                    ++missCount;
                }
            }
        }

        for (int i = 0; i < missCount; ++i)
        {
            if (noteStreak > 0)
                Debug.Log("Missed due to note falling out of window");

            MissNote(time, GuitarNoteHitAndMissDetect.MissSubType.NoteMiss, null);
        }

        guitarSustainHitKnowledge.Update(time);
        hitAndMissNoteDetect.Update(time, hitWindow, guitarInput, noteStreak, guitarSustainHitKnowledge);
        sustainBreakDetect.Update(time, guitarSustainHitKnowledge, guitarInput, noteStreak);
    }

    public void Reset()
    {
        hitAndMissNoteDetect.Reset();
        sustainBreakDetect.Reset();
        guitarSustainHitKnowledge.Reset();
        stats.Reset();
    }

    void HitNote(float time, GuitarNoteHitKnowledge noteHitKnowledge)
    {
        // Force the note out of the window
        noteHitKnowledge.hasBeenHit = true;
        noteHitKnowledge.shouldExitWindow = true;

        Note note = noteHitKnowledge.note;

        ++stats.noteStreak;
        ++stats.notesHit;
        ++stats.totalNotes;

        foreach (Note chordNote in note.GetChord())
        {
            chordNote.controller.hit = true;
            chordNote.controller.PlayIndicatorAnim();
        }

        if (note.sustain_length > 0 && note.controller)
            guitarSustainHitKnowledge.Add(note);
    }

    void MissNote(float time, GuitarNoteHitAndMissDetect.MissSubType missSubType, GuitarNoteHitKnowledge noteHitKnowledge)
    {
        if (stats.noteStreak > 10)
        {
            missFeedbackFn();
        }

        stats.noteStreak = 0;

        if (missSubType == GuitarNoteHitAndMissDetect.MissSubType.NoteMiss)
            ++stats.totalNotes;

        if (noteHitKnowledge != null)
        {
            noteHitKnowledge.hasBeenHit = true; // Don't want to count this as a miss twice when it gets removed from the window
            noteHitKnowledge.shouldExitWindow = true;
        }
    }

    void SustainBreak(float time, Note note)
    {
        foreach (Note chordNote in note.GetChord())
        {
            if (chordNote.controller)
                chordNote.controller.sustainBroken = true;
            else
                Debug.LogError("Trying to break the sustain of a note without a controller");
        }
    }
}
