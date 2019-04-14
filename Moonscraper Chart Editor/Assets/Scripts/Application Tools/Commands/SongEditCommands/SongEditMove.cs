using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditMove : SongEditCommand
{
    protected List<SongEditCommand> commands = new List<SongEditCommand>();

    public SongEditMove(SongEditDelete deleteCommands, SongEditAdd addCommands)
    {
        commands.Add(deleteCommands);
        commands.Add(addCommands);
    }

    public override void InvokeSongEditCommand()
    {
        foreach(SongEditCommand command in commands)
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

            foreach(BaseAction action in newSubActions)
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
}
