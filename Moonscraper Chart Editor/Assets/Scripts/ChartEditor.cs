#define TIMING_DEBUG

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ChartEditor : MonoBehaviour {
    const int POOL_SIZE = 100;

    [Header("Prefabs")]
    public GameObject note;
    public GameObject section;
    public GameObject timeSignatureLine;
    [Header("Indicator Parents")]
    public GameObject guiIndicators;
    [Header("Song properties Display")]
    public Text songNameText;
    [Header("Misc.")]
    public Button play;
    public Transform strikeline;
    public TimelineHandler timeHandler;
    public Transform camYMin;
    public Transform camYMax;

    public uint minPos { get; private set; }
    public uint maxPos { get; private set; }
    AudioSource musicSource;

    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    string currentFileName = string.Empty;

    MovementController movement;
    GameObject[] timeSignatureLinePool = new GameObject[POOL_SIZE];
    GameObject timeSignatureLineParent;

    string lastLoadedFile = string.Empty;

    GameObject songObjectParent;
    GameObject chartObjectParent;

    // Use this for initialization
    void Awake () {
        minPos = 0;
        maxPos = 0;

        songObjectParent = new GameObject();
        songObjectParent.name = "Song Objects";
        songObjectParent.tag = "Song Object";

        chartObjectParent = new GameObject();
        chartObjectParent.name = "Chart Objects";
        chartObjectParent.tag = "Chart Object";

        currentSong = new Song();
        currentChart = currentSong.expert_single;
        musicSource = GetComponent<AudioSource>();

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();
        timeSignatureLineParent = new GameObject("Time Signature Lines");
        for (int i = 0; i < POOL_SIZE; ++i)
        {
            timeSignatureLinePool[i] = Instantiate(timeSignatureLine);
            timeSignatureLinePool[i].transform.SetParent(timeSignatureLineParent.transform);
            timeSignatureLinePool[i].SetActive(false);
        }
    }

    void Update()
    {
        // Update object positions that supposed to be visible into the range of the camera
        minPos = currentSong.WorldYPositionToChartPosition(camYMin.position.y);
        maxPos = currentSong.WorldYPositionToChartPosition(camYMax.position.y);

        // Update time signature lines SNAPPED
        uint snappedLinePos = Snapable.ChartPositionToSnappedChartPosition(minPos, 4);
        int i = 0;
        while (snappedLinePos < maxPos && i < timeSignatureLinePool.Length)
        {
            timeSignatureLinePool[i].SetActive(true);
            timeSignatureLinePool[i].transform.position = new Vector3(0, currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
            snappedLinePos += Globals.FULL_STEP / 4;
            ++i;
        }

        // Disable any unused lines
        while (i < timeSignatureLinePool.Length)
        {
            timeSignatureLinePool[i++].SetActive(false);
        }

        //Debug.Log(currentSong.ChartPositionToTime(maxPos) + ", " + currentSong.length);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && Globals.applicationMode == Globals.ApplicationMode.Playing)
            Play();
        else
            musicSource.Stop();
    }

    void OnApplicationQuit()
    {
        // Check for unsaved changes
    }

    // Wrapper function
    public void LoadSong()
    {
        Stop();
        StartCoroutine(_LoadSong());
    }

    public void Save()
    {
        if (lastLoadedFile != string.Empty)
            Save(lastLoadedFile);
        else
            SaveAs();
    }

    public void SaveAs()
    {
        try {
            string fileName = UnityEditor.EditorUtility.SaveFilePanel("Save as...", "", currentSong.name, "chart");

            Save(fileName);
            lastLoadedFile = fileName;
        }
        catch (System.Exception e)
        {
            // User probably just canceled
            Debug.LogError(e.Message);
        }
    }

    void Save (string filename)
    {
        if (currentSong != null)
            currentSong.Save(filename);
    }

    public void Play()
    {
        float strikelinePos = strikeline.position.y;
        musicSource.time = Song.WorldYPositionToTime(strikelinePos) + currentSong.offset;       // No need to add audio calibration as position is base on the strikeline position
        play.interactable = false;
        Globals.applicationMode = Globals.ApplicationMode.Playing;
        musicSource.Play();
    }

    public void Stop()
    {
        play.interactable = true;
        Globals.applicationMode = Globals.ApplicationMode.Editor;
        musicSource.Stop();
    }

    IEnumerator _LoadSong()
    {
#if TIMING_DEBUG
        float totalLoadTime = 0;
#endif
        try
        {
            currentFileName = UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart");
#if TIMING_DEBUG
            totalLoadTime = Time.realtimeSinceStartup;
#endif
            currentSong = new Song(currentFileName);
#if TIMING_DEBUG
            Debug.Log("Song load time: " + (Time.realtimeSinceStartup - totalLoadTime));

            float objectDestroyTime = Time.realtimeSinceStartup;
#endif
            // Remove objects from previous chart
            foreach (Transform chartObject in chartObjectParent.transform)
            {
                Destroy(chartObject.gameObject);
            }
            foreach (Transform songObject in songObjectParent.transform)
            {
                Destroy(songObject.gameObject);
            }
            foreach (Transform child in guiIndicators.transform)
            {
                Destroy(child.gameObject);
            }
#if TIMING_DEBUG
            Debug.Log("Chart objects destroy time: " + (Time.realtimeSinceStartup - objectDestroyTime));
#endif
            currentChart = currentSong.expert_single;
#if TIMING_DEBUG
            float objectLoadTime = Time.realtimeSinceStartup;
#endif
            // Create the actual objects
            CreateSongObjects(currentSong);
            CreateChartObjects(currentChart);

#if TIMING_DEBUG
            Debug.Log("Chart objects load time: " + (Time.realtimeSinceStartup - objectLoadTime));
#endif
            songNameText.text = currentSong.name;

            lastLoadedFile = currentFileName;
        }
        catch (System.Exception e)
        {
            // Most likely closed the window explorer, just ignore for now.
            currentFileName = string.Empty;
            currentSong = new Song();
            Debug.LogError(e.Message);

            yield break;
        }

        while (currentSong.musicStream != null && currentSong.musicStream.loadState != AudioDataLoadState.Loaded)
        {
            Debug.Log("Loading audio...");
            yield return null;
        }

        if (currentSong.musicStream != null)
        {
            musicSource.clip = currentSong.musicStream;
            movement.SetPosition(0);
        }
#if TIMING_DEBUG
        Debug.Log("Total load time: " + (Time.realtimeSinceStartup - totalLoadTime));
#endif
    }

    // Create Sections, bpms, events and time signature objects
    GameObject CreateSongObjects(Song song)
    {
        for (int i = 0; i < song.sections.Length; ++i)
        {
            // Convert the chart data into gameobject
            GameObject sectionObject = Instantiate(this.section);

            sectionObject.transform.SetParent(songObjectParent.transform);
            
            // Attach the note to the object
            SectionController controller = sectionObject.GetComponentInChildren<SectionController>();

            // Link controller and note together
            controller.Init(song.sections[i], timeHandler, guiIndicators);
            
            controller.UpdateSongObject();
            
        }
        
        return songObjectParent;
    }

    // Create note, starpower and chart event objects
    GameObject CreateChartObjects(Chart chart)
    {    
        // Get reference to the current set of notes in case real notes get deleted
        Note[] notes = chart.notes;
        for (int i = 0; i < notes.Length; ++i)
        {
            // Make sure notes haven't been deleted
            if (notes[i].song != null)
            {
                NoteController controller = CreateNoteObject(notes[i], chartObjectParent);
                controller.UpdateSongObject();
            }
        }

        return chartObjectParent;
    }

    public NoteController CreateNoteObject(Note note, GameObject parent = null)
    {
        // Convert the chart data into gameobject
        GameObject noteObject = Instantiate(this.note);

        if (parent)
            noteObject.transform.SetParent(parent.transform);
        else
            noteObject.transform.SetParent(chartObjectParent.transform);

        // Attach the note to the object
        NoteController controller = noteObject.GetComponent<NoteController>();

        // Link controller and note together
        controller.Init(note);

        return controller;
    }
}
