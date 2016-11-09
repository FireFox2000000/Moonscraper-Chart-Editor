using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class ChartObject {
    public int position;

    public abstract string GetSaveString();
    
    public static bool operator ==(ChartObject a, ChartObject b)
    {
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            else if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null))
                return true;
            else
                return false;
        }
        else
        {
            if (a.position == b.position)
                return true;
            else
                return false;
        }
    }

    public static bool operator !=(ChartObject a, ChartObject b)
    {
        return !(a == b);
    }

    public static bool operator <(ChartObject a, ChartObject b)
    {
        if (a.position < b.position)
            return true;
        else 
            return false;
    }

    public static bool operator >(ChartObject a, ChartObject b)
    {
        if (a != b)
            return !(a < b);
        else
            return false;
    }

    public static int FindClosestPosition<T>(T searchItem, T[] objects) where T : ChartObject
    {
        int lowerBound = 0;
        int upperBound = objects.Length - 1;
        int index = Globals.NOTFOUND;

        int midPoint = Globals.NOTFOUND;

        while (lowerBound <= upperBound)
        {
            midPoint = (lowerBound + upperBound) / 2;
            index = midPoint;

            if (objects[midPoint] == searchItem)
            {
                break;
            }
            else
            {
                if (objects[midPoint] < searchItem)
                {
                    // data is in upper half
                    lowerBound = midPoint + 1;
                }
                else
                {
                    // data is in lower half 
                    upperBound = midPoint - 1;
                }
            }
        }

        return index;
    }

    public static int FindClosestPosition<T>(int position, T[] objects) where T : ChartObject
    {
        int lowerBound = 0;
        int upperBound = objects.Length - 1;
        int index = Globals.NOTFOUND;

        int midPoint = Globals.NOTFOUND;

        while (lowerBound <= upperBound)
        {
            midPoint = (lowerBound + upperBound) / 2;
            index = midPoint;

            if (objects[midPoint].position == position)
            {
                break;
            }
            else
            {
                if (objects[midPoint].position < position)
                {
                    // data is in upper half
                    lowerBound = midPoint + 1;
                }
                else
                {
                    // data is in lower half 
                    upperBound = midPoint - 1;
                }
            }
        }

        return index;
    }

    public static T[] FindObjectsAtPosition<T>(int position, T[] objects) where T : ChartObject
    {
        int index = FindClosestPosition(position, objects);

        if (index != Globals.NOTFOUND && objects[index].position == position)
        {
            int lowRange = index, highRange = index;

            while (lowRange > 0 && objects[index].position == objects[lowRange - 1].position)
            {
                --lowRange;
            }

            while (highRange < objects.Length - 1 && objects[index].position == objects[highRange + 1].position)
            {
                ++highRange;
            }

            int length = highRange - lowRange + 1;
            
            T[] objectSelection = new T[length];
            System.Array.Copy(objects, lowRange, objectSelection, 0, length);
            
            return objectSelection;
        }
        else
            return new T[0];
    }

    public static int FindObjectPosition<T>(T searchItem, T[] objects) where T : ChartObject
    {
        int pos = FindClosestPosition(searchItem, objects);

        if (pos != Globals.NOTFOUND && objects[pos] != searchItem)
        {
            pos = Globals.NOTFOUND;
        }

        return pos;
    }

    public static int SortedInsert<T>(T item, List<T> list) where T : ChartObject
    {
        int insertionPos = FindClosestPosition(item, list.ToArray()); //BinarySearchChartClosestNote(note);

        if (list.Count > 0 && insertionPos != Globals.NOTFOUND && list[insertionPos] != item)
        {
            // Insert into sorted position
            if (item > list[insertionPos])
            {
                ++insertionPos;
            }
            list.Insert(insertionPos, item);
        }
        else
        {
            // Adding the first note
            list.Add(item);
            insertionPos = list.Count - 1;
        }

        return insertionPos;
    }

    public float WorldPosition(Song song)
    {
        return song.positionToTime(position) / Globals.zoom;
    }
}

public class Event : ChartObject
{
    public string title;

    public Event(string _title, int _position)
    {
        title = _title;
        position = _position;
    }

    public override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = E \"" + title + "\"\n";
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = E " + @"""[^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }
}

public class Section : Event
{
    public Section(string _title, int _position) : base(_title, _position) { }

    public override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = E \"section " + title + "\"\n";
    }

    new public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = E " + @"""section [^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }
}

public abstract class SyncTrack : ChartObject
{
    public int value;

    public SyncTrack (int _position, int _value)
    {
        position = _position;
        value = _value;
    }
}

public class TimeScale : SyncTrack
{
    public TimeScale(int _position = 0, int _value = 4) : base (_position, _value) {}

    override public string GetSaveString()
    {
        //0 = TS 4
        return Globals.TABSPACE + position + " = TS " + value + "\n";
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = TS \d+").IsMatch(line);
    }
}

public class BPM : SyncTrack
{
    public BPM(int _position = 0, int _value = 120000) : base (_position, _value) { }

    override public string GetSaveString()
    {
        //0 = B 140000
        return Globals.TABSPACE + position + " = B " + value + "\n";
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = B \d+").IsMatch(line);
    }
}
