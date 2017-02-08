using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabGlobals : MonoBehaviour {

    ChartEditor editor;

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

    static Vector2 noteColliderSize, spColliderSize, bpmColliderSize, tsColliderSize, sectionColliderSize;

    Vector2 GetColliderSize(GameObject gameObject)
    {
        Vector2 size;

        GameObject copy = Instantiate(gameObject);
        copy.SetActive(true);
        Collider col3d = copy.GetComponent<Collider>();
        Collider2D col2d = copy.GetComponent<Collider2D>();

        /************ Note Unity documentation- ************/
        // Bounds: The world space bounding volume of the collider.
        // Note that this will be an empty bounding box if the collider is disabled or the game object is inactive.
        if (col3d)
            size = col3d.bounds.size;
        else if (col2d)
            size = col2d.bounds.size;
        else
            size = Vector2.zero;

        Destroy(copy);
        return size;
    }

    // Use this for initialization
    void Awake () {
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

        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        // Collect prefab collider sizes
        noteColliderSize = GetColliderSize(editor.notePrefab);
        spColliderSize = GetColliderSize(editor.starpowerPrefab);
        bpmColliderSize = GetColliderSize(editor.bpmPrefab);
        tsColliderSize = GetColliderSize(editor.tsPrefab);
        sectionColliderSize = GetColliderSize(editor.sectionPrefab);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public static Rect GetCollisionRect(SongObject songObject, float posOfChart = 0)
    {
        Vector2 colliderSize;
        Vector2 position;

        switch ((SongObject.ID)songObject.classID)
        {
            case (SongObject.ID.Note):
                colliderSize = noteColliderSize;
                position = new Vector2(NoteController.noteToXPos((Note)songObject), 0);
                break;
            case (SongObject.ID.Starpower):
                colliderSize = spColliderSize;
                position = new Vector2(StarpowerController.position, 0);
                break;
            case (SongObject.ID.BPM):
                colliderSize = bpmColliderSize;
                position = new Vector2(BPMController.position, 0);
                break;
            case (SongObject.ID.TimeSignature):
                colliderSize = tsColliderSize;
                position = new Vector2(TimesignatureController.position, 0);
                break;
            case (SongObject.ID.Section):
                colliderSize = sectionColliderSize;
                position = new Vector2(SectionController.position, 0);
                break;
            default:
                return new Rect();
        }

        Vector2 min = new Vector2(position.x + posOfChart - colliderSize.x / 2, position.y - colliderSize.y / 2);
        return new Rect(min, noteColliderSize);
    }

    public static bool HorizontalCollisionCheck(Rect rectA, Rect rectB)
    {
        if (rectA.width == 0 || rectB.width == 0)
            return false;

        // AABB, check for any gaps
        if (rectA.x <= rectB.x + rectB.width &&
               rectA.x + rectA.width >= rectB.x)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
