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

    GameObject noteParent;
    GameObject starpowerParent;
    GameObject bpmParent;
    GameObject timesignatureParent;
    GameObject sectionParent;

    // Use this for initialization
    void Awake () {
        editor = GetComponent<ChartEditor>();

        GameObject groupMovePool = new GameObject("Main Song Object Pool");

        noteParent = new GameObject("Notes");
        starpowerParent = new GameObject("Starpowers");
        bpmParent = new GameObject("BPMs");
        timesignatureParent = new GameObject("Time Signatures");
        sectionParent = new GameObject("Sections");

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

    }
	
	// Update is called once per frame
	void Update () {
        EnableNotes(editor.currentChart.notes);
        EnableSP(editor.currentChart.starPower);
        EnableBPM(editor.currentSong.bpms);
        EnableTS(editor.currentSong.timeSignatures);
        EnableSections(editor.currentSong.sections);
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

        int index, length;
        SongObject.GetRange(notes, min_pos, editor.maxPos, out index, out length);

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
                    rangedNotes.Add(editor.currentChart.notes[minArrayPos]);

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
}
