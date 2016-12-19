using UnityEngine;
using System.Collections;

public abstract class PlaceSongObject : ToolObject {
    protected SongObject songObject;
    protected SongObjectController controller;

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;
    }

    protected virtual void OnEnable()
    {
        Update();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        songObject.song = editor.currentSong;
        songObject.position = objectSnappedChartPos;
    }

    protected abstract void AddObject();

    protected void MovementControls()
    {
        if (Input.GetMouseButtonUp(0))
        {
            AddObject();

            Destroy(gameObject);
        }
    }
}
