[System.Serializable]
public class ChartEvent : ChartObject
{
    private readonly ID _classID = ID.ChartEvent;

    public override int classID { get { return (int)_classID; } }

    public string eventName;

    public ChartEvent(ChartEvent chartEvent) : base(chartEvent.position)
    {
        eventName = chartEvent.eventName;
    }

    public ChartEvent(uint _position, string _eventName) : base(_position)
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

    internal override string GetSaveString()
    {
        // 1728 = E T
        return Globals.TABSPACE + position + " = E " + eventName + Globals.LINE_ENDING;
    }

    public override SongObject Clone()
    {
        return new ChartEvent(this);
    }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as ChartEvent).eventName == eventName)
            return true;
        else
            return false;
    }
}

