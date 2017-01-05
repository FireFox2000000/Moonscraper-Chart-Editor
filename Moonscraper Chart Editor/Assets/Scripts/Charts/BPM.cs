using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class BPM : SyncTrack
{
    private readonly int _classID = 1;

    public override int classID { get { return _classID; } }

    public BPM(uint _position = 0, uint _value = 120000) : base(_position, _value) { }

    public BPM(BPM _bpm) : base(_bpm.position, _bpm.value) { }

    override public string GetSaveString()
    {
        //0 = B 140000
        return Globals.TABSPACE + position + " = B " + value + Globals.LINE_ENDING;
    }

    public float assignedTime = 0;

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = B \d+").IsMatch(line);
    }
}
