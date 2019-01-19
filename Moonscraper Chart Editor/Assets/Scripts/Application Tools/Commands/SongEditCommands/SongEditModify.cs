using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditModify<T> : SongEditAdd where T : SongObject
{
    public SongEditModify(T before, T after) : base(after)
    {
        Debug.Assert(after.song == null, "Must add a new song object!");
        Debug.Assert(before.tick == after.tick, "Song object is being moved rather than modified!");
        Debug.Assert(FindObjectToModify(before) != null, "Unable to find a song object to modify!");
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
                if(index == SongObjectHelper.NOTFOUND)
                {
                    return null;
                }
                return chart.notes[index];

            case SongObject.ID.Starpower:
                index = SongObjectHelper.FindObjectPosition(so as Starpower, chart.starPower);
                if (index == SongObjectHelper.NOTFOUND)
                {
                    return null;
                }
                return chart.starPower[index];

            case SongObject.ID.ChartEvent:
                index = SongObjectHelper.FindObjectPosition(so as ChartEvent, chart.events);
                if (index == SongObjectHelper.NOTFOUND)
                {
                    return null;
                }
                return chart.events[index];

            case SongObject.ID.BPM:
                index = SongObjectHelper.FindObjectPosition(so as BPM, song.bpms);
                if (index == SongObjectHelper.NOTFOUND)
                {
                    return null;
                }
                return song.bpms[index];

            case SongObject.ID.TimeSignature:
                index = SongObjectHelper.FindObjectPosition(so as TimeSignature, song.timeSignatures);
                if (index == SongObjectHelper.NOTFOUND)
                {
                    return null;
                }
                return song.timeSignatures[index];

            case SongObject.ID.Section:
                index = SongObjectHelper.FindObjectPosition(so as Section, song.sections);
                if (index == SongObjectHelper.NOTFOUND)
                {
                    return null;
                }
                return song.sections[index];

            case SongObject.ID.Event:
                index = SongObjectHelper.FindObjectPosition(so as Event, song.events);
                if (index == SongObjectHelper.NOTFOUND)
                {
                    return null;
                }
                return song.events[index];

            default:
                Debug.LogError("Object to modify not implemented for object. Object will not be modified.");
                break;
        }

        return so;
    }
}
