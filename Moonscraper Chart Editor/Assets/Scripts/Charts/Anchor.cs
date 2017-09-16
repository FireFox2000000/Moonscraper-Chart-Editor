using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : BPM
{
    private readonly ID _classID = ID.Anchor;

    float _lockedTime;

    public float lockedTime { get { return _lockedTime; } }

    public Anchor(float time) : base(0)
    {
        _lockedTime = time;
        position = song.TimeToChartPosition(time, song.resolution);
    }

    public Anchor(uint position, uint bpmValue) : base(position, bpmValue) {}

    public Anchor(Anchor _anchor) : base(_anchor.position, _anchor.value) 
    {
        _lockedTime = _anchor.lockedTime;
    }

    public override int classID { get { return (int)_classID; } }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && songObject as Anchor != null)
            return true;
        else
            return false;
    }

    public override SongObject Clone()
    {
        return new Anchor(this);
    }

    internal override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = A " + (int)((time - song.offset) * 1000000) + Globals.LINE_ENDING;
    }
}
