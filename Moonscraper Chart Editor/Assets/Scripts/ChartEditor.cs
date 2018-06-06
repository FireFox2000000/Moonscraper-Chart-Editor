// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

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
    static ChartEditor currentEditor;

    public static ChartEditor GetInstance ()
    {
        if (currentEditor)
            return currentEditor;
        else
            return GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    public static bool isDirty = false;

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
    [SerializeField]
    MenuBar menuBar;
    [SerializeField]
    SaveAnimController saveAnim;

    uint _minPos;
    uint _maxPos;
    public uint minPos { get { return _minPos; } }
    public uint maxPos { get { return _maxPos; } }

    [HideInInspector]
    public InputManager inputManager;

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

            timeHandler.RefreshHighlightIndicator();
        }
    }


    SongObject[] m_currentSelectedObjects = new SongObject[0];
    public SongObject[] currentSelectedObjects
    {
        get
        {
            return m_currentSelectedObjects;
        }
        set
        {
            m_currentSelectedObjects = value;
            timeHandler.RefreshHighlightIndicator();
        }
    }

    public uint currentTickPos {
        get
        {
            if (MovementController.explicitChartPos != null)
                return (uint)MovementController.explicitChartPos;
            else
                return currentSong.WorldYPositionToTick(visibleStrikeline.position.y);
        }
    }

    Vector3? stopResetPos = null;

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
    [DllImport("user32.dll")]
    public static extern System.IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

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

    public float currentVisibleTime
    {
        get
        {
            return TickFunctions.WorldYPositionToTime(visibleStrikeline.transform.position.y);
        }
    }

    public float currentAudioTime
    {
        get
        {
            return currentVisibleTime + currentSong.offset + (GameSettings.audioCalibrationMS / 1000.0f * GameSettings.gameSpeed);
        }
    }

    // Use this for initialization
    void Awake () {
        currentEditor = this;
        _songObjectPoolManager = GetComponent<SongObjectPoolManager>();

        _minPos = 0;
        _maxPos = 0;

        // Create a default song
        currentSong = new Song();
        LoadSong(currentSong, true);

        // Bass init
        if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            Debug.LogError("Failed Bass.Net initialisation");
        else
            Debug.Log("Bass.Net initialised");

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();

        isDirty = false;
        SetApplicationWindowPointer();

        loadingScreen.gameObject.SetActive(true);

        inputManager = gameObject.AddComponent<InputManager>();
        gameObject.AddComponent<UITabbing>();
    }

    IEnumerator Start()
    {
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

    public bool SaveErrorCheck()
    {
        bool saveError = currentSong.saveError;
        if (currentSong.saveError)
        {
            Debug.Log("Save error detected, opening up error menu");
            errorMenu.gameObject.SetActive(true);
            currentSong.saveError = false;
        }

        return saveError;
    }

    public void Update()
    {
        SaveErrorCheck();

        // Update object positions that supposed to be visible into the range of the camera
        _minPos = currentSong.WorldYPositionToTick(camYMin.position.y);
        _maxPos = currentSong.WorldYPositionToTick(camYMax.position.y);

        // Set window text to represent if the current song has been saved or not
#if !UNITY_EDITOR
        if (windowPtr != IntPtr.Zero)
        {
            if (isDirty)
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

    private static Song autosaveSong = null;
    void Autosave()
    {
        autosaveSong = new Song(currentSong);

        autosave = new System.Threading.Thread(() =>
        {
            autosaveTimer = 0;
            Debug.Log("Autosaving...");
            autosaveSong.Save(Globals.autosaveLocation, currentSong.defaultExportOptions);
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
        /*
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
        }*/
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
            while (currentSong.isSaving) ;
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
        if (isDirty)
        {
            if (quitting)
                UnityEngine.Application.CancelQuit();
#if !UNITY_EDITOR
            const int YES = 6;
            const int NO = 7;
            const int CANCEL = 2;

            int result = MessageBox(new IntPtr(), "Want to save unsaved changes?", "Warning", 3);
            if (quitting)
                UnityEngine.Application.CancelQuit();

            if (result == YES)
            {
                if (!_Save())
                {
                    quitting = false;
                    return false;
                }
            }
            else if (result == CANCEL)
            {
                quitting = false;
                return false;
            }

            /*
            DialogResult result = MessageBox.Show("Want to save unsaved changes?", "Warning", MessageBoxButtons.YesNoCancel);
            
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
            }*/
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

        while (currentSong.isSaving) ;

        if (SaveErrorCheck())
            return;

        lastLoadedFile = string.Empty;
        FreeAudio();
        currentSong = new Song();

        LoadSong(currentSong);

        movement.SetPosition(0);
        //StartCoroutine(resetLag());

        currentSelectedObject = null;
        isDirty = true;
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
        if (currentSong != null && !currentSong.isSaving)
        {
            Debug.Log("Saving to file- " + System.IO.Path.GetFullPath(filename));
          
            currentSong.SaveAsync(filename, exportOptions);
            lastLoadedFile = System.IO.Path.GetFullPath(filename);

            if (currentSong.isSaving)
                saveAnim.StartFade();

            isDirty = false;
        }
    }
    public static float? startGameplayPos = null;
    public void StartGameplay()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Playing || 
            movement.transform.position.y < movement.initPos.y || 
            Globals.ghLiveMode)
            return;

        if (GameSettings.resetAfterGameplay)
            stopResetPos = movement.transform.position;
        
        float strikelineYPos = visibleStrikeline.position.y - (0.01f * GameSettings.hyperspeed);     // Offset to prevent errors where it removes a note that is on the strikeline
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
        float time = TickFunctions.WorldYPositionToTime(strikelineYPos);
        movement.SetTime(time - GameSettings.gameplayStartDelayTime);

        GameSettings.bot = false;
        Play();
    }

    void PlayAudio(float playPoint)
    {
        StrikelineAudioController.startYPoint = visibleStrikeline.transform.position.y;

        SetBassStreamProperties(currentSong.GetBassAudioStream(Song.AudioInstrument.Song), GameSettings.gameSpeed, GameSettings.vol_song);
        SetBassStreamProperties(currentSong.GetBassAudioStream(Song.AudioInstrument.Guitar), GameSettings.gameSpeed, GameSettings.vol_guitar);
        SetBassStreamProperties(currentSong.GetBassAudioStream(Song.AudioInstrument.Bass), GameSettings.gameSpeed, GameSettings.vol_bass);
        SetBassStreamProperties(currentSong.GetBassAudioStream(Song.AudioInstrument.Rhythm), GameSettings.gameSpeed, GameSettings.vol_rhythm);
        SetBassStreamProperties(currentSong.GetBassAudioStream(Song.AudioInstrument.Drum), GameSettings.gameSpeed, GameSettings.vol_drum);

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
        movement.playStartTime = Time.realtimeSinceStartup;
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

            MovementController.timeSync.SongTime = playPoint;
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

            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_VOL, vol * GameSettings.vol_master);
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_PAN, GameSettings.audio_pan);

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

        if (GameSettings.bot && GameSettings.resetAfterPlay)
            stopResetPos = movement.transform.position;

        foreach (HitAnimation hitAnim in indicators.animations)
            hitAnim.StopAnim();

        Globals.applicationMode = Globals.ApplicationMode.Playing;
        cancel = false;

        float playPoint = currentAudioTime;

        if (playPoint < 0)
        {
            StartCoroutine(delayedStartAudio(-playPoint * GameSettings.gameSpeed));
        }
        else
        {
            PlayAudio(playPoint);
        } 
    }

    IEnumerator delayedStartAudio(float delay)
    {
        yield return new WaitForSeconds(delay);
        float playPoint = currentAudioTime;

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
        {
            // Check if the user switched view modes while playing
            if (Globals.viewMode == Globals.ViewMode.Chart)
            {
                if (selectedBeforePlay[0].GetType().IsSubclassOf(typeof(ChartObject)))
                    currentSelectedObjects = selectedBeforePlay;
            }
            else
            {
                if (!selectedBeforePlay[0].GetType().IsSubclassOf(typeof(ChartObject)))
                    currentSelectedObjects = selectedBeforePlay;
            }
        }

        selectedBeforePlay = new SongObject[0];

        GameSettings.bot = true;
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
        while (currentSong.isSaving)
            yield return null;

        if (SaveErrorCheck())
        {
            yield break;
        }

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

        Song newSong = null;

        System.Threading.Thread songLoadThread = new System.Threading.Thread(() =>
        {
            mid = (System.IO.Path.GetExtension(currentFileName) == ".mid");

            try
            {
                if (mid)
                    newSong = MidReader.ReadMidi(currentFileName);
                else
                    newSong = ChartReader.ReadChart(currentFileName);
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
        songLoadThread.Priority = System.Threading.ThreadPriority.Highest;
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

        // Free the previous audio clips
        FreeAudio();
        newSong.LoadAllAudioClips();

        while (newSong.isAudioLoading)
            yield return null;

#if TIMING_DEBUG
        Debug.Log("All audio files load time: " + (Time.realtimeSinceStartup - time));
#endif
        yield return null;

        isDirty = false;

#if TIMING_DEBUG
        Debug.Log("File load time: " + (Time.realtimeSinceStartup - totalLoadTime));
#endif

        // Wait for audio to fully load
        while (newSong.isAudioLoading)
            yield return null;

        if (mid)
        {
            currentFileName = string.Empty;
            isDirty = true;
            Debug.Log("Loaded mid file");
        }

        if (recordLastLoaded && currentFileName != string.Empty && !mid)
            lastLoadedFile = System.IO.Path.GetFullPath(currentFileName);
        else
            lastLoadedFile = string.Empty;
        currentSong = newSong;

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

        while (currentSong.isSaving)
            yield return null;

        if (SaveErrorCheck())
        {
            yield break;
        }

        Song backup = currentSong;

        try
        {
            currentFileName = FileExplorer.OpenFilePanel("Chart files (*.chart, *.mid)\0*.chart;*.mid", "chart,mid");
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

    void LoadSong(Song song, bool awake = false)
    {
        if (lastLoadedFile != string.Empty)
            isDirty = false;

        if (awake)
        {
            MenuBar.currentInstrument = Song.Instrument.Guitar;
            MenuBar.currentDifficulty = Song.Difficulty.Expert;
        }
        else
        {
            menuBar.SetInstrument("guitar");
            menuBar.SetDifficulty("expert");
        }

        // Load the default chart
        LoadChart(currentSong.GetChart(MenuBar.currentInstrument, MenuBar.currentDifficulty));
#if !BASS_AUDIO
        // Reset audioSources upon successfull load
        foreach (AudioSource source in musicSources)
            source.clip = null;

        // Load audio
        if (currentSong.musicStream != null)
#else
        if (currentSong.GetBassAudioStream(Song.AudioInstrument.Song) != 0)
#endif
        {
            movement.SetPosition(0);
        }

        menuBar.LoadCurrentInstumentAndDifficulty();
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

    public void FreeAudio()
    {
        foreach(SampleData sampleData in currentSong.GetSampleData())
        {
            sampleData.Free();
        }
#if !BASS_AUDIO
        currentSong.FreeAudioClips();
#else
        currentSong.FreeBassAudioStreams();
#endif
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
                int pos = SongObjectHelper.FindClosestPosition(songObject, selectedObjectsList.ToArray());
                if (pos != SongObjectHelper.NOTFOUND)
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

    public void AddOrRemoveSelectedObjects(System.Collections.Generic.IEnumerable<SongObject> songObjects)
    {
        var selectedObjectsList = new System.Collections.Generic.List<SongObject>(currentSelectedObjects);

        foreach (SongObject songObject in songObjects)
        {
            if (!selectedObjectsList.Contains(songObject))
            {
                AddToSelectedObjects(songObject);
            }
            else
            {
                RemoveFromSelectedObjects(songObject);
            }
        }
    }

    public bool IsSelected(SongObject songObject)
    {
        return (SongObjectHelper.FindObjectPosition(songObject, currentSelectedObjects) != SongObjectHelper.NOTFOUND);
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
            bottomLeft = new Vector2((float)left, currentSong.TickToWorldYPosition(songObjectsCopy[0].tick));
            upperRight = new Vector2((float)right, currentSong.TickToWorldYPosition(songObjectsCopy[songObjectsCopy.Length - 1].tick));
            area = new Clipboard.SelectionArea(bottomLeft, upperRight, songObjectsCopy[0].tick, songObjectsCopy[songObjectsCopy.Length - 1].tick);
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
                uint newValue = (uint)Mathf.Round((float)(TickFunctions.DisToBpm(bpmToAdjust.tick, anchorBPM.tick, deltaTime, currentSong.resolution) * 1000.0d));

                if (deltaTime > 0 && newValue > 0)
                {
                    if (bpmToAdjust.value != newValue)
                    {
                        BPM original = new BPM(bpmToAdjust);
                        bpmToAdjust.value = newValue;
                        anchorBPM.assignedTime = currentSong.LiveTickToTime(anchorBPM.tick, currentSong.resolution);

                        record.Add(new ActionHistory.Modify(original, bpmToAdjust));
                    }
                }
            }
        }

        return record;
    }
}
