// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.EventSystems;

public class Globals : MonoBehaviour {
    public const uint FULL_STEP = 768;
    public static readonly float STANDARD_BEAT_RESOLUTION = 192.0f;
    public static readonly string LINE_ENDING = "\r\n";
    public static string autosaveLocation;
    const float FRAMERATE = 25;

    public static readonly string[] validAudioExtensions = { ".ogg", ".wav", ".mp3" };
    public static readonly string[] validTextureExtensions = { ".jpg", ".png" };
    public static string[] localEvents = { };
    public static string[] globalEvents = { };

    public const string TABSPACE = "  ";

    [Header("Area range")]
    public RectTransform area;
    [Header("Misc.")]
    [SerializeField]
    GroupSelect groupSelect;
    [SerializeField]
    Text snapLockWarning;
    [SerializeField]
    GUIStyle hintMouseOverStyle;

    Rect toolScreenArea;

    public bool InToolArea
    {
        get
        {
            //Rect toolScreenArea = area.GetScreenCorners();

            if (Input.mousePosition.x < toolScreenArea.xMin ||
                    Input.mousePosition.x > toolScreenArea.xMax ||
                    Input.mousePosition.y < toolScreenArea.yMin ||
                    Input.mousePosition.y > toolScreenArea.yMax)
                return false;
            else
                return true;
        }
    }
    public static bool IsInDropDown = false;

    static bool _IsInDropDown
    {
        get
        {
            //System.Collections.Generic.List<RaycastResult> result = Mouse.RaycastFromPointer();

            GameObject currentUIUnderPointer = Mouse.GetUIRaycastableUnderPointer();
            if (currentUIUnderPointer != null && (currentUIUnderPointer.GetComponentInChildren<ScrollRect>() || currentUIUnderPointer.GetComponentInParent<ScrollRect>()))
                return true;

            if ((EventSystem.current.currentSelectedGameObject == null ||
                EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>() == null) && !Mouse.GetUIUnderPointer<Dropdown>())
            {
                return false;
            }
            else
                return true;
        }
    }

    public static bool IsTyping
    {
        get
        {
            if (EventSystem.current.currentSelectedGameObject == null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
                return false;
            else
                return true;
        }
    }
    public static bool drumMode
    {
        get
        {
            return MenuBar.currentInstrument == Song.Instrument.Drums;
        }
    }
    public static bool ghLiveMode
    {
        get
        {
            return MenuBar.currentInstrument == Song.Instrument.GHLiveGuitar || MenuBar.currentInstrument == Song.Instrument.GHLiveBass;
        }
    }
    // Settings
    public static float hyperspeed = 5.0f;
    public static float highwayLength = 0;
    public static Step snappingStep = new Step(16);
    public static int step { get { return snappingStep.value; } set { snappingStep.value = value; } }
    public static bool lockToStrikeline = false;
    public static ClapToggle clapSetting = ClapToggle.NONE;
    public static ClapToggle clapProperties = ClapToggle.NONE;
    public static bool metronomeActive = false;
    public static int audioCalibrationMS = 200;                     // Increase to start the audio sooner
    public static int clapCalibrationMS = 200;
    public static ApplicationMode applicationMode = ApplicationMode.Editor;
    public static ViewMode viewMode { get; set; }
    public static NotePlacementMode notePlacementMode = NotePlacementMode.LeftyFlip;
    public static bool extendedSustainsEnabled = false;
    public static bool sustainGapEnabled { get; set; }
    public static bool resetAfterPlay = false;
    public static bool resetAfterGameplay = false;
    public static Step sustainGapStep;
    public static int sustainGap { get { return sustainGapStep.value; } set { sustainGapStep.value = value; } }
    public static bool bot = true;
    public static int customBgSwapTime;

    // Audio stuff
    static float _sfxVolume = 1;
    public static float gameSpeed = 1;
    public static float gameplayStartDelayTime = 3.0f;
    public static float sfxVolume
    {
        get { return _sfxVolume; }
        set
        {
            if (value < 0)
                _sfxVolume = 0;
            else if (value > 1)
                _sfxVolume = 1;
            else
                _sfxVolume = value;
        }
    }
    public static float vol_master, vol_song, vol_guitar, vol_rhythm, vol_drum, audio_pan;


    ChartEditor editor;
    static string workingDirectory = string.Empty;
    public static string realWorkingDirectory { get { return workingDirectory; } }

    Resolution largestRes;
    static Vector2 prevScreenSize;
    public static bool HasScreenResized
    {
        get
        {
            return (prevScreenSize.x != Screen.width || prevScreenSize.y != Screen.height);
        }
    }

    void Awake()
    {
        largestRes = Screen.resolutions[0];
        foreach (Resolution res in Screen.resolutions)
        {
            if (res.width > largestRes.width)
                largestRes = res;
        }
        autosaveLocation = Application.persistentDataPath + "/autosave.chart";

        viewMode = ViewMode.Chart;
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
#if !UNITY_EDITOR
        workingDirectory = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
#else
        workingDirectory = Application.dataPath;
#endif

        LoadConfigFile();

        localEvents = LoadCommonEvents("local_events.txt");
        globalEvents = LoadCommonEvents("global_events.txt");

        InputField[] allInputFields = Resources.FindObjectsOfTypeAll<InputField>();
        foreach (InputField inputField in allInputFields)
            inputField.gameObject.AddComponent<InputFieldDoubleClick>();

        HintMouseOver.style = hintMouseOverStyle;
    }

    void Start()
    {
        toolScreenArea = area.GetScreenCorners();
        prevScreenSize.x = Screen.width;
        prevScreenSize.y = Screen.height;
        StartCoroutine(AutosaveCheck());
    }

    IEnumerator AutosaveCheck()
    {
        yield return null;

        if (System.IO.File.Exists(autosaveLocation))
        {
#if !UNITY_EDITOR
            System.Windows.Forms.DialogResult result;

            result = System.Windows.Forms.MessageBox.Show("An autosave was detected indicating that the program did not corretly shut down during the last session. \nWould you like to reload the autosave?", 
                "Warning", System.Windows.Forms.MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                yield return StartCoroutine(editor._Load(autosaveLocation, false));
                ChartEditor.isDirty = true;
            }
#endif
        }
    }
    
    void Update()
    {
        //Debug.Log(SongObject.FindObjectPosition(new ChartEvent(8256, "*"), editor.currentChart.events));
        IsInDropDown = _IsInDropDown;

        // Disable controls while user is in an input field
        if (!IsTyping)
            Controls();
        ModifierControls();

        snapLockWarning.gameObject.SetActive((lockToStrikeline && Toolpane.currentTool != Toolpane.Tools.Cursor && Toolpane.currentTool != Toolpane.Tools.Eraser));

        if (HasScreenResized)
            OnScreenResize();

        // IsTyping can still be active if this isn't manually detected
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && IsTyping && applicationMode == ApplicationMode.Editor)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void LateUpdate()
    {
        prevScreenSize.x = Screen.width;
        prevScreenSize.y = Screen.height;
    }

    public void OnScreenResize()
    {
        toolScreenArea = area.GetScreenCorners();
    }

    public static bool modifierInputActive { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand); } }
    public static bool secondaryInputActive { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }

    void ModifierControls()
    {
        if (modifierInputActive)
        {
            if (Input.GetKeyDown("s"))
            {
                if (secondaryInputActive)
                    editor.SaveAs();
                else
                    editor._Save();
            }
            else if (Input.GetKeyDown("o"))
                editor.Load();
            else if (Input.GetKeyDown("n"))
                editor.New();
            else if (Input.GetKeyDown("z") && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                EventSystem.current.SetSelectedGameObject(null);
                bool success;

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    success = editor.actionHistory.Redo(editor);
                else
                    success = editor.actionHistory.Undo(editor);

                if (success)
                {
                    groupSelect.reset();
                    TimelineHandler.externalUpdate = true;
                }
            }
            else if (Input.GetKeyDown("y") && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                if (editor.actionHistory.Redo(editor))
                {
                    groupSelect.reset();
                    TimelineHandler.externalUpdate = true;
                }
            }
            else if (Input.GetKeyDown("a"))
            {
                editor.currentSelectedObject = null;

                if (viewMode == ViewMode.Chart)
                {
                    editor.currentSelectedObjects = editor.currentChart.chartObjects;
                    //editor.AddToSelectedObjects(editor.currentChart.starPower);
                }
                else
                {
                    editor.currentSelectedObjects = editor.currentSong.syncTrack;
                    editor.AddToSelectedObjects(editor.currentSong.eventsAndSections);
                }
            }
            else if (editor.currentSelectedObjects.Length > 0)
            {
                if (Input.GetKeyDown(KeyCode.X))
                    editor.Cut();
                else if (Input.GetKeyDown(KeyCode.C))
                    editor.Copy();
            }
            
            /*
            else if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                //Screen.fullScreen = !Screen.fullScreen;

                if (!Screen.fullScreen)
                    StartCoroutine(WaitForScreenChange(true, largestRes));
                else
                    StartCoroutine(WaitForScreenChange(false, initRes));
            }*/
        }
    }

    private IEnumerator WaitForScreenChange(bool fullscreen, Resolution res)
    {
        int width = res.width;
        int height = res.height;

        Screen.fullScreen = fullscreen;

        yield return new WaitForSeconds(1);
        //yield return null;
        Debug.Log(res);
        Screen.SetResolution(width, height, Screen.fullScreen);
    }

    void Controls()
    {
        if (!modifierInputActive)
        {
            if (Input.GetButtonDown("PlayPause"))
            {
                if (applicationMode == ApplicationMode.Editor)
                    editor.Play();
                else if (applicationMode == ApplicationMode.Playing)
                    editor.Stop();
            }

            if (Input.GetButtonDown("IncreaseStep"))
                snappingStep.Increment();
            else if (Input.GetButtonDown("DecreaseStep"))
                snappingStep.Decrement();

            // Generic delete key
            if (Input.GetButtonDown("Delete") && editor.currentSelectedObjects.Length > 0)// && Toolpane.currentTool == Toolpane.Tools.Cursor)
            {
                editor.Delete();
                /*
                editor.actionHistory.Insert(new ActionHistory.Delete(editor.currentSelectedObject));
                editor.currentSelectedObject.Delete();
                editor.currentSelectedObject = null;*/
            }

#if true
            if (GameplayManager.gamepad != null && GameplayManager.previousGamepad != null &&
                ((XInputDotNetPure.GamePadState)GameplayManager.gamepad).Buttons.Start == XInputDotNetPure.ButtonState.Pressed &&
                ((XInputDotNetPure.GamePadState)GameplayManager.previousGamepad).Buttons.Start == XInputDotNetPure.ButtonState.Released)
#else
            if (Input.GetButtonDown("Start Gameplay"))
#endif
            {
                if (applicationMode != ApplicationMode.Playing)
                    editor.StartGameplay();
                else
                    editor.Stop();
            }
        }
    }

    public void Quit()
    {
        INIParser iniparse = new INIParser();
        iniparse.Open(workingDirectory + "\\config.ini");

        iniparse.WriteValue("Settings", "Framerate", Application.targetFrameRate);
        iniparse.WriteValue("Settings", "Hyperspeed", hyperspeed);
        iniparse.WriteValue("Settings", "Highway Length", highwayLength);
        iniparse.WriteValue("Settings", "Audio calibration", audioCalibrationMS);
        iniparse.WriteValue("Settings", "Clap calibration", clapCalibrationMS);
        iniparse.WriteValue("Settings", "Clap", (int)clapProperties);
        iniparse.WriteValue("Settings", "Extended sustains", extendedSustainsEnabled);
        iniparse.WriteValue("Settings", "Sustain Gap", sustainGapEnabled);
        iniparse.WriteValue("Settings", "Sustain Gap Step", sustainGap);
        iniparse.WriteValue("Settings", "Note Placement Mode", (int)notePlacementMode);
        iniparse.WriteValue("Settings", "Gameplay Start Delay", gameplayStartDelayTime);
        iniparse.WriteValue("Settings", "Reset After Play", resetAfterPlay);
        iniparse.WriteValue("Settings", "Reset After Gameplay", resetAfterGameplay);
        iniparse.WriteValue("Settings", "Custom Background Swap Time", customBgSwapTime);

        // Audio levels
        iniparse.WriteValue("Audio Volume", "Master", vol_master);
        iniparse.WriteValue("Audio Volume", "Music Stream", vol_song);
        iniparse.WriteValue("Audio Volume", "Guitar Stream", vol_guitar);
        iniparse.WriteValue("Audio Volume", "Rhythm Stream", vol_rhythm);
        iniparse.WriteValue("Audio Volume", "Drum Stream", vol_drum);
        iniparse.WriteValue("Audio Volume", "Audio Pan", audio_pan);
        iniparse.WriteValue("Audio Volume", "SFX", sfxVolume);

        iniparse.Close();

        // Delete autosaved chart. If chart is not deleted then that means there may have been a problem like a crash and the autosave should be reloaded the next time the program is opened. 
        if (File.Exists(autosaveLocation))
            File.Delete(autosaveLocation);
    }

    public static void DeselectCurrentUI()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ClickButton(Button button)
    {
        button.onClick.Invoke();
    }

    [System.Flags]
    public enum ClapToggle
    {
        NONE = 0, ALL = ~0, STRUM = 1, HOPO = 2, TAP = 4
    }

    public enum ApplicationMode
    {
        Editor, Playing, Menu, Loading
    }

    public enum ViewMode
    {
        Chart, Song
    }

    public enum NotePlacementMode
    {
        Default, LeftyFlip
    }

    public void ResetAspectRatio()
    {
        int height = Screen.height;
        int width = (int)(16.0f / 9.0f * height);

        Screen.SetResolution(width, height, false);
    }

    void LoadConfigFile()
    {
        INIParser iniparse = new INIParser();

        iniparse.Open(workingDirectory + "\\config.ini");

        // Check for valid fps values
        int fps = iniparse.ReadValue("Settings", "Framerate", 120);
        if (fps != 60 && fps != 120 && fps != 240)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = fps;

        hyperspeed = (float)iniparse.ReadValue("Settings", "Hyperspeed", 5.0f);
        highwayLength = (float)iniparse.ReadValue("Settings", "Highway Length", 0);
        audioCalibrationMS = iniparse.ReadValue("Settings", "Audio calibration", 0);
        clapCalibrationMS = iniparse.ReadValue("Settings", "Clap calibration", 0);
        clapProperties = (ClapToggle)iniparse.ReadValue("Settings", "Clap", (int)ClapToggle.ALL);
        extendedSustainsEnabled = iniparse.ReadValue("Settings", "Extended sustains", false);
        clapSetting = ClapToggle.NONE;
        sustainGapEnabled = iniparse.ReadValue("Settings", "Sustain Gap", false);
        sustainGapStep = new Step((int)iniparse.ReadValue("Settings", "Sustain Gap Step", (int)16));
        notePlacementMode = (NotePlacementMode)iniparse.ReadValue("Settings", "Note Placement Mode", (int)NotePlacementMode.Default);
        gameplayStartDelayTime = (float)iniparse.ReadValue("Settings", "Gameplay Start Delay", 3.0f);
        resetAfterPlay = iniparse.ReadValue("Settings", "Reset After Play", false);
        resetAfterGameplay = iniparse.ReadValue("Settings", "Reset After Gameplay", false);
        customBgSwapTime = iniparse.ReadValue("Settings", "Custom Background Swap Time", 30);
        // Check that the gameplay start delay time is a multiple of 0.5 and is
        gameplayStartDelayTime = Mathf.Clamp(gameplayStartDelayTime, 0, 3.0f);
        gameplayStartDelayTime = (float)(System.Math.Round(gameplayStartDelayTime * 2.0f, System.MidpointRounding.AwayFromZero) / 2.0f);

        // Audio levels
        vol_master = (float)iniparse.ReadValue("Audio Volume", "Master", 0.5f);
        vol_song = (float)iniparse.ReadValue("Audio Volume", "Music Stream", 1.0f);
        vol_guitar = (float)iniparse.ReadValue("Audio Volume", "Guitar Stream", 1.0f);
        vol_rhythm = (float)iniparse.ReadValue("Audio Volume", "Rhythm Stream", 1.0f);
        vol_drum = (float)iniparse.ReadValue("Audio Volume", "Drum Stream", 1.0f);
        audio_pan = (float)iniparse.ReadValue("Audio Volume", "Audio Pan", 0.0f);

        AudioListener.volume = vol_master;

        //editor.clapSource.volume = (float)iniparse.ReadValue("Audio Volume", "Clap", 1.0f);
        sfxVolume = (float)iniparse.ReadValue("Audio Volume", "SFX", 1.0f);

        iniparse.Close();
    }

    static string[] LoadCommonEvents(string filename)
    {
#if UNITY_EDITOR
        string filepath = workingDirectory + "/ExtraBuildFiles/" + filename;
#else
        string filepath = workingDirectory + "/" + filename;
#endif
        Debug.Log(Path.GetFullPath(filepath));
        if (File.Exists(filepath))
        {
            Debug.Log("Loading events from " + filepath);

            StreamReader ifs = null;
            try
            {
                ifs = File.OpenText(filepath);
                var events = new System.Collections.Generic.List<string>();
                
                while (true)
                {
                    string line = ifs.ReadLine();
                    if (line == null)
                        break;

                    line.Replace('"', '\0');

                    if (line != string.Empty)
                        events.Add(line);
                }

                Debug.Log(events.Count + " event strings loaded");
                return events.ToArray();               
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error: unable to load events- " + e.Message);
            }

            if (ifs != null)
                ifs.Close();
        }
        else
        {
            Debug.Log("No events file found. Skipping loading of default events.");
        }

        return new string[0];
    }
}
