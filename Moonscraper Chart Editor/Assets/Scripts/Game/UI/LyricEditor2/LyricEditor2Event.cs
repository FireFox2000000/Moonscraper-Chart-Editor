using MoonscraperChartEditor.Song;
using System.Collections.Generic;

public class LyricEditor2Event
{
    private Event referencedEvent;
    public bool hasBeenPlaced {get; private set;}
    public string text {get; private set;}
    public string formattedText;
    public uint? tick {get {return referencedEvent?.tick;}}


    public LyricEditor2Event (string text) {
        this.text = text;
        this.hasBeenPlaced = false;
    }

    public LyricEditor2Event(Event existingEvent) {
        referencedEvent = existingEvent;
        this.text = existingEvent.title;
        this.hasBeenPlaced = true;
    }

    // Remove lyric from the editor
    public void Pickup() {
        List<SongEditCommand> commands = new List<SongEditCommand>();
        if (this.referencedEvent != null) {
            commands.Add(new SongEditDelete(this.referencedEvent));
        }
        this.referencedEvent = null;
        this.hasBeenPlaced = false;
        BatchedSongEditCommand batchedCommands = new BatchedSongEditCommand(commands);
        ChartEditor.Instance.commandStack.Push(batchedCommands);
    }

    public void SetText (string newText) {
        this.text = newText;
        if (referencedEvent != null) {
            this.SetTick(referencedEvent.tick);
        }
    }

    public void SetTick (uint tick) {
        List<SongEditCommand> commands = new List<SongEditCommand>();

        if (this.referencedEvent != null)
        {
            commands.Add(new SongEditDelete(this.referencedEvent));
        }

        Event newLyric = new Event(this.text, tick);
        commands.Add(new SongEditAdd(newLyric));

        BatchedSongEditCommand batchedCommands = new BatchedSongEditCommand(commands);
        ChartEditor.Instance.commandStack.Push(batchedCommands);

        this.referencedEvent = newLyric;
        this.hasBeenPlaced = true;
    }
}
