// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

[System.Serializable]
public abstract class ChartObject : SongObject
{
    [System.NonSerialized]
    public Chart chart;

    public ChartObject(uint position) : base(position){}

    public override void Delete(bool update = true)
    {
        base.Delete(update);
        if (chart != null)
            chart.Remove(this, update);
    }
}