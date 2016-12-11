using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Globals : MonoBehaviour {
    public static readonly uint FULL_STEP = 768;
    public static readonly float STANDARD_BEAT_RESOLUTION = 192.0f;
    public static readonly string LINE_ENDING = "\r\n";
    static readonly uint MIN_STEP = 1;
    public Text stepText;

    [Header("Initialize GUI")]
    public Toggle clapToggle;

    static int lsbOffset = 3;
    static int _step = 4;

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

    // 3D
    public static MeshFilter standardModel { get; private set; }
    public static MeshFilter spModel { get; private set; }
    public static MeshFilter openModel { get; private set; }

    public static Renderer strumRenderer { get; private set; }
    public static Renderer hopoRenderer { get; private set; }
    public static Renderer tapRenderer { get; private set; }

    public static Material[] strumColors { get; private set; }
    public static Material[] tapColors { get; private set; }

    public static Material spTemp { get; private set; }
    public static Material spTapTemp { get; private set; }

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

    [Header("Note models")]
    [SerializeField]
    MeshFilter standardNoteModel;
    [SerializeField]
    MeshFilter starpowerNoteModel;
    [SerializeField]
    MeshFilter openNoteModel;
    [SerializeField]
    Renderer strum3dRenderer;
    [SerializeField]
    Renderer hopo3dRenderer;
    [SerializeField]
    Renderer tap3dRenderer;

    [SerializeField]
    Material[] strum3dColorMaterials = new Material[6];
    [SerializeField]
    Material[] tap3dColorMaterials = new Material[5];

    [SerializeField]
    Material spTempColor;
    [SerializeField]
    Material spTapTempColor;

    [Header("Area range")]
    public RectTransform area;

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
    public static int step { get { return _step; } }
    public static ClapToggle clapSetting = ClapToggle.NONE;
    public static int audioCalibrationMS = 100;                     // Increase to start the audio sooner
    public static ApplicationMode applicationMode = ApplicationMode.Editor;

    ChartEditor editor;
    string workingDirectory;

    void Awake()
    {
        workingDirectory = System.IO.Directory.GetCurrentDirectory();

        INIParser iniparse = new INIParser();
        iniparse.Open("config.ini");

        hyperspeed = (float)iniparse.ReadValue("Settings", "Hyperspeed", 5.0f);
        audioCalibrationMS = iniparse.ReadValue("Settings", "Audio calibration", 100);
        clapSetting = (ClapToggle)iniparse.ReadValue("Settings", "Clap", (int)ClapToggle.ALL);
        // Audio levels

        iniparse.Close();

        // Initialize notes
        strumSprites = strumNotes;          // 2D
        hopoSprites = hopoNotes;
        tapSprites = tapNotes;
        sustainColours = sustains;
        spStrumSprite = spStrumNote;
        spHopoSprite = spHOPONote;
        spTapSprite = spTapNote;

        standardModel = standardNoteModel;          // 3D
        spModel = standardNoteModel;
        openModel = openNoteModel;

        strumRenderer = strum3dRenderer;
        hopoRenderer = hopo3dRenderer;
        tapRenderer = tap3dRenderer;

        strumColors = strum3dColorMaterials;
        tapColors = tap3dColorMaterials;

        spTemp = spTempColor;
        spTapTemp = spTapTempColor;

        SetStep(16);
    }

    void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        // Initialize GUI
        editor.hyperspeedSlider.value = hyperspeed;
        clapToggle.onValueChanged.AddListener((value) => { ToggleClap(value); });
        if (clapSetting == ClapToggle.NONE)
            clapToggle.isOn = false;
        else
            clapToggle.isOn = true;
    }

    int lastWidth = Screen.width;
    void Update()
    {
        stepText.text = "1/" + _step.ToString();

        if (Screen.width != lastWidth)
        {
            // User is resizing width
            Screen.SetResolution(Screen.width, Screen.width * 9 / 16, false);
            lastWidth = Screen.width;
        }
        else
        {
            // User is resizing height
            Screen.SetResolution(Screen.height * 16 / 9, Screen.height, false);
        }

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
                editor.LoadSong();
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
            IncrementStep();
        else if (Input.GetButtonDown("DecreaseStep"))
            DecrementStep();
    }

    public void IncrementStep()
    {
        if (_step < FULL_STEP)
        {
            if (lsbOffset % 2 == 0)
            {
                _step &= 1 << (lsbOffset / 2);
                _step <<= 1;
            }
            else
            {
                _step |= 1 << (lsbOffset / 2);
            }
            ++lsbOffset;
        }
    }

    public void DecrementStep()
    {
        if (_step > MIN_STEP)
        {
            if (lsbOffset % 2 == 0)
            {
                _step &= ~(1 << ((lsbOffset - 1) / 2));
            }
            else
            {
                _step |= 1 << (lsbOffset / 2);
                _step >>= 1;              
            }

            --lsbOffset;
        }
    }

    public void SetStep(uint step)
    {
        if (step < MIN_STEP)
            step = MIN_STEP;
        else if (step > FULL_STEP)
            step = FULL_STEP;

        if (_step < step)
        {
            while (_step < step)
            {
                IncrementStep();
            }
        }
        else
        {
            while (_step > step)
            {
                DecrementStep();
            }
        }
    }

    public void ToggleClap(bool value)
    {
        if (value)
            clapSetting = ClapToggle.ALL;
        else
            clapSetting = ClapToggle.NONE;
    }

    void OnApplicationQuit()
    {
        INIParser iniparse = new INIParser();
        iniparse.Open(workingDirectory + "\\config.ini");

        iniparse.WriteValue("Settings", "Hyperspeed", hyperspeed);
        iniparse.WriteValue("Settings", "Audio calibration", audioCalibrationMS);
        iniparse.WriteValue("Settings", "Clap", (int)clapSetting);
        // Audio levels

        iniparse.Close();
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
}
