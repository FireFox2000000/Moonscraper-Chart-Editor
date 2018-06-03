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
}
