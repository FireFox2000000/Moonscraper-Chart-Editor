using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotGameplayRulestate : BaseGameplayRulestate
{
    public BotGameplayRulestate(MissFeedback missFeedbackFn) : base(missFeedbackFn)
    {

    }

    public void Update(float time, HitWindow<NoteHitKnowledge> hitWindow)
    {
        NoteHitKnowledge nextNoteToHit = hitWindow.oldestUnhitNote;
        if (nextNoteToHit != null)
        {
            NoteController nCon = nextNoteToHit.note.controller;
            if (nCon != null)
            {
                Vector3 notePosition = nCon.transform.position;
                Vector3 strikelinePosition = ChartEditor.Instance.visibleStrikeline.position;

                bool belowStrikeLine = notePosition.y <= strikelinePosition.y + (Time.deltaTime * GameSettings.hyperspeed / GameSettings.gameSpeed);
                if (belowStrikeLine)
                {
                    HitNote(time, nextNoteToHit);
                }
            }
        }
    }

    void HitNote(float time, NoteHitKnowledge noteHitKnowledge)
    {
        base.HitNote(time, noteHitKnowledge);
    }
}
