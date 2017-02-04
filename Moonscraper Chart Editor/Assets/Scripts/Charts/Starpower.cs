using UnityEngine;
using System.Collections;

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

    public override string GetSaveString()
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
}
