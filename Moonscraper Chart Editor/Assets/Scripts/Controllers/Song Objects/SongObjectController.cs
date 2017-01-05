using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public abstract class SongObjectController : SelectableClick {
    protected const float CHART_CENTER_POS = 0;

    protected ChartEditor editor;
    public SongObject songObject = null;
    Bounds bounds;

    public abstract void Delete(bool update = true);
    public abstract void UpdateSongObject();

    Collider col3d;
    Collider2D col2d;

    Vector2 colSize = Vector2.zero;

    void OnMouseDown()
    {
        Debug.Log(GetAABBBoundsRect());
    }

    protected void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        col3d = GetComponent<Collider>();
        col2d = GetComponent<Collider2D>();

        /************ Note Unity documentation- ************/
        // Bounds: The world space bounding volume of the collider.
        // Note that this will be an empty bounding box if the collider is disabled or the game object is inactive.
        if (col3d)
            colSize = col3d.bounds.size;
        else if (col2d)
            colSize = col2d.bounds.size;
    }

    protected virtual void OnEnable()
    {
        Update();
    }

    protected virtual void Update()
    {
        if (songObject != null && songObject.position >= editor.minPos && songObject.position < editor.maxPos)
            UpdateSongObject();
        else if (songObject != null)
            gameObject.SetActive(false);
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

    public Rect GetAABBBoundsRect()
    {
        if (colSize == Vector2.zero)
            throw new System.Exception("No collision attached to object");

        Vector2 min = new Vector2(transform.position.x - colSize.x, transform.position.y - colSize.y);
        return new Rect(min, colSize);
    }

    public bool AABBcheck(Rect rect)
    {
        Rect colRect;

        try
        {
            colRect = GetAABBBoundsRect();
        }
        catch
        {
            return false;
        }

        // AABB, check for any gaps
        if (colRect.x < rect.x + rect.width &&
               colRect.x + colRect.width > rect.x &&
               colRect.y < rect.y + rect.height &&
               colRect.height + colRect.y > rect.y)
        {
            if (songObject.classID == (int)SongObject.ID.Starpower)
            {
                Debug.Log(colRect);
            }
            return true;
        }
        else
        { 
            return false;
        }
    }
}
