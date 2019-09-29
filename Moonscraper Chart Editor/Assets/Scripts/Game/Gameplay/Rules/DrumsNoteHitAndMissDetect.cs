using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimingConfig;

public class DrumsNoteHitAndMissDetect {
    public enum MissSubType
    {
        NoteMiss,
        Overhit,
    }

    public delegate void HitNoteFactory(float time, DrumsNoteHitKnowledge noteHitKnowledge);
    public delegate void MissNoteFactory(float time, MissSubType missSubType, DrumsNoteHitKnowledge noteHitKnowledge);

    HitNoteFactory m_hitNoteFactory;
    MissNoteFactory m_missNoteFactory;

    public void Reset()
    {
    }

    public DrumsNoteHitAndMissDetect(HitNoteFactory hitNoteFactory, MissNoteFactory missNoteFactory)
    {
        m_hitNoteFactory = hitNoteFactory;
        m_missNoteFactory = missNoteFactory;
    }

    public void Update(float time, HitWindow<DrumsNoteHitKnowledge> hitWindow, uint noteStreak, LaneInfo laneInfo)
    {
        DrumsNoteHitKnowledge nextNoteToHit = hitWindow.oldestUnhitNote;
        int inputMask = DrumsInput.GetPadPressedInputMask(laneInfo);

        if (nextNoteToHit != null)
        {
            int noteMask = nextNoteToHit.note.mask;
            int laneMask = laneInfo.laneMask;

            // Cull notes from the notemask by lanes that are being used
            foreach (Note.DrumPad pad in EnumX<Note.DrumPad>.Values)
            {
                if (pad == Note.DrumPad.Kick)
                    continue;

                int padBitInput = (int)pad;
                int padMask = 1 << padBitInput;

                bool includeLane = (padMask & laneMask) != 0;
                if (!includeLane)
                {
                    noteMask &= ~padMask;
                }
            }

            bool badHit = false;

            if ((inputMask | noteMask) != noteMask)
            {
                badHit = true;
            }
            else
            {
                foreach (Note.DrumPad drumPad in EnumX<Note.DrumPad>.Values)
                {
                    bool hitPad = DrumsInput.GetPadPressedInput(drumPad, laneInfo);
                    if (hitPad)
                    {
                        if (nextNoteToHit.GetHitTime(drumPad) == NoteHitKnowledge.NULL_TIME)
                            nextNoteToHit.SetHitTime(drumPad, time);
                        else
                            badHit = true;
                    }
                }
            }

            if (badHit)
            {
                // Bad input
                Debug.Log("Missed due to bad input");
                MissNote(time, MissSubType.Overhit);

                foreach (Note.DrumPad drumPad in EnumX<Note.DrumPad>.Values)
                {
                    nextNoteToHit.SetHitTime(drumPad, NoteHitKnowledge.NULL_TIME);
                }
            }
            else
            {
                float min = float.MaxValue, max = float.MinValue;
                int totalHitsMask = 0;

                foreach (Note.DrumPad drumPad in EnumX<Note.DrumPad>.Values)
                {
                    if (nextNoteToHit.GetHitTime(drumPad) != NoteHitKnowledge.NULL_TIME)
                    {
                        float hitTime = nextNoteToHit.GetHitTime(drumPad);
                        min = Mathf.Min(min, hitTime);
                        max = Mathf.Max(max, hitTime);

                        totalHitsMask |= 1 << (int)drumPad;
                    }
                }

                float totalSlop = max - min;

                if (min != float.MaxValue && time - min > DrumsTiming.slopBufferTime)
                {
                    // Technically an underhit
                    Debug.Log("Missed due to underhit");
                    MissNote(time, MissSubType.Overhit);
                }
                else if (totalHitsMask == noteMask && totalSlop < DrumsTiming.slopBufferTime)
                    HitNote(time, nextNoteToHit);
            }
        }
        else if (inputMask != 0)
        {
            Debug.Log("Missed due to hitting pad when there was no hit in window");
            MissNote(time, MissSubType.Overhit);
        }
    }

    void HitNote(float time, DrumsNoteHitKnowledge noteHitKnowledge)
    {
        m_hitNoteFactory(time, noteHitKnowledge);
    }

    void MissNote(float time, MissSubType missSubType, DrumsNoteHitKnowledge noteHitKnowledge = null)
    {
        m_missNoteFactory(time, missSubType, noteHitKnowledge);
    }
}
