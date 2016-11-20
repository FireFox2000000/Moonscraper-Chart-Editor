using UnityEngine;

[RequireComponent(typeof(Renderer))]
public abstract class SongObjectController : Draggable {
    protected const float CHART_CENTER_POS = 0;

    protected ChartEditor editor;
    private SongObject songObject = null;
    Renderer ren;

    public abstract void Delete();
    public abstract void UpdateSongObject();

    protected void Awake()
    {
        ren = GetComponent<Renderer>();
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    protected virtual void Update()
    {
        if (ren.isVisible || songObject != null && (songObject.position > editor.minPos && songObject.position < editor.maxPos))
            UpdateSongObject();
    }
    
    protected void OnBecameVisible()
    {
        UpdateSongObject();
    }

    protected void OnMouseEnter()
    {
        OnMouseOver();
    }

    protected void OnMouseOver()
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
}
