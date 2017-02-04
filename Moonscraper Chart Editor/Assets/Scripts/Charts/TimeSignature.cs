using UnityEngine;
using System.Collections;

public class TimeSignature : SyncTrack
{
    private readonly ID _classID = ID.TimeSignature;

    public override int classID { get { return (int)_classID; } }

    public TimeSignature(uint _position = 0, uint _value = 4) : base(_position, _value) { }

    public TimeSignature(TimeSignature ts) : base(ts.position, ts.value) { }

    override public string GetSaveString()
    {
        //0 = TS 4
        return Globals.TABSPACE + position + " = TS " + value + Globals.LINE_ENDING;
    }

    public static bool regexMatch(string line)
    {
        return new System.Text.RegularExpressions.Regex(@"\d+ = TS \d+").IsMatch(line);
    }

    public override SongObject Clone()
    {
        return new TimeSignature(this);
    }
}
