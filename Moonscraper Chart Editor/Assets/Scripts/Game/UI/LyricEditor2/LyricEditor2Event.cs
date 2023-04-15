// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;
using System.Collections.Generic;

public class LyricEditor2Event
{
    class PickupCommand : MoonscraperEngine.ICommand {
        public delegate void RevokeCallback(string formattedText, Event oldEvent);
        public delegate void InvokeCallback();

        RevokeCallback revokeCommand;
        InvokeCallback invokeCommand;
        SongEditDelete deleteCommand;
        LyricEditor2Controller mainController;
        Event referencedEvent;
        string formattedText;

        public PickupCommand(Event referencedEvent, string formattedText, InvokeCallback invokeCommand, RevokeCallback revokeCommand, LyricEditor2Controller mainController) {
            this.referencedEvent = referencedEvent;
            deleteCommand = new SongEditDelete(referencedEvent);
            this.formattedText = formattedText;
            this.revokeCommand = revokeCommand;
            this.invokeCommand = invokeCommand;
            this.mainController = mainController;
        }

        public void Invoke() {
            deleteCommand.Invoke();
            mainController.editCommands.Add(deleteCommand);
            invokeCommand();
        }

        public void Revoke() {
            deleteCommand.Revoke();
            mainController.editCommands.Remove(deleteCommand);
            revokeCommand(formattedText, (Event)deleteCommand.GetSongObjects()[0]);
        }
    }

    public bool hasBeenPlaced {get; private set;}
    public string text {get; private set;}
    public string formattedText = "";
    public uint? tick {get {return referencedEvent?.tick;}}
    LyricEditor2Controller mainController;
    Event referencedEvent;


    public LyricEditor2Event(string text, LyricEditor2Controller controller) {
        this.text = text;
        this.hasBeenPlaced = false;
        this.mainController = controller;
    }

    public LyricEditor2Event(Event existingEvent, LyricEditor2Controller controller) {
        SetEvent(existingEvent);
        this.mainController = controller;
    }

    public void SetEvent(Event existingEvent) {
        referencedEvent = existingEvent;
        this.text = existingEvent?.title ?? "";
        this.hasBeenPlaced = (existingEvent != null);
    }

    public void SetText(string newText) 
    {
        this.text = newText;
        if (referencedEvent != null) 
        {
            SetTick(referencedEvent.tick);
        }
    }

    public void SetTick (uint tick) 
    {
        List<SongEditCommand> commands = new List<SongEditCommand>();

        if (this.referencedEvent != null)
        {
            commands.Add(new SongEditDelete(this.referencedEvent));
        }

        Event newLyric = new Event(this.text, tick);
        commands.Add(new SongEditAdd(newLyric));

        BatchedSongEditCommand batchedCommands = new BatchedSongEditCommand(commands);
        batchedCommands.Invoke();
        mainController.editCommands.Add(batchedCommands);

        this.referencedEvent = newLyric;
        this.hasBeenPlaced = true;
    }

    // Remove lyric from the editor
    public MoonscraperEngine.ICommand Pickup() 
    {
        if (this.referencedEvent != null) 
        {
            return new PickupCommand(referencedEvent, formattedText, InvokePickup, RevokePickup, mainController);
        }
        return null;
    }

    // Invoke a pickup command
    public void InvokePickup() {
        referencedEvent = null;
        hasBeenPlaced = false;
    }

    // Revert to a previous state after Pickup() is revoked
    public void RevokePickup(string formattedText, Event oldEvent) {
        this.formattedText = formattedText;
        SetEvent(oldEvent);
    }
}
