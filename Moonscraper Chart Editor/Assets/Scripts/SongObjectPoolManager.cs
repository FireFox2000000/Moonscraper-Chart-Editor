using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChartEditor))]
public class SongObjectPoolManager : MonoBehaviour {

    const int NOTE_POOL_SIZE = 200;
    const int POOL_SIZE = 50;

    ChartEditor editor;

    public NoteController[] noteControllers { get; private set; }
    public StarpowerController[] starpowerControllers { get; private set; }
    public BPMController[] bpmControllers { get; private set; }
    public TimesignatureController[] tsControllers { get; private set; }
    public SectionController[] sectionControllers { get; private set; }

    // Use this for initialization
    void Awake () {
        editor = GetComponent<ChartEditor>();

        GameObject groupMovePool = new GameObject("Main Song Object Pool");

        GameObject notes;
        noteControllers = SOConInstanciate<NoteController>(editor.notePrefab, NOTE_POOL_SIZE, out notes);
        notes.name = "Notes";
        notes.transform.SetParent(groupMovePool.transform);

        GameObject starpowers;
        starpowerControllers = SOConInstanciate<StarpowerController>(editor.starpowerPrefab, POOL_SIZE, out starpowers);
        starpowers.name = "Starpower";
        starpowers.transform.SetParent(groupMovePool.transform);

        GameObject bpms;
        bpmControllers = SOConInstanciate<BPMController>(editor.bpmPrefab, POOL_SIZE, out bpms);
        bpms.name = "BPMs";
        bpms.transform.SetParent(groupMovePool.transform);

        GameObject timesignatures;
        tsControllers = SOConInstanciate<TimesignatureController>(editor.tsPrefab, POOL_SIZE, out timesignatures);
        timesignatures.name = "Time Signatures";
        timesignatures.transform.SetParent(groupMovePool.transform);

        GameObject sections;
        sectionControllers = SOConInstanciate<SectionController>(editor.sectionPrefab, POOL_SIZE, out sections);
        sections.name = "Sections";
        sections.transform.SetParent(groupMovePool.transform);

    }
	
	// Update is called once per frame
	void Update () {
        // If a new chart is loaded, all objects need to be disabled/reset

        EnableNotes(editor.currentChart.notes);
        EnableSP(editor.currentChart.starPower);
        EnableBPM(editor.currentSong.bpms);
        EnableTS(editor.currentSong.timeSignatures);
        EnableSections(editor.currentSong.sections);
    }

    public void NewChartReset()
    {
        if (enabled && noteControllers != null)
        {
            disableReset(noteControllers);
            disableReset(starpowerControllers);
            disableReset(bpmControllers);
            disableReset(tsControllers);
            disableReset(sectionControllers);
        }
    }

    void disableReset(SongObjectController[] controllers)
    {
        foreach (SongObjectController controller in controllers)
            controller.gameObject.SetActive(false);
    }

    public void EnableNotes(Note[] notes)
    {
        List<Note> rangedNotes = new List<Note>(SongObject.GetRange(notes, editor.minPos, editor.maxPos));

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

        int pos = 0;
        foreach (Note note in rangedNotes)
        {
            if (ChartEditor.startGameplayPos != null && note.worldYPosition < (float)ChartEditor.startGameplayPos)
                continue;

            if (note.controller == null)
            {
                while (pos < noteControllers.Length && noteControllers[pos].gameObject.activeSelf)
                    ++pos;

                if (pos < noteControllers.Length && !noteControllers[pos].gameObject.activeSelf)
                {
                    // Assign pooled objects
                    noteControllers[pos].note = note;
                    noteControllers[pos].Activate();
                    noteControllers[pos].gameObject.SetActive(true);
                }
                else
                    break;
            }
        }
    }

    public void EnableNotes(Note note)
    {
        EnableNotes(new Note[] { note });
    }

    public void EnableSP(Starpower[] starpowers)
    {
        List<Starpower> rangedSP = new List<Starpower>(SongObject.GetRange(starpowers, editor.minPos, editor.maxPos));

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
                rangedSP.Add(editor.currentChart.starPower[arrayPos]);
            }
        }

        int pos = 0;
        foreach (Starpower sp in rangedSP)
        {
            if (sp.controller == null)
            {
                while (pos < starpowerControllers.Length && starpowerControllers[pos].gameObject.activeSelf)
                    ++pos;

                if (pos < starpowerControllers.Length && !starpowerControllers[pos].gameObject.activeSelf)
                {
                    // Assign pooled objects
                    starpowerControllers[pos].starpower = sp;
                    starpowerControllers[pos].gameObject.SetActive(true);
                }
                else
                    break;
            }
        }
    }

    public void EnableSP(Starpower starpower)
    {
        EnableSP(new Starpower[] { starpower });
    }

    public void EnableBPM(BPM[] bpms)
    {
        int pos = 0;
        foreach (BPM bpm in SongObject.GetRange(bpms, editor.minPos, editor.maxPos))
        {
            if (bpm.controller == null)
            {
                while (pos < bpmControllers.Length && bpmControllers[pos].gameObject.activeSelf)
                    ++pos;

                if (pos < bpmControllers.Length && !bpmControllers[pos].gameObject.activeSelf)
                {
                    // Assign pooled objects
                    bpmControllers[pos].bpm = bpm;
                    bpmControllers[pos].gameObject.SetActive(true);
                }
                else
                    break;
            }
        }
    }

    public void EnableBPM(BPM bpm)
    {
        EnableBPM(new BPM[] { bpm });
    }

    public void EnableTS(TimeSignature[] timeSignatures)
    {
        int pos = 0;
        foreach (TimeSignature ts in SongObject.GetRange(timeSignatures, editor.minPos, editor.maxPos))
        {
            if (ts.controller == null)
            {
                while (pos < tsControllers.Length && tsControllers[pos].gameObject.activeSelf)
                    ++pos;

                if (pos < tsControllers.Length && !tsControllers[pos].gameObject.activeSelf)
                {
                    // Assign pooled objects
                    tsControllers[pos].ts = ts;
                    tsControllers[pos].gameObject.SetActive(true);
                }
                else
                    break;
            }
        }
    }

    public void EnableTS(TimeSignature timeSignature)
    {
        EnableTS(new TimeSignature[] { timeSignature });
    }

    public void EnableSections(Section[] sections)
    {
        int pos = 0;

        foreach (Section section in SongObject.GetRange(sections, editor.minPos, editor.maxPos))
        {
            if (section.controller == null)
            {
                while (pos < sectionControllers.Length && sectionControllers[pos].gameObject.activeSelf)
                    ++pos;

                if (pos < sectionControllers.Length && !sectionControllers[pos].gameObject.activeSelf)
                {
                    // Assign pooled objects
                    sectionControllers[pos].section = section;
                    sectionControllers[pos].gameObject.SetActive(true);
                }
                else
                    break;
            }
        }
    }

    public void EnableSections(Section section)
    {
        EnableSections(new Section[] { section });
    }

    public static T[] SOConInstanciate<T>(GameObject prefab, int count, out GameObject parent) where T : SongObjectController
    {
        if (!prefab.GetComponentInChildren<SongObjectController>())
        {
            throw new System.Exception("No SongObjectController attached to prefab");
        }

        T[] soCons = new T[count];
        parent = new GameObject();

        for (int i = 0; i < soCons.Length; ++i)
        {
            GameObject gameObject = Instantiate(prefab);
            gameObject.transform.SetParent(parent.transform);
            soCons[i] = gameObject.GetComponentInChildren<T>();
        }

        return soCons;
    }
}
