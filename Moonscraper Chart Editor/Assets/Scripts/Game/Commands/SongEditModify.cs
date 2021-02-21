// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;
using System.Collections.Generic;

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
            AddAndInvokeSubActions(before, after, subActions, ChartEditor.Instance.currentHelperContext);
        }
        else
        {
            InvokeSubActions();
        }
    }

    public override void RevokeSongEditCommand()
    {
        RevokeSubActions();
    }

    public static void AddAndInvokeSubActions(SongObject before, SongObject after, IList<BaseAction> subActions, in NoteFunctions.Context context)
    {
        AddAndInvokeSubAction(new CloneAction(before, after), subActions);

        switch ((SongObject.ID)before.classID)
        {
            case SongObject.ID.Note:
                {
                    Note oldNote = before as Note;
                    Note newNote = after as Note;

                    PerformNoteCorrections(oldNote, newNote, subActions, context);
                    break;
                }
            default:
                break;
        }
    }

    static void PerformNoteCorrections(Note oldNote, Note newNote, IList<BaseAction> subActions, in NoteFunctions.Context context)
    {
        ChartEditor editor = ChartEditor.Instance;
        Song song = editor.currentSong;

        // Perform sustain capping for previous drum rolls if the sustain length changes to where we're starting a new roll
        if (NoteFunctions.SustainsAreDrumRollsRuleActive(context))
        {
            // Check to see if we're cutting off a roll already present and starting a new one instead
            if (oldNote.length == 0 && newNote.length > 0)
            {
                int realNotePos = SongObjectHelper.FindObjectPosition(oldNote, ChartEditor.Instance.currentChart.chartObjects);
                UnityEngine.Debug.Assert(realNotePos != SongObjectHelper.NOTFOUND);
                Note currentNote = ChartEditor.Instance.currentChart.chartObjects[realNotePos] as Note;

                Note previousDrumRoll = currentNote.previousSeperateNote;
                while (previousDrumRoll != null)
                {
                    if (previousDrumRoll.rawNote == oldNote.rawNote && previousDrumRoll.length > 0)
                        break;

                    previousDrumRoll = previousDrumRoll.previous;
                }

                if (previousDrumRoll != null)
                {
                    previousDrumRoll = NoteFunctions.GetDrumRollStartNote(previousDrumRoll, context);
                }

                if (previousDrumRoll != null)
                {
                    // Possible overlap, modify this roll and all the roles leading up to it
                    Note rollNote = previousDrumRoll;

                    while (rollNote != null && rollNote.tick < oldNote.tick)
                    {
                        Note clonedRoll = rollNote.CloneAs<Note>();
                        clonedRoll.length = clonedRoll.GetCappedLength(oldNote, song);
                        AddAndInvokeSubAction(new CloneAction(rollNote, clonedRoll), subActions);

                        rollNote = rollNote.next;
                    }
                }
            }

            // Next, check if we're rolling into another roll on the same lane
            if (newNote.length > oldNote.length)
            {
                int realNotePos = SongObjectHelper.FindObjectPosition(oldNote, ChartEditor.Instance.currentChart.chartObjects);
                UnityEngine.Debug.Assert(realNotePos != SongObjectHelper.NOTFOUND);
                Note currentNote = ChartEditor.Instance.currentChart.chartObjects[realNotePos] as Note;

                // Check if we're already a part of another roll
                Note rollRoot = NoteFunctions.GetDrumRollStartNote(currentNote, context);

                UnityEngine.Debug.LogFormat("Roll root: tick = {0}, pad = {1}", rollRoot.tick, rollRoot.drumPad);

                Note nextDrumRoll = rollRoot.nextSeperateNote;
                int rollsIncluded = 0;
                while (nextDrumRoll != null)
                {
                    if (nextDrumRoll.length > 0)
                        ++rollsIncluded;

                    if (rollsIncluded >= SongConfig.MAX_ROLL_LANES && nextDrumRoll.length > 0)
                    {
                        // Reached roll, only allowed 2 rolls at a time
                        break;
                    }

                    if (((nextDrumRoll.mask & rollRoot.mask) != 0) && nextDrumRoll.length > 0)
                    {
                        break;
                    }

                    nextDrumRoll = nextDrumRoll.next;
                }

                if (nextDrumRoll != null)
                {
                    Note rollNote = rollRoot;

                    UnityEngine.Debug.LogFormat("Next drum roll: tick = {0}, pad = {1}", nextDrumRoll.tick, nextDrumRoll.drumPad);

                    while (rollNote != null && rollNote.tick < nextDrumRoll.tick)
                    {
                        Note clonedRoll = rollNote.CloneAs<Note>();
                        clonedRoll.length = clonedRoll.GetCappedLength(nextDrumRoll, song);
                        AddAndInvokeSubAction(new CloneAction(rollNote, clonedRoll), subActions);

                        rollNote = rollNote.next;
                    }
                }
            }
        }
    }
}

public class SongEditModifyValidated : SongEditAdd
{
    public SongEditModifyValidated(Note before, Note after) : base(after)
    {
        UnityEngine.Debug.Assert(after.song == null, "Must add a new song object!");
        UnityEngine.Debug.Assert(before.tick == after.tick, "Song object is being moved rather than modified!");
        UnityEngine.Debug.Assert(CloneAction.FindObjectToModify(before) != null, "Unable to find a song object to modify!");
    }
}
