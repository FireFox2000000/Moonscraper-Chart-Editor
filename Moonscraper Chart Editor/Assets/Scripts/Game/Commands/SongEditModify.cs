// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

public class SongEditModify<T> : SongEditCommand where T : SongObject
{
    T before { get { return songObjects[0] as T; } }
    T after { get { return songObjects[1] as T; } }

    public SongEditModify(T before, T after)
    {
        UnityEngine.Debug.Assert(after.song == null, "Must add a new song object!");
        UnityEngine.Debug.Assert(before.tick == after.tick, "Song object is being moved rather than modified!");

        songObjects.Add(before.Clone());
        songObjects.Add(after);

        if (typeof(T) == typeof(Note))
        {
            Note beforeNote = before as Note;
            Note afterNote = after as Note;
            UnityEngine.Debug.Assert(beforeNote.rawNote == afterNote.rawNote, "Note modifying is not supported by SongEditModify<T>(T, T). Use SongEditModify(Note, Note) instead.");
        }
    }

    public override void InvokeSongEditCommand()
    {
        if (subActions.Count <= 0)
        {
            CloneInto(FindObjectToModify(before), after);
        }
        else
        {
            InvokeSubActions();
        }
    }

    public override void RevokeSongEditCommand()
    {
        if (subActions.Count <= 0)
        {
            CloneInto(FindObjectToModify(after), before);
        }
        else
        {
            RevokeSubActions();
        }
    }

    void CloneInto(SongObject objectToCopyInto, SongObject objectToCopyFrom)
    {
        Chart chart = ChartEditor.Instance.currentChart;

        switch ((SongObject.ID)objectToCopyInto.classID)
        {
            case SongObject.ID.Note:
                (objectToCopyInto as Note).CopyFrom((objectToCopyFrom as Note));
                break;

            case SongObject.ID.Starpower:
                SongEditAdd.SetNotesDirty(objectToCopyInto as Starpower, chart.chartObjects);
                SongEditAdd.SetNotesDirty(objectToCopyFrom as Starpower, chart.chartObjects);
                (objectToCopyInto as Starpower).CopyFrom((objectToCopyFrom as Starpower));
                break;

            case SongObject.ID.ChartEvent:
                AddAndInvokeSubAction(new DeleteAction(objectToCopyInto), subActions);
                AddAndInvokeSubAction(new AddAction(objectToCopyFrom), subActions);
                break;

            case SongObject.ID.BPM:
                (objectToCopyInto as BPM).CopyFrom((objectToCopyFrom as BPM));
                ChartEditor.Instance.songObjectPoolManager.SetAllPoolsDirty();
                break;

            case SongObject.ID.TimeSignature:
                (objectToCopyInto as TimeSignature).CopyFrom((objectToCopyFrom as TimeSignature));
                break;

            case SongObject.ID.Event:
                AddAndInvokeSubAction(new DeleteAction(objectToCopyInto), subActions);
                AddAndInvokeSubAction(new AddAction(objectToCopyFrom), subActions);
                break;

            case SongObject.ID.Section:
                (objectToCopyInto as Section).CopyFrom((objectToCopyFrom as Section));
                break;

            default:
                UnityEngine.Debug.LogError("Object to modify not supported.");
                break;
        }

        if (objectToCopyInto.controller)
            objectToCopyInto.controller.SetDirty();
    }

    public static SongObject FindObjectToModify(SongObject so)
    {
        ChartEditor editor = ChartEditor.Instance;
        Song song = editor.currentSong;
        Chart chart = editor.currentChart;

        int index;

        switch ((SongObject.ID)so.classID)
        {
            case SongObject.ID.Note:
                index = SongObjectHelper.FindObjectPosition(so as Note, chart.notes);
                if (index == SongObjectHelper.NOTFOUND)
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
                UnityEngine.Debug.LogError("Object to modify not implemented for object. Object will not be modified.");
                break;
        }

        return so;
    }
}

public class SongEditModifyValidated : SongEditAdd
{
    public SongEditModifyValidated(Note before, Note after) : base(after)
    {
        UnityEngine.Debug.Assert(after.song == null, "Must add a new song object!");
        UnityEngine.Debug.Assert(before.tick == after.tick, "Song object is being moved rather than modified!");
        UnityEngine.Debug.Assert(SongEditModify<SongObject>.FindObjectToModify(before) != null, "Unable to find a song object to modify!");
    }
}
