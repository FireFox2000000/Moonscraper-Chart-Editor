using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandStack {
    List<ICommand> commands = new List<ICommand>();
    int currentStackIndex = -1;
    ICommand currentTentativeCommand = null;

    public void Push(ICommand command)
    {
        if (currentTentativeCommand != null)
        {
            // Tentative is in effect, add it to the stack without invoking it
            ++currentStackIndex;
            commands.RemoveRange(currentStackIndex, commands.Count - currentStackIndex);
            commands.Add(currentTentativeCommand);
            currentTentativeCommand = null;
        }

        ++currentStackIndex;
        commands.RemoveRange(currentStackIndex, commands.Count - currentStackIndex);
        command.Invoke();
        commands.Add(command);
    }

    public void Pop()
    {
        if (currentTentativeCommand != null)
        {
            // Act like this is on top of the stack
            currentTentativeCommand.Revoke();
            commands.Add(currentTentativeCommand);
            currentTentativeCommand = null;
        }
        else
            commands[currentStackIndex--].Revoke();
    }

    // Pushing tentative consecutively will revoke the previous tentative before invoking this new tentative
    public void PushTentative(ICommand command)
    {
        if (currentTentativeCommand != null)
            currentTentativeCommand.Revoke();

        currentTentativeCommand = command;
        currentTentativeCommand.Invoke();
    }
}
