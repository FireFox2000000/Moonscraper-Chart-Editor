// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

public class HitWindowFeeder
{
    public IHitWindow hitWindow = null;

    int feederNoteIndex = -1;
    uint lastAddedTick = uint.MaxValue;

    public HitWindowFeeder()
    {
        Reset();
    }

    public void Update()
    {
        ChartEditor editor = ChartEditor.Instance;
        float time = editor.currentVisibleTime;
        Song song = editor.currentSong;

        if (feederNoteIndex <= 0)
        {
            uint currentTick = song.TimeToTick(time, song.resolution);
            feederNoteIndex = SongObjectHelper.FindClosestPositionRoundedDown(currentTick, editor.currentChart.notes);
        }

        if (hitWindow != null && feederNoteIndex >= 0 && feederNoteIndex < editor.currentChart.notes.Count)
        {
            uint maxScanTick = editor.maxPos;
            int noteIndex = feederNoteIndex;

            while (noteIndex < editor.currentChart.notes.Count)
            {
                Note note = editor.currentChart.notes[noteIndex];
                if (note.tick > maxScanTick)
                {
                    break;
                }

                ++noteIndex;

                bool validControllerAttached = note.controller != null && note.controller.isActiveAndEnabled && !note.controller.hit;
                if (validControllerAttached && note.tick != lastAddedTick)
                {
                    if (!hitWindow.DetectEnter(note, time))
                    {
                        break;
                    }
                    else
                    {
                        lastAddedTick = note.tick;
                    }

                    feederNoteIndex = noteIndex;    // Only increase the feeder note index if we pass notes that are completely valid. The notes ahead may have simply not been loaded into view yet. Need to reproccess them once they do come into view
                }
            }
        }
    }

    public void Reset()
    {
        if (hitWindow != null)
        {
            hitWindow.Clear();
        }
        feederNoteIndex = -1;
        lastAddedTick = uint.MaxValue;
    }
}
