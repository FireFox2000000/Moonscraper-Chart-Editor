// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

public class DrumsNoteHitKnowledge : NoteHitKnowledge
{
    public class InputTiming
    {
        float[] drumHitTimes;

        public InputTiming()
        {
            drumHitTimes = new float[System.Enum.GetNames(typeof(Note.DrumPad)).Length];
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < drumHitTimes.Length; ++i)
            {
                drumHitTimes[i] = NULL_TIME;
            }
        }

        public float GetHitTime(Note.DrumPad noteType)
        {
            return drumHitTimes[(int)noteType];
        }

        public void SetHitTime(Note.DrumPad noteType, float time)
        {
            drumHitTimes[(int)noteType] = time;
        }
    }

    public InputTiming standardPadTiming { get; private set; }
    public InputTiming cymbalPadTiming { get; private set; }

    public DrumsNoteHitKnowledge() : base()
    {
        standardPadTiming = new InputTiming();
        cymbalPadTiming = new InputTiming();
    }

    public override void SetFrom(Note note)
    {
        base.SetFrom(note);
        standardPadTiming.Reset();
        cymbalPadTiming.Reset();
    }
}
