using System;

public abstract class ChartObject : SongObject
{
    public Chart chart;

    public ChartObject(uint position) : base(position){}
}

public class StarPower : ChartObject
{
    private readonly ID _classID = ID.Starpower;

    public override int classID { get { return (int)_classID; } }

    public uint length;

    public StarPower(uint _position, uint _length) : base (_position)
    {
        length = _length;
    }

    public StarPower(StarPower _starpower) : base(_starpower.position)
    {
        length = _starpower.length;
    }

    public override string GetSaveString()
    {
        // 768 = S 2 768
        return Globals.TABSPACE + position + " = S 2 " + length + Globals.LINE_ENDING; ;
    }

    public override SongObject Clone()
    {
        return new StarPower(this);
    }

    public override bool ValueCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as StarPower).length == length)
            return true;
        else
            return false;
    }
}

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

    public override string GetSaveString()
    {
        // 1728 = E T
        return Globals.TABSPACE + position + " = E " + eventName + Globals.LINE_ENDING;
    }

    public override SongObject Clone()
    {
        return new ChartEvent(this);
    }

    public override bool ValueCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as ChartEvent).eventName == eventName)
            return true;
        else
            return false;
    }
}
