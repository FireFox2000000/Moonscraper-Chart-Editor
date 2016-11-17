using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Globals : MonoBehaviour {
    public static readonly uint FULL_STEP = 768;
    public Text stepText;

    static int lsbOffset = 3;
    static int _step = 4;

    // Settings
    public static float hyperspeed = 5.0f;
    public static int step { get { return _step; } }
    public static ClapToggle clapToggle = ClapToggle.NONE;
    public static int audioCalibrationMS = 100;                     // Increase to start the audio sooner
    public static ApplicationMode applicationMode = ApplicationMode.Editor;

    ClapToggle currentClapSettings;

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
        if (_step > 1)
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

    public void ToggleClap()
    {
        if (clapToggle == ClapToggle.NONE)
            clapToggle = currentClapSettings;
        else
            clapToggle = ClapToggle.NONE;
    }

    public static readonly int NOTFOUND = -1;
    public static readonly string TABSPACE = "  ";

    public static Sprite[] normalSprites { get; private set; }
    public static Sprite[] hopoSprites { get; private set; }
    public static Sprite[] tapSprites { get; private set; }
    public static Material[] sustainColours { get; private set; }

    [Header("Note sprites")]
    [SerializeField]
    Sprite[] normalNotes = new Sprite[6];
    [SerializeField]
    Sprite[] hopoNotes = new Sprite[6];
    [SerializeField]
    Sprite[] tapNotes = new Sprite[6];
    [SerializeField]
    Material[] sustains = new Material[6];

    void Awake()
    {
        normalSprites = normalNotes;
        hopoSprites = hopoNotes;
        tapSprites = tapNotes;
        sustainColours = sustains;

        // Load clap settings (eventually from save file)
        currentClapSettings = ClapToggle.ALL;

        // Enable clap
        ToggleClap();
    }

    int lastWidth = Screen.width;
    void Update()
    {
        stepText.text = "1/" + _step.ToString();

        if (applicationMode == ApplicationMode.Editor)
        { 
            if (Input.GetKeyDown("a"))
            {
                IncrementStep();
                Debug.Log(_step);
            }
            if (Input.GetKeyDown("s"))
            {
                DecrementStep();
                Debug.Log(_step);
            }
                
            /*
            if (Input.GetKeyDown("a"))
            {
                if (hyperspeed < 15)
                    hyperspeed += 1;
                Debug.Log(hyperspeed);
            }
            if (Input.GetKeyDown("s"))
            {
                if (hyperspeed > 1)
                    hyperspeed -= 1;
                Debug.Log(hyperspeed);
            */
        }

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
    }

    [System.Flags]
    public enum ClapToggle
    {
        NONE = 0, ALL = ~0, STRUM = 1, HOPO = 2, TAP = 4
    }

    public enum ApplicationMode
    {
        Editor, Playing
    }
}
