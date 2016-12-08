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
    public GameObject notePrefab;
    public GameObject starpowerPrefab;
    public GameObject sectionPrefab;
    public GameObject beatLine1;
    public GameObject beatLine2;
    [Header("Indicator Parents")]
    public GameObject guiIndicators;
    [Header("Song properties Display")]
    public Text songNameText;
    public Slider hyperspeedSlider;
    [Header("Inspectors")]
    public NotePropertiesPanelController noteInspector;
    public SectionPropertiesPanelController sectionInspector;
    [Header("Tool prefabs")]
    public GameObject ghostNote;
    public GameObject ghostStarpower;
    public GameObject ghostSection;
    [Header("Misc.")]
    public UnityEngine.UI.Button play;
    public Transform strikeline;
    public TimelineHandler timeHandler;
    public Transform camYMin;
    public Transform camYMax;
    public Transform autoUpScroll;
    public GameObject willClickOn;
    public GameObject selectedHighlight;
    
    public uint minPos { get; private set; }
    public uint maxPos { get; private set; }
    [HideInInspector]
    public AudioSource musicSource;

    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    string currentFileName = string.Empty;

    MovementController movement;
    GameObject[] beatLinePool1 = new GameObject[POOL_SIZE];
    GameObject[] beatLinePool2 = new GameObject[POOL_SIZE];
    GameObject timeSignatureLineParent;

    string lastLoadedFile = string.Empty;

    GameObject songObjectParent;
    GameObject chartObjectParent;

    OpenFileName saveFileDialog;

    public SongObject currentSelectedObject = null;
    GameObject currentPropertiesPanel = null;

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
    [DllImport("user32.dll")]
    public static extern System.IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

#if !UNITY_EDITOR
    SaveFileDialog saveDialog;
#endif

    System.IntPtr windowPtr;
    string originalWindowName;

    // Use this for initialization
    void Awake () {
        const int nChars = 256;
        System.Text.StringBuilder buffer = new System.Text.StringBuilder(nChars);

        windowPtr = GetForegroundWindow();
        GetWindowText(windowPtr, buffer, nChars);
        originalWindowName = buffer.ToString();

        minPos = 0;
        maxPos = 0;

        noteInspector.gameObject.SetActive(false);
        sectionInspector.gameObject.SetActive(false);

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
            beatLinePool1[i] = Instantiate(beatLine1);
            beatLinePool1[i].transform.SetParent(timeSignatureLineParent.transform);
            beatLinePool1[i].SetActive(false);

            beatLinePool2[i] = Instantiate(beatLine2);
            beatLinePool2[i].transform.SetParent(timeSignatureLineParent.transform);
            beatLinePool2[i].SetActive(false);
        }

        willClickOn.SetActive(false);
        selectedHighlight.SetActive(false);
    }

    void Update()
    {
        songNameText.text = currentSong.name;

        // Update object positions that supposed to be visible into the range of the camera
        minPos = currentSong.WorldYPositionToChartPosition(camYMin.position.y);
        maxPos = currentSong.WorldYPositionToChartPosition(camYMax.position.y);

        // Update song objects within range
#if true
        enableSongObjects(currentSong.events, SongObject.ID.Event, minPos, maxPos);
        enableSongObjects(currentSong.syncTrack, SongObject.ID.BPM, minPos, maxPos);

        enableSongObjects(currentChart.notes, SongObject.ID.Note, minPos, maxPos);
        enableSongObjects(currentChart.starPower, SongObject.ID.Starpower, minPos, maxPos);
        enableSongObjects(currentChart.events, SongObject.ID.ChartEvent, minPos, maxPos);
#endif
        // Update the lines that go on the fretboard
        UpdateBeatLines();

        // Update the current properties panel
        if (currentSelectedObject != null)
        {
            GameObject previousPanel = currentPropertiesPanel;

            switch (currentSelectedObject.classID)
            {
                case ((int)SongObject.ID.Note):
                    noteInspector.currentNote = (Note)currentSelectedObject;
                    currentPropertiesPanel = noteInspector.gameObject;
                    break;
                case ((int)SongObject.ID.Section):
                    sectionInspector.currentSection = (Section)currentSelectedObject;
                    currentPropertiesPanel = sectionInspector.gameObject;
                    break;
                default:
                    currentPropertiesPanel = null;
                    currentSelectedObject = null;
                    break;
            }

            if (currentPropertiesPanel != previousPanel)
            {
                if (previousPanel)
                    previousPanel.SetActive(false);
            }

            if (currentPropertiesPanel != null)
            {
                currentPropertiesPanel.gameObject.SetActive(true);
            }
        }
        else if (currentPropertiesPanel)
        {
            currentPropertiesPanel.gameObject.SetActive(false);
        }

        // Show a highlight over the current selected object
        if (currentSelectedObject != null && currentSelectedObject.controller != null && currentSelectedObject.controller.gameObject != null)
        {
            Collider col3d = currentSelectedObject.controller.GetComponent<Collider>();
            Collider2D col = currentSelectedObject.controller.GetComponent<Collider2D>();
            if (col3d || col)
            {
                selectedHighlight.SetActive(true);
                selectedHighlight.transform.position = currentSelectedObject.controller.transform.position;

                Vector3 scale = currentSelectedObject.controller.transform.localScale;

                if (col3d)
                    scale = col3d.bounds.size;
                else
                    scale = col.bounds.size;

                if (scale.z == 0)
                    scale.z = 0.1f;
                selectedHighlight.transform.localScale = scale;
            }
            else
                selectedHighlight.SetActive(false);
        }
        else
            selectedHighlight.SetActive(false);

        // Show a preview if the user will click on an object
        GameObject songObject = Mouse.GetSelectableObjectUnderMouse();
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && songObject != null)
        {
            willClickOn.SetActive(true);
            willClickOn.transform.position = songObject.transform.position;

            Vector3 scale = songObject.transform.localScale;
            Collider col3d = songObject.GetComponent<Collider>();
            Collider2D col = songObject.GetComponent<Collider2D>();

            if (col3d)
                scale = col3d.bounds.size;
            else
                scale = col.bounds.size;

            if (scale.z == 0)
                scale.z = 0.1f;
            willClickOn.transform.localScale = scale;
        }
        else
            willClickOn.SetActive(false);

        // Set window text to represent if the current song has been saved or not
#if !UNITY_EDITOR
        if (editOccurred)
            SetWindowText(windowPtr, originalWindowName + "*");
        else
            SetWindowText(windowPtr, originalWindowName);
#endif
        Globals.hyperspeed = hyperspeedSlider.value;
    }
    
    void OnApplicationFocus(bool hasFocus)
    {      
        if (hasFocus)
            Time.timeScale = 1;
        else
        {
            Time.timeScale = 0;
        }

        if (hasFocus && Globals.applicationMode == Globals.ApplicationMode.Playing)
            Play();
        else
            musicSource.Stop();
    }

    void OnApplicationQuit()
    {    
        editCheck();

        while (currentSong.IsSaving);
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
            /*
            saveDialog.Filter = "chart files (*.chart)|*.chart";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
            }
            else
                throw new System.Exception("File was not saved");*/

            OpenFileName openSaveFileDialog = new OpenFileName();

            openSaveFileDialog.structSize = Marshal.SizeOf(openSaveFileDialog);
            openSaveFileDialog.filter = "Chart files (*.chart)\0*.chart";
            openSaveFileDialog.file = new String(new char[256]);
            openSaveFileDialog.maxFile = openSaveFileDialog.file.Length;

            openSaveFileDialog.fileTitle = new String(new char[64]);
            openSaveFileDialog.maxFileTitle = openSaveFileDialog.fileTitle.Length;

            openSaveFileDialog.initialDir = "";
            openSaveFileDialog.title = "Save as";
            openSaveFileDialog.defExt = "chart";
            openSaveFileDialog.flags = 0x000002;        // Overwrite warning

            if (LibWrap.GetSaveFileName(openSaveFileDialog))
            {
                fileName = openSaveFileDialog.file;
            }
            else
            {
                throw new System.Exception("Could not open file");
            }
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
            OpenFileName openChartFileDialog = new OpenFileName();

            openChartFileDialog.structSize = Marshal.SizeOf(openChartFileDialog);
            openChartFileDialog.filter = "Chart files (*.chart)\0*.chart";
            openChartFileDialog.file = new String(new char[256]);
            openChartFileDialog.maxFile = openChartFileDialog.file.Length;

            openChartFileDialog.fileTitle = new String(new char[64]);
            openChartFileDialog.maxFileTitle = openChartFileDialog.fileTitle.Length;

            openChartFileDialog.initialDir = "";
            openChartFileDialog.title = "Open file";
            openChartFileDialog.defExt = "chart";

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

    // Create Sections, bpms, events and time signature objects
    GameObject CreateSongObjects(Song song)
    {
        for (int i = 0; i < song.sections.Length; ++i)
        {           
            // Attach the note to the object
            SectionController controller = CreateSectionObject(song.sections[i]);
            
            controller.UpdateSongObject();
            
        }
        
        return songObjectParent;
    }

    public SectionController CreateSectionObject(Section section)
    {
        // Attach the note to the object
        SectionController controller = CreateSongObject(this.sectionPrefab).GetComponentInChildren<SectionController>();

        // Link controller and note together
        controller.Init(section, timeHandler, guiIndicators);
        return controller;
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
                NoteController controller = CreateNoteObject(notes[i]);
                controller.UpdateSongObject();
            }
        }

        StarPower[] starpowers = chart.starPower;
        for (int i = 0; i < starpowers.Length; ++i)
        {
            // Make sure notes haven't been deleted
            if (notes[i].song != null)
            {
                StarpowerController controller = CreateStarpowerObject(starpowers[i]);
                controller.UpdateSongObject();
            }
        }

        return chartObjectParent;
    }

    public NoteController CreateNoteObject(Note note)
    {
        // Attach the note to the object
        NoteController controller = CreateChartObject(this.notePrefab).GetComponent<NoteController>();

        // Link controller and note together
        controller.Init(note);
        return controller;
    }

    public StarpowerController CreateStarpowerObject(StarPower starpower)
    {
        // Attach the note to the object
        StarpowerController controller = CreateChartObject(this.starpowerPrefab).GetComponent<StarpowerController>();

        // Link controller and note together
        controller.Init(starpower);
        return controller;
    }

    GameObject CreateChartObject(GameObject chartObjectPrefab)
    {
        // Convert the chart data into gameobject
        GameObject chartObject = Instantiate(chartObjectPrefab);

        chartObject.transform.SetParent(chartObjectParent.transform);
        chartObject.SetActive(false);
        return chartObject;
    }

    GameObject CreateSongObject(GameObject songObjectPrefab)
    {
        // Convert the chart data into gameobject
        GameObject chartObject = Instantiate(songObjectPrefab);

        chartObject.transform.SetParent(songObjectParent.transform);
        //chartObject.SetActive(false);
        return chartObject;
    }

    // For dropdown UI
    public void LoadExpert()
    {
        LoadChart(currentSong.expert_single);
    }

    public void LoadExpertDoubleGuitar()
    {
        LoadChart(currentSong.expert_double_guitar);
    }

    public void LoadExpertDoubleBass()
    {
        LoadChart(currentSong.expert_double_bass);
    }

    public void LoadHard()
    {
        LoadChart(currentSong.hard_single);
    }

    public void LoadHardDoubleGuitar()
    {
        LoadChart(currentSong.hard_double_guitar);
    }

    public void LoadHardDoubleBass()
    {
        LoadChart(currentSong.hard_double_bass);
    }

    public void LoadMedium()
    {
        LoadChart(currentSong.medium_single);
    }

    public void LoadMediumDoubleGuitar()
    {
        LoadChart(currentSong.medium_double_guitar);
    }

    public void LoadMediumDoubleBass()
    {
        LoadChart(currentSong.medium_double_bass);
    }

    public void LoadEasy()
    {
        LoadChart(currentSong.easy_single);
    }

    public void LoadEasyDoubleGuitar()
    {
        LoadChart(currentSong.easy_double_guitar);
    }

    public void LoadEasyDoubleBass()
    {
        LoadChart(currentSong.easy_double_bass);
    }

    void enableSongObjects(SongObject[] songObjects, SongObject.ID id, uint min, uint max)
    {
        // Enable all objects within the min-max position
        int arrayPos = SongObject.FindClosestPosition(min, songObjects);
        if (arrayPos != Globals.NOTFOUND)
        {
            // Find the back-most position
            while (arrayPos > 0 && songObjects[arrayPos].position > min)
            {
                --arrayPos;
            }

            // Check if sustains need to be rendered
            if (id == SongObject.ID.Note)
            {
                // Find the last known note of each fret type to find any sustains that might overlap. Cancel if there's an open note.
                bool green = false, red = false, yellow = false, blue = false, orange = false, open = false;
                Note[] notes = (Note[])songObjects;

                int pos = arrayPos - 1;
                while (pos > 0)
                {
                    switch (notes[pos].fret_type)
                    {
                        case (Note.Fret_Type.GREEN):
                            if (!green && notes[pos].controller != null)
                                notes[pos].controller.gameObject.SetActive(true);

                            green = true;
                            break;
                        case (Note.Fret_Type.RED):
                            if (!red && notes[pos].controller != null)
                                notes[pos].controller.gameObject.SetActive(true);

                            red = true;
                            break;
                        case (Note.Fret_Type.YELLOW):
                            if (!yellow && notes[pos].controller != null)
                                notes[pos].controller.gameObject.SetActive(true);

                            yellow = true;
                            break;
                        case (Note.Fret_Type.BLUE):
                            if (!blue && notes[pos].controller != null)
                                notes[pos].controller.gameObject.SetActive(true);

                            blue = true;
                            break;
                        case (Note.Fret_Type.ORANGE):
                            if (!orange && notes[pos].controller != null)
                                notes[pos].controller.gameObject.SetActive(true);

                            orange = true;
                            break;
                        case (Note.Fret_Type.OPEN):
                            if (!open && notes[pos].controller != null)
                                notes[pos].controller.gameObject.SetActive(true);

                            open = true;
                            break;
                        default:
                            break;
                    }

                    if ((green && red && yellow && blue && orange) || open)
                        break;

                    --pos;
                }
            }
            else if (id == SongObject.ID.Starpower)
            {
                // Render previous sp sustain in case of overlap into current position
                if (songObjects[arrayPos].position > min && arrayPos > 0)
                {
                    if (songObjects[arrayPos - 1].controller != null)
                        songObjects[arrayPos - 1].controller.gameObject.SetActive(true);
                }
            }

            enableSongObjects(songObjects, arrayPos, max);
        }
    }

    void enableSongObjects(SongObject[] songObjects, int startArrayPos, uint max)
    {
        // Enable objects through range
        for (int i = startArrayPos; i < songObjects.Length && songObjects[i].position < max; ++i)
        {
            if (songObjects[i].controller != null)
                songObjects[i].controller.gameObject.SetActive(true);
        }
    }

    void UpdateBeatLines()
    {
        // Update time signature lines SNAPPED
        uint initSnappedLinePos = currentSong.WorldPositionToSnappedChartPosition(camYMin.position.y, 4);
        uint snappedLinePos = initSnappedLinePos;

        // Place main beat lines
        int i = 0;
        while (snappedLinePos < maxPos && i < beatLinePool1.Length)
        {
            beatLinePool1[i].SetActive(true);
            beatLinePool1[i].transform.position = new Vector3(0, currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
            snappedLinePos += (uint)(currentSong.resolution);
            ++i;
        }

        // Disable any unused lines
        while (i < beatLinePool1.Length)
        {
            beatLinePool1[i++].SetActive(false);
        }

        // Place faded beat lines
        i = 0;
        if ((uint)(currentSong.resolution / 2) < initSnappedLinePos)
            snappedLinePos = initSnappedLinePos - (uint)(currentSong.resolution / 2);
        else
            snappedLinePos = initSnappedLinePos + (uint)(currentSong.resolution / 2);

        while (snappedLinePos < maxPos && i < beatLinePool2.Length)
        {
            beatLinePool2[i].SetActive(true);
            beatLinePool2[i].transform.position = new Vector3(0, currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
            snappedLinePos += (uint)(currentSong.resolution);
            ++i;
        }

        // Disable any unused lines
        while (i < beatLinePool2.Length)
        {
            beatLinePool2[i++].SetActive(false);
        }
    }

    public void EnableMenu(DisplayMenu menu)
    {
        menu.gameObject.SetActive(true);
    }
}
