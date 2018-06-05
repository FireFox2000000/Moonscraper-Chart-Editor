using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrumsInput;
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

    public void Update(float time, HitWindow<DrumsNoteHitKnowledge> hitWindow, GamepadInput drumsInput, uint noteStreak)
    {
        DrumsNoteHitKnowledge nextNoteToHit = hitWindow.oldestUnhitNote;
        int inputMask = drumsInput.GetPadPressedInputMaskControllerOrKeyboard();

        if (nextNoteToHit != null)
        {
            int noteMask = nextNoteToHit.note.mask;

            bool badHit = false;

            if ((inputMask | noteMask) != noteMask)
            {
                badHit = true;
            }
            else
            {
                foreach (Note.Drum_Fret_Type drumPad in System.Enum.GetValues(typeof(Note.Drum_Fret_Type)))
                {
                    bool hitPad = drumsInput.GetPadInputControllerOrKeyboard(drumPad);
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

                foreach (Note.Drum_Fret_Type drumPad in System.Enum.GetValues(typeof(Note.Drum_Fret_Type)))
                {
                    nextNoteToHit.SetHitTime(drumPad, NoteHitKnowledge.NULL_TIME);
                }
            }
            else
            {
                float min = float.MaxValue, max = float.MinValue;
                int totalHitsMask = 0;

                foreach (Note.Drum_Fret_Type drumPad in System.Enum.GetValues(typeof(Note.Drum_Fret_Type)))
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
