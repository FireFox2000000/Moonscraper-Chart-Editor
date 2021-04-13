// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

// Use BatchedSongEditCommand where possible!

using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine;

public class BatchedICommand : ICommand
{
    List<ICommand> commands = new List<ICommand>();

    public BatchedICommand(IList<ICommand> newCommands) {
        foreach (ICommand c in newCommands) {
            if (c != null) {
                commands.Add(c);
            }
        }
    }

    public void Invoke() {
        for (int i = 0; i < commands.Count; i++) {
            commands[i].Invoke();
        }
    }

    public void Revoke() {
        for (int i = commands.Count - 1; i >= 0; i--) {
            commands[i].Revoke();
        }
    }
}
