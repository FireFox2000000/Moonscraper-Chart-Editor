
public class NoteHitKnowledge {
    public const float NULL_TIME = -1;

    public Note note;
    public bool hasBeenHit;
    public bool shouldExitWindow;

    public NoteHitKnowledge()
    {
        this.note = null;
        hasBeenHit = false;
        shouldExitWindow = false;
    }

    public NoteHitKnowledge(Note note)
    {
        this.note = note;
        hasBeenHit = false;
        shouldExitWindow = false;
    }
}
