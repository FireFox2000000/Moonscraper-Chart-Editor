// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

[System.Serializable]
public class Starpower : ChartObject
{
    private readonly ID _classID = ID.Starpower;

    public override int classID { get { return (int)_classID; } }

    public uint length;

    public Starpower(uint _position, uint _length) : base(_position)
    {
        length = _length;
    }

    public Starpower(Starpower _starpower) : base(_starpower.tick)
    {
        length = _starpower.length;
    }

    internal override string GetSaveString()
    {
        // 768 = S 2 768
        return Globals.TABSPACE + tick + " = S 2 " + length + Globals.LINE_ENDING;
    }

    public override SongObject Clone()
    {
        return new Starpower(this);
    }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as Starpower).length == length)
            return true;
        else
            return false;
    }

    public void SetLengthByPos(uint pos)
    {
        if (pos > tick)
            length = pos - tick;
        else
            length = 0;

        Starpower nextSp = null;
        if (song != null && chart != null)
        {
            int arrayPos = SongObjectHelper.FindClosestPosition(this, chart.starPower);
            if (arrayPos == SongObjectHelper.NOTFOUND)
                return;

            while (arrayPos < chart.starPower.Length - 1 && chart.starPower[arrayPos].tick <= tick)
            {
                ++arrayPos;
            }

            if (chart.starPower[arrayPos].tick > tick)
                nextSp = chart.starPower[arrayPos];

            if (nextSp != null)
            {
                // Cap sustain length
                if (nextSp.tick < tick)
                    length = 0;
                else if (tick + length > nextSp.tick)
                    // Cap sustain
                    length = nextSp.tick - tick;
            }
            // else it's the only starpower or it's the last starpower 
        }
    }

    public override void Delete(bool update = true)
    {
        base.Delete(update);
        ChartEditor.GetInstance().songObjectPoolManager.SetAllPoolsDirty();
    }
}
