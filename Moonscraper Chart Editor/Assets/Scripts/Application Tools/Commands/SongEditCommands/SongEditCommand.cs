using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SongEditCommand : ICommand {

    protected List<SongObject> songObjects = new List<SongObject>();
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

        //Debug.Assert(songObjects.Count > 0, "No song objects were provided in a song edit command");
        //if (songObjects.Count > 0)
        //{
        //    uint jumpToPos = songObjects[0].tick;       // Jump to the lowest tick, maybe search through if nessacary?
        //    editor.movement.SetPosition(jumpToPos);
        //}
    }
}
