using UnityEngine;
using System.Collections;

public class TimeSignature : SyncTrack
{
    private readonly ID _classID = ID.TimeSignature;

    public override int classID { get { return (int)_classID; } }

    public uint numerator;
    public readonly uint denominator = 4;

    public TimeSignature(uint _position = 0, uint _numerator = 4) : base(_position)
    {
        numerator = _numerator;
    }

    public TimeSignature(TimeSignature ts) : base(ts.position)
    {
        numerator = ts.numerator;
    }

    override public string GetSaveString()
    {
        //0 = TS 4
        return Globals.TABSPACE + position + " = TS " + numerator + Globals.LINE_ENDING;
    }

    public static bool regexMatch(string line)
    {
        return new System.Text.RegularExpressions.Regex(@"\d+ = TS \d+").IsMatch(line);
    }

    public override SongObject Clone()
    {
        return new TimeSignature(this);
    }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && songObject as TimeSignature != null && (songObject as TimeSignature).numerator == numerator)
            return true;
        else
            return false;
    }
}
