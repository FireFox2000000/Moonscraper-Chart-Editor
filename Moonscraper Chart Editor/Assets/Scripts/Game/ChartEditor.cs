// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

#define TIMING_DEBUG
//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class ChartEditor : UnitySingleton<ChartEditor> {
    protected override bool WantDontDestroyOnLoad { get { return false; } }

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
    public LaneInfo laneInfo;
    [SerializeField]
    TextAsset versionNumber;

    uint _minPos;
    uint _maxPos;
    public uint minPos { get { return _minPos; } }
    public uint maxPos { get { return _maxPos; } }

    [HideInInspector]
    public InputManager inputManager;

    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    public Chart.GameMode currentGameMode { get { return currentChart.gameMode; } }
    string currentFileName = string.Empty;

    [HideInInspector]
    public MovementController movement;

    SongObjectPoolManager _songObjectPoolManager;
    public SongObjectPoolManager songObjectPoolManager { get { return _songObjectPoolManager; } }

    string lastLoadedFile = string.Empty;
    public WindowHandleManager windowHandleManager { get; private set; }
    [HideInInspector]
    public ErrorManager errorManager { get; private set; }
    public static bool hasFocus { get { return Application.isFocused; } }

    public CommandStack commandStack;

    public enum State
    {
        Editor,
        Playing,
        Menu,
        Loading,
    }
    public StateMachine applicationStateMachine = new StateMachine();
    SystemManagerState editorState = new SystemManagerState();
    SystemManagerState playingState = new SystemManagerState();
    SystemManagerState menuState = new SystemManagerState();
    SystemManagerState loadingState = new SystemManagerState();
    public State currentState { get; private set; }

    struct UndoRedoSnapInfo
    {
        public uint tick;
        public Globals.ViewMode viewMode;
    }

    UndoRedoSnapInfo? undoRedoSnapInfo = null;
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
    public delegate void OnClickEventFn();
    public System.Collections.Generic.List<OnClickEventFn> onClickEventFnList = new System.Collections.Generic.List<OnClickEventFn>();

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

    public Services services { get { return globals.services; } }
    public UIServices uiServices { get { return services.uiServices; } }

    // Use this for initialization
    void Awake () {
        Debug.Log("Initialising " + versionNumber.text);

        _songObjectPoolManager = GetComponent<SongObjectPoolManager>();

        _minPos = 0;
        _maxPos = 0;

        // Create a default song
        currentSong = new Song();
        LoadSong(currentSong, true);

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();

        isDirty = false;

        inputManager = gameObject.AddComponent<InputManager>();
        gameObject.AddComponent<UITabbing>();

        windowHandleManager = new WindowHandleManager(versionNumber.text, GetComponent<Settings>().productName);
        errorManager = gameObject.AddComponent<ErrorManager>();

        RegisterSystems();
        ChangeState(State.Editor);
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

    public void Update()
    {
        foreach(var onClickFunction in onClickEventFnList)
        {
            onClickFunction();
        }
        onClickEventFnList.Clear();

        // Update object positions that supposed to be visible into the range of the camera
        _minPos = currentSong.WorldYPositionToTick(camYMin.position.y);
        _maxPos = currentSong.WorldYPositionToTick(camYMax.position.y);

        // Set window text to represent if the current song has been saved or not
        windowHandleManager.UpdateDirtyNotification(isDirty);

        applicationStateMachine.Update();
    }

#if UNITY_EDITOR
    bool allowedToQuit = true;        // Won't be save checking if in editor
#else
    bool allowedToQuit = false;
#endif

    void OnApplicationFocus(bool hasFocus)
    {
        windowHandleManager.OnApplicationFocus(hasFocus);
    }

    void OnApplicationQuit()
    {
        StartCoroutine(CheckForUnsavedChangesQuit());

        if (allowedToQuit)
        {
            EventsManager.ClearAll();
            globals.Quit();
            FreeAudio();
            AudioManager.Dispose();

            while (currentSong.isSaving) ;
        }
        // Can't run edit check here because quitting seems to run in a seperate thread
        else
        {
            Application.CancelQuit();
        }
    }

    IEnumerator CheckForUnsavedChangesQuit()
    {
        yield return null;

        if (EditCheck())
        {
            allowedToQuit = true;
            Application.Quit();
        }       
    }

    bool EditCheck()
    {    
        // Check for unsaved changes
        if (isDirty)
        {
            NativeMessageBox.Result result = NativeMessageBox.Show("Do you want to save unsaved changes?", "Warning", NativeMessageBox.Type.YesNoCancel);

            if (result == NativeMessageBox.Result.Yes)
            {
                if (!_Save())
                {
                    return false;
                }
            }
            else if (result == NativeMessageBox.Result.Cancel)
            {
                return false;
            }
        }

        return true;
    }

    public void EnableMenu(DisplayMenu menu)
    {
        menu.gameObject.SetActive(true);
    }

    void RegisterSystems()
    {
        RegisterSystemStateSystem(State.Editor, new AutoSaveSystem());
    }

    #region State Control

    SystemManagerState GetStateForEnum(State state)
    {
        switch (state)
        {
            case State.Editor: return editorState;
            case State.Playing: return playingState;
            case State.Menu: return menuState;
            case State.Loading: return loadingState;
            default: break;
        }

        return null;
    }

    public void ChangeState(State state)
    {
        var newState = GetStateForEnum(state);
        if (newState != null)
        {
            applicationStateMachine.currentState = GetStateForEnum(state);
            currentState = state;

            EventsManager.FireEditorStateChangedEvent();
        }
        else
        {
            Debug.LogError("Unable to change to state " + state.ToString() + ". State either not handled or is a null state, which is not valid for Moonscraper.");
        }
    }

    public void RegisterSystemStateSystem(State state, SystemManagerState.System system)
    {
        SystemManagerState systemState = GetStateForEnum(state);
        if (systemState != null)
        {
            systemState.AddSystem(system);
        }
        else
        {
            Debug.LogError("Unable to register system for state provided.");
        }
    }

    #endregion

    #region Chart Loading/Saving
    public void New()
    {
        if (!EditCheck())
            return;

        while (currentSong.isSaving);

        if (errorManager.HasErrorToDisplay())
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
        onClickEventFnList.Add(LoadQueued);
    }

    void LoadQueued()
    {
        Stop();

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
        onClickEventFnList.Add(SaveAsQueued);
    }

    void SaveAsQueued()
    {
        _SaveAs(true);
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
        string defaultFileName;

        if (lastLoadedFile != string.Empty)
            defaultFileName = System.IO.Path.GetFileNameWithoutExtension(lastLoadedFile);
        else
            defaultFileName = new String(currentSong.name.ToCharArray());

        if (!forced)
            defaultFileName += "(UNFORCED)";

        string fileName;
        if (FileExplorer.SaveFilePanel(new ExtensionFilter("Chart files", "chart"), defaultFileName, "chart", out fileName))
        {
            ExportOptions exportOptions = currentSong.defaultExportOptions;
            exportOptions.forced = forced;

            Save(fileName, exportOptions);
            return true;
        }

        // User canceled
        return false;
    }

    void Save (string filename, ExportOptions exportOptions)
    {
        if (currentSong != null && !currentSong.isSaving)
        {
            Debug.Log("Saving to file- " + System.IO.Path.GetFullPath(filename));
          
            currentSong.SaveAsync(filename, exportOptions);
            lastLoadedFile = System.IO.Path.GetFullPath(filename);

            if (currentSong.isSaving)
                EventsManager.FireSaveEvent();

            isDirty = false;
        }
    }

    public IEnumerator _Load(string currentFileName, bool recordLastLoaded = true)
    {
        LoadingTasksManager tasksManager = services.loadingTasksManager;

        bool error = false;
        Song backup = currentSong;
#if TIMING_DEBUG
        float totalLoadTime = Time.realtimeSinceStartup;
#endif
        bool mid = false;

        Song newSong = null;
        MidReader.CallbackState midiCallbackState = MidReader.CallbackState.None;

        List<LoadingTask> tasks = new List<LoadingTask>()
        {
            new LoadingTask("Loading file", () =>
            {
                // Wait for saving to complete just in case
                while (currentSong.isSaving){ }

                if (errorManager.HasErrorToDisplay())
                {
                    error = true;
                    return;
                }

                mid = System.IO.Path.GetExtension(currentFileName) == ".mid";

                try
                {
                    if (mid)
                        newSong = MidReader.ReadMidi(currentFileName, ref midiCallbackState);
                    else
                        newSong = ChartReader.ReadChart(currentFileName);
                }
                catch (Exception e)
                {
                    currentSong = backup;

                    if (mid)
                        errorManager.QueueErrorMessage(Logger.LogException(e, "Failed to open mid file"));
                    else
                        errorManager.QueueErrorMessage(Logger.LogException(e, "Failed to open chart file"));

                    error = true;
                }
            }),

            new LoadingTask("Loading audio", () =>
            {
                if (error)
                    return;

                // Free the previous audio clips
                FreeAudio();

                newSong.LoadAllAudioClips();
            }),
        };

        tasksManager.KickTasks(tasks);

        while (tasksManager.isRunningTask)
        {
            while (midiCallbackState == MidReader.CallbackState.WaitingForExternalInformation)
            {
                // Halt main thread until message box is complete
            }
            yield return null;
        }

        // Tasks have finished
        if (error)
            yield break;    // Immediate exit

        isDirty = false;

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
    }

    IEnumerator _Load()
    {
        if (!EditCheck())
            yield break;

        while (currentSong.isSaving)
            yield return null;

        if (errorManager.HasErrorToDisplay())
        {
            yield break;
        }

        Song backup = currentSong;

        if (!FileExplorer.OpenFilePanel(new ExtensionFilter("Chart files", "chart", "mid"), "chart,mid", out currentFileName))
        {
            currentSong = backup;

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

        if (AudioManager.StreamIsValid(currentSong.GetAudioStream(Song.AudioInstrument.Song)))
        {
            movement.SetPosition(0);
        }

        menuBar.LoadCurrentInstumentAndDifficulty();
    }

    // Chart should be part of the current song
    public void LoadChart(Chart chart)
    {
        commandStack = new CommandStack();
        Stop();

        currentChart = chart;

        songObjectPoolManager.NewChartReset();
    }

    #endregion

    #region Audio Functions
    void PlayAudio(float playPoint)
    {
        services.strikelineAudio.startYPoint = visibleStrikeline.transform.position.y;

        SetBassStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Song), GameSettings.gameSpeed, GameSettings.vol_song);
        SetBassStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Guitar), GameSettings.gameSpeed, GameSettings.vol_guitar);
        SetBassStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Bass), GameSettings.gameSpeed, GameSettings.vol_bass);
        SetBassStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Rhythm), GameSettings.gameSpeed, GameSettings.vol_rhythm);
        SetBassStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Drum), GameSettings.gameSpeed, GameSettings.vol_drum);

        foreach (var bassStream in currentSong.bassAudioStreams)
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
        foreach (var bassStream in currentSong.bassAudioStreams)
        {
            if (AudioManager.StreamIsValid(bassStream))
                bassStream.Stop();
        }

        movement.playStartPosition = null;
        movement.playStartTime = null;
    }

    void PlayBassStream(AudioStream audioStream, float playPoint)
    {
        if (audioStream != null && audioStream.isValid)
        {
            audioStream.Play(playPoint);
            MovementController.timeSync.SongTime = playPoint;
        }
    }

    void SetBassStreamProperties(TempoStream stream, float speed, float vol)
    {
        if (AudioManager.StreamIsValid(stream))
        {
            // Reset
            stream.frequency = 0;
            stream.tempoPitch = 0;
            stream.tempo = 0;

            stream.volume = vol * GameSettings.vol_master;
            stream.pan = GameSettings.audio_pan;

            if (speed < 1)
            {
                float originalFreq = stream.frequency;

                float freq = originalFreq * speed;
                if (freq < 100)
                    freq = 100;
                else if (freq > 100000)
                    freq = 100000;

                stream.frequency = freq;
#if false
                // Pitch shifting equation
                Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, Mathf.Log(1.0f / speed, Mathf.Pow(2, 1.0f / 12.0f)));
#endif
            }
            else
            {
                stream.tempo = speed * 100 - 100;
            }
        }
    }

    public void FreeAudio()
    {
        currentSong.FreeAudioStreams();
    }

    #endregion

    #region Pause/Play Functions
    public static float? startGameplayPos = null;
    public void StartGameplay()
    {
        if (currentState == State.Playing ||
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

    bool cancel;
    List<SongObject> selectedBeforePlay = new List<SongObject>();
    public void Play()
    {
        selectedBeforePlay.Clear();
        selectedBeforePlay.AddRange(currentSelectedObjects);
        currentSelectedObject = null;

        if (GameSettings.bot && GameSettings.resetAfterPlay)
            stopResetPos = movement.transform.position;

        foreach (HitAnimation hitAnim in indicators.animations)
            hitAnim.StopAnim();

        ChangeState(State.Playing);
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

        if (!cancel && currentState == State.Playing)
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

        ChangeState(State.Editor);

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

        if (selectedBeforePlay.Count > 0)
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

        selectedBeforePlay.Clear();

        GameSettings.bot = true;
        stopResetPos = null;
    }
    #endregion

    #region Selected Objects Management Functions

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
                int pos = SongObjectHelper.FindClosestPosition(songObject, selectedObjectsList);
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

        currentSelectedObjects = selectedObjectsList;
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

        currentSelectedObjects = selectedObjectsList;
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

    public T SelectSongObject<T>(T songObject, IList<T> arrToSearch) where T : SongObject
    {
        int insertionIndex = SongObjectHelper.FindObjectPosition(songObject, arrToSearch);
        Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Failed to find songObject to highlight");
        currentSelectedObject = arrToSearch[insertionIndex];
        return currentSelectedObject as T;
    }

    List<SongObject> foundSongObjects = new List<SongObject>();
    public void TryFindAndSelectSongObjects(IList<SongObject> songObjects)
    {
        Song song = currentSong;
        Chart chart = currentChart;
        foundSongObjects.Clear();

        foreach (SongObject so in songObjects)
        {
            ChartObject chartObject = so as ChartObject;
            SyncTrack syncTrack = so as SyncTrack;
            Event eventObject = so as Event;
            if (chartObject != null)
            {
                int insertionIndex = SongObjectHelper.FindObjectPosition(chartObject, chart.chartObjects);
                if (insertionIndex != SongObjectHelper.NOTFOUND)
                {
                    foundSongObjects.Add(chart.chartObjects[insertionIndex]);
                }
                else
                {
                    Debug.LogWarning("Failed to find chart object to highlight");
                }
            }
            else if (syncTrack != null)
            {
                int insertionIndex = SongObjectHelper.FindObjectPosition(syncTrack, song.syncTrack);
                if (insertionIndex != SongObjectHelper.NOTFOUND)
                {
                    foundSongObjects.Add(song.syncTrack[insertionIndex]);
                }
                else
                {
                    Debug.LogWarning("Failed to find synctrack to highlight");
                }
            }
            else if (eventObject != null)
            {
                int insertionIndex = SongObjectHelper.FindObjectPosition(eventObject, song.eventsAndSections);
                if (insertionIndex != SongObjectHelper.NOTFOUND)
                {
                    foundSongObjects.Add(song.eventsAndSections[insertionIndex]);
                }
                else
                {
                    Debug.LogWarning("Failed to find event to highlight");
                }
            }
            else
            {
                Debug.LogError("Unable to handle object " + so.ToString());
            }
        }

        currentSelectedObjects = foundSongObjects;
        foundSongObjects.Clear();
    }

    #endregion

    #region Undo/Redo/Cut/Copy/Paste etc...

    public void FillUndoRedoSnapInfo(uint tick, Globals.ViewMode viewMode)
    {
        UndoRedoSnapInfo newUndoRedoSnapInfo;
        newUndoRedoSnapInfo.tick = tick;
        newUndoRedoSnapInfo.viewMode = viewMode;

        undoRedoSnapInfo = newUndoRedoSnapInfo;
    }

    void ApplyUndoRedoSnapInfo()
    {
        if (undoRedoSnapInfo.HasValue)
        {
            UndoRedoSnapInfo info = undoRedoSnapInfo.Value;
            Globals.ViewMode viewMode = info.viewMode;
            uint snapTick = info.tick;

            if (Globals.viewMode != viewMode)
            {
                toolPanel.ToggleSongViewMode(viewMode == Globals.ViewMode.Song);
            }

            if (snapTick < minPos || snapTick > maxPos)
                movement.SetPosition(snapTick);
        }
    }

    public void UndoWrapper()
    {
        if (!commandStack.isAtStart)
        {
            undoRedoSnapInfo = null;

            commandStack.Pop();
            groupSelect.reset();

            ApplyUndoRedoSnapInfo();
            undoRedoSnapInfo = null;
        }
    }

    public void RedoWrapper()
    {
        if (!commandStack.isAtEnd)
        {
            undoRedoSnapInfo = null;

            commandStack.Push();
            groupSelect.reset();

            ApplyUndoRedoSnapInfo();
            undoRedoSnapInfo = null;
        }
    }

    public void Copy()
    {
        const float DEFAULT_LEFT = -2;
        const float DEFAULT_RIGHT = 2;

        var songObjectsCopy = new SongObject[currentSelectedObjects.Count];
        float? left = null, right = null;
        float position = 0;

        bool containsNotes = false;

        // Scan through all the current objects to determine width of scanned area
        for (int i = 0; i < currentSelectedObjects.Count; ++i)
        {
            if (!containsNotes && currentSelectedObjects[i].GetType() == typeof(Note))
                containsNotes = true;

            songObjectsCopy[i] = currentSelectedObjects[i].Clone();

            position = SongObjectController.GetXPos(currentSelectedObjects[i]);

            if (left == null || position < left)
                left = position;

            if (right == null || position > right)
                right = position;
        }

        // Default collision size
        if (containsNotes)
        {
            if (left > DEFAULT_LEFT)
                left = DEFAULT_LEFT;
            if (right < DEFAULT_RIGHT)
                right = DEFAULT_RIGHT;
        }

        if (left == null)
            left = DEFAULT_LEFT;
        if (right == null)
            right = DEFAULT_RIGHT;

        Vector2 bottomLeft = Vector2.zero;
        Vector2 upperRight = Vector2.zero;
        var area = new Clipboard.SelectionArea();

        if (currentSelectedObjects.Count > 0)
        {
            bottomLeft = new Vector2((float)left, currentSong.TickToWorldYPosition(songObjectsCopy[0].tick));
            upperRight = new Vector2((float)right, currentSong.TickToWorldYPosition(songObjectsCopy[songObjectsCopy.Length - 1].tick));
            area = new Clipboard.SelectionArea(bottomLeft, upperRight, songObjectsCopy[0].tick, songObjectsCopy[songObjectsCopy.Length - 1].tick);
        }        

        ClipboardObjectController.SetData(songObjectsCopy, area, currentSong);
    }

    public void Delete()
    {
        if (currentSelectedObjects.Count > 0)
        {
            SongEditCommand[] commands = new SongEditCommand[]
            {
                new SongEditDelete(currentSelectedObjects),

            };

            BatchedSongEditCommand commandBatch = new BatchedSongEditCommand(commands);
            commandStack.Push(commandBatch);

            currentSelectedObject = null;

            groupSelect.reset();
        }
    }

    public void Cut()
    {
        Copy();
        Delete();
    }

    #endregion

    #region Selected object management

    public SongObject currentSelectedObject
    {
        get
        {
            if (currentSelectedObjects.Count == 1)
                return currentSelectedObjects[0];
            else
                return null;
        }
        set
        {
            currentSelectedObjects.Clear();
            if (value != null)
            {
                currentSelectedObjects.Add(value);
            }

            timeHandler.RefreshHighlightIndicator();
        }
    }


    List<SongObject> m_currentSelectedObjects = new List<SongObject>();
    public IList<SongObject> currentSelectedObjects
    {
        get
        {
            return m_currentSelectedObjects;
        }
        set
        {
            SetCurrentSelectedObjects(value);
        }
    }

    public void SetCurrentSelectedObjects<T>(IEnumerable<T> list) where T : SongObject
    {
        m_currentSelectedObjects.Clear();

        foreach (T so in list)
        {
            m_currentSelectedObjects.Add(so);
        }

        timeHandler.RefreshHighlightIndicator();
    }

    public void SetCurrentSelectedObjects<T>(IList<T> list, int index, int length) where T : SongObject
    {
        m_currentSelectedObjects.Clear();
        for (int i = index; i < index + length; ++i)
        {
            m_currentSelectedObjects.Add(list[i]);
        }
        timeHandler.RefreshHighlightIndicator();
    }

    #endregion
}
