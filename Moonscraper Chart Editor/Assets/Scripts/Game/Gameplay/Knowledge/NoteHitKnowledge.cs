// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

public class NoteHitKnowledge {
    public const float NULL_TIME = -1;

    public Note note;
    public bool hasBeenHit;
    public bool shouldExitWindow;

    public NoteHitKnowledge()
    {
    }

    public virtual void SetFrom(Note note)
    {
        this.note = note;
        hasBeenHit = false;
        shouldExitWindow = false;
    }
}
