﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

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
    public void Update (float time, HitWindow<GuitarNoteHitKnowledge> hitWindow) {
        int missCount = UpdateWindowExit(time, hitWindow);

        for (int i = 0; i < missCount; ++i)
        {
            if (stats.noteStreak > 0)
                Debug.Log("Missed due to note falling out of window");

            MissNote(time, GuitarNoteHitAndMissDetect.MissSubType.NoteMiss, null);
        }

        guitarSustainHitKnowledge.Update(time);
        sustainBreakDetect.Update(time, guitarSustainHitKnowledge, stats.noteStreak);     
        hitAndMissNoteDetect.Update(time, hitWindow, stats.noteStreak, guitarSustainHitKnowledge);        
    }

    public override void Reset()
    {
        base.Reset();
        hitAndMissNoteDetect.Reset();
        sustainBreakDetect.Reset();
        guitarSustainHitKnowledge.Reset();
    }

    void HitNote(float time, GuitarNoteHitKnowledge noteHitKnowledge)
    {
        base.HitNote(time, noteHitKnowledge);

        Note note = noteHitKnowledge.note;
        if (note.length > 0 && note.controller)
            guitarSustainHitKnowledge.Add(note);
    }

    void MissNote(float time, GuitarNoteHitAndMissDetect.MissSubType missSubType, GuitarNoteHitKnowledge noteHitKnowledge)
    {
        base.MissNote(time, missSubType == GuitarNoteHitAndMissDetect.MissSubType.NoteMiss, noteHitKnowledge);
    }

    void SustainBreak(float time, Note note)
    {
        foreach (Note chordNote in note.chord)
        {
            if (chordNote.controller)
                chordNote.controller.sustainBroken = true;
            else
                Debug.LogError("Trying to break the sustain of a note without a controller");
        }
    }
}
