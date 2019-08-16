
public class GuitarNoteHitKnowledge : NoteHitKnowledge
{
    public int strumCounter;
    public float fretValidationTime;
    public float strumValidationTime;
    public float lastestFretInvalidationTime;
    public float lastestStrumInvalidationTime;

    public GuitarNoteHitKnowledge() : base()
    {
    }

    public GuitarNoteHitKnowledge(Note note) : base(note)
    {
        fretValidationTime = strumValidationTime = NULL_TIME;
        strumCounter = 0;
    }

    public bool fretsValidated
    {
        get
        {
            return fretValidationTime != NULL_TIME;
        }
    }

    public bool strumValidated
    {
        get
        {
            return strumValidationTime != NULL_TIME;
        }
    }
}