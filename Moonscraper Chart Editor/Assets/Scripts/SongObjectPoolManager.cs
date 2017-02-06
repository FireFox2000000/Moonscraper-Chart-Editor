using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongObjectPoolManager : MonoBehaviour {

    const int POOL_SIZE = 100;

    ChartEditor editor;

    NoteController[] noteControllers;
    StarpowerController[] starpowerControllers;
    BPMController[] bpmControllers;
    TimesignatureController[] tsControllers;
    SectionController[] sectionControllers;

    // Use this for initialization
    void Start () {
        editor = GetComponent<ChartEditor>();

        GameObject groupMovePool = new GameObject("Main Song Object Pool");

        GameObject notes;
        noteControllers = SOConInstanciate<NoteController>(editor.notePrefab, POOL_SIZE, out notes);
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
        EnableNotes();

        // If a new chart is loaded, all objects need to be disabled
    }

    void EnableNotes()
    {
        List<Note> rangedNotes = new List<Note>(SongObject.GetRange(editor.currentChart.notes, editor.minPos, editor.maxPos));

        if (rangedNotes.Count > 0)
        {
            // Find the last known note of each fret type to find any sustains that might overlap into the camera view
            foreach (Note prevNote in Note.GetPreviousOfSustains(rangedNotes[0] as Note))
            {
                rangedNotes.Add(prevNote);
            }
        }

        int pos = 0;

        foreach (Note note in rangedNotes)
        {
            if (note.controller == null)
            {
                while (noteControllers[pos].gameObject.activeSelf && pos < noteControllers.Length)
                    ++pos;

                if (pos < noteControllers.Length)
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
        /*
            // Assign pooled objects
            foreach (NoteController nCon in noteControllers)
        {
            if (pos < rangedNotes.Count)
            {
                if (!nCon.gameObject.activeSelf && rangedNotes[pos].controller == null)
                {                 
                    nCon.note = rangedNotes[pos++];
                    nCon.gameObject.SetActive(true);                 
                }
            }
            else
                break;
        }    */  
    }

    T[] GetRangeOfSongObjectsToEnable<T>(T[] songObjects, uint min, uint max) where T : SongObject
    {
        List<T> songObjectsRanged = new List<T>(SongObject.GetRange(songObjects, min, max));

        // Check if sustains need to be rendered
        if (typeof(T) == typeof(Note) && songObjectsRanged.Count > 0)
        {
            // Find the last known note of each fret type to find any sustains that might overlap. Cancel if there's an open note.
            foreach (Note prevNote in Note.GetPreviousOfSustains(songObjectsRanged[0] as Note))
            {
                songObjectsRanged.Add(prevNote as T);
            }
        }
        else if (typeof(T) == typeof(Starpower))
        {
            int arrayPos = SongObject.FindClosestPosition(min, songObjects);
            if (arrayPos != Globals.NOTFOUND)
            {
                // Find the back-most position
                while (arrayPos > 0 && songObjects[arrayPos].position >= min)
                {
                    --arrayPos;
                }
                // Render previous sp sustain in case of overlap into current position
                if (arrayPos >= 0)
                {
                    songObjectsRanged.Add(songObjects[arrayPos]);
                }
            }
        }

        return songObjectsRanged.ToArray();
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
