#define TIMING_DEBUG
//#undef UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;

[RequireComponent(typeof(AudioSource))]
public class ChartEditor : MonoBehaviour {
    public static bool editOccurred = false;
    const int POOL_SIZE = 100;

    [Header("Prefabs")]
    public GameObject note;
    public GameObject section;
    public GameObject timeSignatureLine;
    [Header("Indicator Parents")]
    public GameObject guiIndicators;
    [Header("Song properties Display")]
    public Text songNameText;
    public Slider hyperspeedSlider;
    [Header("Inspectors")]
    public NotePropertiesPanelController noteInspector;
    [Header("Misc.")]
    public UnityEngine.UI.Button play;
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

    OpenFileName saveFileDialog;

    public Note currentSelectedNote = null;

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

#if !UNITY_EDITOR
    SaveFileDialog saveDialog;
#endif

    System.IntPtr windowPtr;

    // Use this for initialization
    void Awake () {
        windowPtr = FindWindow(null, "Moonscraper Chart Editor v0.1");

        minPos = 0;
        maxPos = 0;

        noteInspector.gameObject.SetActive(false);

#if !UNITY_EDITOR
        saveDialog = new SaveFileDialog();
        saveDialog.InitialDirectory = "";
        saveDialog.RestoreDirectory = true;
#endif

        // Create grouping objects to make reading the inspector easier
        songObjectParent = new GameObject();
        songObjectParent.name = "Song Objects";
        songObjectParent.tag = "Song Object";

        chartObjectParent = new GameObject();
        chartObjectParent.name = "Chart Objects";
        chartObjectParent.tag = "Chart Object";

        // Create a default song
        currentSong = new Song();
        editOccurred = false;

        currentChart = currentSong.expert_single;
        musicSource = GetComponent<AudioSource>();

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();

        // Initialize object pool
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
        if (currentSelectedNote != null)
        {
            noteInspector.currentNote = currentSelectedNote;
            noteInspector.gameObject.SetActive(true);  
        }
        else
            noteInspector.gameObject.SetActive(false);

        Shortcuts();

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

        Globals.hyperspeed = hyperspeedSlider.value;
    }

    void Shortcuts()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand))
        {
            if (Input.GetKeyDown("s"))
                Save();
            else if (Input.GetKeyDown("o"))
                LoadSong();
        }
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
        editCheck();

        while (currentSong.IsSaving);

        //UnityEngine.Application.Quit();
        Debug.Log("Quit");
    }
    bool checking = false;
    void editCheck()
    {
        /*
        if (editOccurred)
        {
            //#if !UNITY_EDITOR
            // Check for unsaved changes
            UnityEngine.Application.CancelQuit();
            DialogResult result = MessageBox.Show("Want to save unsaved changes?", "Warning", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {                    
                Save();
                return true;
            }
//#endif
        }
    
        return false;
        */
    }

    public void New()
    {

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
            string fileName;
#if UNITY_EDITOR
            fileName = UnityEditor.EditorUtility.SaveFilePanel("Save as...", "", currentSong.name, "chart");
#else
            saveDialog.Filter = "chart files (*.chart)|*.chart";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
            }
            else
                throw new System.Exception("File was not saved");
#endif
            Save(fileName);           
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
        {
            editOccurred = false;            
            currentSong.Save(filename);
            lastLoadedFile = filename;
        }
    }

    public void Play()
    {
        hyperspeedSlider.interactable = false;
        float strikelinePos = strikeline.position.y;
        musicSource.time = Song.WorldYPositionToTime(strikelinePos) + currentSong.offset;       // No need to add audio calibration as position is base on the strikeline position
        play.interactable = false;
        Globals.applicationMode = Globals.ApplicationMode.Playing;
        musicSource.Play();
    }

    public void Stop()
    {
        hyperspeedSlider.interactable = true;
        play.interactable = true;
        Globals.applicationMode = Globals.ApplicationMode.Editor;
        musicSource.Stop();
    }

    IEnumerator _LoadSong()
    {
        Song backup = currentSong;
#if TIMING_DEBUG
        float totalLoadTime = 0;
#endif
        try
        {          
#if UNITY_EDITOR
            currentFileName = UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart");
#else
            OpenFileName openChartFileDialog;

            openChartFileDialog = new OpenFileName();

            openChartFileDialog.structSize = Marshal.SizeOf(openChartFileDialog);

            openChartFileDialog.file = new String(new char[256]);
            openChartFileDialog.maxFile = openChartFileDialog.file.Length;

            openChartFileDialog.fileTitle = new String(new char[64]);
            openChartFileDialog.maxFileTitle = openChartFileDialog.fileTitle.Length;

            openChartFileDialog.initialDir = "";
            openChartFileDialog.title = "Open file";
            openChartFileDialog.defExt = "txt";

            openChartFileDialog.filter = "Chart files (*.chart)\0*.chart";

            if (LibWrap.GetOpenFileName(openChartFileDialog))
            {
                currentFileName = openChartFileDialog.file;
            }
            else
            {
                throw new System.Exception("Could not open file");
            }
#endif
#if TIMING_DEBUG
            totalLoadTime = Time.realtimeSinceStartup;
#endif
            editCheck();

            // Wait for saving to complete just in case
            while (currentSong.IsSaving) ;    

            currentSong = new Song(currentFileName);
            editOccurred = false;

#if TIMING_DEBUG
            Debug.Log("File load time: " + (Time.realtimeSinceStartup - totalLoadTime));
#endif
            foreach (Transform songObject in songObjectParent.transform)
            {
                Destroy(songObject.gameObject);
            }
            foreach (Transform child in guiIndicators.transform)
            {
                Destroy(child.gameObject);
            }

#if TIMING_DEBUG
            float objectLoadTime = Time.realtimeSinceStartup;
#endif
            // Create the song objects
            CreateSongObjects(currentSong);

#if TIMING_DEBUG
            Debug.Log("Song objects load time: " + (Time.realtimeSinceStartup - objectLoadTime));
#endif
            // Load the default chart
            LoadChart(currentSong.expert_single);

            songNameText.text = currentSong.name;
            lastLoadedFile = currentFileName;
        }
        catch (System.Exception e)
        {
            // Most likely closed the window explorer, just ignore for now.
            currentSong = backup;
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

    // Chart should be part of the current song
    void LoadChart(Chart chart)
    {
        Stop();
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        // Remove objects from previous chart
        foreach (Transform chartObject in chartObjectParent.transform)
        {
            Destroy(chartObject.gameObject);
        }

        currentChart = chart;

        CreateChartObjects(currentChart);
#if TIMING_DEBUG
        Debug.Log("Chart objects load time: " + (Time.realtimeSinceStartup - time));
#endif
    }

    public void LoadAudio()
    {
        try
        {
            Stop();
            string audioFilepath = string.Empty;
            string file = lastLoadedFile;

#if UNITY_EDITOR
            audioFilepath = UnityEditor.EditorUtility.OpenFilePanel("Select Audio", "", "*.mp3;*.ogg;*.wav");
#else
            OpenFileName openAudioDialog = new OpenFileName();
            openAudioDialog = new OpenFileName();

            openAudioDialog.structSize = Marshal.SizeOf(openAudioDialog);

            openAudioDialog.file = new String(new char[256]);
            openAudioDialog.maxFile = openAudioDialog.file.Length;

            openAudioDialog.fileTitle = new String(new char[64]);
            openAudioDialog.maxFileTitle = openAudioDialog.fileTitle.Length;

            openAudioDialog.initialDir = "";
            openAudioDialog.title = "Open file";
            openAudioDialog.defExt = "txt";

            openAudioDialog.filter = "Audio files (*.ogg,*.mp3,*.wav)\0*.mp3;*.ogg;*.wav";

            if (LibWrap.GetOpenFileName(openAudioDialog))
            {
                audioFilepath = openAudioDialog.file;
            
            }
            else
                throw new System.Exception("Could not open file");
#endif
            currentSong.LoadAudio(audioFilepath);

            while (currentSong.musicStream != null && currentSong.musicStream.loadState != AudioDataLoadState.Loaded)
            {
                Debug.Log("Loading audio...");
                //yield return null;
            }

            if (currentSong.musicStream != null)
            {
                musicSource.clip = currentSong.musicStream;
                //movement.SetPosition(0);
            }
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
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

    // For dropdown UI
    public void LoadExpert()
    {
        LoadChart(currentSong.expert_single);
    }

    public void LoadExpertBass()
    {
        LoadChart(currentSong.expert_double_bass);
    }

    public void LoadHard()
    {
        LoadChart(currentSong.hard_single);
    }

    public void LoadHardBass()
    {
        LoadChart(currentSong.hard_double_bass);
    }

    public void LoadMedium()
    {
        LoadChart(currentSong.medium_single);
    }

    public void LoadMediumBass()
    {
        LoadChart(currentSong.medium_double_bass);
    }

    public void LoadEasy()
    {
        LoadChart(currentSong.easy_single);
    }

    public void LoadEasyBass()
    {
        LoadChart(currentSong.easy_double_bass);
    }
}
