using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuitarNoteHitKnowledge
{
    // Move this to an abstract parent class if/when new note types get added
    public bool hasBeenHit;
    public bool shouldExitWindow;

    public const float NULL_TIME = -1;

    public Note note;
    public int strumCounter;

    public float fretValidationTime;
    public float strumValidationTime;
    public float lastestFretInvalidationTime;
    public float lastestStrumInvalidationTime;
    
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

    public GuitarNoteHitKnowledge(Note note)
    {
        this.note = note;
        fretValidationTime = strumValidationTime = NULL_TIME;
        strumCounter = 0;
        hasBeenHit = false;
        shouldExitWindow = false;
    }
}