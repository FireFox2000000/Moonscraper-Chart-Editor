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

    public abstract void Delete();
    public abstract void UpdateSongObject();

    protected void Awake()
    {
        ren = GetComponent<Renderer>();
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
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
}
