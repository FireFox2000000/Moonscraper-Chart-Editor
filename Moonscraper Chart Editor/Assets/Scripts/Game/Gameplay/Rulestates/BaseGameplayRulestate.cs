// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGameplayRulestate {
    public struct NoteStats
    {
        public uint noteStreak;
        public uint notesHit;
        public uint totalNotes;

        public void Reset()
        {
            noteStreak = 0;
            notesHit = 0;
            totalNotes = 0;
        }
    }

    public delegate void MissFeedback();

    protected MissFeedback missFeedbackFn;
    public NoteStats stats;

    public BaseGameplayRulestate(MissFeedback missFeedbackFn)
    {
        this.missFeedbackFn = missFeedbackFn;
        stats.Reset();
    }

    public virtual void Reset()
    {
        stats.Reset();
    }


    protected int UpdateWindowExit<TNoteHitKnowledge>(float time, HitWindow<TNoteHitKnowledge> hitWindow) where TNoteHitKnowledge : NoteHitKnowledge
    {
        uint noteStreak = stats.noteStreak;
        int missCount = 0;

        {
            var notesRemoved = hitWindow.DetectExit(time);

            foreach (var noteKnowledge in notesRemoved)
            {
                // Miss, exited window
                if (!noteKnowledge.hasBeenHit)
                {
                    foreach (Note chordNote in noteKnowledge.note.chord)
                    {
                        chordNote.controller.sustainBroken = true;

                        if (noteStreak > 0)
                            chordNote.controller.DeactivateNote();
                    }

                    ++missCount;
                }
            }
        }

        return missCount;
    }

    protected void HitNote<TNoteHitKnowledge>(float time, TNoteHitKnowledge noteHitKnowledge) where TNoteHitKnowledge : NoteHitKnowledge
    {
        // Force the note out of the window
        noteHitKnowledge.hasBeenHit = true;
        noteHitKnowledge.shouldExitWindow = true;

        ++stats.noteStreak;
        ++stats.notesHit;
        ++stats.totalNotes;

        Note note = noteHitKnowledge.note;
        foreach (Note chordNote in note.chord)
        {
            if (chordNote.controller != null)       // Note may not actually be present on the highway due to laneinfo culling.
            {
                chordNote.controller.hit = true;
                chordNote.controller.PlayIndicatorAnim();
            }
        }
    }

    protected void MissNote<TNoteHitKnowledge>(float time, bool hasMissedActualNote, TNoteHitKnowledge noteHitKnowledge) where TNoteHitKnowledge : NoteHitKnowledge
    {
        if (stats.noteStreak > 10)
        {
            missFeedbackFn();
        }

        stats.noteStreak = 0;

        if (hasMissedActualNote)
            ++stats.totalNotes;

        if (noteHitKnowledge != null)
        {
            noteHitKnowledge.hasBeenHit = true; // Don't want to count this as a miss twice when it gets removed from the window
            noteHitKnowledge.shouldExitWindow = true;
        }
    }
}
