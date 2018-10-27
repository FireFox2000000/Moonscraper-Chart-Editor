using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditModify<T> : SongEditCommand where T : SongObject
{
    T before { get { return songObjects[0] as T; } }
    T after { get { return songObjects[1] as T; } }

    public SongEditModify(T before, T after) 
    {
        Debug.Assert(after.song == null, "Must add a new song object!");
        Debug.Assert(before.tick == after.tick, "Song object is being moved rather than modified!");

        songObjects.Add(before.Clone());
        songObjects.Add(after);             // After should be a new object, take ownership to save allocation
    }

    public override void Invoke()
    {
        CloneInto(FindObjectToModify(before), after);
        PostExecuteUpdate();
    }

    public override void Revoke()
    {
        CloneInto(FindObjectToModify(after), before);
        PostExecuteUpdate();
    }

    void CloneInto(SongObject objectToCopyInto, SongObject objectToCopyFrom)
    {
        switch ((SongObject.ID)objectToCopyInto.classID)
        {
            case SongObject.ID.Note:
                (objectToCopyInto as Note).CopyFrom((objectToCopyFrom as Note));
                break;

            case SongObject.ID.ChartEvent:
                (objectToCopyInto as ChartEvent).CopyFrom((objectToCopyFrom as ChartEvent));
                break;

            case SongObject.ID.BPM:
                (objectToCopyInto as BPM).CopyFrom((objectToCopyFrom as BPM));
                break;

            case SongObject.ID.Event:
                (objectToCopyInto as Event).CopyFrom((objectToCopyFrom as Event));
                break;

            case SongObject.ID.Section:
                (objectToCopyInto as Section).CopyFrom((objectToCopyFrom as Section));
                break;

            default:
                Debug.LogError("Object to modify not supported.");
                break;
        }
    }

    SongObject FindObjectToModify(SongObject so)
    {
        ChartEditor editor = ChartEditor.Instance;
        Song song = editor.currentSong;
        Chart chart = editor.currentChart;

        int index;

        switch ((SongObject.ID)so.classID)
        {
            case SongObject.ID.Note:
                index = SongObjectHelper.FindObjectPosition(so as Note, chart.notes);
                return chart.notes[index];

            case SongObject.ID.Starpower:
                index = SongObjectHelper.FindObjectPosition(so as Starpower, chart.starPower);
                return chart.starPower[index];

            case SongObject.ID.ChartEvent:
                index = SongObjectHelper.FindObjectPosition(so as ChartEvent, chart.events);
                return chart.events[index];

            case SongObject.ID.BPM:
                index = SongObjectHelper.FindObjectPosition(so as BPM, song.bpms);
                return song.bpms[index];

            case SongObject.ID.TimeSignature:
                index = SongObjectHelper.FindObjectPosition(so as TimeSignature, song.timeSignatures);
                return song.timeSignatures[index];

            case SongObject.ID.Section:
                index = SongObjectHelper.FindObjectPosition(so as Section, song.sections);
                return song.sections[index];

            case SongObject.ID.Event:
                index = SongObjectHelper.FindObjectPosition(so as Event, song.events);
                return song.events[index];

            default:
                Debug.LogError("Object to modify not implemented for object. Object will not be modified.");
                break;
        }

        return so;
    }
}
