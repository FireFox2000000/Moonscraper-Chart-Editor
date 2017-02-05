public abstract class ChartObject : SongObject
{
    public Chart chart;

    public ChartObject(uint position) : base(position){}

    public override void Delete(bool update = true)
    {
        base.Delete();
        chart.Remove(this, update);
    }
}