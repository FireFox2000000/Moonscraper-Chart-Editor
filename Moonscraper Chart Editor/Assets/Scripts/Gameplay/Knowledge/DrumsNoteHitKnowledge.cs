
public class DrumsNoteHitKnowledge : NoteHitKnowledge  {

    float[] drumHitTimes = new float[System.Enum.GetNames(typeof(Note.Drum_Fret_Type)).Length];

    public DrumsNoteHitKnowledge(Note note) : base(note)
    {
        for (int i = 0; i < drumHitTimes.Length; ++i)
        {
            drumHitTimes[i] = NULL_TIME;
        }
    }

    public float GetHitTime(Note.Drum_Fret_Type noteType)
    {
        return drumHitTimes[(int)noteType];
    }

    public void SetHitTime(Note.Drum_Fret_Type noteType, float time)
    {
        drumHitTimes[(int)noteType] = time;
    }
}
