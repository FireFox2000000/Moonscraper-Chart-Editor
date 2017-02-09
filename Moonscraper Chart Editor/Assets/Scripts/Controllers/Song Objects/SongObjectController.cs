using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public abstract class SongObjectController : SelectableClick {
    public const float CHART_CENTER_POS = 0;

    protected ChartEditor editor;
    protected SongObject songObject = null;
    Bounds bounds;

    public abstract void UpdateSongObject();
    public bool disableCancel = true;

    Collider col3d;
    Collider2D col2d;

    Vector2 colSize = Vector2.zero;
    /*
    void OnMouseDown()
    {
        Debug.Log(GetAABBBoundsRect());
    }*/

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
        if (songObject != null)
            UpdateSongObject();
    }

    
    void OnDisable()
    {
        if (disableCancel && songObject != null)
        {
            Init(null, null);
        }
    }

    protected bool moveCheck { get { return Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0) && !Input.GetMouseButton(1); } }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (moveCheck)
        {
            editor.groupMove.SetSongObjects(songObject);
            songObject.Delete();
        }
    }

    void Update()
    {
        if (songObject != null)
            UpdateCheck();
        else
            gameObject.SetActive(false);
    }

    protected virtual void UpdateCheck()
    {
        if (songObject != null && songObject.position >= editor.minPos && songObject.position < editor.maxPos)
            UpdateSongObject();
        else if (songObject != null)
            gameObject.SetActive(false);
    }
    
    protected void OnBecameVisible()
    {
        UpdateCheck();
    }
    /*
    public override void OnSelectableMouseOver()
    {
        // Delete the object on erase tool
        if (Toolpane.currentTool == Toolpane.Tools.Eraser && Input.GetMouseButton(0) && Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            Delete();
        }
    }*/

    protected void Init(SongObject _songObject, SongObjectController controller)
    {
        if (_songObject == null && songObject != null)
        {
            songObject.controller = null;
        }

        songObject = _songObject;

        if (songObject != null)
            songObject.controller = controller;
    }

    public override void OnSelectableMouseDown()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            editor.currentSelectedObject = songObject;
        }

        // Delete the object on erase tool or by holding right click and pressing left-click
        if ((Toolpane.currentTool == Toolpane.Tools.Eraser && Input.GetMouseButtonDown(0) && Globals.applicationMode == Globals.ApplicationMode.Editor) ||
            (Input.GetMouseButtonDown(0) && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1)))
        {
            editor.actionHistory.Insert(new ActionHistory.Delete(songObject));
            songObject.Delete();
            editor.currentSelectedObject = null;
        }
    }
    /*
    public Rect GetAABBBoundsRect()
    {
        // Move this and get direct static value

        if (colSize == Vector2.zero)
            throw new System.Exception("No collision attached to object");

        Vector2 min = new Vector2(transform.position.x - colSize.x / 2, transform.position.y - colSize.y / 2);
        return new Rect(min, colSize);
    }

    public bool HorizontalCollisionCheck(Rect rectA, Rect rectB)
    {
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
    }*/
}
