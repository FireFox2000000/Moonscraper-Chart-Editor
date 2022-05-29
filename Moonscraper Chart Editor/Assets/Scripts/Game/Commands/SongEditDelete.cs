// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using MoonscraperChartEditor.Song;

public class SongEditDelete : SongEditCommand
{
    public SongEditDelete(IList<SongObject> songObjects) : base(songObjects) { SnapshotGameSettings(); }
    public SongEditDelete(SongObject songObject) : base(songObject) { SnapshotGameSettings(); }

    public override void InvokeSongEditCommand()
    {
        if (subActions.Count <= 0)
        {
            AddAndInvokeSubActions(songObjects, subActions);
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

    public void AddAndInvokeSubActions(IList<SongObject> songObjects, IList<BaseAction> subActions)
    {
        foreach (SongObject songObject in songObjects)
        {
            AddAndInvokeSubActions(songObject, subActions);
        }
    }

    public void AddAndInvokeSubActions(SongObject songObject, IList<BaseAction> subActions)
    {
        Note note = songObject as Note;
        Note next = null;

        if (note != null)
        {
            var chartObjects = ChartEditor.Instance.currentChart.chartObjects;
            int arrayPos = SongObjectHelper.FindObjectPosition(note, chartObjects);
            if (arrayPos != SongObjectHelper.NOTFOUND)
            {
                Note foundNote = chartObjects[arrayPos] as Note;         
                if (foundNote != null)
                {
                    next = foundNote.nextSeperateNote;
                }
            }
        }

        AddAndInvokeSubAction(new DeleteAction(songObject), subActions);

        if (next != null)
        {
            GeneratePostDeleteSubActions(next, subActions);
        }
    }

    public void GeneratePostDeleteSubActions(Note nextSeperateNoteFromDeleted, IList<BaseAction> subActions)
    {
        if (subActions != null && nextSeperateNoteFromDeleted != null)    // Overwrite can be null for special case with song edit add, as corrections can mess SEA up
        {
            // Perform note corrections
            Note.Flags wholeChordFlags = nextSeperateNoteFromDeleted.flags;
            if (nextSeperateNoteFromDeleted.cannotBeForced)
                wholeChordFlags &= ~Note.Flags.Forced;

            wholeChordFlags &= ~Note.PER_NOTE_FLAGS;

            foreach (Note chordNote in nextSeperateNoteFromDeleted.chord)
            {
                if (wholeChordFlags != (chordNote.flags & ~Note.PER_NOTE_FLAGS))
                {
                    bool willBeDeleted = false;
                    foreach (SongObject so in songObjects)
                    {
                        Note note = so as Note;
                        if (note != null && note.tick == chordNote.tick && note.rawNote == chordNote.rawNote)
                        {
                            willBeDeleted = true;
                            break;
                        }
                    }

                    if (willBeDeleted)      // If we continue it may false flag BatchedSongEditCommand to fixed forced flag on a note that shouldn't be corrected
                        continue;

                    Note newChordNote = new Note(chordNote.tick, chordNote.rawNote, chordNote.length, wholeChordFlags | (chordNote.flags & Note.PER_NOTE_FLAGS));

                    AddAndInvokeSubAction(new DeleteAction(chordNote), subActions);
                    AddAndInvokeSubAction(new AddAction(newChordNote, BaseAction.TypeTag.DeleteForcedCorrection), subActions);
                }
            }
        }
    }
}
