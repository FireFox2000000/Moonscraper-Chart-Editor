using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditDelete : SongEditCommand
{
    public SongEditDelete(IList<SongObject> songObjects) : base(songObjects) { SnapshotGameSettings(); }
    public SongEditDelete(SongObject songObject) : base(songObject) { SnapshotGameSettings(); }
    bool extendedSustainsEnabled;

    public static void ApplyAction(IList<SongObject> songObjects)
    {
        foreach (SongObject songObject in songObjects)
        {
            ApplyAction(songObject);
        }
    }

    public static void ApplyAction(SongObject songObject)
    {
        ChartEditor editor = ChartEditor.Instance;

        // Find each item
        if (songObject.GetType().IsSubclassOf(typeof(ChartObject)) || songObject.GetType() == typeof(ChartObject))
        {
            TryDeleteSongObject((ChartObject)songObject, editor.currentChart.chartObjects);
        }
        else
        {
            if (songObject.GetType().IsSubclassOf(typeof(Event)) || songObject.GetType() == typeof(Event))
            {
                TryDeleteSongObject((Event)songObject, editor.currentSong.events);
            }
            else
            {
                TryDeleteSongObject(songObject, editor.currentSong.syncTrack);
            }
        }
    }

    static void TryDeleteSongObject<T>(T songObject, IList<T> arrayToSearch) where T : SongObject
    {
        int arrayPos = SongObjectHelper.FindObjectPosition(songObject, arrayToSearch);

        if (arrayPos != SongObjectHelper.NOTFOUND)
        {
            T foundSongObject = arrayToSearch[arrayPos];
            foundSongObject.Delete(false);
        }
        else
        {
            Debug.LogError("Delete SongObject command cannot find a song object to delete!");
        }
    }

    public override void Invoke()
    {
        ApplyAction(songObjects);
        PostExecuteUpdate();
    }

    public override void Revoke()
    {
        List<SongObject> overwriteList = new List<SongObject>();

        SongEditAdd.ApplyAction(songObjects, overwriteList, extendedSustainsEnabled);
        Debug.Assert(overwriteList.Count <= 0, "SongEditDelete revoke overwrote an object. Should be adding an object that was deleted.");

        PostExecuteUpdate();
    }

    void SnapshotGameSettings()
    {
        extendedSustainsEnabled = GameSettings.extendedSustainsEnabled;
    }
}
