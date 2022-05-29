// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using MoonscraperChartEditor.Song;

// Provides bare bones functionality, no other validation or checking

public abstract class BaseAction
{
    public enum TypeTag
    {
        None,
        DeleteForcedCorrection,
    }

    public SongObject songObject { get; private set; }
    public TypeTag typeTag { get; private set; }

    protected BaseAction(SongObject so)
    {
        songObject = so.Clone();        // Clone to preserve the state it was added in
        typeTag = TypeTag.None;
    }

    protected BaseAction(SongObject so, TypeTag tag)
    {
        songObject = so.Clone();        // Clone to preserve the state it was added in
        typeTag = tag;
    }

    public abstract void Invoke();
    public abstract void Revoke();
}

public class AddAction : BaseAction
{
    public AddAction(SongObject so) : base(so) {}
    public AddAction(SongObject so, TypeTag tag) : base(so, tag) { }

    public override void Invoke()
    {
        ApplyAction(songObject);
    }

    public override void Revoke()
    {
        DeleteAction.ApplyAction(songObject);
    }

    public static void ApplyAction(SongObject songObject)
    {
        ChartEditor editor = ChartEditor.Instance;
        Song song = editor.currentSong;
        Chart chart = editor.currentChart;

        songObject = songObject.Clone();    // Add a new version of the object

        switch (songObject.classID)
        {
            case (int)SongObject.ID.Note:
                Note note = songObject as Note;
                chart.Add(note);

                foreach (Note chordNote in note.chord)
                {
                    if (chordNote.controller)
                        chordNote.controller.SetDirty();
                }

                Note next = note.nextSeperateNote;
                if (next != null)
                {
                    foreach (Note chordNote in next.chord)
                    {
                        if (chordNote.controller)
                            chordNote.controller.SetDirty();
                    }
                }
                break;

            case (int)SongObject.ID.Starpower:
                Starpower sp = songObject as Starpower;
                chart.Add(sp, false);
                SongEditAdd.SetNotesDirty(sp, editor.currentChart.chartObjects);
                break;

            case (int)SongObject.ID.ChartEvent:
                chart.Add(songObject as ChartObject, false);
                break;

            case (int)SongObject.ID.Section:
            case (int)SongObject.ID.Event:
                song.Add(songObject as Event, false);
                break;

            case (int)SongObject.ID.BPM:
                song.Add(songObject as SyncTrack, false);
                ChartEditor.Instance.songObjectPoolManager.SetAllPoolsDirty();
                break;

            case (int)SongObject.ID.TimeSignature:
                song.Add(songObject as SyncTrack, false);
                break;
        }
    }
}

public class DeleteAction : BaseAction
{
    public DeleteAction(SongObject so) : base(so) { }
    public DeleteAction(SongObject so, TypeTag tag) : base(so, tag) { }

    public override void Invoke()
    {
        ApplyAction(songObject);
    }

    public override void Revoke()
    {
        AddAction.ApplyAction(songObject);
    }

    public static void ApplyAction(SongObject songObject)
    {
        ChartEditor editor = ChartEditor.Instance;

        if (songObject.GetType().IsSubclassOf(typeof(ChartObject)) || songObject.GetType() == typeof(ChartObject))
        {
            TryDeleteSongObject((ChartObject)songObject, editor.currentChart.chartObjects);
        }
        else
        {
            if (songObject.GetType().IsSubclassOf(typeof(Event)) || songObject.GetType() == typeof(Event))
            {
                TryDeleteSongObject((Event)songObject, editor.currentSong.eventsAndSections);
            }
            else
            {
                TryDeleteSongObject((SyncTrack)songObject, editor.currentSong.syncTrack);
            }
        }
    }

    static void TryDeleteSongObject<T>(T songObject, IList<T> arrayToSearch) where T : SongObject
    {
        int arrayPos = SongObjectHelper.FindObjectPosition(songObject, arrayToSearch);

        if (arrayPos != SongObjectHelper.NOTFOUND)
        {
            T foundSongObject = arrayToSearch[arrayPos];

            Note next = null;
            if ((SongObject.ID)foundSongObject.classID == SongObject.ID.Note)
            {
                next = (foundSongObject as Note).nextSeperateNote;
            }

            // Actual deletion of the note from the chart
            DeleteSongObject(foundSongObject);

            if (next != null)
            {
                foreach (Note chordNote in next.chord)
                {
                    if (chordNote.controller)
                        chordNote.controller.SetDirty();
                }
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Delete SongObject command cannot find a song object to delete!");
        }
    }

    static void DeleteSongObject<T>(T foundSongObject) where T : SongObject
    {
        bool updateCache = false;

        if (foundSongObject.controller)
        {
            foundSongObject.controller.gameObject.SetActive(false); // Forces the controller to release the note
        }

        // Update the surrounding objects based on changes made here
        switch ((SongObject.ID)foundSongObject.classID)
        {
            case SongObject.ID.Starpower:
                {
                    ChartEditor.Instance.songObjectPoolManager.SetAllPoolsDirty();  // Update notes in the range
                    break;
                }

            case SongObject.ID.BPM:
                {
                    ChartEditor.Instance.songObjectPoolManager.SetAllPoolsDirty();  // Re position all objects
                    break;
                }

            case SongObject.ID.Note:
                {
                    Note note = foundSongObject as Note;

                    // Update the previous note in the case of chords with 2 notes
                    if (note.previous != null && note.previous.controller)
                        note.previous.controller.SetDirty();
                    if (note.next != null && note.next.controller)
                        note.next.controller.SetDirty();

                    break;
                }
        }

        {
            ChartObject chartObj = foundSongObject as ChartObject;
            if (chartObj != null)
            {
                Chart chart = chartObj.chart;
                if (chart != null)
                    chart.Remove(chartObj, updateCache);

                return;
            }
        }

        {
            SyncTrack syncObj = foundSongObject as SyncTrack;
            if (syncObj != null)
            {
                if (syncObj.tick != 0)
                {
                    Song song = foundSongObject.song;
                    if (song != null)
                        song.Remove(syncObj, updateCache);
                }

                return;
            }
        }

        {
            Event eventObj = foundSongObject as Event;
            if (eventObj != null)
            {
                Song song = foundSongObject.song;
                if (song != null)
                    song.Remove(eventObj, updateCache);

                return;
            }
        }

        UnityEngine.Debug.LogError("Unhandled songobject deletion case!");
    }
}
