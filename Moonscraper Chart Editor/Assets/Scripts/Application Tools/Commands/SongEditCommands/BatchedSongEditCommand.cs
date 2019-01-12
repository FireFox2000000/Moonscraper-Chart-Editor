using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatchedSongEditCommand : SongEditCommand
{
    protected List<SongEditCommand> commands = new List<SongEditCommand>();

    public BatchedSongEditCommand(IList<SongEditCommand> newCommands)
    {
        commands.AddRange(newCommands);

        foreach (SongEditCommand command in commands)
        {
            command.postExecuteEnabled = false;
        }
    }

    public override void Invoke()
    {
        foreach (ICommand command in commands)
        {
            command.Invoke();
        }

        PostExecuteUpdate();
    }

    public override void Revoke()
    {
        for (int i = commands.Count - 1; i >= 0; --i)
        {
            commands[i].Revoke();
        }

        PostExecuteUpdate();
    }
}
