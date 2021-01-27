// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

#define TIMING_DEBUG
//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;
using MoonscraperChartEditor.Song.IO;

/// <summary>
/// The central point of the entire editor. Container for all the data and systems nessacary for the editor to function.
/// Initialises all the systems nessacary for the editor to function, and manages the current state of the application.
/// </summary>
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
    public LaneInfo laneInfo;

    uint _minPos;
    uint _maxPos;
    public uint minPos { get { return _minPos; } }
    public uint maxPos { get { return _maxPos; } }

    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    public Chart.GameMode currentGameMode { get { return currentChart.gameMode; } }
    public Song.Instrument currentInstrument { get { return MenuBar.currentInstrument; } }
    public Song.Difficulty currentDifficulty { get { return MenuBar.currentDifficulty; } }
    public SongAudioManager currentSongAudio { get; private set; }
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

    /// <summary>
    /// State machine for the entire application. 
    /// Editor and Play states are instanciated as we need them.
    /// </summary>
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
    List<Action> interruptTasks = new List<Action>();

    /// <summary>
    /// Persistent systems are systems that need to hold onto data between different states, rather than creating a new instance and destroying it every time
    /// An example of this is the object pooling system, needs to hold onto those objects for the entire lifetime of the scene as objects always need to be visible. Would also be dumb memory-wise.
    /// See #region State Control for full implementation
    /// </summary>
    Dictionary<State, List<SystemManagerState.ISystem>> persistentSystemsForStates = new Dictionary<State, List<SystemManagerState.ISystem>>();     

    static readonly Dictionary<string, LoadedStreamStore.StreamConfig> soundMapConfig = new Dictionary<string, LoadedStreamStore.StreamConfig>(){
            { SkinKeys.metronome,   new LoadedStreamStore.StreamConfig(System.IO.Path.Combine(Application.streamingAssetsPath, "SFX/metronome.wav")) },
            { SkinKeys.clap,        new LoadedStreamStore.StreamConfig(System.IO.Path.Combine(Application.streamingAssetsPath, "SFX/clap.wav")) },
            { SkinKeys.break0,      new LoadedStreamStore.StreamConfig(System.IO.Path.Combine(Application.streamingAssetsPath, "SFX/combobreak.wav")) },
        };
    public LoadedStreamStore sfxAudioStreams { get; private set; }
    public ChartEditorEvents events = new ChartEditorEvents();
    public GameplayEvents gameplayEvents = new GameplayEvents();

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
            return WorldYPositionToTime(visibleStrikeline.transform.position.y);
        }
    }

    public Services services { get { return globals.services; } }
    public UIServices uiServices { get { return services.uiServices; } }

    Task _saveTask;

    [HideInInspector]
    public ChartEditorSessionFlags sessionFlags = ChartEditorSessionFlags.None;

    // Use this for initialization
    void Awake () {
        Debug.Log(string.Format("Initialising {0} v{1}", Application.productName, Application.version));

#if !UNITY_EDITOR
        Application.wantsToQuit += QuittingEditCheck;
#endif
        Application.quitting += FinaliseQuit;

        currentSongAudio = new SongAudioManager();

        assets = GetComponent<ChartEditorAssets>();
        selectedObjectsManager = new SelectedObjectsManager(this);
        sfxAudioStreams = new LoadedStreamStore(soundMapConfig);

        _minPos = 0;
        _maxPos = 0;

        RegisterSystems();

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();

        isDirty = false;

        gameObject.AddComponent<UITabbing>();

        windowHandleManager = new WindowHandleManager(string.Format("{0} v{1} {2}", Application.productName, Application.version, Globals.applicationBranchName), GetComponent<Settings>().productName);
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

        // Note, need to do this after loading game settings for this to actually take effect
        if (Globals.gameSettings.automaticallyCheckForUpdates)
        {
            services.updateManager.CheckForUpdates((Octokit.Release latestRelease) => {
                if (latestRelease != null)
                {
                    OnUserCheckForUpdatesComplete(latestRelease);
                }
            });
        }
    }

    public void Update()
    {
        if (haltThreadForQuit)
            return;

        if (quittingCheckPassed)
        {
            Application.Quit();
        }

        if (interruptTasks.Count > 0 && (currentState == State.Editor || currentState == State.Playing))      // Don't interrupt loading or if the user is in a menu
        {
            foreach (var task in interruptTasks)
            {
                task();
            }

            interruptTasks.Clear();
        }

        foreach (var onClickFunction in onClickEventFnList)
        {
            onClickFunction();
        }
        onClickEventFnList.Clear();

        // Update object positions that supposed to be visible into the range of the camera
        _minPos = currentSong.WorldYPositionToTick(camYMin.position.y);

        float maxTime = currentSongLength;
        uint maxTick = currentSong.TimeToTick(maxTime, currentSong.resolution);
        _maxPos = (uint)Mathf.Min(maxTick, currentSong.WorldYPositionToTick(camYMax.position.y));

        // Set window text to represent if the current song has been saved or not
        windowHandleManager.UpdateDirtyNotification(isDirty);

        applicationStateMachine.Update();
    }

#if UNITY_EDITOR
    bool allowedToQuit = true;        // Won't be save checking if in editor
#else
    bool allowedToQuit = false;
#endif

    bool queueQuitCheck = false;

    void OnApplicationFocus(bool hasFocus)
    {
        windowHandleManager.OnApplicationFocus(hasFocus);
    }

    void FinaliseQuit()
    {
        globals.Quit();
        currentSongAudio.FreeAudioStreams();
        sfxAudioStreams.DisposeSounds();
        AudioManager.Dispose();

        Debug.Log("Disposing SDL");
        InputManager.Instance.Dispose();

        SDL2.SDL.SDL_Quit();

        while (isSaving) ;

        applicationStateMachine.currentState = null; // Force call exit on current state;
    }

    bool EditCheck()
    {    
        // Check for unsaved changes
        if (isDirty)
        {
            NativeMessageBox.Result result = NativeMessageBox.Show("Do you want to save unsaved changes?", "Warning", NativeMessageBox.Type.YesNoCancel, windowHandleManager.nativeWindow);
            Debug.Log("Message box result = " + result);

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

    volatile bool haltThreadForQuit = false;
    volatile bool quittingCheckPassed = false;
    bool QuittingEditCheck()
    {
        if (currentState == State.Playing)
        {
            ChangeStateToEditor();
        }

        if (quittingCheckPassed)
            return true;

        if (haltThreadForQuit)
            return false;

        haltThreadForQuit = true;

        bool result = EditCheck();

        if (result)
        {
            quittingCheckPassed = true;
        }

        haltThreadForQuit = false;
        return result;
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

    public void QueueInterruptTask(Action task)
    {
        interruptTasks.Add(task);
    }

    public void UserCheckForUpdates()
    {
        ApplicationUpdateManager updateManager = services.updateManager;
        updateManager.CheckForUpdates(OnUserCheckForUpdatesComplete);

        LoadingTask waitForUpdateCheckTask = new LoadingTask("Checking for updates...", () =>
        {
            while (updateManager.UpdateCheckInProgress) ;
        });

        List<LoadingTask> tasks = new List<LoadingTask>() { waitForUpdateCheckTask };

        LoadingTasksManager tasksManager = services.loadingTasksManager;
        tasksManager.KickTasks(tasks);
    }

    void OnUserCheckForUpdatesComplete(Octokit.Release release)
    {
        // Queue up the action to open the menu in a safe spot, editor or playing. Don't want to try to open this up if the user already has a menu open
        QueueInterruptTask(() =>
        {
            UpdatesMenu updatesMenu = uiServices.gameObject.GetComponentInChildren<UpdatesMenu>(true);
            updatesMenu.Populate(release);
            EnableMenu(updatesMenu.gameObject.GetComponent<DisplayMenu>());
        });
    }

    public static float WorldYPositionToTime(float worldYPosition)
    {
        return worldYPosition / (Globals.gameSettings.hyperspeed / Globals.gameSettings.gameSpeed);
    }

    public static float TimeToWorldYPosition(float time)
    {
        return time * Globals.gameSettings.hyperspeed / Globals.gameSettings.gameSpeed;
    }

    public static float WorldYPosition(SongObject songObject)
    {
        return songObject.song.TickToWorldYPosition(songObject.tick);
    }

    public static float WorldYPositionLength(Note note)
    {
        return note.song.TickToWorldYPosition(note.tick + note.length);
    }

    #region State Control

    SystemManagerState GetStateForEnum(State state)
    {
        SystemManagerState newState = null;
        switch (state)
        {
            case State.Editor: newState = new EditorState(); break;
            case State.Playing: Debug.LogError("Attempting to change state to a default Playing State. This is not allowed."); return null; // call from Play function in this editor instead
            case State.Menu: newState = menuState; break;
            case State.Loading: newState = loadingState; break;
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

        while (isSaving);

        if (errorManager.HasErrorToDisplay())
            return;

        lastLoadedFile = string.Empty;
        currentSongAudio.FreeAudioStreams();
        currentSong = new Song();

        sessionFlags &= ~ChartEditorSessionFlags.CurrentChartSavedInProprietyFormat;

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
            var exportOptions = currentSong.defaultExportOptions;
            exportOptions.isGeneralSave = true;

            Save(lastLoadedFile, exportOptions);
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
        else if (currentSong.name != string.Empty)
            defaultFileName = new String(currentSong.name.ToCharArray());
        else
            defaultFileName = "Untitled";

        if (!forced)
            defaultFileName += "(UNFORCED)";

        string fileName;
        if (FileExplorer.SaveFilePanel(new ExtensionFilter("Chart files", "chart"), defaultFileName, "chart", out fileName))
        {
            ExportOptions exportOptions = currentSong.defaultExportOptions;
            exportOptions.forced = forced;
            exportOptions.isGeneralSave = true;

            Save(fileName, exportOptions);
            return true;
        }

        // User canceled
        return false;
    }

    void Save (string filename, ExportOptions exportOptions)
    {
        if (currentSong != null && !isSaving)
        {
            Debug.Log("Saving to file- " + System.IO.Path.GetFullPath(filename));
            lastLoadedFile = System.IO.Path.GetFullPath(filename);

            _saveTask = SaveCurrentSongAsync(filename, exportOptions);

            if (isSaving)
                events.saveEvent.Fire();

            isDirty = false;

            if (Globals.gameSettings.autoValidateSongOnSave)
            {
                bool hasErrors;
                SongValidate.ValidationParameters validateParams = new SongValidate.ValidationParameters() { songLength = currentSongLength, checkMidiIssues = false, };
                SongValidate.GenerateReport(Globals.gameSettings.songValidatorModes, currentSong, validateParams, out hasErrors);

                if (hasErrors)
                {
                    EnableMenu(uiServices.gameObject.GetComponentInChildren<ValidationMenu>(true));
                }
            }
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
                while (isSaving){ }

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

                try
                {
                    string directory = System.IO.Path.GetDirectoryName(currentFileName);
                    string iniPath = System.IO.Path.Combine(directory, "song.ini");
                    if (newSong != null && System.IO.File.Exists(iniPath))
                    {
                        try
                        {
                            newSong.iniProperties.Open(iniPath);
                            newSong.iniProperties = SongIniFunctions.FixupSongIniWhitespace(newSong.iniProperties);
                        }
                        catch (Exception e)
                        {
                            errorManager.QueueErrorMessage(Logger.LogException(e, "Failed to parse song.ini"));
                        }
                        finally
                        {
                            newSong.iniProperties.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    // TODO
                }
            }),

            new LoadingTask("Loading audio", () =>
            {
                if (error)
                    return;

                // Free the previous audio clips
                currentSongAudio.FreeAudioStreams();
                currentSongAudio.LoadAllAudioClips(newSong);
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

        sessionFlags &= ~ChartEditorSessionFlags.CurrentChartSavedInProprietyFormat;

        LoadSong(currentSong);

#if TIMING_DEBUG
        Debug.Log("Total load time: " + (Time.realtimeSinceStartup - totalLoadTime));
#endif
    }

    IEnumerator _Load()
    {
        if (!EditCheck())
            yield break;

        while (isSaving)
            yield return null;

        if (errorManager.HasErrorToDisplay())
        {
            yield break;
        }

        Song backup = currentSong;

        if (!FileExplorer.OpenFilePanel(new ExtensionFilter("Chart files", "chart", "mid"), "chart,mid,msce", out currentFileName))
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

        MenuBar menuBar = uiServices.menuBar;

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

        if (AudioManager.StreamIsValid(currentSongAudio.GetAudioStream(Song.AudioInstrument.Song)))
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

    /// <summary>
    /// Is this song currently being saved asyncronously?
    /// </summary>
    public bool isSaving
    {
        get
        {
            return _saveTask != null && _saveTask.Status != TaskStatus.RanToCompletion;
        }
    }

    /// <summary>
    /// Starts a thread that saves the song data in a .chart format to the specified path asynchonously. Can be monitored with the "IsSaving" parameter.
    /// </summary>
    /// <param name="filepath">The path and filename to save to.</param>
    /// <param name="forced">Will the notes from each chart have their flag properties saved into the file?</param>
    Task SaveCurrentSongAsync(string filepath, ExportOptions exportOptions)
    {
#if false
        Song songCopy = new Song(this);
        songCopy.Save(filepath, exportOptions);

#if !UNITY_EDITOR
        This is for debugging only you moron
#endif
#else
        if (!isSaving)
        {
            Song songCopy = new Song(currentSong);
            return Task.Run(() => SaveSong(songCopy, filepath, exportOptions));
        }
#endif
        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the song data in a .chart format to the specified path.
    /// </summary>
    /// <param name="filepath">The path and filename to save to.</param>
    /// <param name="forced">Will the notes from each chart have their flag properties saved into the file?</param>
    void SaveSong(Song song, string filepath, ExportOptions exportOptions)
    {
        string saveErrorMessage;
        try
        {
            ChartWriter.ErrorReport errorReport;
            new ChartWriter(filepath).Write(song, exportOptions, out errorReport);

            Debug.Log("Save complete!");

            saveErrorMessage = errorReport.errorList.ToString();

            bool shouldQueueErrors = true; /*errorReport.hasNonErrorFileTypeRelatedErrors;
            if (!sessionFlags.HasFlag(ChartEditorSessionFlags.CurrentChartSavedInProprietyFormat))
            {
                // We haven't warned users about this particular error yet, let's queue it up.
                shouldQueueErrors = true;
            }*/

            if (errorReport.resultantFileType == ChartIOHelper.FileSubType.MoonscraperPropriety)
            {
                filepath = System.IO.Path.ChangeExtension(filepath, MsceIOHelper.FileExtention);
                lastLoadedFile = filepath;
            }

            if (saveErrorMessage != string.Empty && shouldQueueErrors)
            {
                errorManager.QueueErrorMessage("Save completed with the following errors: " + Globals.LINE_ENDING + saveErrorMessage);
            }

            if (errorReport.resultantFileType == ChartIOHelper.FileSubType.MoonscraperPropriety)
            {
                sessionFlags |= ChartEditorSessionFlags.CurrentChartSavedInProprietyFormat;
            }

            string iniPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filepath), SongIniFunctions.INI_FILENAME);
            if (!song.iniProperties.IsEmpty)
            {
                Debug.Log("Saving song.ini");

                INIParser parser = new INIParser();
                try
                {
                    parser.Open(iniPath);
                    parser.WriteValue(song.iniProperties);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error encountered when trying to write song.ini. " + e.Message);
                }
                finally
                {
                    parser.Close();
                }

                Debug.Log("song.ini save complete!");
            }
            else if (System.IO.File.Exists(iniPath))
            {
                System.IO.File.Delete(iniPath);
            }
        }
        catch (System.Exception e)
        {
            errorManager.QueueErrorMessage(Logger.LogException(e, "Save failed!"));
        }
    }

#endregion

#region Audio Functions
    public void PlayAudio(float playPoint)
    {
        SongAudioManager songAudioManager = currentSongAudio;
        SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Song), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_song);
        SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Guitar), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_guitar);
        SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Bass), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_bass);
        SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Rhythm), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_rhythm);
		SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Keys), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_keys);
        SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Drum), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_drums);
		SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Drums_2), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_drums2);
		SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Drums_3), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_drums3);
		SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Drums_4), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_drums4);
		SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Vocals), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_vocals);
		SetStreamProperties(songAudioManager.GetAudioStream(Song.AudioInstrument.Crowd), Globals.gameSettings.gameSpeed, Globals.gameSettings.vol_crowd);

        AudioStream primaryStream = null;
        foreach (var bassStream in songAudioManager.bassAudioStreams)
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
        foreach (var bassStream in currentSongAudio.bassAudioStreams)
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

            stream.volume = vol * Globals.gameSettings.vol_master;
            stream.pan = Globals.gameSettings.audio_pan;

            if (speed < 1 && !Globals.gameSettings.slowdownPitchCorrectionEnabled)
            {
                float originalFreq = stream.frequency;

                float freq = originalFreq * speed;
                if (freq < 100)
                    freq = 100;
                else if (freq > 100000)
                    freq = 100000;

                stream.frequency = freq;

                //    // Pitch shifting equation
                //    stream.tempoPitch = Mathf.Log(1.0f / speed, Mathf.Pow(2, 1.0f / 12.0f));
            }
            else
            {
                stream.tempo = speed * 100 - 100;
            }
        }
    }

    public float currentSongLength
    {
        get
        {
            float DEFAULT_SONG_LENGTH = 300;     // 5 minutes
            float songLengthInSeconds = DEFAULT_SONG_LENGTH;

            if (currentSong == null)
                return 0;

            if (currentSong.manualLength.HasValue)
            {
                songLengthInSeconds = currentSong.manualLength.Value;
            }
            else
            {
                float length = 0;

                // Find the longest valid audio track
                for (int i = 0; i < EnumX<Song.AudioInstrument>.Count; ++i)
                {
                    Song.AudioInstrument audio = (Song.AudioInstrument)i;
                    AudioStream stream = currentSongAudio.GetAudioStream(audio);
                    if (AudioManager.StreamIsValid(stream))
                    {
                        length = Mathf.Max(length, stream.ChannelLengthInSeconds());
                    }
                }

                if (length > 0)
                    songLengthInSeconds = length;
            }

            if (currentSong.offset < 0)
            {
                songLengthInSeconds -= currentSong.offset;
            }

            return Mathf.Max(songLengthInSeconds, 0);
        }
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

        if (Globals.gameSettings.resetAfterGameplay)
            stopResetTime = currentVisibleTime;

        // Set position x seconds beforehand
        float startTime = WorldYPositionToTime(strikelineYPos) - Globals.gameSettings.gameplayStartDelayTime - (0.01f * Globals.gameSettings.hyperspeed); // Offset to prevent errors where it removes a note that is on the strikeline
        movement.SetTime(startTime);

        // Hide everything behind the strikeline
        foreach (Note note in currentChart.notes)
        {
            if (note.controller)
            {
                if (WorldYPosition(note) < strikelineYPos)
                {
                    note.controller.HideFullNote();
                }
                else
                    break;
            }
        }

        foreach (HitAnimation hitAnim in indicators.animations)
            hitAnim.StopAnim();

        SystemManagerState playingState = new PlayingState(false, startTime, stopResetTime);
        PopulatePersistentSystemsForNewState(State.Playing, playingState);
        ChangeState(State.Playing, playingState);
    }

    public void Play()
    {
        float? stopResetTime = null;

        if (Globals.gameSettings.resetAfterPlay)
        {
            stopResetTime = currentVisibleTime;
        }

        {
            float strikelineYPos = visibleStrikeline.position.y;

            // Hide everything behind the strikeline
            foreach (Note note in currentChart.notes)
            {
                if (note.controller)
                {
                    if (WorldYPositionLength(note) < strikelineYPos)    // Allows the bot to continue to hit sustains upon play
                    {
                        note.controller.HideFullNote();
                    }
                    else
                        break;
                }
            }
        }

        foreach (HitAnimation hitAnim in indicators.animations)
            hitAnim.StopAnim();

        SystemManagerState playingState = new PlayingState(true, currentVisibleTime, stopResetTime);
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
        events.playbackStoppedEvent.Fire();

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
