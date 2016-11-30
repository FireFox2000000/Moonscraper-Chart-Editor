using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public abstract class SongObjectController : MonoBehaviour {
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
        if (songObject.position >= editor.minPos && songObject.position < editor.maxPos)
            UpdateSongObject();
        else
            gameObject.SetActive(false);
#endif
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
            if (!deleteStart)
                StartCoroutine(startDelete());
        }
    }

    IEnumerator startDelete()
    {
        deleteStart = true;

        while (Time.frameCount == lastDeleteFrame)
        {           
            yield return null;
        }

        lastDeleteFrame = Time.frameCount;
        Delete();
    }

    protected void Init(SongObject _songObject)
    {
        songObject = _songObject;
    }

    protected Vector2 prevMousePos = Vector2.zero;
    void OnMouseDown()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
        {
            editor.currentSelectedObject = songObject;
            prevMousePos = Input.mousePosition;
        }
    }
}
