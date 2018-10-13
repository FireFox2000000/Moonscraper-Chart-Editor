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
        ApplyAction(songObjects, overwrittenSongObjects);
        PostExecuteUpdate();
    }

    public override void Revoke()
    {
        SongEditDelete.ApplyAction(songObjects);
        ApplyAction(overwrittenSongObjects, null);

        overwrittenSongObjects.Clear();

        PostExecuteUpdate();
    }

    public static void ApplyAction(IList<SongObject> songObjects, IList<SongObject> overwriteList)
    {
        foreach (SongObject songObject in songObjects)
        {
            ApplyAction(songObject, overwriteList);
        }
    }

    public static void ApplyAction(SongObject songObject, IList<SongObject> overwriteList)
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
                AddChartEvent((ChartEvent)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.BPM):
                AddBPM((BPM)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.Section):
                AddSection((Section)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.TimeSignature):
                AddTimeSignature((TimeSignature)songObject, overwriteList);
                break;

            case ((int)SongObject.ID.Event):
                AddEvent((Event)songObject, overwriteList);
                break;

            default:
                Debug.LogError("Unhandled songobject!");
                break;
        }
    }

    #region Object specific add functions

    static void TryRecordOverwrite<T>(T songObject, IList<T> searchObjects, IList<SongObject> overwrittenObjects) where T : SongObject
    {
        if (overwrittenObjects == null)
            return;

        ChartEditor editor = ChartEditor.Instance;
        int overwriteIndex = SongObjectHelper.FindObjectPosition(songObject.tick, editor.currentChart.chartObjects);

        if (overwriteIndex != SongObjectHelper.NOTFOUND)
        {
            overwrittenObjects.Add(editor.currentChart.chartObjects[overwriteIndex].Clone());
        }
    }

    static void AddChartEvent(ChartEvent chartEvent, IList<SongObject> overwrittenList)
    {      
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(chartEvent, editor.currentChart.chartObjects, overwrittenList);

        ChartEvent eventToAdd = new ChartEvent(chartEvent);

        editor.currentChart.Add(eventToAdd, false);
        Debug.Log("Added new chart event");
    }

    static void AddBPM(BPM bpm, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(bpm, editor.currentSong.bpms, overwrittenList);

        BPM bpmToAdd = new BPM(bpm);
        editor.currentSong.Add(bpmToAdd, false);
        Debug.Log("Added new bpm");

        if (bpmToAdd.anchor != null)
        {
            bpmToAdd.anchor = bpmToAdd.song.LiveTickToTime(bpmToAdd.tick, bpmToAdd.song.resolution);
        }

        ChartEditor.Instance.songObjectPoolManager.SetAllPoolsDirty();
    }

    static void AddTimeSignature(TimeSignature timeSignature, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(timeSignature, editor.currentSong.timeSignatures, overwrittenList);

        TimeSignature tsToAdd = new TimeSignature(timeSignature);
        editor.currentSong.Add(tsToAdd, false);
        Debug.Log("Added new timesignature");
    }

    static void AddEvent(Event songEvent, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(songEvent, editor.currentSong.events, overwrittenList);

        Event eventToAdd = new Event(songEvent);
        editor.currentSong.Add(eventToAdd, false);

        Debug.Log("Added new song event");
    }

    static void AddSection(Section section, IList<SongObject> overwrittenList)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(section, editor.currentSong.sections, overwrittenList);

        Section sectionToAdd = new Section(section);
        editor.currentSong.Add(sectionToAdd, false);

        Debug.Log("Added new section");
    }

    #endregion
}
