using System;

public abstract class ChartObject : SongObject
{
    public Chart chart;

    public ChartObject(uint position) : base(position){}
}
