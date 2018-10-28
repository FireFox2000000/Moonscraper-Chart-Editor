// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

[System.Serializable]
public class ChartEvent : ChartObject
{
    private readonly ID _classID = ID.ChartEvent;

    public override int classID { get { return (int)_classID; } }

    public string eventName { get; private set; }

    public ChartEvent(ChartEvent chartEvent) : base(chartEvent.tick)
    {
        eventName = chartEvent.eventName;
    }

    public ChartEvent(uint _position, string _eventName) : base(_position)
    {
        eventName = _eventName;
    }

    public void CopyFrom(ChartEvent chartEvent)
    {
        tick = chartEvent.tick;
        eventName = chartEvent.eventName;
    }

    protected override bool Equals(SongObject b)
    {
        if (b.GetType() == typeof(ChartEvent))
        {
            ChartEvent realB = b as ChartEvent;
            if (tick == realB.tick && eventName == realB.eventName)
                return true;
            else
                return false;
        }
        else
            return base.Equals(b);
    }

    protected override bool LessThan(SongObject b)
    {
        if (b.GetType() == typeof(ChartEvent))
        {
            ChartEvent realB = b as ChartEvent;
            if (tick < b.tick)
                return true;
            else if (tick == b.tick)
            {
                if (string.Compare(eventName, realB.eventName) < 0)
                    return true;
            }

            return false;
        }
        else
            return base.LessThan(b);
    }

    internal override string GetSaveString()
    {
        // 1728 = E T
        return Globals.TABSPACE + tick + " = E " + eventName + Globals.LINE_ENDING;
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

