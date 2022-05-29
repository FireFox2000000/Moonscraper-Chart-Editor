// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

/// <summary>
/// Creates a pool of objects that notes and other events can be assigned to for rendering the event.
/// Disabled objects are considered a part of the pool and are free to assign. Active objects should be avoided as they are currently in use.
/// Objects automatically disable themselves when they fall out of the view-range of the editor.
/// Every frame, this manager scans the chart for all nessacary events that should be a part of the view range. 
/// If it finds events in the view range and they aren't assigned an object (known as a controller), it assigns an appropirate one from the pool.
/// </summary>
public class SongObjectPoolManager : SystemManagerState.MonoBehaviourSystem
{
    ChartEditor editor;

    const int NOTE_POOL_SIZE = 200;
    const int POOL_SIZE = 50;

    NotePool notePool;
    StarpowerPool spPool;
    BPMPool bpmPool;
    TimesignaturePool tsPool;
    SectionPool sectionPool;
    EventPool songEventPool;
    ChartEventPool chartEventPool;

    GameObject noteParent;
    GameObject starpowerParent;
    GameObject bpmParent;
    GameObject timesignatureParent;
    GameObject sectionParent;
    GameObject songEventParent;
    GameObject chartEventParent;

    List<Note> collectedNotesInRange = new List<Note>();
    List<Starpower> collectedStarpowerInRange = new List<Starpower>();
    List<Note> prevSustainCache = new List<Note>();

    public float? noteVisibilityRangeYPosOverride;

    // Use this for initialization
    void Awake()
    {
        editor = ChartEditor.Instance;

        GameObject groupMovePool = new GameObject("Main Song Object Pool");

        noteParent = new GameObject("Notes");
        starpowerParent = new GameObject("Starpowers");
        bpmParent = new GameObject("BPMs");
        timesignatureParent = new GameObject("Time Signatures");
        sectionParent = new GameObject("Sections");
        songEventParent = new GameObject("Global Events");
        chartEventParent = new GameObject("Chart Events");

        notePool = new NotePool(noteParent, editor.assets.notePrefab, NOTE_POOL_SIZE);
        noteParent.transform.SetParent(groupMovePool.transform);

        spPool = new StarpowerPool(starpowerParent, editor.assets.starpowerPrefab, POOL_SIZE);
        starpowerParent.transform.SetParent(groupMovePool.transform);

        bpmPool = new BPMPool(bpmParent, editor.assets.bpmPrefab, POOL_SIZE);
        bpmParent.transform.SetParent(groupMovePool.transform);

        tsPool = new TimesignaturePool(timesignatureParent, editor.assets.tsPrefab, POOL_SIZE);
        timesignatureParent.transform.SetParent(groupMovePool.transform);

        sectionPool = new SectionPool(sectionParent, editor.assets.sectionPrefab, POOL_SIZE);
        sectionParent.transform.SetParent(groupMovePool.transform);

        songEventPool = new EventPool(songEventParent, editor.assets.songEventPrefab, POOL_SIZE);
        songEventParent.transform.SetParent(groupMovePool.transform);

        chartEventPool = new ChartEventPool(chartEventParent, editor.assets.chartEventPrefab, POOL_SIZE);
        chartEventParent.transform.SetParent(groupMovePool.transform);

        editor.events.hyperspeedChangeEvent.Register(SetAllPoolsDirty);
        editor.events.chartReloadedEvent.Register(SetAllPoolsDirty);
        editor.events.leftyFlipToggledEvent.Register(SetAllPoolsDirty);
        editor.events.drumsModeOptionChangedEvent.Register(SetAllNotesDirty);
        editor.events.playbackStoppedEvent.Register(OnPlaybackStopped);
    }

    // Update is called once per frame
    void LateUpdate ()
    {
        if (editor.currentChart.notes.Count > 0)
            EnableNotes(editor.currentChart.notes);

        if (Globals.viewMode == Globals.ViewMode.Chart)
        {
            if (editor.currentChart.starPower.Count > 0)
                EnableSP(editor.currentChart.starPower);

            EnableChartEvents(editor.currentChart.events);
        }
        else
        {
            EnableBPM(editor.currentSong.bpms);
            EnableTS(editor.currentSong.timeSignatures);

            if (editor.currentSong.sections.Count > 0)
                EnableSections(editor.currentSong.sections);

            EnableSongEvents(editor.currentSong.events);
        }
    }

    public void NewChartReset()
    {
        if (notePool != null)
        {
            notePool.Reset();
            spPool.Reset();
            bpmPool.Reset();
            tsPool.Reset();
            sectionPool.Reset();
            songEventPool.Reset();
            chartEventPool.Reset();
        }
    }

    void disableReset(SongObjectController[] controllers)
    {
        foreach (SongObjectController controller in controllers)
            controller.gameObject.SetActive(false);
    }

    void CollectNotesInViewRange(IList<Note> notes)
    {
        bool extendedSustainsEnabled = Globals.gameSettings.extendedSustainsEnabled;

        uint min_pos = editor.minPos;
        if (noteVisibilityRangeYPosOverride.HasValue)
        {
            uint gameplayPos = editor.currentSong.WorldYPositionToTick(noteVisibilityRangeYPosOverride.Value, editor.currentSong.resolution);
            if (min_pos < gameplayPos)
                min_pos = gameplayPos;
        }

        collectedNotesInRange.Clear();
        int index, length;
        SongObjectHelper.GetRange(notes, min_pos, editor.maxPos, out index, out length);
        for (int i = index; i < index + length; ++i)
        {
            collectedNotesInRange.Add(notes[i]);
        }

        if (min_pos == editor.minPos)
        {
            if (collectedNotesInRange.Count > 0)
            {
                NoteFunctions.GetPreviousOfSustains(prevSustainCache, collectedNotesInRange[0], extendedSustainsEnabled);
                // Find the last known note of each fret type to find any sustains that might overlap into the camera view
                foreach (Note prevNote in prevSustainCache)
                {
                    if (prevNote.tick + prevNote.length > editor.minPos)
                        collectedNotesInRange.Add(prevNote);
                }
            }
            else
            {
                int minArrayPos = SongObjectHelper.FindClosestPosition(editor.minPos, editor.currentChart.notes);

                if (minArrayPos != SongObjectHelper.NOTFOUND)
                {
                    while (minArrayPos > 0 && editor.currentChart.notes[minArrayPos].tick == editor.currentChart.notes[minArrayPos - 1].tick)
                        --minArrayPos;

                    Note minNote = editor.currentChart.notes[minArrayPos];

                    if (minNote.tick + minNote.length > editor.minPos && minNote.tick < editor.maxPos)
                    {
                        foreach (Note note in minNote.chord)
                        {
                            if (note.tick + note.length > editor.minPos)
                                collectedNotesInRange.Add(note);
                        }
                    }

                    NoteFunctions.GetPreviousOfSustains(prevSustainCache, minNote, extendedSustainsEnabled);
                    foreach (Note prevNote in prevSustainCache)
                    {
                        if (prevNote.tick + prevNote.length > editor.minPos)
                            collectedNotesInRange.Add(prevNote);
                    }
                }
            }
        }

        // Make sure the notes are within the allowable lanes
        for (int i = collectedNotesInRange.Count - 1; i >= 0; --i)
        {
            Note note = collectedNotesInRange[i];
            Note prev = note.previous;

            if (note.ShouldBeCulledFromLanes(editor.laneInfo))
            {
                if (prev == null || prev.tick != note.tick || prev.rawNote < editor.laneInfo.laneCount - 1)
                {
                    // if the previous note is not on the edge of the lane, then we are allowed to show this note in the remaining lane
                    continue;
                }

                collectedNotesInRange.RemoveAt(i);
            }
        }
    }
  
    public void EnableNotes(IList<Note> notes)
    {
        CollectNotesInViewRange(notes);
        notePool.Activate(collectedNotesInRange, 0, collectedNotesInRange.Count);
    }

    void CollectStarpowerInViewRange(IList<Starpower> starpowers)
    {
        collectedStarpowerInRange.Clear();
        int index, length;
        SongObjectHelper.GetRange(starpowers, editor.minPos, editor.maxPos, out index, out length);
        for (int i = index; i < index + length; ++i)
        {
            collectedStarpowerInRange.Add(starpowers[i]);
        }

        int arrayPos = SongObjectHelper.FindClosestPosition(editor.minPos, editor.currentChart.starPower);
        if (arrayPos != SongObjectHelper.NOTFOUND)
        {
            // Find the back-most position
            while (arrayPos > 0 && editor.currentChart.starPower[arrayPos].tick >= editor.minPos)
            {
                --arrayPos;
            }
            // Render previous sp sustain in case of overlap into current position
            if (arrayPos >= 0 && editor.currentChart.starPower[arrayPos].tick + editor.currentChart.starPower[arrayPos].length > editor.minPos &&
                (editor.currentChart.starPower[arrayPos].tick + editor.currentChart.starPower[arrayPos].length) < editor.maxPos)
            {
                collectedStarpowerInRange.Add(editor.currentChart.starPower[arrayPos]);
            }
        }
    }

    public void EnableSP(IList<Starpower> starpowers)
    {
        CollectStarpowerInViewRange(starpowers);
        spPool.Activate(collectedStarpowerInRange, 0, collectedStarpowerInRange.Count);
    }

    public void EnableBPM(IList<BPM> bpms)
    {
        int index, length;
        SongObjectHelper.GetRange(bpms, editor.minPos, editor.maxPos, out index, out length);

        bpmPool.Activate(bpms, index, length);
    }

    public void EnableTS(IList<TimeSignature> timeSignatures)
    {
        int index, length;
        SongObjectHelper.GetRange(timeSignatures, editor.minPos, editor.maxPos, out index, out length);

        tsPool.Activate(timeSignatures, index, length);
    }

    public void EnableSections(IList<Section> sections)
    {
        int index, length;
        SongObjectHelper.GetRange(sections, editor.minPos, editor.maxPos, out index, out length);
        sectionPool.Activate(sections, index, length);
    }

    public void EnableSongEvents(IList<MoonscraperChartEditor.Song.Event> events)
    {
        int index, length;
        SongObjectHelper.GetRange(events, editor.minPos, editor.maxPos, out index, out length);
        songEventPool.Activate(events, index, length);
    }

    public void EnableChartEvents(IList<ChartEvent> events)
    {
        int index, length;
        SongObjectHelper.GetRange(events, editor.minPos, editor.maxPos, out index, out length);
        chartEventPool.Activate(events, index, length);
    }

    public void SetAllPoolsDirty()
    {
        Song song = editor.currentSong;
        Chart chart = editor.currentChart;

        SetInViewRangeDirty(chart.notes);
        SetInViewRangeDirty(chart.starPower);
        SetInViewRangeDirty(chart.events);
        SetInViewRangeDirty(song.eventsAndSections);
        SetInViewRangeDirty(song.syncTrack);

        TimelineHandler.Repaint();
    }

    void SetAllNotesDirty()
    {
        Song song = editor.currentSong;
        Chart chart = editor.currentChart;
        SetInViewRangeDirty(chart.notes);
    }

    public void SetInViewRangeDirty<T>(IList<T> songObjects) where T : SongObject
    {
        int index, length;
        SongObjectHelper.GetRange(songObjects, editor.minPos, editor.maxPos, out index, out length);
        
        for (int i = index; i < index + length; ++i)
        {
            if (songObjects[i].controller)
                songObjects[i].controller.SetDirty();
        }
    }

    public void SetInViewRangeDirty(IList<Note> songObjects)
    {
        CollectNotesInViewRange(songObjects);

        for (int i = 0; i < collectedNotesInRange.Count; ++i)
        {
            if (collectedNotesInRange[i].controller)
                collectedNotesInRange[i].controller.SetDirty();
        }
    }

    public void SetInViewRangeDirty(IList<Starpower> songObjects)
    {
        CollectStarpowerInViewRange(songObjects);

        for (int i = 0; i < collectedStarpowerInRange.Count; ++i)
        {
            if (collectedStarpowerInRange[i].controller)
                collectedStarpowerInRange[i].controller.SetDirty();
        }
    }

    void OnPlaybackStopped()
    {
        // Make notes that were visually turned off during gameplay back on
        if (editor.currentChart != null && editor.currentChart.notes.Count > 0)
        {
            CollectNotesInViewRange(editor.currentChart.notes);
            for (int i = 0; i < collectedNotesInRange.Count; ++i)
            {
                if (collectedNotesInRange[i].controller)
                    collectedNotesInRange[i].controller.Activate();
            }
        }
    }
}
