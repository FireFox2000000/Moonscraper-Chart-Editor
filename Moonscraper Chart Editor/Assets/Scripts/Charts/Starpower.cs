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

    public Starpower(Starpower _starpower) : base(_starpower.position)
    {
        length = _starpower.length;
    }

    internal override string GetSaveString()
    {
        // 768 = S 2 768
        return Globals.TABSPACE + position + " = S 2 " + length + Globals.LINE_ENDING;
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
        if (pos > position)
            length = pos - position;
        else
            length = 0;

        Starpower nextSp = null;
        if (song != null && chart != null)
        {
            int arrayPos = FindClosestPosition(this, chart.starPower);
            if (arrayPos == NOTFOUND)
                return;

            while (arrayPos < chart.starPower.Length - 1 && chart.starPower[arrayPos].position <= position)
            {
                ++arrayPos;
            }

            if (chart.starPower[arrayPos].position > position)
                nextSp = chart.starPower[arrayPos];

            if (nextSp != null)
            {
                // Cap sustain length
                if (nextSp.position < position)
                    length = 0;
                else if (position + length > nextSp.position)
                    // Cap sustain
                    length = nextSp.position - position;
            }
            // else it's the only starpower or it's the last starpower 
        }
    }
}
