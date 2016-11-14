using UnityEngine;
using System.Collections;

public abstract class SongObjectController : MonoBehaviour {
    protected MovementController movement;

    public abstract void Delete();
    public abstract void UpdateSongObject();

    protected void OnMouseOver()
    {
        // Delete the object on right-click
        if (Input.GetMouseButtonDown(1) && movement != null && movement.applicationMode == MovementController.ApplicationMode.Editor)
        {
            Delete();
        }
    }

    public void Init(MovementController _movement)
    {
        movement = _movement;
    }
}
