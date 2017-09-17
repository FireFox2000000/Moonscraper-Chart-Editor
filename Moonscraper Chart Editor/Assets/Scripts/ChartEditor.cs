#define TIMING_DEBUG
#define BASS_AUDIO
//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;
using Un4seen.Bass;

public class ChartEditor : MonoBehaviour { 
    public static ChartEditor FindCurrentEditor ()
    {
        return GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    public static bool editOccurred = false;
    const int POOL_SIZE = 100;
    public const int MUSIC_STREAM_ARRAY_POS = 0;
    public const int GUITAR_STREAM_ARRAY_POS = 1;
    public const int RHYTHM_STREAM_ARRAY_POS = 2;

    [Header("Prefabs")]
    public GameObject notePrefab;
    public GameObject starpowerPrefab;
    public GameObject sectionPrefab;
    public GameObject bpmPrefab;
    public GameObject tsPrefab;
    public GameObject songEventPrefab;
    public GameObject chartEventPrefab;
    [Header("Tool prefabs")]
    public GameObject ghostNote;
    public GameObject ghostStarpower;
    public GameObject ghostSection;
    public GameObject ghostBPM;
    public GameObject ghostTimeSignature;
    public GameObject ghostEvent;
    public GroupMove groupMove;
    [Header("Misc.")]
    public ToolPanelController toolPanel;       // Used to toggle view mode during undo action
    public Transform visibleStrikeline;
    public TimelineHandler timeHandler;
    public Transform camYMin;
    public Transform camYMax;
    public Transform mouseYMaxLimit;
    public Transform mouseYMinLimit;
    public LoadingScreenFader loadingScreen;
    public ErrorMessage errorMenu;
    public Indicators indicators;               // Cancels hit animations upon stopping playback
    [SerializeField]
    GroupSelect groupSelect;
    public Globals globals;
    [SerializeField]
    ClipboardObjectController clipboard;
    [SerializeField]
    GameplayManager gameplayManager;

    uint _minPos;
    uint _maxPos;
    public uint minPos { get { return _minPos; } }
    public uint maxPos { get { return _maxPos; } }

#if !BASS_AUDIO
    [HideInInspector]
    public AudioSource[] musicSources;
#endif
    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    string currentFileName = string.Empty;

    System.Threading.Thread autosave;
    const float AUTOSAVE_RUN_INTERVAL = 60; // Once a minute
    float autosaveTimer = 0;

    public MovementController movement;
    SongObjectPoolManager _songObjectPoolManager;
    public SongObjectPoolManager songObjectPoolManager { get { return _songObjectPoolManager; } }

    string lastLoadedFile = string.Empty;

    GameObject songObjectParent;
    GameObject chartObjectParent;

    OpenFileName saveFileDialog;

    public ActionHistory actionHistory;
    public SongObject currentSelectedObject
    {
        get
        {
            if (currentSelectedObjects.Length == 1)
                return currentSelectedObjects[0];
            else
                return null;
        }
        set
        {
            if (value == null)
            {
                currentSelectedObjects = new SongObject[0];
            }
            else
                currentSelectedObjects = new SongObject[] { value };
        }
    }
    public SongObject[] currentSelectedObjects = new SongObject[0];

    Vector3? stopResetPos = null;

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
    [DllImport("user32.dll")]
    public static extern System.IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

#if !UNITY_EDITOR
    System.IntPtr windowPtr = IntPtr.Zero;
    string originalWindowName;
#endif

    void SetApplicationWindowPointer()
    {
#if !UNITY_EDITOR
        const int nChars = 256;
        System.Text.StringBuilder buffer = new System.Text.StringBuilder(nChars);
        windowPtr = GetForegroundWindow();
        GetWindowText(windowPtr, buffer, nChars);
        if (buffer.ToString() != GetComponent<Settings>().productName)
        {
            windowPtr = IntPtr.Zero;
            buffer.Length = 0;
            Debug.LogError("Couldn't find window handle");  
        }
        else
            originalWindowName = buffer.ToString();
#endif
    }

    // Use this for initialization
    void Awake () {
        _songObjectPoolManager = GetComponent<SongObjectPoolManager>();

        _minPos = 0;
        _maxPos = 0;

        // Create grouping objects to make reading the inspector easier
        songObjectParent = new GameObject();
        songObjectParent.name = "Song Objects";
        songObjectParent.tag = "Song Object";

        chartObjectParent = new GameObject();
        chartObjectParent.name = "Chart Objects";
        chartObjectParent.tag = "Chart Object";

        // Create a default song
        currentSong = new Song();
        LoadSong(currentSong);

        // Bass init
        if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            Debug.LogError("Failed Bass.Net initialisation");
        else
            Debug.Log("Bass.Net initialised");

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();

        editOccurred = false;
        SetApplicationWindowPointer();

        loadingScreen.gameObject.SetActive(true);
    }

    IEnumerator Start()
    {
        SetVolume();

        yield return null;
        yield return null;

#if !UNITY_EDITOR
        string[] args = Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (System.IO.File.Exists(arg) && (System.IO.Path.GetExtension(arg) == ".chart" || System.IO.Path.GetExtension(arg) == ".mid"))
            {
                StartCoroutine(_Load(arg));
                break;
            }
        }
#endif
    }

    public void Update()
    {
        // Update object positions that supposed to be visible into the range of the camera
        _minPos = currentSong.WorldYPositionToChartPosition(camYMin.position.y);
        _maxPos = currentSong.WorldYPositionToChartPosition(camYMax.position.y);

        // Set window text to represent if the current song has been saved or not
#if !UNITY_EDITOR
        if (windowPtr != IntPtr.Zero)
        {
            if (editOccurred)
                SetWindowText(windowPtr, originalWindowName + "*");
            else
                SetWindowText(windowPtr, originalWindowName);
        }
#endif

        if (autosave == null || autosave.ThreadState != System.Threading.ThreadState.Running)
        {
            autosaveTimer += Time.deltaTime;

            if (autosaveTimer > AUTOSAVE_RUN_INTERVAL)
            {
                Autosave();
            }
        }
        else
            autosaveTimer = 0;

        if (quitting)
        {
            if (editCheck())
            {
                wantsToQuit = true;
                UnityEngine.Application.Quit();
            }
        }
    }

    void Autosave()
    {
        autosave = new System.Threading.Thread(() =>
        {
            autosaveTimer = 0;
            Debug.Log("Autosaving...");
            currentSong.Save(Globals.autosaveLocation, currentSong.defaultExportOptions);
            Debug.Log("Autosave complete!");
            autosaveTimer = 0;
        });

        autosave.Start();
    }
#if UNITY_EDITOR
    bool wantsToQuit = true;        // Won't be save checking if in editor
#else
    bool wantsToQuit = false;
#endif

    void OnApplicationFocus(bool hasFocus)
    {
#if !UNITY_EDITOR
        if (hasFocus && windowPtr == IntPtr.Zero)
            SetApplicationWindowPointer();
#endif
        if (hasFocus)
            Time.timeScale = 1;
        else
        {
            Time.timeScale = 0;
        }

        if (hasFocus && Globals.applicationMode == Globals.ApplicationMode.Playing)
            Play();
        else
        {
            StopAudio();
        }
    }

    static bool quitting = false;
    void OnApplicationQuit()
    {
        quitting = true;

        if (wantsToQuit)
        {
            globals.Quit();
            FreeAudio();

            Bass.BASS_Free();
            Debug.Log("Freed Bass Audio memory");
            while (currentSong.IsSaving) ;
        }
        // Can't run edit check here because quitting seems to run in a seperate thread
        else
        {
            UnityEngine.Application.CancelQuit();
        }
    }

    bool editCheck()
    {    
        // Check for unsaved changes
        if (editOccurred)
        {
            if (quitting)
                UnityEngine.Application.CancelQuit();
#if !UNITY_EDITOR
            DialogResult result;
            //if (windowPtr != IntPtr.Zero)
                //result = MessageBox.Show("Want to save unsaved changes?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (MessageBoxOptions)0x40000);
            //else
            result = MessageBox.Show("Want to save unsaved changes?", "Warning", MessageBoxButtons.YesNoCancel);
            if (quitting)
                UnityEngine.Application.CancelQuit();
            if (result == DialogResult.Yes)
            {
                if (!_Save())
                {
                    quitting = false;
                    return false;
                }
            }
            else if (result == DialogResult.Cancel)
            {
                quitting = false;
                return false;
            }
#endif

            if (quitting)
                UnityEngine.Application.Quit();
        }

        return true;
    }

    public void New()
    {
        if (!editCheck())
            return;

        lastLoadedFile = string.Empty;
        FreeAudio();
        currentSong = new Song();

        LoadSong(currentSong);

        movement.SetPosition(0);
        //StartCoroutine(resetLag());

        currentSelectedObject = null;
        editOccurred = true;
    }
    /*
    IEnumerator resetLag()
    {
        yield return null;
        songPropertiesCon.gameObject.SetActive(true);
    }*/

    // Wrapper function
    public void Load()
    {
        Stop();
        autosaveTimer = 0;
        if (System.IO.File.Exists(Globals.autosaveLocation))
            System.IO.File.Delete(Globals.autosaveLocation);

        StartCoroutine(_Load());
    }

    public void Save()
    {
        _Save();
    }

    public void SaveAs(bool forced = true)
    {
        _SaveAs(forced);
    }

    public bool _Save()
    {
        if (lastLoadedFile != string.Empty)
        {
            Save(lastLoadedFile, currentSong.defaultExportOptions);
            return true;
        }
        else
            return _SaveAs();
    }

    public bool _SaveAs(bool forced = true)
    {
        try {
            string defaultFileName;

            if (lastLoadedFile != string.Empty)
                defaultFileName = System.IO.Path.GetFileNameWithoutExtension(lastLoadedFile);
            else
                defaultFileName = new String(currentSong.name.ToCharArray());

            if (!forced)
                defaultFileName += "(UNFORCED)";

            string fileName = FileExplorer.SaveFilePanel("Chart files (*.chart)\0*.chart", defaultFileName, "chart");

            ExportOptions exportOptions = currentSong.defaultExportOptions;
            exportOptions.forced = forced;

            Save(fileName, exportOptions);

            return true;          
        }
        catch (System.Exception e)
        {
            // User probably just canceled
            Debug.LogError(e.Message);
            return false;
        }
    }

    void Save (string filename, ExportOptions exportOptions)
    {
        if (currentSong != null)
        {
            Debug.Log("Saving to file- " + System.IO.Path.GetFullPath(filename));

            editOccurred = false;            
            currentSong.SaveAsync(filename, exportOptions);
            lastLoadedFile = System.IO.Path.GetFullPath(filename);
        }
    }
    public static float? startGameplayPos = null;
    public void StartGameplay()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Playing || movement.transform.position.y < movement.initPos.y || Globals.drumMode)
            return;

        if (Globals.resetAfterGameplay)
            stopResetPos = movement.transform.position;

        float strikelineYPos = visibleStrikeline.position.y - (0.01f * Globals.hyperspeed);     // Offset to prevent errors where it removes a note that is on the strikeline
        startGameplayPos = strikelineYPos;

        // Hide everything behind the strikeline
        foreach (Note note in currentChart.notes)
        {
            if (note.controller)
            {
                if (note.worldYPosition < strikelineYPos)
                {
                    note.controller.HideFullNote();
                }
                else
                    break;
            }
        }

        // Set position x seconds beforehand
        float time = Song.WorldYPositionToTime(strikelineYPos);
        movement.SetTime(time - Globals.gameplayStartDelayTime);
        //movement.transform.position = new Vector3(movement.transform.position.x, Song.TimeToWorldYPosition(time - Globals.gameplayStartDelayTime), movement.transform.position.z);

        Globals.bot = false;
        Play();
    }

    void PlayAudio(float playPoint)
    {
        StrikelineAudioController.startYPoint = visibleStrikeline.transform.position.y;

        SetBassStreamProperties(currentSong.bassMusicStream, Globals.gameSpeed, Globals.vol_song);
        SetBassStreamProperties(currentSong.bassGuitarStream, Globals.gameSpeed, Globals.vol_guitar);
        SetBassStreamProperties(currentSong.bassRhythmStream, Globals.gameSpeed, Globals.vol_rhythm);
        SetBassStreamProperties(currentSong.bassDrumStream, Globals.gameSpeed, Globals.vol_drum);

        foreach (int bassStream in currentSong.bassAudioStreams)
        {
            PlayBassStream(bassStream, playPoint);
        }
        /*
        PlayBassStream(currentSong.bassMusicStream, playPoint);
        PlayBassStream(currentSong.bassGuitarStream, playPoint);
        PlayBassStream(currentSong.bassRhythmStream, playPoint);
        PlayBassStream(currentSong.bassDrumStream, playPoint);*/

        movement.playStartPosition = movement.transform.position.y;
        movement.playStartTime = Time.time;
    }

    void StopAudio()
    {
#if !BASS_AUDIO
        // Stop the audio from continuing to play
        foreach (AudioSource source in musicSources)
            source.Stop();
#else
        foreach (int bassStream in currentSong.bassAudioStreams)
        {
            if (bassStream != 0)
                Bass.BASS_ChannelStop(bassStream);
        }
        /*
        if (currentSong.bassMusicStream != 0)
            Bass.BASS_ChannelStop(currentSong.bassMusicStream);

        if (currentSong.bassGuitarStream != 0)
            Bass.BASS_ChannelStop(currentSong.bassGuitarStream);

        if (currentSong.bassRhythmStream != 0)
            Bass.BASS_ChannelStop(currentSong.bassRhythmStream);

        if (currentSong.bassDrumStream != 0)
            Bass.BASS_ChannelStop(currentSong.bassDrumStream);*/
#endif

        movement.playStartPosition = null;
        movement.playStartTime = null;
    }

    void PlayBassStream(int handle, float playPoint)
    {
        if (handle != 0)
        {
            Bass.BASS_ChannelSetPosition(handle, playPoint);
            Bass.BASS_ChannelPlay(handle, false);
            //while (!(Bass.BASS_ChannelIsActive(handle) == BASSActive.BASS_ACTIVE_PLAYING));
        }
    }

    void SetBassStreamProperties(int handle, float speed, float vol)
    {
        if (handle != 0)
        {
            // Reset
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_FREQ, 0);
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, 0);
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_TEMPO, 0);

            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_VOL, vol * Globals.vol_master);
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_PAN, Globals.audio_pan);

            if (speed < 1)
            {
                float originalFreq = 0;

                Bass.BASS_ChannelGetAttribute(handle, BASSAttribute.BASS_ATTRIB_FREQ, ref originalFreq);

                float freq = originalFreq * speed;
                if (freq < 100)
                    freq = 100;
                else if (freq > 100000)
                    freq = 100000;
                Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_FREQ, freq);
#if false
                // Pitch shifting equation
                Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, Mathf.Log(1.0f / speed, Mathf.Pow(2, 1.0f / 12.0f)));
#endif
            }
            else
            {
                Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_TEMPO, speed * 100 - 100);
            }
        }
    }

    bool cancel;
    SongObject[] selectedBeforePlay = new SongObject[0];
    public void Play()
    {
        selectedBeforePlay = currentSelectedObjects;
        currentSelectedObject = null;

        if (Globals.bot && Globals.resetAfterPlay)
            stopResetPos = movement.transform.position;

        foreach (HitAnimation hitAnim in indicators.animations)
            hitAnim.StopAnim();

        Globals.applicationMode = Globals.ApplicationMode.Playing;
        cancel = false;

        float playPoint = Song.WorldYPositionToTime(visibleStrikeline.transform.position.y) + currentSong.offset + (Globals.audioCalibrationMS / 1000.0f * Globals.gameSpeed);

        if (playPoint < 0)
        {
            StartCoroutine(delayedStartAudio(-playPoint * Globals.gameSpeed));
        }
        else
        {
            PlayAudio(playPoint);
        } 
    }

    IEnumerator delayedStartAudio(float delay)
    {
        yield return new WaitForSeconds(delay);
        float playPoint = Song.WorldYPositionToTime(visibleStrikeline.transform.position.y) + currentSong.offset + (Globals.audioCalibrationMS / 1000.0f * Globals.gameSpeed);

        if (!cancel && Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            if (playPoint >= 0)
            {
                PlayAudio(playPoint);
            }
            else
            {
                StartCoroutine(delayedStartAudio(-playPoint));
            }
        }
    }

    public IEnumerator PlayAutoStop(float playTime)
    {
        Debug.Log(playTime);
        Play();
        yield return new WaitForSeconds(playTime);
        Stop();
    }

    public void Stop()
    {
        if (indicators && indicators.animations != null)
            foreach (HitAnimation hitAnim in indicators.animations)
            {
                if (hitAnim)
                    hitAnim.StopAnim();
            }

        startGameplayPos = null;
        cancel = true;

        Globals.applicationMode = Globals.ApplicationMode.Editor;

        StopAudio();

        if (currentChart != null)
        {
            foreach (Note note in currentChart.notes)
            {
                if (note.controller)
                    note.controller.Activate();
            }
        }
        if (stopResetPos != null)
            movement.transform.position = (Vector3)stopResetPos;

        if (selectedBeforePlay.Length > 0)
            currentSelectedObjects = selectedBeforePlay;

        selectedBeforePlay = new SongObject[0];

        // Reset gameplay stats and window
        gameplayManager.Reset();

        Globals.bot = true;
        stopResetPos = null;
    }

    string ImportMidToTempChart(string filepath)
    {
        if (System.IO.File.Exists(filepath) && System.IO.Path.GetExtension(filepath) == ".mid")
        {
            try
            {
                const string tempFileName = "moonscraperMid2Chart.temp.chart";

                string file = System.IO.Path.GetDirectoryName(filepath) + "\\" + tempFileName;

                mid2chart.Program.readOpenNotes = true;
                mid2chart.Program.dontWriteDummy = true;
                mid2chart.Program.skipPause = true;
            
                mid2chart.Song midSong = mid2chart.MidReader.ReadMidi(filepath, false);
            
                mid2chart.ChartWriter.WriteChart(midSong, file, false);

                if (System.IO.File.Exists(file))
                    return file;

                /*
                mid2chart.Program.Run(new string[] { filepath, "-p", "-m", "-k" });
                if (System.IO.File.Exists(file + ".chart"))
                    return file + ".chart";*/
            }
            catch (Exception e)
            {
                ErrorMessage.errorMessage = "Failed to convert mid file: " + e.Message;
                Debug.LogError("Failed to convert mid file: " + e.Message);
            }
        }

        return string.Empty;
    }

    public IEnumerator _Load(string currentFileName, bool recordLastLoaded = true)
    {
        bool error = false;
        Song backup = currentSong;
#if TIMING_DEBUG
        float totalLoadTime = 0;
#endif

        // Start loading animation
        Globals.applicationMode = Globals.ApplicationMode.Loading;
        loadingScreen.FadeIn();
        yield return null;

        // Wait for saving to complete just in case
        while (currentSong.IsSaving)
            yield return null;

#if TIMING_DEBUG
        totalLoadTime = Time.realtimeSinceStartup;
#endif
        bool mid = false;

#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        // Load the actual file
        loadingScreen.loadingInformation.text = "Loading file";
        yield return null;

        // Free the audio clips
        FreeAudio();

        System.Threading.Thread songLoadThread = new System.Threading.Thread(() =>
        {
            mid = (System.IO.Path.GetExtension(currentFileName) == ".mid");

            try
            {
                if (mid)
                    currentSong = MidReader.ReadMidi(currentFileName);
                else
                    currentSong = new Song(currentFileName);
            }
            catch (Exception e)
            {
                currentSong = backup;

                if (mid)
                    ErrorMessage.errorMessage = "Failed to open mid file: " + e.Message;
                else
                    ErrorMessage.errorMessage = "Failed to open chart file: " + e.Message;

                error = true;

                // Immediate exit
                Debug.LogError(e.Message);
            }
        });

        songLoadThread.Start();
        while (songLoadThread.ThreadState == System.Threading.ThreadState.Running)
            yield return null;

        if (error)
        {
            loadingScreen.FadeOut();
            errorMenu.gameObject.SetActive(true);
            yield break;
        }

#if TIMING_DEBUG
        Debug.Log("Chart file load time: " + (Time.realtimeSinceStartup - time));
        time = Time.realtimeSinceStartup;
#endif
        // Load the audio clips
        loadingScreen.loadingInformation.text = "Loading audio";
        yield return null;
        currentSong.LoadAllAudioClips();

        while (currentSong.IsAudioLoading)
            yield return null;

#if TIMING_DEBUG
        Debug.Log("All audio files load time: " + (Time.realtimeSinceStartup - time));
#endif
        yield return null;
        //currentSong = new Song(currentFileName);
        editOccurred = false;

#if TIMING_DEBUG
        Debug.Log("File load time: " + (Time.realtimeSinceStartup - totalLoadTime));
#endif

        // Wait for audio to fully load
        while (currentSong.IsAudioLoading)
            yield return null;

        if (mid)
        {
            currentFileName = string.Empty;
            editOccurred = true;
            Debug.Log("Loaded mid file");
        }

        if (recordLastLoaded && currentFileName != string.Empty && !mid)
            lastLoadedFile = System.IO.Path.GetFullPath(currentFileName);
        else
            lastLoadedFile = string.Empty;

        LoadSong(currentSong);

#if TIMING_DEBUG
        Debug.Log("Total load time: " + (Time.realtimeSinceStartup - totalLoadTime));
#endif

        // Stop loading animation
        Globals.applicationMode = Globals.ApplicationMode.Editor;
        loadingScreen.FadeOut();
        loadingScreen.loadingInformation.text = "Complete!";
    }

    IEnumerator _Load()
    {
        if (!editCheck())
            yield break;

        Song backup = currentSong;

        try
        {
#if UNITY_EDITOR
            currentFileName = UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart,mid");
            if (currentFileName == string.Empty)
                yield break;
#else
            OpenFileName openChartFileDialog = new OpenFileName();

            openChartFileDialog.structSize = Marshal.SizeOf(openChartFileDialog);
            openChartFileDialog.filter = "Chart files (*.chart, *.mid)\0*.chart;*.mid";
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

        }
        catch (System.Exception e)
        {
            // Most likely closed the window explorer, just ignore for now.
            currentSong = backup;
            Debug.LogError(e.Message);

            // Immediate exit
            yield break;
        }

        Debug.Log("Loading song: " + System.IO.Path.GetFullPath(currentFileName));

        yield return StartCoroutine(_Load(currentFileName));

        currentSelectedObject = null;
    }

    void LoadSong(Song song)
    {
        if (lastLoadedFile != string.Empty)
            editOccurred = false;

        MenuBar.currentInstrument = Song.Instrument.Guitar;
        MenuBar.currentDifficulty = Song.Difficulty.Expert;

        // Load the default chart
        LoadChart(currentSong.GetChart(Song.Instrument.Guitar, Song.Difficulty.Expert));
#if !BASS_AUDIO
        // Reset audioSources upon successfull load
        foreach (AudioSource source in musicSources)
            source.clip = null;

        // Load audio
        if (currentSong.musicStream != null)
#else
        if (currentSong.bassMusicStream != 0)
#endif
        {
            SetAudioSources();
            movement.SetPosition(0);
        }
    }

    // Chart should be part of the current song
    public void LoadChart(Chart chart)
    {
        actionHistory = new ActionHistory();
        Stop();

        currentChart = chart;

        songObjectPoolManager.NewChartReset();
    }

    public void EnableMenu(DisplayMenu menu)
    {
        menu.gameObject.SetActive(true);
    }

    public void SetAudioSources()
    {
#if !BASS_AUDIO
        musicSources[MUSIC_STREAM_ARRAY_POS].clip = currentSong.musicStream;
        musicSources[GUITAR_STREAM_ARRAY_POS].clip = currentSong.guitarStream;
        musicSources[RHYTHM_STREAM_ARRAY_POS].clip = currentSong.rhythmStream;
#endif
    }

    public void FreeAudio()
    {
        currentSong.musicSample.Free();
        currentSong.guitarSample.Free();
        currentSong.rhythmSample.Free();
        currentSong.drumSample.Free();
#if !BASS_AUDIO
        currentSong.FreeAudioClips();
#else
        currentSong.FreeBassAudioStreams();
#endif
        /*
        foreach (AudioSource source in musicSources)
        {
            if (source.clip)
                source.clip.UnloadAudioData();

            Destroy(source.clip);
        }*/
    }

    public void AddToSelectedObjects(SongObject songObjects)
    {
        AddToSelectedObjects(new SongObject[] { songObjects });
    }

    public void AddToSelectedObjects(System.Collections.Generic.IEnumerable<SongObject> songObjects)
    {
        var selectedObjectsList = new System.Collections.Generic.List<SongObject>(currentSelectedObjects);

        foreach (SongObject songObject in songObjects)
        {
            if (!selectedObjectsList.Contains(songObject))
            {
                int pos = SongObject.FindClosestPosition(songObject, selectedObjectsList.ToArray());
                if (pos != SongObject.NOTFOUND)
                {
                    if (selectedObjectsList[pos] > songObject)
                        selectedObjectsList.Insert(pos, songObject);
                    else
                        selectedObjectsList.Insert(pos + 1, songObject);
                }
                else
                    selectedObjectsList.Add(songObject);
            }
        }

        currentSelectedObjects = selectedObjectsList.ToArray();
    }

    public void RemoveFromSelectedObjects(SongObject songObjects)
    {
        RemoveFromSelectedObjects(new SongObject[] { songObjects });
    }

    public void RemoveFromSelectedObjects(System.Collections.Generic.IEnumerable<SongObject> songObjects)
    {
        var selectedObjectsList = new System.Collections.Generic.List<SongObject>(currentSelectedObjects);

        foreach (SongObject songObject in songObjects)
        {
            selectedObjectsList.Remove(songObject);
        }

        currentSelectedObjects = selectedObjectsList.ToArray();
    }

    public bool IsSelected(SongObject songObject)
    {
        return (SongObject.FindObjectPosition(songObject, currentSelectedObjects) != SongObject.NOTFOUND);
    }

    public void UndoWrapper()
    {
        if (actionHistory.Undo(this))
            groupSelect.reset();
    }

    public void RedoWrapper()
    {
        if (actionHistory.Redo(this))
            groupSelect.reset();
    }

    public void Copy()
    {
        const float DEFAULT_LEFT = -2;
        const float DEFAULT_RIGHT = 2;

        var songObjectsCopy = new SongObject[currentSelectedObjects.Length];
        float? left = null, right = null;
        float position = 0;

        // Scan through all the current objects to determine width of scanned area
        for (int i = 0; i < currentSelectedObjects.Length; ++i)
        {
            songObjectsCopy[i] = currentSelectedObjects[i].Clone();

            position = SongObjectController.GetXPos(currentSelectedObjects[i]);

            if (left == null || position < left)
                left = position;

            if (right == null || position > right)
                right = position;
        }

        // Default collision size
        if (left == null || left > DEFAULT_LEFT)
            left = DEFAULT_LEFT;
        if (right == null || right < DEFAULT_RIGHT)
            right = DEFAULT_RIGHT;

        Vector2 bottomLeft = Vector2.zero;
        Vector2 upperRight = Vector2.zero;
        var area = new Clipboard.SelectionArea();

        if (currentSelectedObjects.Length > 0)
        {
            bottomLeft = new Vector2((float)left, currentSong.ChartPositionToWorldYPosition(songObjectsCopy[0].position));
            upperRight = new Vector2((float)right, currentSong.ChartPositionToWorldYPosition(songObjectsCopy[songObjectsCopy.Length - 1].position));
            area = new Clipboard.SelectionArea(bottomLeft, upperRight, songObjectsCopy[0].position, songObjectsCopy[songObjectsCopy.Length - 1].position);
        }        

        ClipboardObjectController.SetData(songObjectsCopy, area, currentSong);
    }

    public void Delete()
    {
        if (currentSelectedObjects.Length > 0)
        {
            actionHistory.Insert(new ActionHistory.Delete(currentSelectedObjects));

            foreach (SongObject songObject in currentSelectedObjects)
            {
                songObject.Delete(false);
            }

            currentChart.UpdateCache();
            currentSong.UpdateCache();

            actionHistory.Insert(FixUpBPMAnchors().ToArray());

            currentSelectedObject = null;

            groupSelect.reset();
        }
    }

    public void Cut()
    {
        Copy();
        Delete();
    }

    public void SetVolume()
    {
#if !BASS_AUDIO
        AudioListener.volume = Globals.vol_master;

        musicSources[MUSIC_STREAM_ARRAY_POS].volume = Globals.vol_song;
        musicSources[GUITAR_STREAM_ARRAY_POS].volume = Globals.vol_guitar;
        musicSources[RHYTHM_STREAM_ARRAY_POS].volume = Globals.vol_rhythm;

        musicSources[MUSIC_STREAM_ARRAY_POS].panStereo = Globals.audio_pan;
        musicSources[GUITAR_STREAM_ARRAY_POS].panStereo = Globals.audio_pan;
        musicSources[RHYTHM_STREAM_ARRAY_POS].panStereo = Globals.audio_pan;
#endif
    }

    public System.Collections.Generic.List<ActionHistory.Action> FixUpBPMAnchors()
    {
        System.Collections.Generic.List<ActionHistory.Action> record = new System.Collections.Generic.List<ActionHistory.Action>();

        BPM[] bpms = currentSong.bpms;
        // Fix up any anchors
        for (int i = 0; i < bpms.Length; ++i)
        {
            if (bpms[i].anchor != null && i > 0)
            {
                BPM anchorBPM = bpms[i];
                BPM bpmToAdjust = bpms[i - 1];

                double deltaTime = (double)anchorBPM.anchor - bpmToAdjust.time;
                uint newValue = (uint)Mathf.Round((float)(Song.dis_to_bpm(bpmToAdjust.position, anchorBPM.position, deltaTime, currentSong.resolution) * 1000.0d));
                //Debug.Log(newBpmValue + ", " + deltaTime + ", " + newValue);
                if (deltaTime > 0 && newValue > 0)
                {
                    if (bpmToAdjust.value != newValue)
                    {
                        BPM original = new BPM(bpmToAdjust);
                        bpmToAdjust.value = newValue;
                        Debug.Log(newValue);
                        record.Add(new ActionHistory.Modify(original, bpmToAdjust));
                    }
                }


            }
        }

        return record;
    }
}
