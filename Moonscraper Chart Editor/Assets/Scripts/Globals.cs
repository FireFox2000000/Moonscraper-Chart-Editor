using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Globals : MonoBehaviour {
    public const uint FULL_STEP = 768;
    public static readonly float STANDARD_BEAT_RESOLUTION = 192.0f;
    public static readonly string LINE_ENDING = "\r\n";

    [Header("Initialize GUI")]
    public Toggle clapToggle;
    public Toggle viewModeToggle;
    public AudioCalibrationMenuScript audioCalibrationMenu;

    public static readonly int NOTFOUND = -1;
    public static readonly string TABSPACE = "  ";

    // 2D
    public static Sprite[] strumSprites { get; private set; }
    public static Sprite[] hopoSprites { get; private set; }
    public static Sprite[] tapSprites { get; private set; }
    public static Material[] sustainColours { get; private set; }
    public static Sprite[] spStrumSprite { get; private set; }
    public static Sprite[] spHopoSprite { get; private set; }
    public static Sprite[] spTapSprite { get; private set; }
    public static Sprite openSustainSprite { get; private set; }

    // 3D
    public static MeshFilter standardModel { get; private set; }
    public static MeshFilter spModel { get; private set; }
    public static MeshFilter openModel { get; private set; }

    public static Renderer strumRenderer { get; private set; }
    public static Renderer hopoRenderer { get; private set; }
    public static Renderer tapRenderer { get; private set; }
    public static Renderer openRenderer { get; private set; }
    public static Renderer spStrumRenderer { get; private set; }
    public static Renderer spHopoRenderer { get; private set; }
    public static Renderer spTapRenderer { get; private set; }

    public static Material[] strumColors { get; private set; }
    public static Material[] tapColors { get; private set; }

    public static Material spTemp { get; private set; }
    public static Material spTapTemp { get; private set; }

    public static Material[] openMaterials { get; private set; }

    [Header("Note sprites")]
    [SerializeField]
    Sprite[] strumNotes = new Sprite[6];
    [SerializeField]
    Sprite[] hopoNotes = new Sprite[6];
    [SerializeField]
    Sprite[] tapNotes = new Sprite[6];
    [SerializeField]
    Material[] sustains = new Material[6];
    [SerializeField]
    Sprite[] spStrumNote = new Sprite[6];
    [SerializeField]
    Sprite[] spHOPONote = new Sprite[6];
    [SerializeField]
    Sprite[] spTapNote = new Sprite[6];
    [SerializeField]
    Sprite openSustain;

    [Header("Note models")]
    [SerializeField]
    MeshFilter standardNoteModel;
    [SerializeField]
    MeshFilter starpowerNoteModel;
    [SerializeField]
    MeshFilter openNoteModel;
    [SerializeField]

    [Header("Note renderers")]
    Renderer strum3dRenderer;
    [SerializeField]
    Renderer hopo3dRenderer;
    [SerializeField]
    Renderer tap3dRenderer;
    [SerializeField]
    Renderer open3dRenderer;
    [SerializeField]
    Renderer spStrum3dRenderer;
    [SerializeField]
    Renderer spHopo3dRenderer;
    [SerializeField]
    Renderer spTap3dRenderer;

    [Header("Note colours")]
    [SerializeField]
    Material[] strum3dColorMaterials = new Material[6];
    [SerializeField]
    Material[] tap3dColorMaterials = new Material[5];

    [SerializeField]
    Material spTempColor;
    [SerializeField]
    Material spTapTempColor;

    [SerializeField]
    Material[] open3dMaterials = new Material[4];

    [Header("Area range")]
    public RectTransform area;
    [Header("Misc.")]
    [SerializeField]
    Button defaultViewSwitchButton;

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
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null ||
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>() == null)
                return false;
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
     
    public static ClapToggle clapSetting = ClapToggle.NONE;
    public static ClapToggle clapProperties = ClapToggle.NONE;
    public static int audioCalibrationMS = 200;                     // Increase to start the audio sooner
    public static ApplicationMode applicationMode = ApplicationMode.Editor;
    public static ViewMode viewMode { get; private set; }
    public static bool extendedSustainsEnabled = false;
    public static bool sustainGapEnabled { get; set; }
    public static Step sustainGapStep;
    public static int sustainGap { get { return sustainGapStep.value; } set { sustainGapStep.value = value; } }
    public static bool bot = true;
    static float _sfxVolume = 1;
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

        bool configFileExisted;
        configFileExisted = System.IO.File.Exists("config.ini");

        iniparse.Open("config.ini");

        hyperspeed = (float)iniparse.ReadValue("Settings", "Hyperspeed", 5.0f);
        audioCalibrationMS = iniparse.ReadValue("Settings", "Audio calibration", 200);
        clapProperties = (ClapToggle)iniparse.ReadValue("Settings", "Clap", (int)ClapToggle.ALL);
        extendedSustainsEnabled = iniparse.ReadValue("Settings", "Extended sustains", false);
        clapSetting = ClapToggle.NONE;
        sustainGapEnabled = iniparse.ReadValue("Settings", "Sustain Gap", false);
        sustainGapStep = new Step((int)iniparse.ReadValue("Settings", "Sustain Gap Step", (int)16));

        // Audio levels
        editor.musicSources[ChartEditor.MUSIC_STREAM_ARRAY_POS].volume = (float)iniparse.ReadValue("Audio Volume", "Music Stream", 1.0f);
        editor.musicSources[ChartEditor.GUITAR_STREAM_ARRAY_POS].volume = (float)iniparse.ReadValue("Audio Volume", "Guitar Stream", 1.0f);
        editor.musicSources[ChartEditor.RHYTHM_STREAM_ARRAY_POS].volume = (float)iniparse.ReadValue("Audio Volume", "Rhythm Stream", 1.0f);
        editor.clapSource.volume = (float)iniparse.ReadValue("Audio Volume", "Clap", 1.0f);
        sfxVolume = (float)iniparse.ReadValue("Audio Volume", "SFX", 1.0f);

        iniparse.Close();
        /*
        if (!configFileExisted)
            editor.EnableMenu(audioCalibrationMenu);*/

        // Initialize notes
        strumSprites = strumNotes;          // 2D
        hopoSprites = hopoNotes;
        tapSprites = tapNotes;
        sustainColours = sustains;
        spStrumSprite = spStrumNote;
        spHopoSprite = spHOPONote;
        spTapSprite = spTapNote;

        openSustainSprite = openSustain;

        standardModel = standardNoteModel;          // 3D
        spModel = starpowerNoteModel;
        openModel = openNoteModel;

        strumRenderer = strum3dRenderer;
        hopoRenderer = hopo3dRenderer;
        tapRenderer = tap3dRenderer;
        openRenderer = open3dRenderer;
        spStrumRenderer = spStrum3dRenderer;
        spHopoRenderer = spHopo3dRenderer;
        spTapRenderer = spTap3dRenderer;

        strumColors = strum3dColorMaterials;
        tapColors = tap3dColorMaterials;

        spTemp = spTempColor;
        spTapTemp = spTapTempColor;

        openMaterials = open3dMaterials;
    }

    void Start()
    {
        if (clapSetting == ClapToggle.NONE)
            clapToggle.isOn = false;
        else
            clapToggle.isOn = true;
    }

    void Update()
    {
        // Disable controls while user is in an input field
        if (!IsTyping)
            Controls();
    }

    void Controls()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand))
        {
            if (Input.GetKeyDown("s"))
                editor.Save();
            else if (Input.GetKeyDown("o"))
                editor.Load();
        }

        if (Input.GetButtonDown("PlayPause"))
        {
            if (applicationMode == Globals.ApplicationMode.Editor)
                editor.Play();
            else if (applicationMode == Globals.ApplicationMode.Playing)
                editor.Stop();
        }

        if (Input.GetButtonDown("ToggleClap"))
        {
            if (clapToggle.isOn)
                clapToggle.isOn = false;
            else
                clapToggle.isOn = true;
        }

        if (Input.GetButtonDown("IncreaseStep"))
            snappingStep.Increment();
        else if (Input.GetButtonDown("DecreaseStep"))
            snappingStep.Decrement();

        if (Input.GetButtonDown("Delete") && editor.currentSelectedObject != null)
        {
            if (editor.currentSelectedObject.controller)
                editor.currentSelectedObject.controller.Delete();
        }

        if (Input.GetButtonDown("Start Gameplay"))
        {
            if (applicationMode != ApplicationMode.Playing)
                editor.StartGameplay();
            else
                editor.Stop();
        }
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
            viewModeToggle.isOn = value;

        editor.currentSelectedObject = null;
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

        // Audio levels
        iniparse.WriteValue("Audio Volume", "Music Stream", editor.musicSources[ChartEditor.MUSIC_STREAM_ARRAY_POS].volume);
        iniparse.WriteValue("Audio Volume", "Guitar Stream", editor.musicSources[ChartEditor.GUITAR_STREAM_ARRAY_POS].volume);
        iniparse.WriteValue("Audio Volume", "Rhythm Stream", editor.musicSources[ChartEditor.RHYTHM_STREAM_ARRAY_POS].volume);
        iniparse.WriteValue("Audio Volume", "Clap", editor.clapSource.volume);
        iniparse.WriteValue("Audio Volume", "SFX", sfxVolume);

        iniparse.Close();
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
        Editor, Playing, Menu
    }

    public enum ViewMode
    {
        Chart, Song
    }
}
