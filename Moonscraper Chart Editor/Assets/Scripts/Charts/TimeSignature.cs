// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

[System.Serializable]
public class TimeSignature : SyncTrack
{
    private readonly ID _classID = ID.TimeSignature;

    public override int classID { get { return (int)_classID; } }

    public uint numerator;
    public uint denominator;

    public uint quarterNotesPerMeasure { get { return numerator; } }
    public uint beatsPerMeasure { get { return denominator; } }

    public TimeSignature(uint _position = 0, uint _numerator = 4, uint _denominator = 4) : base(_position)
    {
        numerator = _numerator;
        denominator = _denominator;
    }

    public TimeSignature(TimeSignature ts) : base(ts.tick)
    {
        numerator = ts.numerator;
        denominator = ts.denominator;
    }

    internal override string GetSaveString()
    {
        //0 = TS 4 4
        string saveString = Globals.TABSPACE + tick + " = TS " + numerator;

        if (denominator != 4)
            saveString +=  " " + (uint)UnityEngine.Mathf.Log(denominator, 2) + Globals.LINE_ENDING;
        else
            saveString += Globals.LINE_ENDING;

        return saveString;
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

    public void CopyFrom(TimeSignature ts)
    {
        tick = ts.tick;
        numerator = ts.numerator;
        denominator = ts.denominator;
    }
}
