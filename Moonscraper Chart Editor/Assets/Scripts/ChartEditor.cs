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
    [Header("Inspectors")]
    public NotePropertiesPanelController noteInspector;
    public SectionPropertiesPanelController sectionInspector;
    public BPMPropertiesPanelController bpmInspector;
    public TimesignaturePropertiesPanelController tsInspector;
    public GameObject groupSelectInspector;
    [Header("Tool prefabs")]
    public GameObject ghostNote;
    public GameObject ghostStarpower;
    public GameObject ghostSection;
    public GameObject ghostBPM;
    public GameObject ghostTimeSignature;
    public GroupMove groupMove;
    [Header("Misc.")]
    public UnityEngine.UI.Button play;
    public UnityEngine.UI.Button undo;
    public UnityEngine.UI.Button redo;
    public Transform strikelineAudio;
    public Transform visibleStrikeline;
    public TimelineHandler timeHandler;
    public Transform camYMin;
    public Transform camYMax;
    public Transform autoUpScroll;
    public Transform mouseYMaxLimit;
    public Transform mouseYMinLimit;
    public SongPropertiesPanelController songPropertiesCon;
    public AudioSource clapSource;
    public UnityEngine.Audio.AudioMixerGroup mixer;
    public LoadingScreenFader loadingScreen;
    public ErrorMessage errorMenu;
    public Indicators indicators;
    [SerializeField]
    GroupSelect groupSelect;
    public Globals globals;

    public uint minPos { get; private set; }
    public uint maxPos { get; private set; }
#if !BASS_AUDIO
    [HideInInspector]
    public AudioSource[] musicSources;
#endif
    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    string currentFileName = string.Empty;

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

    GameObject currentPropertiesPanel = null;
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

        minPos = 0;
        maxPos = 0;

        noteInspector.gameObject.SetActive(false);
        sectionInspector.gameObject.SetActive(false);
        bpmInspector.gameObject.SetActive(false);
        tsInspector.gameObject.SetActive(false);

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

#if BASS_AUDIO
        // Bass init
        if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
        {
            Debug.LogError("Bass audio not initialised");
        }
#else

        musicSources = new AudioSource[3];
        for (int i = 0; i < musicSources.Length; ++i)
        {
            musicSources[i] = gameObject.AddComponent<AudioSource>();
            musicSources[i].volume = 0.5f;
            musicSources[i].outputAudioMixerGroup = mixer;
        }
#endif
        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();

        // Initialise object
        songPropertiesCon.gameObject.SetActive(true);
        songPropertiesCon.gameObject.SetActive(false);

        editOccurred = false;
        SetApplicationWindowPointer();

        loadingScreen.gameObject.SetActive(true);
    }

    void Start()
    {
        SetVolume();
    }

    Vector3 mousePos = Vector3.zero;
    bool mouseDownOverUI = false;
    GameObject clickedSelectableObject;
    public void Update()
    {
        if ((Toolpane.currentTool == Toolpane.Tools.Cursor || Toolpane.currentTool == Toolpane.Tools.GroupSelect) && Input.GetButtonDown("Delete"))
            Delete();

        if (Globals.modifierInputActive && Toolpane.currentTool == Toolpane.Tools.Cursor)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                Cut();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                Copy();
            }
        }

        if (Input.GetMouseButtonDown(0))
            clickedSelectableObject = Mouse.GetSelectableObjectUnderMouse();
        else if (Input.GetMouseButtonUp(0))
            clickedSelectableObject = null;

            // Group move/deselect
            if (Toolpane.currentTool == Toolpane.Tools.Cursor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mousePos = Input.mousePosition;

                mouseDownOverUI = Mouse.IsUIUnderPointer();
            }

            if (Input.GetMouseButton(0) && mousePos != Input.mousePosition && currentSelectedObjects.Length > 0 && clickedSelectableObject/*Mouse.GetSelectableObjectUnderMouse()*/ && !mouseDownOverUI)
            {
                // Find anchor point
                int anchorPoint = SongObject.NOTFOUND;

                if (clickedSelectableObject)
                {
                    for (int i = 0; i < currentSelectedObjects.Length; ++i)
                    {
                        if (currentSelectedObjects[i].controller != null && currentSelectedObjects[i].controller.gameObject == clickedSelectableObject)
                        {
                            anchorPoint = i;
                            break;
                        }
                    }
                    //Debug.Log("Not found " + anchorPoint);
                }
                groupMove.SetSongObjects(currentSelectedObjects, anchorPoint, true);
                currentSelectedObject = null;
            }

            if (Input.GetMouseButtonUp(0) && !Mouse.GetSelectableObjectUnderMouse() && !Mouse.IsUIUnderPointer() && mousePos == Input.mousePosition)
            {
                currentSelectedObject = null;
                mousePos = Vector3.zero;
            }
        }
        else
            mousePos = Vector3.zero;

        // Update object positions that supposed to be visible into the range of the camera
        minPos = currentSong.WorldYPositionToChartPosition(camYMin.position.y);
        maxPos = currentSong.WorldYPositionToChartPosition(camYMax.position.y);

        // Update the current properties panel     
        if ((Toolpane.currentTool == Toolpane.Tools.GroupSelect || Toolpane.currentTool == Toolpane.Tools.Cursor) && currentSelectedObjects.Length > 1)
        {
            if (currentPropertiesPanel != groupSelectInspector)
            {
                currentPropertiesPanel.SetActive(false);
                currentPropertiesPanel = groupSelectInspector;
            }
            if (!currentPropertiesPanel.gameObject.activeSelf)
                currentPropertiesPanel.gameObject.SetActive(true);
        }

        else if (currentSelectedObject != null)
        {
            GameObject previousPanel = currentPropertiesPanel;
            
            switch (currentSelectedObjects[0].classID)
            {
                case ((int)SongObject.ID.Note):
                    noteInspector.currentNote = (Note)currentSelectedObject;
                    currentPropertiesPanel = noteInspector.gameObject;
                    break;
                case ((int)SongObject.ID.Section):
                    sectionInspector.currentSection = (Section)currentSelectedObject;
                    currentPropertiesPanel = sectionInspector.gameObject;
                    break;
                case ((int)SongObject.ID.BPM):
                    bpmInspector.currentBPM = (BPM)currentSelectedObject;
                    currentPropertiesPanel = bpmInspector.gameObject;
                    break;
                case ((int)SongObject.ID.TimeSignature):
                    tsInspector.currentTS = (TimeSignature)currentSelectedObject;
                    currentPropertiesPanel = tsInspector.gameObject;
                    break;
                default:
                    currentPropertiesPanel = null;
                    currentSelectedObject = null;
                    break;
            }

            if (currentPropertiesPanel != previousPanel)
            {
                if (previousPanel)
                {
                    previousPanel.SetActive(false);
                }
            }

            if (currentPropertiesPanel != null && !currentPropertiesPanel.gameObject.activeSelf)
            {
                currentPropertiesPanel.gameObject.SetActive(true);
            }
        }
        else if (currentPropertiesPanel)
        {
            currentPropertiesPanel.gameObject.SetActive(false);
        }/*

        if ((Toolpane.currentTool == Toolpane.Tools.GroupSelect || Toolpane.currentTool == Toolpane.Tools.Cursor) && currentSelectedObjects.Length > 1)
        {
            currentPropertiesPanel = groupSelectInspector;
            if (!currentPropertiesPanel.gameObject.activeSelf)
                currentPropertiesPanel.gameObject.SetActive(true);
        }*/

        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            undo.interactable = actionHistory.canUndo;
            redo.interactable = actionHistory.canRedo;
        }
        else
        {
            undo.interactable = false;
            redo.interactable = false;
        }

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

        if (quitting)
        {
            if (editCheck())
            {
                wantsToQuit = true;
                UnityEngine.Application.Quit();
            }
        }
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

        //if (editCheck())
        if (wantsToQuit)
        {
            currentSong.musicSample.Stop();
            currentSong.guitarSample.Stop();
            currentSong.rhythmSample.Stop();
            currentSong.FreeBassAudioStreams();

            Bass.BASS_Free();
            Debug.Log("Freed Bass Audio memory");
            while (currentSong.IsSaving) ;
        }
        // Can't run edit check here because it seems to run in a seperate thread
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
        FreeAudioClips();
        currentSong = new Song();

        LoadSong(currentSong);

        movement.SetPosition(0);
        StartCoroutine(resetLag());

        currentSelectedObject = null;
        editOccurred = true;
    }

    IEnumerator resetLag()
    {
        yield return null;
        songPropertiesCon.gameObject.SetActive(true);
    }

    // Wrapper function
    public void Load()
    {
        Stop();
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
            Save(lastLoadedFile);
            return true;
        }
        else
            return _SaveAs();
    }

    public bool _SaveAs(bool forced = true)
    {
        try {
            string fileName;

#if UNITY_EDITOR
            fileName = UnityEditor.EditorUtility.SaveFilePanel("Save as...", "", currentSong.name, "chart");
#else

            OpenFileName openSaveFileDialog = new OpenFileName();

            openSaveFileDialog.structSize = Marshal.SizeOf(openSaveFileDialog);
            openSaveFileDialog.filter = "Chart files (*.chart)\0*.chart";
            openSaveFileDialog.file = new String(new char[256]);
            openSaveFileDialog.maxFile = openSaveFileDialog.file.Length;

            openSaveFileDialog.fileTitle = new String(new char[64]);
            openSaveFileDialog.maxFileTitle = openSaveFileDialog.fileTitle.Length;

            if (lastLoadedFile != string.Empty)
                openSaveFileDialog.file = System.IO.Path.GetFileNameWithoutExtension(lastLoadedFile);
            else
            {
                openSaveFileDialog.file = new String(currentSong.name.ToCharArray());
            }

            if (!forced)
                openSaveFileDialog.file += "(UNFORCED)";

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

            Save(fileName, forced);

            return true;          
        }
        catch (System.Exception e)
        {
            // User probably just canceled
            Debug.LogError(e.Message);
            return false;
        }
    }

    void Save (string filename, bool forced = true)
    {
        if (currentSong != null)
        {
            Debug.Log("Saving to file- " + System.IO.Path.GetFullPath(filename));

            editOccurred = false;            
            currentSong.SaveAsync(filename, forced);
            lastLoadedFile = System.IO.Path.GetFullPath(filename);
        }
    }
    public static float? startGameplayPos = null;
    public void StartGameplay()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Playing || movement.transform.position.y < movement.initPos.y)
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

        // Set position 3 seconds beforehand
        float time = Song.WorldYPositionToTime(strikelineYPos);
        movement.transform.position = new Vector3(movement.transform.position.x, Song.TimeToWorldYPosition(time - Globals.gameplayStartDelayTime), movement.transform.position.z);

        Globals.bot = false;
        Play();
    }

    void PlayAudio(float playPoint)
    {
#if !BASS_AUDIO
        foreach (AudioSource source in musicSources)
            source.time = playPoint;       // No need to add audio calibration as position is base on the strikeline position

        foreach (AudioSource source in musicSources)
        {
            source.pitch = Globals.gameSpeed;
            source.Play();
        }

#else
        playBassStream(currentSong.bassMusicStream, playPoint, Globals.gameSpeed, Globals.vol_song);
        playBassStream(currentSong.bassGuitarStream, playPoint, Globals.gameSpeed, Globals.vol_guitar);
        playBassStream(currentSong.bassRhythmStream, playPoint, Globals.gameSpeed, Globals.vol_rhythm);
#endif
    }

    void StopAudio()
    {
#if !BASS_AUDIO
        // Stop the audio from continuing to play
        foreach (AudioSource source in musicSources)
            source.Stop();
#else
        if (currentSong.bassMusicStream != 0)
            Bass.BASS_ChannelStop(currentSong.bassMusicStream);

        if (currentSong.bassGuitarStream != 0)
            Bass.BASS_ChannelStop(currentSong.bassGuitarStream);

        if (currentSong.bassRhythmStream != 0)
            Bass.BASS_ChannelStop(currentSong.bassRhythmStream);
#endif
    }

    void playBassStream(int handle, float playPoint, float speed, float vol)
    {
        if (handle != 0)
        {
            Bass.BASS_ChannelSetPosition(handle, playPoint);
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_VOL, vol * Globals.vol_master);
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_PAN,  Globals.audio_pan);

            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_TEMPO, speed * 100 - 100);

                //Debug.LogError(Bass.BASS_ErrorGetCode());
            //Bass.BASS_ChannelSetFX(handle, )
            //Un4seen.Bass.AddOn.Fx.BassFx.
            Bass.BASS_ChannelPlay(handle, false);
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

        mixer.audioMixer.SetFloat("Pitch", 1 / (Globals.gameSpeed));
        play.interactable = false;
        Globals.applicationMode = Globals.ApplicationMode.Playing;
        cancel = false;

        float playPoint = Song.WorldYPositionToTime(strikelineAudio.position.y) + currentSong.offset;       // Audio calibration handled by the position of the strikeline audio

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
        float playPoint = Song.WorldYPositionToTime(strikelineAudio.position.y) + currentSong.offset;

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
        play.interactable = true;
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

        currentSelectedObjects = selectedBeforePlay;
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

    IEnumerator _Load()
    {
        if (!editCheck())
            yield break;

        Song backup = currentSong;
#if TIMING_DEBUG
        float totalLoadTime = 0;
#endif
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
        string originalMidFile = string.Empty;

        // Convert mid to chart
        if (System.IO.Path.GetExtension(currentFileName) == ".mid")
        {
            originalMidFile = currentFileName;
            
            loadingScreen.loadingInformation.text = "Coverting .mid to .chart";
            System.Threading.Thread midConversionThread = new System.Threading.Thread(() => { currentFileName = ImportMidToTempChart(currentFileName); });

            midConversionThread.Start();

            while (midConversionThread.ThreadState == System.Threading.ThreadState.Running)
                yield return null;

            if (currentFileName == string.Empty)
            {
                currentSong = backup;
                //Globals.applicationMode = Globals.ApplicationMode.Editor;
                loadingScreen.FadeOut();
                errorMenu.gameObject.SetActive(true);
                // Immediate exit
                yield break;
            }
#if TIMING_DEBUG
            Debug.Log("Mid conversion time: " + (Time.realtimeSinceStartup - totalLoadTime));
#endif
        }

        currentSong.musicSample.Stop();
        currentSong.guitarSample.Stop();
        currentSong.rhythmSample.Stop();

#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        // Load the actual file
        loadingScreen.loadingInformation.text = "Loading file";
        yield return null;

        // Free the audio clips
        FreeAudioClips();

        System.Threading.Thread songLoadThread = new System.Threading.Thread(() => 
        {
            try
            {
                currentSong = new Song(currentFileName);
            }
            catch
            {
                currentSong = backup;
            }
        });
        songLoadThread.Start();
        while (songLoadThread.ThreadState == System.Threading.ThreadState.Running)
            yield return null;

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

        if (originalMidFile != string.Empty)
        {
            // Delete the temp chart
            System.IO.File.Delete(currentFileName);
            currentFileName = string.Empty;
        }

        if (currentFileName != string.Empty)
            lastLoadedFile = System.IO.Path.GetFullPath(currentFileName);
        else
            lastLoadedFile = string.Empty;

        LoadSong(currentSong);

#if TIMING_DEBUG
        Debug.Log("Total load time: " + (Time.realtimeSinceStartup - totalLoadTime));
#endif

        if (originalMidFile != string.Empty)
        {
            editOccurred = true;
        }

        // Stop loading animation
        Globals.applicationMode = Globals.ApplicationMode.Editor;
        loadingScreen.FadeOut();
        loadingScreen.loadingInformation.text = "Complete!";
    }

    void LoadSong(Song song)
    {
        editOccurred = false;

        // Load the default chart
        LoadChart(song.expert_single);
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
    void LoadChart(Chart chart)
    {
        actionHistory = new ActionHistory();
        Stop();

        currentChart = chart;

        songObjectPoolManager.NewChartReset();
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

    public void FreeAudioClips()
    {
#if !BASS_AUDIO
        currentSong.FreeAudioClips();
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

    public void AddToSelectedObjects(SongObject[] songObjects)
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

    public void RemoveFromSelectedObjects(SongObject[] songObjects)
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

    void Copy()
    {
        var songObjectsCopy = new SongObject[currentSelectedObjects.Length];
        float? left = null, right = null;
        float position = 0;

        for (int i = 0; i < currentSelectedObjects.Length; ++i)
        {
            songObjectsCopy[i] = currentSelectedObjects[i].Clone();

            position = SongObjectController.GetXPos(currentSelectedObjects[i]);

            if (left == null || position < left)
                left = position;

            if (right == null || position > right)
                right = position;
        }

        if (left == null)
            left = 0;
        if (right == null)
            right = 0;

        Vector2 bottomLeft = new Vector2((float)left, currentSong.ChartPositionToWorldYPosition(songObjectsCopy[0].position));
        Vector2 upperRight = new Vector2((float)right, currentSong.ChartPositionToWorldYPosition(songObjectsCopy[songObjectsCopy.Length - 1].position));

        var area = new Clipboard.SelectionArea(bottomLeft, upperRight, songObjectsCopy[0].position, songObjectsCopy[songObjectsCopy.Length - 1].position);

        ClipboardObjectController.clipboard = new Clipboard(songObjectsCopy, area, currentSong);
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

            currentChart.updateArrays();
            currentSong.updateArrays();

            currentSelectedObject = null;

            groupSelect.reset();
        }
    }

    void Cut()
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
}
