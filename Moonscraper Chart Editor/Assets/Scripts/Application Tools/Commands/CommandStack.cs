using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandStack {
    List<ICommand> commands = new List<ICommand>();
    int currentStackIndex = -1;
    public bool isAtStart { get { return currentStackIndex < 0; } }
    public bool isAtEnd { get { return currentStackIndex >= commands.Count - 1; } }

    public void Push()
    {
        if (!isAtEnd)
            commands[++currentStackIndex].Invoke();
    }

    public void Push(ICommand command)
    {
        ResetTail();
        ++currentStackIndex;   
        command.Invoke();
        commands.Add(command);
    }

    public void Pop()
    {
        if (!isAtStart)
            commands[currentStackIndex--].Revoke();
    }

    public void ResetTail()
    {
        commands.RemoveRange(currentStackIndex + 1, commands.Count - (currentStackIndex + 1));
    }
}
