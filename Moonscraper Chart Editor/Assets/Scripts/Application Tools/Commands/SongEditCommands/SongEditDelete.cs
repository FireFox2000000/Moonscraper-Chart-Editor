using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditDelete : SongEditCommand
{
    public SongEditDelete(IList<SongObject> songObjects) : base(songObjects) { SnapshotGameSettings(); }
    public SongEditDelete(SongObject songObject) : base(songObject) { SnapshotGameSettings(); }
    bool extendedSustainsEnabled;
    List<SongObject> overwrittenSongObjects = new List<SongObject>();

    public static void ApplyAction(IList<SongObject> songObjects, IList<SongObject> overwrittenList)
    {
        foreach (SongObject songObject in songObjects)
        {
            ApplyAction(songObject, overwrittenList);
        }
    }

    public static void ApplyAction(SongObject songObject, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;

        // Find each item
        if (songObject.GetType().IsSubclassOf(typeof(ChartObject)) || songObject.GetType() == typeof(ChartObject))
        {
            TryDeleteSongObject((ChartObject)songObject, editor.currentChart.chartObjects, overwrittenList);
        }
        else
        {
            if (songObject.GetType().IsSubclassOf(typeof(Event)) || songObject.GetType() == typeof(Event))
            {
                TryDeleteSongObject((Event)songObject, editor.currentSong.events, overwrittenList);
            }
            else
            {
                TryDeleteSongObject((SyncTrack)songObject, editor.currentSong.syncTrack, overwrittenList);
            }
        }
    }

    static void TryDeleteSongObject<T>(T songObject, IList<T> arrayToSearch, IList<SongObject> overwrittenList) where T : SongObject
    {
        int arrayPos = SongObjectHelper.FindObjectPosition(songObject, arrayToSearch);

        if (arrayPos != SongObjectHelper.NOTFOUND)
        {
            T foundSongObject = arrayToSearch[arrayPos];
 
            Note next = null;
            {
                Note note = foundSongObject as Note;
                if (note != null)
                {
                    next = note.nextSeperateNote;
                }
            }

            foundSongObject.Delete(false);
            
            if (overwrittenList != null && next != null)    // Overwrite can be null for special case with song edit add, as corrections can mess SEA up
            {
                // Perform note corrections
                Note.Flags flags = next.flags;
                if (next.cannotBeForced)
                    flags &= ~Note.Flags.Forced;

                foreach (Note chordNote in next.chord)
                {
                    if (flags != chordNote.flags)
                    {
                        overwrittenList.Add(chordNote.Clone());

                        Note newChordNote = new Note(chordNote.tick, chordNote.rawNote, chordNote.length, flags);
                        chordNote.CopyFrom(newChordNote);

                        if (chordNote.controller)
                            chordNote.controller.SetDirty();
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Delete SongObject command cannot find a song object to delete!");
        }
    }

    public override void InvokeSongEditCommand()
    {
        ApplyAction(songObjects, overwrittenSongObjects);
    }

    public override void RevokeSongEditCommand()
    {
        List<SongObject> overwriteList = new List<SongObject>();

        SongEditAdd.ApplyAction(songObjects, overwriteList, extendedSustainsEnabled);
        Debug.Assert(overwriteList.Count <= 0, "SongEditDelete revoke overwrote an object. Should be adding an object that was deleted.");

        overwriteList.Clear();

        SongEditAdd.ApplyAction(overwrittenSongObjects, overwriteList, extendedSustainsEnabled);
        Debug.AssertFormat(overwriteList.Count == overwrittenSongObjects.Count, "SongEditDelete revoke overwrite invalid. OverwriteList = {0}, overwritten = {1}", overwriteList.Count, overwrittenSongObjects.Count);
        overwrittenSongObjects.Clear();
        
    }

    void SnapshotGameSettings()
    {
        extendedSustainsEnabled = GameSettings.extendedSustainsEnabled;
    }
}
