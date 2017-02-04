using UnityEngine;
using System.Collections;

public abstract class SyncTrack : SongObject
{
    public uint value;

    public SyncTrack(uint _position, uint _value) : base(_position)
    {
        value = _value;
    }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as SyncTrack).value == value)
            return true;
        else
            return false;
    }
}
