// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

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
    Note.ChordEnumerateFn setNoteHitFn;

    public BaseGameplayRulestate(MissFeedback missFeedbackFn)
    {
        setNoteHitFn = SetNoteHit;

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
                        var controller = chordNote.controller;

                        if (controller != null)
                        {
                            controller.sustainBroken = true;

                            if (noteStreak > 0)
                                controller.DeactivateNote();
                        }
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
        note.EnumerateChord(setNoteHitFn);
    }

    void SetNoteHit(Note note)
    {
        if (note.controller != null)       // Note may not actually be present on the highway due to laneinfo culling.
        {
            note.controller.hit = true;
            note.controller.PlayIndicatorAnim();
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
