using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {
    static int lsbOffset = 3;
    static int _step = 4;

    // Settings
    public static float hyperspeed = 5.0f;
    public static int step { get { return _step; } }
    public static ClapToggle clapToggle = ClapToggle.ALL;
    public static int audioCalibrationMS = 100;

    public static void IncrementStep()
    {
        if (_step < 768)
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

    public static void DecrementStep()
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

    public static readonly int NOTFOUND = -1;
    public static readonly string TABSPACE = "  ";

    public static Sprite[] normalSprites { get; private set; }
    public static Sprite[] hopoSprites { get; private set; }
    public static Sprite[] tapSprites { get; private set; }

    [Header("Note sprites")]
    [SerializeField]
    Sprite[] normalNotes = new Sprite[5];
    [SerializeField]
    Sprite[] hopoNotes = new Sprite[5];
    [SerializeField]
    Sprite[] tapNotes = new Sprite[5];

    void Awake()
    {
        normalSprites = normalNotes;
        hopoSprites = hopoNotes;
        tapSprites = tapNotes;
    }

    [System.Flags]
    public enum ClapToggle
    {
        NONE = 0, ALL = ~0, STRUM = 1, HOPO = 2, TAP = 4
    }
}
