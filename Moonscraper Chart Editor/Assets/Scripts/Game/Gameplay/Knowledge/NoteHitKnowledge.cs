// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

public class NoteHitKnowledge {
    public const float NULL_TIME = -1;

    public Note note;
    public bool hasBeenHit;
    public bool shouldExitWindow;

    public NoteHitKnowledge() : this(null)
    {
    }

    public NoteHitKnowledge(Note note)
    {
        this.note = note;
        hasBeenHit = false;
        shouldExitWindow = false;
    }
}
