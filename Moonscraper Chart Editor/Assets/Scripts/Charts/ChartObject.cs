using UnityEngine;
using System.Collections;
using System;

public abstract class ChartObject : SongObject {

}

public class StarPower : ChartObject
{
    public int length;

    public StarPower(int _position, int _length)
    {
        position = _position;
        length = _length;
    }

    public override string GetSaveString()
    {
        // 768 = S 2 768
        return Globals.TABSPACE + position + " = S 2 " + length + "\n";
    }
}

public class ChartEvent : ChartObject
{
    public string eventName;

    public ChartEvent(int _position, string _eventName)
    {
        position = _position;
        eventName = _eventName;
    }

    public override string GetSaveString()
    {
        // 1728 = E T
        return Globals.TABSPACE + position + " = E " + eventName + "\n";
    }
}
