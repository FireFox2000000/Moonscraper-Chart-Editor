using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Globals : MonoBehaviour {
    public const uint FULL_STEP = 768;
    public static readonly float STANDARD_BEAT_RESOLUTION = 192.0f;
    public static readonly string LINE_ENDING = "\r\n";
    const float FRAMERATE = 25;

    public static readonly string[] validAudioExtensions = { ".ogg", ".wav", ".mp3" };
    public static readonly string[] validTextureExtensions = { ".jpg", ".png" };

    [Header("Initialize GUI")]
    public Toggle viewModeToggle;
    public AudioCalibrationMenuScript audioCalibrationMenu;

    public static readonly int NOTFOUND = -1;
    public static readonly string TABSPACE = "  ";

    [Header("Area range")]
    public RectTransform area;
    [Header("Misc.")]
    [SerializeField]
    Button defaultViewSwitchButton;
    [SerializeField]
    GroupSelect groupSelect;
    [SerializeField]
    Text snapLockWarning;

    public bool InToolArea
    {
        get
        {
            Rect toolScreenArea = area.GetScreenCorners();

            if (Input.mousePosition.x < toolScreenArea.xMin ||
                    Input.mousePosition.x > toolScreenArea.xMax ||
                    Input.mousePosition.y < toolScreenArea.yMin ||
                    Input.mousePosition.y > toolScreenArea.yMax)
                return false;
            else
                return true;
        }
    }

    public static bool IsInDropDown
    {
        get
        {
            GameObject currentUIUnderPointer = Mouse.GetUIRaycastableUnderPointer();
            if (currentUIUnderPointer != null && (currentUIUnderPointer.GetComponentInChildren<ScrollRect>() || currentUIUnderPointer.GetComponentInParent<ScrollRect>()))
                return true;

            if ((UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null ||
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>() == null) && !Mouse.GetUIUnderPointer<UnityEngine.UI.Dropdown>())
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
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null ||
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
                return false;
            else
                return true;
        }
    }

    // Settings
    public static float hyperspeed = 5.0f;
    public static Step snappingStep = new Step(16);
    public static int step { get { return snappingStep.value; } set { snappingStep.value = value; } }
    public static bool lockToStrikeline = false;
    public static ClapToggle clapSetting = ClapToggle.NONE;
    public static ClapToggle clapProperties = ClapToggle.NONE;
    public static bool metronomeActive = false;
    public static int audioCalibrationMS = 200;                     // Increase to start the audio sooner
    public static ApplicationMode applicationMode = ApplicationMode.Editor;
    public static ViewMode viewMode { get; private set; }
    public static NotePlacementMode notePlacementMode = NotePlacementMode.LeftyFlip;
    public static bool extendedSustainsEnabled = false;
    public static bool sustainGapEnabled { get; set; }
    public static Step sustainGapStep;
    public static int sustainGap { get { return sustainGapStep.value; } set { sustainGapStep.value = value; } }
    public static bool bot = true;
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

    ChartEditor editor;
    string workingDirectory;

    void Awake()
    {
        viewMode = ViewMode.Chart;
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        workingDirectory = System.IO.Directory.GetCurrentDirectory();

        INIParser iniparse = new INIParser();

        iniparse.Open("config.ini");
        
        hyperspeed = (float)iniparse.ReadValue("Settings", "Hyperspeed", 5.0f);
        audioCalibrationMS = iniparse.ReadValue("Settings", "Audio calibration", 0);
        clapProperties = (ClapToggle)iniparse.ReadValue("Settings", "Clap", (int)ClapToggle.ALL);
        extendedSustainsEnabled = iniparse.ReadValue("Settings", "Extended sustains", false);
        clapSetting = ClapToggle.NONE;
        sustainGapEnabled = iniparse.ReadValue("Settings", "Sustain Gap", false);
        sustainGapStep = new Step((int)iniparse.ReadValue("Settings", "Sustain Gap Step", (int)16));
        notePlacementMode = (NotePlacementMode)iniparse.ReadValue("Settings", "Note Placement Mode", (int)NotePlacementMode.Default);
        gameplayStartDelayTime = (float)iniparse.ReadValue("Settings", "Gameplay Start Delay", 3.0f);

        // Check that the gameplay start delay time is a multiple of 0.5 and is
        gameplayStartDelayTime = Mathf.Clamp(gameplayStartDelayTime, 0, 3.0f);
        gameplayStartDelayTime = (float)(System.Math.Round(gameplayStartDelayTime * 2.0f, System.MidpointRounding.AwayFromZero) / 2.0f);

        // Audio levels
        AudioListener.volume = (float)iniparse.ReadValue("Audio Volume", "Master", 0.5f);
        editor.musicSources[ChartEditor.MUSIC_STREAM_ARRAY_POS].volume = (float)iniparse.ReadValue("Audio Volume", "Music Stream", 1.0f);
        editor.musicSources[ChartEditor.GUITAR_STREAM_ARRAY_POS].volume = (float)iniparse.ReadValue("Audio Volume", "Guitar Stream", 1.0f);
        editor.musicSources[ChartEditor.RHYTHM_STREAM_ARRAY_POS].volume = (float)iniparse.ReadValue("Audio Volume", "Rhythm Stream", 1.0f);

        editor.musicSources[ChartEditor.MUSIC_STREAM_ARRAY_POS].panStereo = (float)iniparse.ReadValue("Audio Volume", "Audio Pan", 0.0f);
        editor.musicSources[ChartEditor.GUITAR_STREAM_ARRAY_POS].panStereo = (float)iniparse.ReadValue("Audio Volume", "Audio Pan", 0.0f);
        editor.musicSources[ChartEditor.RHYTHM_STREAM_ARRAY_POS].panStereo = (float)iniparse.ReadValue("Audio Volume", "Audio Pan", 0.0f);

        editor.clapSource.volume = (float)iniparse.ReadValue("Audio Volume", "Clap", 1.0f);
        sfxVolume = (float)iniparse.ReadValue("Audio Volume", "SFX", 1.0f);

        iniparse.Close();
        /*
        if (!configFileExisted)
            editor.EnableMenu(audioCalibrationMenu);*/

        InputField[] allInputFields = Resources.FindObjectsOfTypeAll<InputField>();
        foreach (InputField inputField in allInputFields)
            inputField.gameObject.AddComponent<InputFieldDoubleClick>();
    }

    void Update()
    {
        // Disable controls while user is in an input field
        if (!IsTyping)
            Controls();
        ModifierControls();

        if (System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.Scroll) && 
            (Toolpane.currentTool != Toolpane.Tools.Cursor && Toolpane.currentTool != Toolpane.Tools.Eraser && Toolpane.currentTool != Toolpane.Tools.GroupSelect))
            lockToStrikeline = true;
        else
            lockToStrikeline = false;
        snapLockWarning.gameObject.SetActive(lockToStrikeline);
    }

    void OnGUI()
    {
        //Debug.Log(System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock));
    }

    void ModifierControls()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand))
        {
            if (Input.GetKeyDown("s"))
                editor.Save();
            else if (Input.GetKeyDown("o"))
                editor.Load();
            else if (Input.GetKeyDown("z") && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                bool success;

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    success = editor.actionHistory.Redo(editor);
                else
                    success = editor.actionHistory.Undo(editor);

                if (success)
                    groupSelect.reset();
            }
            else if (Input.GetKeyDown("y") && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                if (editor.actionHistory.Redo(editor))
                    groupSelect.reset();
            }
        }
    }

    void Controls()
    {
        if (Input.GetButtonDown("PlayPause"))
        {
            if (applicationMode == Globals.ApplicationMode.Editor)
                editor.Play();
            else if (applicationMode == Globals.ApplicationMode.Playing)
                editor.Stop();
        }

        if (Input.GetButtonDown("IncreaseStep"))
            snappingStep.Increment();
        else if (Input.GetButtonDown("DecreaseStep"))
            snappingStep.Decrement();

        if (Input.GetButtonDown("Delete") && editor.currentSelectedObject != null && Toolpane.currentTool == Toolpane.Tools.Cursor)
        {
            editor.actionHistory.Insert(new ActionHistory.Delete(editor.currentSelectedObject));
            editor.currentSelectedObject.Delete();
            editor.currentSelectedObject = null;
        }

        if (Input.GetButtonDown("Start Gameplay"))
        {
            if (applicationMode != ApplicationMode.Playing)
                editor.StartGameplay();
            else
                editor.Stop();
        }

        //if (Input.GetButtonDown("Next Frame"))
            //StartCoroutine(editor.PlayAutoStop(1 / FRAMERATE));
    }

    public void ToggleSongViewMode(bool value)
    {
        if (value)
        {
            viewMode = ViewMode.Song;

            if (Toolpane.currentTool == Toolpane.Tools.Note || Toolpane.currentTool == Toolpane.Tools.Starpower || Toolpane.currentTool == Toolpane.Tools.ChartEvent || Toolpane.currentTool == Toolpane.Tools.GroupSelect)
            {
                defaultViewSwitchButton.onClick.Invoke();
            }
        }
        else
        {
            viewMode = ViewMode.Chart;

            if (Toolpane.currentTool == Toolpane.Tools.BPM || Toolpane.currentTool == Toolpane.Tools.Timesignature || Toolpane.currentTool == Toolpane.Tools.Section || Toolpane.currentTool == Toolpane.Tools.SongEvent)
            {
                defaultViewSwitchButton.onClick.Invoke();
            }
        }

        if (viewModeToggle.isOn != value)
        {
            viewModeToggle.isOn = value;
            editor.currentSelectedObject = null;
        }
    }

    void OnApplicationQuit()
    {
        INIParser iniparse = new INIParser();
        iniparse.Open(workingDirectory + "\\config.ini");

        iniparse.WriteValue("Settings", "Hyperspeed", hyperspeed);
        iniparse.WriteValue("Settings", "Audio calibration", audioCalibrationMS);
        iniparse.WriteValue("Settings", "Clap", (int)clapProperties);
        iniparse.WriteValue("Settings", "Extended sustains", extendedSustainsEnabled);
        iniparse.WriteValue("Settings", "Sustain Gap", sustainGapEnabled);
        iniparse.WriteValue("Settings", "Sustain Gap Step", sustainGap);
        iniparse.WriteValue("Settings", "Note Placement Mode", (int)notePlacementMode);
        iniparse.WriteValue("Settings", "Gameplay Start Delay", gameplayStartDelayTime);

        // Audio levels
        
        //iniparse.WriteValue("Audio Volume", "Master", AudioListener.volume);
        iniparse.WriteValue("Audio Volume", "Music Stream", editor.musicSources[ChartEditor.MUSIC_STREAM_ARRAY_POS].volume);
        iniparse.WriteValue("Audio Volume", "Guitar Stream", editor.musicSources[ChartEditor.GUITAR_STREAM_ARRAY_POS].volume);
        iniparse.WriteValue("Audio Volume", "Rhythm Stream", editor.musicSources[ChartEditor.RHYTHM_STREAM_ARRAY_POS].volume);

        iniparse.WriteValue("Audio Volume", "Audio Pan", editor.musicSources[ChartEditor.MUSIC_STREAM_ARRAY_POS].panStereo);

        iniparse.WriteValue("Audio Volume", "Clap", editor.clapSource.volume);
        iniparse.WriteValue("Audio Volume", "SFX", sfxVolume);

        iniparse.Close();
    }

    public static void DeselectCurrentUI()
    {
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
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
}
