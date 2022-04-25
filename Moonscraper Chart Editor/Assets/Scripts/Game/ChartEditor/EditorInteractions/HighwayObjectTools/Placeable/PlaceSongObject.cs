// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public abstract class PlaceSongObject : ToolObject {
    protected SongObject songObject;
    protected SongObjectController controller;

    Renderer[] renderers;

    protected SongObject initObject;        // Only used for moving objects

    protected abstract void SetSongObjectAndController();

    protected override void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        base.Awake();

        SongObjectController controller = GetComponent<SongObjectController>();
        controller.disableCancel = false;

        SetSongObjectAndController();

        editor.events.keyboardModeToggledEvent.Register(OnKeysModeToggled);
    }

    public override void ToolDisable()
    {
        editor.selectedObjectsManager.currentSelectedObject = null;
    }

    protected virtual void OnEnable()
    {
        Update();
        OnKeysModeToggled(Globals.gameSettings.keysModeEnabled);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        controller.SetDirty();

        songObject.song = editor.currentSong;
        songObject.tick = objectSnappedChartPos;
    }

    protected abstract void AddObject();

    public static void AddObjectToCurrentEditor(SongObject songObject, ChartEditor editor, bool update = true)
    {
        switch (songObject.classID)
        {
            default:
                Debug.LogError("Object not supported to be added to a song via this method");
                break;
        }
    }

    // Used when grabbing and moving objects with the cursor tool
    protected void MovementControls()
    {
        if (Input.GetMouseButtonUp(0))
        {
            AddObject();

            Destroy(gameObject);
        }
    }

    void OnKeysModeToggled(in bool keysModeEnabled)
    {
        foreach (Renderer ren in renderers)
        {
            ren.enabled = !Globals.gameSettings.keysModeEnabled;
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(!Globals.gameSettings.keysModeEnabled);
        }
    }
}
