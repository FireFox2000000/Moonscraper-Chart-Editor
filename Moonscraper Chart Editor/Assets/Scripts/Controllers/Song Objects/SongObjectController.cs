using UnityEngine;

[RequireComponent(typeof(Renderer))]
public abstract class SongObjectController : MonoBehaviour {
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

    protected void OnMouseOver()
    {
        // Delete the object on erase tool
        /*
        if (Input.GetButtonDown("Delete Object") && Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            Delete();
        }*/
    }

    protected void Init(SongObject _songObject)
    {
        songObject = _songObject;
    }
}
