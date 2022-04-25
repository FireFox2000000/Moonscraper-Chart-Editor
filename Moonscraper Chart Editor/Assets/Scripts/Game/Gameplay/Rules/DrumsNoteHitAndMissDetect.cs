// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using TimingConfig;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

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
        switch (Globals.gameSettings.drumsModeOptions)
        {
            case GameSettings.DrumModeOptions.ProDrums:
                {
                    UpdateProDrums(time, hitWindow, noteStreak, laneInfo);
                    break;
                }
            default:
                {
                    UpdateStandardDrums(time, hitWindow, noteStreak, laneInfo);
                    break;
                }
        }
    }

    void UpdateStandardDrums(float time, HitWindow<DrumsNoteHitKnowledge> hitWindow, uint noteStreak, LaneInfo laneInfo)
    {
        DrumsNoteHitKnowledge nextNoteToHit = hitWindow.oldestUnhitNote;
        int inputMask = DrumsInput.GetPadPressedInputMask(laneInfo);

        if (nextNoteToHit != null)
        {
            int noteMask = nextNoteToHit.note.GetMaskCappedLanes(laneInfo);
            DrumsNoteHitKnowledge.InputTiming hitTimingTracker = nextNoteToHit.standardPadTiming;
            bool standardPadSuccess = ShouldHitAndProcessMiss(laneInfo, time, inputMask, noteMask, hitTimingTracker);

            if (standardPadSuccess)
            {
                HitNote(time, nextNoteToHit);
            }
        }
        else if (inputMask != 0)
        {
            Debug.Log("Missed due to hitting pad when there was no hit in window");
            MissNote(time, MissSubType.Overhit);
        }
    }

    void UpdateProDrums(float time, HitWindow<DrumsNoteHitKnowledge> hitWindow, uint noteStreak, LaneInfo laneInfo)
    {
        DrumsNoteHitKnowledge nextNoteToHit = hitWindow.oldestUnhitNote;
        int tomsInputMask = DrumsInput.GetProDrumsTomPressedInputMask(laneInfo);
        int cymbalsInputMask = DrumsInput.GetProDrumsCymbalPressedInputMask(laneInfo);

        if (nextNoteToHit != null)
        {
            // process toms
            bool tomsHitSuccess = false;
            {
                int tomsNoteMask = nextNoteToHit.note.GetMaskWithRequiredFlagsLaneCapped(Note.Flags.None, laneInfo);
                DrumsNoteHitKnowledge.InputTiming tomsHitTimingTracker = nextNoteToHit.standardPadTiming;
                tomsHitSuccess = ShouldHitAndProcessMiss(laneInfo, time, tomsInputMask, tomsNoteMask, tomsHitTimingTracker);
            }

            // process cymbals
            bool cymbalsHitSuccess = false;
            {
                int cymbalsNoteMask = nextNoteToHit.note.GetMaskWithRequiredFlagsLaneCapped(Note.Flags.ProDrums_Cymbal, laneInfo);
                DrumsNoteHitKnowledge.InputTiming cymbalsHitTimingTracker = nextNoteToHit.cymbalPadTiming;
                cymbalsHitSuccess = ShouldHitAndProcessMiss(laneInfo, time, cymbalsInputMask, cymbalsNoteMask, cymbalsHitTimingTracker);
            }

            if (tomsHitSuccess && cymbalsHitSuccess)
            {
                HitNote(time, nextNoteToHit);
            }
        }
        else if (tomsInputMask != 0 || cymbalsInputMask != 0)
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

    bool ShouldHitAndProcessMiss
        (LaneInfo laneInfo
        , float time
        , int inputMask
        , int noteMask
        , DrumsNoteHitKnowledge.InputTiming hitTimingTracker
        )
    {
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

        if ((inputMask | noteMask) != noteMask)     // Have we hit any extra notes? Like note is G Y, but user hit G Y B
        {
            badHit = true;
        }
        else
        {
            foreach (Note.DrumPad drumPad in EnumX<Note.DrumPad>.Values)
            {
                bool hitPad = (inputMask & (1 << (int)drumPad)) != 0;
                if (hitPad)
                {
                    if (hitTimingTracker.GetHitTime(drumPad) == NoteHitKnowledge.NULL_TIME)
                    {
                        hitTimingTracker.SetHitTime(drumPad, time);
                    }
                    else
                    {
                        badHit = true;
                    }
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
                hitTimingTracker.SetHitTime(drumPad, NoteHitKnowledge.NULL_TIME);
            }
        }
        else
        {
            float min = float.MaxValue, max = float.MinValue;
            int totalHitsMask = 0;

            foreach (Note.DrumPad drumPad in EnumX<Note.DrumPad>.Values)
            {
                if (hitTimingTracker.GetHitTime(drumPad) != NoteHitKnowledge.NULL_TIME)
                {
                    float hitTime = hitTimingTracker.GetHitTime(drumPad);
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
            {
                return true;
            }
        }

        return false;
    }
}
