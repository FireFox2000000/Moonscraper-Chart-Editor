using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditAdd : SongEditCommand
{
    List<SongObject> overwrittenSongObjects = new List<SongObject>();       // Todo properly

    public SongEditAdd(IList<SongObject> songObjects) : base(songObjects)
    {
        foreach(SongObject songObject in songObjects)
        {
            Debug.Assert(songObject.song == null, "Must add a new song object!");
        }
    }

    public SongEditAdd(SongObject songObject) : base(songObject)
    {
        Debug.Assert(songObject.song == null, "Must add a new song object!");
    }

    public override void Invoke()
    {
        ApplyAction(songObjects);
        PostExecuteUpdate();
    }

    public override void Revoke()
    {
        SongEditDelete.ApplyAction(songObjects);
        ApplyAction(overwrittenSongObjects);

        overwrittenSongObjects.Clear();

        PostExecuteUpdate();
    }

    public static void ApplyAction(IList<SongObject> songObjects)
    {
        foreach (SongObject songObject in songObjects)
        {
            ApplyAction(songObject);
        }
    }

    public static void ApplyAction(SongObject songObject)
    {
        // Todo, replace this, the functions contained within are horrible, especially for notes. 
        // Need to handle overwriting somehow?

        switch (songObject.classID)
        {
            case ((int)SongObject.ID.Note):
                throw new System.NotImplementedException();
                break;

            case ((int)SongObject.ID.Starpower):
                throw new System.NotImplementedException();
                break;

            case ((int)SongObject.ID.ChartEvent):
                AddChartEvent((ChartEvent)songObject);
                break;

            case ((int)SongObject.ID.BPM):
                AddBPM((BPM)songObject);
                break;

            case ((int)SongObject.ID.Section):
                AddSection((Section)songObject);
                break;

            case ((int)SongObject.ID.TimeSignature):
                AddTimeSignature((TimeSignature)songObject);
                break;

            case ((int)SongObject.ID.Event):
                AddEvent((Event)songObject);
                break;

            default:
                Debug.LogError("Unhandled songobject!");
                break;
        }
    }

    #region Object specific add functions

    static void AddChartEvent(ChartEvent chartEvent)
    {
        ChartEditor editor = ChartEditor.Instance;
        ChartEvent eventToAdd = new ChartEvent(chartEvent);

        editor.currentChart.Add(eventToAdd, false);
        editor.currentSelectedObject = eventToAdd;
    }

    static void AddBPM(BPM bpm)
    {
        ChartEditor editor = ChartEditor.Instance;
        BPM bpmToAdd = new BPM(bpm);
        editor.currentSong.Add(bpmToAdd, false);
        editor.currentSelectedObject = bpmToAdd;

        if (bpmToAdd.anchor != null)
        {
            bpmToAdd.anchor = bpmToAdd.song.LiveTickToTime(bpmToAdd.tick, bpmToAdd.song.resolution);
        }

        ChartEditor.Instance.songObjectPoolManager.SetAllPoolsDirty();
    }

    static void AddTimeSignature(TimeSignature timeSignature)
    {
        ChartEditor editor = ChartEditor.Instance;
        TimeSignature tsToAdd = new TimeSignature(timeSignature);
        editor.currentSong.Add(tsToAdd, false);

        editor.currentSelectedObject = tsToAdd;
    }

    static void AddEvent(Event songEvent)
    {
        ChartEditor editor = ChartEditor.Instance;
        Event eventToAdd = new Event(songEvent);
        editor.currentSong.Add(eventToAdd, false);

        editor.currentSelectedObject = eventToAdd;
    }

    static void AddSection(Section section)
    {
        ChartEditor editor = ChartEditor.Instance;
        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd, false);

        editor.currentSelectedObject = sectionToAdd;
    }

    #endregion
}
