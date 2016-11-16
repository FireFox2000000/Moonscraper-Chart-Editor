using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public abstract class SongObjectController : MonoBehaviour {
    private SongObject songObject = null;
    Renderer ren;

    public abstract void Delete();
    public abstract void UpdateSongObject();

    protected void Awake()
    {
        ren = GetComponent<Renderer>();
    }

    protected void Update()
    {
        if (ren.isVisible)
            UpdatePosition();
    }

    protected void OnBecameVisible()
    {
        UpdatePosition();
    }

    public virtual void UpdatePosition()
    {
        transform.position = new Vector3(transform.position.x, songObject.song.ChartPositionToWorldYPosition(songObject.position), transform.position.z);
    }

    protected void OnMouseOver()
    {
        // Delete the object on right-click
        if (Input.GetMouseButtonDown(1) && Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            Delete();
        }
    }

    protected void Init(SongObject _songObject)
    {
        songObject = _songObject;
    }
}
