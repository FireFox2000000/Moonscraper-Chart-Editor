using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SongEditCommand : ICommand {

    protected List<SongObject> songObjects = new List<SongObject>();
    protected List<SongObject> validatedSongObjects = new List<SongObject>();
    protected bool hasValidatedSongObjects = false;
    public bool postExecuteEnabled = true;

    void AddClone(SongObject songObject)
    {
        songObjects.Add(songObject.Clone());
    }

    protected SongEditCommand()
    {
    }

    protected SongEditCommand(IList<SongObject> songObjects)
    {
        this.songObjects.Capacity = songObjects.Count;
        for (int i = 0; i < songObjects.Count; ++i)
        {
            AddClone(songObjects[i]);
        }
    }

    protected SongEditCommand(SongObject songObject) 
    {
        AddClone(songObject);
    }

    public abstract void Invoke();

    public abstract void Revoke();

    protected void PostExecuteUpdate()
    {
        if (!postExecuteEnabled)
            return;

        ChartEditor editor = ChartEditor.Instance;

        editor.currentChart.UpdateCache();
        editor.currentSong.UpdateCache();
        editor.FixUpBPMAnchors();
        if (Toolpane.currentTool != Toolpane.Tools.Note)
            editor.currentSelectedObject = null;

        ChartEditor.isDirty = true;

        var soList = validatedSongObjects.Count > 0 ? validatedSongObjects : songObjects;
        SongObject lowestTickSo = null;

        foreach (SongObject songObject in soList)
        {
            if (lowestTickSo  == null || songObject.tick < lowestTickSo.tick)
                lowestTickSo = songObject;
        }

        if (lowestTickSo != null)
        {
            uint jumpToPos = lowestTickSo.tick;
            Globals.ViewMode viewMode = lowestTickSo.GetType().IsSubclassOf(typeof(ChartObject)) ? Globals.ViewMode.Chart : Globals.ViewMode.Song;
            editor.FillUndoRedoSnapInfo(jumpToPos, viewMode);
        }
    }
}
