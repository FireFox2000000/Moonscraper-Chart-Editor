// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        EventsManager.onKeyboardModeToggledEvent.Add(OnKeysModeToggled);
    }

    public override void ToolDisable()
    {
        editor.currentSelectedObject = null;
    }

    protected virtual void OnEnable()
    {
        Update();
        OnKeysModeToggled(GameSettings.keysModeEnabled);
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
            //case ((int)SongObject.ID.Note):
            //    PlaceNote.AddObjectToCurrentChart((Note)songObject, editor, update);
            //    break;
            case ((int)SongObject.ID.Starpower):
                PlaceStarpower.AddObjectToCurrentChart((Starpower)songObject, editor, update);
                break;
            case ((int)SongObject.ID.BPM):
                PlaceBPM.AddObjectToCurrentSong((BPM)songObject, editor, update);
                break;
            //case ((int)SongObject.ID.Section):
            //    PlaceSection.AddObjectToCurrentSong((Section)songObject, editor, update);
            //    break;
            case ((int)SongObject.ID.TimeSignature):
                PlaceTimesignature.AddObjectToCurrentSong((TimeSignature)songObject, editor, update);
                break;
            //case ((int)SongObject.ID.Event):
            //    PlaceEvent.AddObjectToCurrentSong((Event)songObject, editor, update);
            //    break;
            //case ((int)SongObject.ID.ChartEvent):
            //    PlaceChartEvent.AddObjectToCurrentChart((ChartEvent)songObject, editor, update);
            //    break;
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

    protected void RecordAddActionHistory<T>(T overwriteCheck, IList<T> overWriteSearch) where T : SongObject
    {
        ActionHistory.Action record = OverwriteActionHistory(overwriteCheck, overWriteSearch);
        if (record != null)
            editor.actionHistory.Insert(record);
    }

    public static ActionHistory.Action OverwriteActionHistory<T>(T overwriteCheck, IList<T> overWriteSearch) where T : SongObject
    {
        int arrayPos = SongObjectHelper.FindObjectPosition(overwriteCheck, overWriteSearch);
        if (arrayPos != SongObjectHelper.NOTFOUND)       // Found an object that matches
        {
            if (!overwriteCheck.AllValuesCompare(overWriteSearch[arrayPos]))
                // Object will changed, therefore record
                return new ActionHistory.Modify(overWriteSearch[arrayPos], overwriteCheck);
            else
                return null;
        }
        else
        {
            return new ActionHistory.Add(overwriteCheck);
        }
    }

    void OnKeysModeToggled(bool keysModeEnabled)
    {
        foreach (Renderer ren in renderers)
        {
            ren.enabled = !GameSettings.keysModeEnabled;
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(!GameSettings.keysModeEnabled);
        }
    }
}
