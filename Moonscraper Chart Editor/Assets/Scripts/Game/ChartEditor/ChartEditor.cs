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

[RequireComponent(typeof(ChartEditorAssets))]
[UnitySingleton(UnitySingletonAttribute.Type.ExistsInScene, true)]
public class ChartEditor : UnitySingleton<ChartEditor>
{
    public static bool isDirty = false;
    public EditorObjectToolManager toolManager;
    public EditorInteractionManager interactionMethodManager;

    [Header("Tool prefabs")]
    public GroupMove groupMove;
    [Header("Misc.")]
    public Transform visibleStrikeline;
    public TimelineHandler timeHandler;
    public Transform camYMin;
    public Transform camYMax;
    public Transform mouseYMaxLimit;
    public Transform mouseYMinLimit;
    public Indicators indicators;               // Cancels hit animations upon stopping playback   
    public GroupSelect groupSelect;
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

    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    public Chart.GameMode currentGameMode { get { return currentChart.gameMode; } }
    string currentFileName = string.Empty;

    [HideInInspector]
    public MovementController movement;
    [HideInInspector]
    public ChartEditorAssets assets;

    SongObjectPoolManager _songObjectPoolManager;
    public SongObjectPoolManager songObjectPoolManager { get { return _songObjectPoolManager; } }

    string lastLoadedFile = string.Empty;
    public WindowHandleManager windowHandleManager { get; private set; }
    [HideInInspector]
    public ErrorManager errorManager { get; private set; }
    public static bool hasFocus { get { return Application.isFocused; } }

    public SelectedObjectsManager selectedObjectsManager;
    public CommandStack commandStack;

    public enum State
    {
        Editor,
        Playing,
        Menu,
        Loading,
    }
    public StateMachine applicationStateMachine = new StateMachine();
    SystemManagerState menuState = new SystemManagerState();
    SystemManagerState loadingState = new SystemManagerState();
    public State currentState { get; private set; }

    Dictionary<State, List<SystemManagerState.ISystem>> persistentSystemsForStates = new Dictionary<State, List<SystemManagerState.ISystem>>();

    static readonly Dictionary<string, LoadedStreamStore.StreamConfig> soundMapConfig = new Dictionary<string, LoadedStreamStore.StreamConfig>(){
            { SkinKeys.metronome,   new LoadedStreamStore.StreamConfig(System.IO.Path.Combine(Application.streamingAssetsPath, "SFX/metronome.wav")) },
            { SkinKeys.clap,        new LoadedStreamStore.StreamConfig(System.IO.Path.Combine(Application.streamingAssetsPath, "SFX/clap.wav")) },
            { SkinKeys.break0,      new LoadedStreamStore.StreamConfig(System.IO.Path.Combine(Application.streamingAssetsPath, "SFX/combobreak.wav")) },
        };
    public LoadedStreamStore sfxAudioStreams { get; private set; }
    public ChartEditorEvents events = new ChartEditorEvents();

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

    public delegate void OnClickEventFn();
    public System.Collections.Generic.List<OnClickEventFn> onClickEventFnList = new System.Collections.Generic.List<OnClickEventFn>();

    public float currentVisibleTime
    {
        get
        {
            return TickFunctions.WorldYPositionToTime(visibleStrikeline.transform.position.y);
        }
    }

    public Services services { get { return globals.services; } }
    public UIServices uiServices { get { return services.uiServices; } }

    // Use this for initialization
    void Awake () {
        Debug.Log("Initialising " + versionNumber.text);
        assets = GetComponent<ChartEditorAssets>();
        selectedObjectsManager = new SelectedObjectsManager(this);
        sfxAudioStreams = new LoadedStreamStore(soundMapConfig);

        _minPos = 0;
        _maxPos = 0;

        RegisterSystems();

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();

        isDirty = false;

        gameObject.AddComponent<UITabbing>();

        windowHandleManager = new WindowHandleManager(versionNumber.text, GetComponent<Settings>().productName);
        errorManager = gameObject.AddComponent<ErrorManager>();
        toolManager.Init();
        interactionMethodManager.Init();

        events.chartReloadedEvent.Register(OnChartReloaded);
    }

    IEnumerator Start()
    {
        sfxAudioStreams.LoadSounds(SkinManager.Instance.currentSkin);

        ChangeState(State.Editor);

        // Create a default song
        currentSong = new Song();
        LoadSong(currentSong, true);

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
            globals.Quit();
            FreeAudio();
            sfxAudioStreams.DisposeSounds();
            AudioManager.Dispose();

            while (currentSong.isSaving) ;

            applicationStateMachine.currentState = null; // Force call exit on current state;
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
            NativeMessageBox.Result result = NativeMessageBox.Show("Do you want to save unsaved changes?", "Warning", NativeMessageBox.Type.YesNoCancel, windowHandleManager.nativeWindow);

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
        _songObjectPoolManager = gameObject.AddComponent<SongObjectPoolManager>();
        DrawBeatLines drawBeatLinesSystem = new DrawBeatLines();

        RegisterPersistentSystem(State.Editor, new AutoSaveSystem());
        RegisterPersistentSystem(State.Editor, _songObjectPoolManager);
        RegisterPersistentSystem(State.Editor, drawBeatLinesSystem);
        RegisterPersistentSystem(State.Editor, new HoverHighlightDisplaySystem(assets));
        RegisterPersistentSystem(State.Editor, new SelectedHighlightDisplaySystem());

        RegisterPersistentSystem(State.Playing, _songObjectPoolManager);
        RegisterPersistentSystem(State.Playing, drawBeatLinesSystem);
    }

    #region State Control

    SystemManagerState GetStateForEnum(State state)
    {
        SystemManagerState newState = null;
        switch (state)
        {
            case State.Editor: newState = new EditorState(); break;
            case State.Playing: Debug.LogError("Attempting to change state to a default Playing State. This is not allowed."); return null; // call from Play function in this editor instead
            case State.Menu: return menuState;
            case State.Loading: return loadingState;
            default: break;
        }

        PopulatePersistentSystemsForNewState(state, newState);

        return newState;
    }

    void PopulatePersistentSystemsForNewState(State state, SystemManagerState newState)
    {
        List<SystemManagerState.ISystem> persistentSystems;
        if (persistentSystemsForStates.TryGetValue(state, out persistentSystems))
            newState.AddSystems(persistentSystemsForStates[state]);
    }

    public void ChangeStateToEditor()
    {
        ChangeState(State.Editor);
    }

    public void ChangeStateToMenu()
    {
        ChangeState(State.Menu);
    }

    public void ChangeStateToLoading()
    {
        ChangeState(State.Loading);
    }

    // Playing state must be called internally

    void ChangeState(State state)
    {
        var newState = GetStateForEnum(state);
        ChangeState(state, newState);
    }

    void ChangeState(State state, StateMachine.IState newState)
    {
        if (newState != null)
        {
            applicationStateMachine.currentState = newState;
            currentState = state;

            events.editorStateChangedEvent.Fire(currentState);
        }
        else
        {
            Debug.LogError("Unable to change to state " + state.ToString() + ". State either not handled or is a null state, which is not valid for Moonscraper.");
        }
    }

    public void RegisterPersistentSystem(State state, SystemManagerState.ISystem system)
    {
        List<SystemManagerState.ISystem> persistentSystems;
        if (!persistentSystemsForStates.TryGetValue(state, out persistentSystems))
        {
            persistentSystems = new List<SystemManagerState.ISystem>();
            persistentSystemsForStates.Add(state, persistentSystems);
        }
        persistentSystems.Add(system);
    }

    public void ChangeInteractionMethod(EditorInteractionManager.InteractionType type)
    {
        interactionMethodManager.ChangeInteraction(type);
    }

    #endregion

    void OnChartReloaded()
    {
        RepaintWindowText();
    }

    public void RepaintWindowText()
    {
        const int SONGNAME_MAX_CHAR = 35;
        string songName = currentSong.name;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(currentSong.name);
        if (sb.Length <= 0)
        {
            sb.Append("Untitled");
        }

        if (sb.Length > SONGNAME_MAX_CHAR)
        {
            const string Ellipsis = "...";
            int charsToRemove = sb.Length - SONGNAME_MAX_CHAR + Ellipsis.Length;
            sb.Remove(sb.Length - charsToRemove, charsToRemove);
            sb.Append(Ellipsis);
        }

        windowHandleManager.SetProjectNameStr(sb.ToString()); 

        sb.Clear();
        sb.Append(currentChart.name);
        windowHandleManager.SetProjectStateStr(sb.ToString());
    }

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

        selectedObjectsManager.currentSelectedObject = null;
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
                events.saveEvent.Fire();

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

        selectedObjectsManager.currentSelectedObject = null;
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

        interactionMethodManager.ChangeInteraction(EditorInteractionManager.InteractionType.HighwayObjectEdit);

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
    public void PlayAudio(float playPoint)
    {
        SetStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Song), GameSettings.gameSpeed, GameSettings.vol_song);
        SetStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Guitar), GameSettings.gameSpeed, GameSettings.vol_guitar);
        SetStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Bass), GameSettings.gameSpeed, GameSettings.vol_bass);
        SetStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Rhythm), GameSettings.gameSpeed, GameSettings.vol_rhythm);
        SetStreamProperties(currentSong.GetAudioStream(Song.AudioInstrument.Drum), GameSettings.gameSpeed, GameSettings.vol_drum);

        AudioStream primaryStream = null;
        foreach (var bassStream in currentSong.bassAudioStreams)
        {
            if (primaryStream != null)
            {
                playPoint = primaryStream.CurrentPositionInSeconds();
            }

            if (primaryStream != null)
            {
                PlayStream(bassStream, primaryStream);
            }
            else if (PlayStream(bassStream, playPoint))
            {
                primaryStream = bassStream;
            }
        }
    }

   public void StopAudio()
    {
        foreach (var bassStream in currentSong.bassAudioStreams)
        {
            if (AudioManager.StreamIsValid(bassStream))
                bassStream.Stop();
        }
    }

    bool PlayStream(AudioStream audioStream, float playPoint)
    {
        if (audioStream != null && audioStream.isValid)
        {
            audioStream.Play(playPoint);
            Debug.Log("Playing stream at " + playPoint);

            return true;
        }

        return false;
    }

    bool PlayStream(AudioStream audioStream, AudioStream syncStream)
    {
        if (audioStream != null && audioStream.isValid)
        {
            audioStream.Play(syncStream.CurrentPositionInSeconds());
            Debug.Log("Playing stream at " + syncStream.CurrentPositionInSeconds());

            return true;
        }

        return false;
    }

    void SetStreamProperties(TempoStream stream, float speed, float vol)
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
    public void StartGameplay()
    {
        if (currentState == State.Playing ||
            movement.transform.position.y < movement.initPos.y ||
            Globals.ghLiveMode)
            return;

        float strikelineYPos = visibleStrikeline.position.y;
        float? stopResetTime = null;

        songObjectPoolManager.noteVisibilityRangeYPosOverride = strikelineYPos;

        if (GameSettings.resetAfterGameplay)
            stopResetTime = currentVisibleTime;

        // Set position x seconds beforehand
        float startTime = TickFunctions.WorldYPositionToTime(strikelineYPos) - GameSettings.gameplayStartDelayTime - (0.01f * GameSettings.hyperspeed); // Offset to prevent errors where it removes a note that is on the strikeline
        movement.SetTime(startTime);

        GameSettings.bot = false;

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

        foreach (HitAnimation hitAnim in indicators.animations)
            hitAnim.StopAnim();

        SystemManagerState playingState = new PlayingState(startTime, stopResetTime);
        PopulatePersistentSystemsForNewState(State.Playing, playingState);
        ChangeState(State.Playing, playingState);
    }

    public void Play()
    {
        float? stopResetTime = null;

        if (GameSettings.bot && GameSettings.resetAfterPlay)
        {
            stopResetTime = currentVisibleTime;
        }

        foreach (HitAnimation hitAnim in indicators.animations)
            hitAnim.StopAnim();

        SystemManagerState playingState = new PlayingState(currentVisibleTime, stopResetTime);
        PopulatePersistentSystemsForNewState(State.Playing, playingState);
        ChangeState(State.Playing, playingState);
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
        {
            foreach (HitAnimation hitAnim in indicators.animations)
            {
                if (hitAnim)
                    hitAnim.StopAnim();
            }
        }

        ChangeState(State.Editor);

        if (currentChart != null)
        {
            foreach (Note note in currentChart.notes)
            {
                if (note.controller)
                    note.controller.Activate();
            }
        }

        GameSettings.bot = true;
        songObjectPoolManager.noteVisibilityRangeYPosOverride = null;
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
                globals.ToggleSongViewMode(viewMode == Globals.ViewMode.Song);
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

        var songObjectsCopy = new SongObject[selectedObjectsManager.currentSelectedObjects.Count];
        float? left = null, right = null;
        float position = 0;

        bool containsNotes = false;

        // Scan through all the current objects to determine width of scanned area
        for (int i = 0; i < selectedObjectsManager.currentSelectedObjects.Count; ++i)
        {
            if (!containsNotes && selectedObjectsManager.currentSelectedObjects[i].GetType() == typeof(Note))
                containsNotes = true;

            songObjectsCopy[i] = selectedObjectsManager.currentSelectedObjects[i].Clone();

            position = SongObjectController.GetXPos(selectedObjectsManager.currentSelectedObjects[i]);

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

        if (selectedObjectsManager.currentSelectedObjects.Count > 0)
        {
            bottomLeft = new Vector2((float)left, currentSong.TickToWorldYPosition(songObjectsCopy[0].tick));
            upperRight = new Vector2((float)right, currentSong.TickToWorldYPosition(songObjectsCopy[songObjectsCopy.Length - 1].tick));
            area = new Clipboard.SelectionArea(bottomLeft, upperRight, songObjectsCopy[0].tick, songObjectsCopy[songObjectsCopy.Length - 1].tick);
        }        

        ClipboardObjectController.SetData(songObjectsCopy, area, currentSong);
    }

    public void Delete()
    {
        if (selectedObjectsManager.currentSelectedObjects.Count > 0)
        {
            SongEditCommand[] commands = new SongEditCommand[]
            {
                new SongEditDelete(selectedObjectsManager.currentSelectedObjects),

            };

            BatchedSongEditCommand commandBatch = new BatchedSongEditCommand(commands);
            commandStack.Push(commandBatch);

            selectedObjectsManager.currentSelectedObject = null;

            groupSelect.reset();
        }
    }

    public void Cut()
    {
        Copy();
        Delete();
    }

    #endregion
}
