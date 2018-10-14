// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

[System.Serializable]
public class Event : SongObject
{
    private readonly ID _classID = ID.Event;

    public override int classID { get { return (int)_classID; } }

    public string title = "";

    public Event(string _title, uint _position) : base(_position)
    {
        title = _title;
    }

    public Event(Event songEvent) : base(songEvent.tick)
    {
        CopyFrom(songEvent);
    }

    public void CopyFrom(Event songEvent)
    {
        tick = songEvent.tick;
        title = songEvent.title;
    }

    internal override string GetSaveString()
    {
        return Globals.TABSPACE + tick + " = E \"" + title + "\"" + Globals.LINE_ENDING;
    }

    public static bool regexMatch(string line)
    {
        return new System.Text.RegularExpressions.Regex(@"\d+ = E " + @"""[^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }

    public override SongObject Clone()
    {
        return new Event(this);
    }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as Event).title == title)
            return true;
        else
            return false;
    }

    protected override bool Equals(SongObject b)
    {
        if (b.GetType() == typeof(Event))
        {
            Event realB = b as Event;
            if (tick == realB.tick && title == realB.title)
                return true;
            else
                return false;
        }
        else
            return base.Equals(b);
    }

    protected override bool LessThan(SongObject b)
    {
        if (b.GetType() == typeof(Event))
        {
            Event realB = b as Event;
            if (tick < b.tick)
                return true;
            else if (tick == b.tick)
            {
                if (string.Compare(title, realB.title) < 0)
                    return true;
            }

            return false;
        }
        else
            return base.LessThan(b);
    }

    public override void Delete(bool update = true)
    {
        base.Delete(update);
        if (song != null)
            song.Remove(this, update);
    }
}
