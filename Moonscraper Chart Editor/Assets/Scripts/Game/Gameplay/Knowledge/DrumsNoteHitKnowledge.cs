// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

public class DrumsNoteHitKnowledge : NoteHitKnowledge  {

    float[] drumHitTimes = new float[System.Enum.GetNames(typeof(Note.DrumPad)).Length];

    public DrumsNoteHitKnowledge(Note note) : base(note)
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
