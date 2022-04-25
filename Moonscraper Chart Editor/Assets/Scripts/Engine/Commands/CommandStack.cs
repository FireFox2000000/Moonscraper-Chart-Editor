// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

#define DEBUG_METHOD_CALL

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonscraperEngine
{
    public class CommandStack
    {
        List<ICommand> commands;
        int currentStackIndex;

        public bool isAtStart { get { return currentStackIndex < 0; } }
        public bool isAtEnd { get { return currentStackIndex >= commands.Count - 1; } }

        protected virtual void OnPush(ICommand command) { }
        protected virtual void OnPop(ICommand command) { }

        public CommandStack()
        {
            Clear();
        }

        public void Clear()
        {
            commands = new List<ICommand>();
            currentStackIndex = -1;
        }

        public void Push()
        {
            if (!isAtEnd)
            {
                OnPush(commands[++currentStackIndex]);
                commands[currentStackIndex].Invoke();
            }
        }

        public void Push(ICommand command)
        {
#if DEBUG_METHOD_CALL
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(1);
            Debug.LogFormat("Command Stack Push: {0} in file {1} at line {2}", frame.GetMethod().Name, System.IO.Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber());
#endif
            OnPush(command);
            ResetTail();
            ++currentStackIndex;
            command.Invoke();
            commands.Add(command);
        }

        public void Pop()
        {
#if DEBUG_METHOD_CALL
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(1);
            Debug.LogFormat("Command Stack Pop: {0} in file {1} at line {2}", frame.GetMethod().Name, System.IO.Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber());
#endif
            if (!isAtStart)
            {
                OnPop(commands[currentStackIndex]);
                commands[currentStackIndex--].Revoke();
            }
        }

        public void ResetTail()
        {
            commands.RemoveRange(currentStackIndex + 1, commands.Count - (currentStackIndex + 1));
        }
    }
}
