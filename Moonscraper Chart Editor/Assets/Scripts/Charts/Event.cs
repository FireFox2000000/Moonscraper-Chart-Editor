[System.Serializable]
public class Event : SongObject
{
    private readonly ID _classID = ID.Event;

    public override int classID { get { return (int)_classID; } }

    public string title;

    public Event(string _title, uint _position) : base(_position)
    {
        title = _title;
    }

    public Event(Event songEvent) : base(songEvent.position)
    {
        position = songEvent.position;
        title = songEvent.title;
    }

    internal override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = E \"" + title + "\"" + Globals.LINE_ENDING;
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
            if (position == realB.position && title == realB.title)
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
            if (position < b.position)
                return true;
            else if (position == b.position)
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
