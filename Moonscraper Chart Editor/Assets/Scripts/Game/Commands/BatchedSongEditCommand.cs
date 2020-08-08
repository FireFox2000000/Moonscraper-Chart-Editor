// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public class BatchedSongEditCommand : SongEditCommand
{
    protected List<SongEditCommand> commands = new List<SongEditCommand>();

    public BatchedSongEditCommand(IList<SongEditCommand> newCommands)
    {
        commands.AddRange(newCommands);

        foreach (SongEditCommand command in commands)
        {
            command.preExecuteEnabled = false;
            command.postExecuteEnabled = false;
        }
    }

    public override void InvokeSongEditCommand()
    {
        foreach (SongEditCommand command in commands)
        {
            command.Invoke();
        }

        if (subActions.Count <= 0)
        {
            // Try to preserve forced flags on notes that aren't being moved. 

            ChartEditor editor = ChartEditor.Instance;
            Chart chart = editor.currentChart;
            List<BaseAction> newSubActions = new List<BaseAction>();

            foreach (SongEditCommand command in commands)
            {
                var commandSubActions = command.subActions;
                foreach (BaseAction action in commandSubActions)
                {
                    if (action.typeTag == BaseAction.TypeTag.DeleteForcedCorrection)
                    {
                        Note note = action.songObject as Note;
                        Debug.Assert(note != null, "Object was incorrectly tagged as DeleteForcedCorrection");

                        int index = SongObjectHelper.FindObjectPosition(note, chart.chartObjects);
                        if (index != SongObjectHelper.NOTFOUND)
                        {
                            Note foundNote = chart.chartObjects[index] as Note;
                            if (!foundNote.cannotBeForced)
                            {
                                Note.Flags flags = foundNote.flags;
                                flags |= Note.Flags.Forced;

                                Note newNote = new Note(note.tick, note.rawNote, note.length, flags);
                                newSubActions.Add(new DeleteAction(foundNote));
                                newSubActions.Add(new AddAction(newNote));
                            }
                        }
                        else
                        {
                            Debug.LogError("Unable to find note object in chart to try to undo forced correction.");
                        }
                    }
                }
            }

            foreach (BaseAction action in newSubActions)
            {
                AddAndInvokeSubAction(action, subActions);
            }
        }
        else
        {
            InvokeSubActions();
        }
    }

    public override void RevokeSongEditCommand()
    {
        RevokeSubActions();

        for (int i = commands.Count - 1; i >= 0; --i)
        {
            commands[i].Revoke();
        }
    }

    protected override UndoRedoJumpInfo GetUndoRedoJumpInfo()
    {
        SongObject lowestTickSo = null;
        SongObject highestTickSo = null;
        UndoRedoJumpInfo info = new UndoRedoJumpInfo();

        foreach (SongEditCommand command in commands)
        {
            if (command.subActions.Count > 0)
            {
                foreach(BaseAction action in command.subActions)
                {
                    SongObject so = action.songObject;
                    if (lowestTickSo == null || so.tick < lowestTickSo.tick)
                        lowestTickSo = so;

                    if (highestTickSo == null || so.tick > highestTickSo.tick)
                        highestTickSo = so;
                }
            }
            else
            {
                foreach (SongObject so in command.GetSongObjects())
                {
                    if (lowestTickSo == null || so.tick < lowestTickSo.tick)
                        lowestTickSo = so;

                    if (highestTickSo == null || so.tick > highestTickSo.tick)
                        highestTickSo = so;
                }
            }
        }

        if (lowestTickSo != null)
        {
            info.jumpToPos = lowestTickSo.tick;
            info.viewMode = lowestTickSo.GetType().IsSubclassOf(typeof(ChartObject)) ? Globals.ViewMode.Chart : Globals.ViewMode.Song;
            info.min = lowestTickSo.tick;
        }
        else
        {
            info.jumpToPos = null;
        }

        if (highestTickSo != null)
        {
            info.max = highestTickSo.tick;
        }

        return info;
    }
}
