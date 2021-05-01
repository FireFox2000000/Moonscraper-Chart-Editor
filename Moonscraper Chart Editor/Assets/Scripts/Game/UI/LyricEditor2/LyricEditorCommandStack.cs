using System.Collections;
using System.Collections.Generic;
using MoonscraperEngine;

public class LyricEditorCommandStack : CommandStack
{
    public Event<ICommand> onPush = new Event<ICommand>();
    public Event<ICommand> onPop = new Event<ICommand>();

    protected override void OnPush(ICommand command)
    {
        onPush.Fire(command);
    }

    protected override void OnPop(ICommand command)
    {
        onPop.Fire(command);
    }
}
