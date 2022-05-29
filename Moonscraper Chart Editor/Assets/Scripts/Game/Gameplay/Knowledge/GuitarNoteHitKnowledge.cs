// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

public class GuitarNoteHitKnowledge : NoteHitKnowledge
{
    public int strumCounter;
    public float fretValidationTime;
    public float strumValidationTime;
    public float lastestFretInvalidationTime;
    public float lastestStrumInvalidationTime;

    public GuitarNoteHitKnowledge() : base()
    {
        fretValidationTime = strumValidationTime = NULL_TIME;
        strumCounter = 0;
    }

    public override void SetFrom(Note note)
    {
        base.SetFrom(note);
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