// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChartEditor))]
public class SongObjectPoolManager : MonoBehaviour {

    const int NOTE_POOL_SIZE = 200;
    const int POOL_SIZE = 50;

    ChartEditor editor;

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

    // Use this for initialization
    void Awake () {
        editor = GetComponent<ChartEditor>();

        GameObject groupMovePool = new GameObject("Main Song Object Pool");

        noteParent = new GameObject("Notes");
        starpowerParent = new GameObject("Starpowers");
        bpmParent = new GameObject("BPMs");
        timesignatureParent = new GameObject("Time Signatures");
        sectionParent = new GameObject("Sections");
        songEventParent = new GameObject("Global Events");
        chartEventParent = new GameObject("Chart Events");

        notePool = new NotePool(noteParent, editor.notePrefab, NOTE_POOL_SIZE);
        noteParent.transform.SetParent(groupMovePool.transform);

        spPool = new StarpowerPool(starpowerParent, editor.starpowerPrefab, POOL_SIZE);
        starpowerParent.transform.SetParent(groupMovePool.transform);

        bpmPool = new BPMPool(bpmParent, editor.bpmPrefab, POOL_SIZE);
        bpmParent.transform.SetParent(groupMovePool.transform);

        tsPool = new TimesignaturePool(timesignatureParent, editor.tsPrefab, POOL_SIZE);
        timesignatureParent.transform.SetParent(groupMovePool.transform);

        sectionPool = new SectionPool(sectionParent, editor.sectionPrefab, POOL_SIZE);
        sectionParent.transform.SetParent(groupMovePool.transform);

        songEventPool = new EventPool(songEventParent, editor.songEventPrefab, POOL_SIZE);
        songEventParent.transform.SetParent(groupMovePool.transform);

        chartEventPool = new ChartEventPool(chartEventParent, editor.chartEventPrefab, POOL_SIZE);
        chartEventParent.transform.SetParent(groupMovePool.transform);
    }
	
	// Update is called once per frame
	void Update () {
        if (editor.currentChart.notes.Length > 0)
            EnableNotes(editor.currentChart.notes);

        if (Globals.viewMode == Globals.ViewMode.Chart)
        {
            if (editor.currentChart.starPower.Length > 0)
                EnableSP(editor.currentChart.starPower);

            EnableChartEvents(editor.currentChart.events);
        }
        else
        {
            EnableBPM(editor.currentSong.bpms);
            EnableTS(editor.currentSong.timeSignatures);

            if (editor.currentSong.sections.Length > 0)
                EnableSections(editor.currentSong.sections);

            EnableSongEvents(editor.currentSong.events);
        }
    }

    public void NewChartReset()
    {
        if (enabled && notePool != null)
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
  
    public void EnableNotes(Note[] notes)
    {
        uint min_pos = editor.minPos;
        if (ChartEditor.startGameplayPos != null)
        {
            uint gameplayPos = editor.currentSong.WorldYPositionToChartPosition((float)ChartEditor.startGameplayPos, editor.currentSong.resolution);
            if (min_pos < gameplayPos)
                min_pos = gameplayPos;
        }

        List<Note> rangedNotes = new List<Note>(SongObject.GetRangeCopy(notes, min_pos, editor.maxPos));
        
        if (min_pos == editor.minPos)
        {
            if (rangedNotes.Count > 0)
            {
                // Find the last known note of each fret type to find any sustains that might overlap into the camera view
                foreach (Note prevNote in Note.GetPreviousOfSustains(rangedNotes[0] as Note))
                {
                    if (prevNote.position + prevNote.sustain_length > editor.minPos)
                        rangedNotes.Add(prevNote);
                }
            }
            else
            {
                int minArrayPos = SongObject.FindClosestPosition(editor.minPos, editor.currentChart.notes);

                if (minArrayPos != SongObject.NOTFOUND)
                {
                    while (minArrayPos > 0 && editor.currentChart.notes[minArrayPos].position == editor.currentChart.notes[minArrayPos - 1].position)
                        --minArrayPos;

                    //rangedNotes.AddRange(editor.currentChart.notes[minArrayPos].GetChord());

                    foreach (Note note in editor.currentChart.notes[minArrayPos].GetChord())
                    {
                        if (note.position + note.sustain_length > editor.minPos)
                            rangedNotes.Add(note);
                    }

                    foreach (Note prevNote in Note.GetPreviousOfSustains(editor.currentChart.notes[minArrayPos] as Note))
                    {
                        if (prevNote.position + prevNote.sustain_length > editor.minPos)
                            rangedNotes.Add(prevNote);
                    }
                }
            }
        }

        Note[] notesToActivate = rangedNotes.ToArray();
        notePool.Activate(notesToActivate, 0, notesToActivate.Length);
    }

    public void EnableSP(Starpower[] starpowers)
    {
        List<Starpower> range = new List<Starpower>(SongObject.GetRangeCopy(starpowers, editor.minPos, editor.maxPos));

        int arrayPos = SongObject.FindClosestPosition(editor.minPos, editor.currentChart.starPower);
        if (arrayPos != SongObject.NOTFOUND)
        {
            // Find the back-most position
            while (arrayPos > 0 && editor.currentChart.starPower[arrayPos].position >= editor.minPos)
            {
                --arrayPos;
            }
            // Render previous sp sustain in case of overlap into current position
            if (arrayPos >= 0 && editor.currentChart.starPower[arrayPos].position + editor.currentChart.starPower[arrayPos].length > editor.minPos)
            {
                range.Add(editor.currentChart.starPower[arrayPos]);
            }
        }

        Starpower[] spToActivate = range.ToArray();
        spPool.Activate(spToActivate, 0, spToActivate.Length);
    }

    public void EnableBPM(BPM[] bpms)
    {
        int index, length;
        SongObject.GetRange(bpms, editor.minPos, editor.maxPos, out index, out length);

        bpmPool.Activate(bpms, index, length);
    }

    public void EnableTS(TimeSignature[] timeSignatures)
    {
        int index, length;
        SongObject.GetRange(timeSignatures, editor.minPos, editor.maxPos, out index, out length);

        tsPool.Activate(timeSignatures, index, length);
    }

    public void EnableSections(Section[] sections)
    {
        int index, length;
        SongObject.GetRange(sections, editor.minPos, editor.maxPos, out index, out length);
        sectionPool.Activate(sections, index, length);
    }

    public void EnableSongEvents(Event[] events)
    {
        int index, length;
        SongObject.GetRange(events, editor.minPos, editor.maxPos, out index, out length);
        songEventPool.Activate(events, index, length);
    }

    public void EnableChartEvents(ChartEvent[] events)
    {
        int index, length;
        SongObject.GetRange(events, editor.minPos, editor.maxPos, out index, out length);
        chartEventPool.Activate(events, index, length);
    }
}
