using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public abstract class SongObjectController : SelectableClick {
    static int lastDeleteFrame = 0;
    bool deleteStart = false;

    protected const float CHART_CENTER_POS = 0;

    protected ChartEditor editor;
    public SongObject songObject = null;
    Renderer ren;
    Bounds bounds;

    public abstract void Delete();
    public abstract void UpdateSongObject();

    Collider col3d;
    Collider2D col2d;

    protected void Awake()
    {
        ren = GetComponent<Renderer>();
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        col3d = GetComponent<Collider>();
        col2d = GetComponent<Collider2D>();
    }

    protected virtual void Update()
    {
#if false
        if (ren.isVisible || songObject != null && (songObject.position > editor.minPos && songObject.position < editor.maxPos))
            UpdateSongObject();
#else
        if (songObject != null && songObject.position >= editor.minPos && songObject.position < editor.maxPos)
            UpdateSongObject();
        else if (songObject != null)
            gameObject.SetActive(false);
#endif
    }
    
    protected void OnBecameVisible()
    {
        UpdateSongObject();
    }

    public override void OnSelectableMouseOver()
    {
        // Delete the object on erase tool
        if (Toolpane.currentTool == Toolpane.Tools.Eraser && Input.GetMouseButton(0) && Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            Delete();
        }
    }

    protected void Init(SongObject _songObject)
    {
        songObject = _songObject;
    }

    public override void OnSelectableMouseDown()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            editor.currentSelectedObject = songObject;
        }
    }

    public bool AABBcheck(Rect rect)
    {
        Rect colRect;

        // Lower left corner
        if (col3d)
            colRect = new Rect(col3d.bounds.min, col3d.bounds.size);
        else if (col2d)
            colRect = new Rect(col2d.bounds.min, col2d.bounds.size);
        else
        {
            Debug.Log("No collider");
            return false;
        }

        // AABB, check for any gaps
        if (colRect.x < rect.x + rect.width &&
               colRect.x + colRect.width > rect.x &&
               colRect.y < rect.y + rect.height &&
               colRect.height + colRect.y > rect.y)
        {
            return true;
        }
        else
            return false;
    }
}
