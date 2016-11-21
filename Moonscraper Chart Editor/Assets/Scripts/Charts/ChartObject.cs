public abstract class ChartObject : SongObject
{
    public Chart chart;

    public ChartObject(Song song, Chart _chart, uint position) : base(song, position)
    {
        chart = _chart;
    }
}

public class StarPower : ChartObject
{
    private readonly ID _classID = ID.Starpower;

    public override int classID { get { return (int)_classID; } }

    public uint length;

    public StarPower(Song song, Chart chart, uint _position, uint _length) : base (song, chart, _position)
    {
        length = _length;
    }

    public override string GetSaveString()
    {
        // 768 = S 2 768
        return Globals.TABSPACE + position + " = S 2 " + length + "\n";
    }
}

public class ChartEvent : ChartObject
{
    private readonly ID _classID = ID.ChartEvent;

    public override int classID { get { return (int)_classID; } }

    public string eventName;

    public ChartEvent(Song song, Chart chart, uint _position, string _eventName) : base(song, chart, _position)
    {
        eventName = _eventName;
    }

    protected override bool Equals(SongObject b)
    {
        if (b.GetType() == typeof(ChartEvent))
        {
            ChartEvent realB = b as ChartEvent;
            if (position == realB.position && eventName == realB.eventName)
                return true;
            else
                return false;
        }
        else
            return base.Equals(b);
    }

    public override string GetSaveString()
    {
        // 1728 = E T
        return Globals.TABSPACE + position + " = E " + eventName + "\n";
    }
}
