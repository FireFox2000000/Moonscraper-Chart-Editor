using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;

public abstract class SongObject
{
    public Song song;
    public uint position;
    public SongObjectController controller;

    public abstract int classID { get; }

    public SongObject (Song _song, uint _position)
    {
        song = _song;
        position = _position;
    }
    
    public float worldYPosition { get { return song.ChartPositionToWorldYPosition(position); } }

    public float time { get { return song.ChartPositionToTime(position, song.resolution); } }

    public abstract string GetSaveString();
    
    public static bool operator ==(SongObject a, SongObject b)
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
            return a.Equals(b);
    }

    protected virtual bool Equals(SongObject b)
    {
        if (position == b.position && classID == b.classID)
            return true;
        else
            return false;
    }

    public static bool operator !=(SongObject a, SongObject b)
    {
        return !(a == b);
    }

    protected virtual bool LessThan(SongObject b)
    {
        if (position < b.position)
            return true;
        else if (position == b.position && classID < b.classID)
            return true;
        else
            return false;
    }

    public static bool operator <(SongObject a, SongObject b)
    {
        return a.LessThan(b);
    }

    public static bool operator >(SongObject a, SongObject b)
    {
        if (a != b)
            return !(a < b);
        else
            return false;
    }

    public override bool Equals(System.Object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static int FindClosestPosition<T>(T searchItem, T[] objects) where T : SongObject
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

    public static int FindClosestPosition<T>(uint position, T[] objects) where T : SongObject
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

    public static T[] FindObjectsAtPosition<T>(uint position, T[] objects) where T : SongObject
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

    public static int FindObjectPosition<T>(T searchItem, T[] objects) where T : SongObject
    {
        int pos = FindClosestPosition(searchItem, objects);

        if (pos != Globals.NOTFOUND && objects[pos] != searchItem)
        {
            pos = Globals.NOTFOUND;
        }

        return pos;
    }

    static int FindPreviousPosition<T>(System.Type type, int startPosition, T[] list) where T : SongObject
    {
        // Linear search
        if (startPosition < 0 || startPosition > list.Length - 1)
            return Globals.NOTFOUND;
        else
        {
            --startPosition;

            while (startPosition >= 0)
            {
                if (list[startPosition].GetType() == type)
                    return startPosition;
                --startPosition;
            }

            return Globals.NOTFOUND;
        }
    }

    public static T FindPreviousOfType<T>(System.Type type, int startPosition, T[] list) where T : SongObject
    {
        
        int pos = FindPreviousPosition(type, startPosition, list);

        if (pos == Globals.NOTFOUND)
            return null;
        else
            return list[pos];
    }

    static int FindNextPosition<T>(System.Type type, int startPosition, T[] list) where T : SongObject
    {
        // Linear search
        if (startPosition < 0 || startPosition > list.Length - 1)
            return Globals.NOTFOUND;
        else
        {
            ++startPosition;

            while (startPosition < list.Length)
            {
                if (list[startPosition].GetType() == type)
                    return startPosition;
                ++startPosition;
            }

            return Globals.NOTFOUND;
        }
    }

    public static T FindNextOfType<T>(System.Type type, int startPosition, T[] list) where T : SongObject
    {
        int pos = FindNextPosition(type, startPosition, list);
        if (pos == Globals.NOTFOUND)
            return null;
        else
            return list[pos];
    }

    public static int Insert<T>(T item, List<T> list) where T : SongObject
    {
        ChartEditor.editOccurred = true;
        int insertionPos = FindClosestPosition(item, list.ToArray());
        
        // Needs to overwrite
        if (list.Count > 0 && insertionPos != Globals.NOTFOUND)
        {
            int prevPosition = FindPreviousPosition(item.GetType(), insertionPos, list.ToArray());
            int nextPosition = FindNextPosition(item.GetType(), insertionPos, list.ToArray());

            if (prevPosition != Globals.NOTFOUND && list[prevPosition] == item)
            {
                // Overwrite
                if (list[prevPosition].controller != null)
                    GameObject.Destroy(list[prevPosition].controller.gameObject);
                
                list[prevPosition] = item;
                insertionPos = prevPosition;       
            }
            else if (nextPosition != Globals.NOTFOUND && list[nextPosition] == item)
            {
                // Overwrite
                if (list[nextPosition].controller != null)
                    GameObject.Destroy(list[nextPosition].controller.gameObject);
                
                list[nextPosition] = item;
                insertionPos = nextPosition;
            }
            else if (item == list[insertionPos] && item.GetType() == list[insertionPos].GetType())
            {
                // Overwrite 
                if (list[insertionPos].controller != null)
                    GameObject.Destroy(list[insertionPos].controller.gameObject);
                
                list[insertionPos] = item;
            }
            // Insert into sorted position
            else
            {   
                if (item > list[insertionPos])
                {
                    ++insertionPos;
                }
                list.Insert(insertionPos, item);
            }
        }
        else
        {
            // Adding the first note
            list.Add(item);
            insertionPos = list.Count - 1;
        }
        
        if (item.GetType() == typeof(Note))
        {
            // Update linked list
            Note current = list[insertionPos] as Note;
            
            Note previous = FindPreviousOfType(typeof(Note), insertionPos, list.ToArray()) as Note;
            Note next = FindNextOfType(typeof(Note), insertionPos, list.ToArray()) as Note;
            
            current.previous = previous;
            if (previous != null)
                previous.next = current;

            current.next = next;
            if (next != null)
                next.previous = current;

            // Update flags depending on open notes
            Note.Flags flags = current.flags;
            previous = current.previous;
            next = current.next;

            bool openFound = false;
            bool standardFound = false;
            
            // Collect all the flags
            while (previous != null && previous.position == current.position)
            {
                if (previous.fret_type == Note.Fret_Type.OPEN)
                    openFound = true;
                else
                    standardFound = true;

                flags |= previous.flags;
                previous = previous.previous;
            }

            while (next != null && next.position == current.position)
            {
                if (next.fret_type == Note.Fret_Type.OPEN)
                    openFound = true;
                else
                    standardFound = true;

                flags |= next.flags;
                next = next.previous;
            }

            // Apply flags
            if (current.fret_type != Note.Fret_Type.OPEN && openFound)
            { }
            else if (current.fret_type == Note.Fret_Type.OPEN && standardFound)
            { }
            else
            {
                current.flags = flags;

                previous = current.previous;
                next = current.next;
                while (previous != null && previous.position == current.position)
                {
                    previous.flags = flags;
                    previous = previous.previous;
                }

                while (next != null && next.position == current.position)
                {
                    next.flags = flags;
                    next = next.previous;
                }
            }
        }

        return insertionPos;
    }

    public static bool Remove<T>(T item, List<T> list) where T : SongObject
    {
        ChartEditor.editOccurred = true;
        int pos = FindObjectPosition(item, list.ToArray());

        if (pos != Globals.NOTFOUND)
        {
            if (item.GetType() == typeof(Note))
            {
                // Update linked list
                Note previous = FindPreviousOfType(item.GetType(), pos, list.ToArray()) as Note;
                Note next = FindNextOfType(item.GetType(), pos, list.ToArray()) as Note;

                if (previous != null)
                    previous.next = next;
                if (next != null)
                    next.previous = previous;
            }

            item.song = null;
            list.RemoveAt(pos);
            
            return true;
        }

        return false;
    }

    public enum ID
    {
        TimeSignature, BPM, Event, Section, Note, Starpower, ChartEvent
    }
}

public class Event : SongObject
{
    private readonly ID _classID = ID.Event;

    public override int classID { get { return (int)_classID; } } 

    public string title;

    public Event(Song song, string _title, uint _position) : base(song, _position)
    {
        title = _title;
    }

    public override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = E \"" + title + "\"" + Globals.LINE_ENDING;
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = E " + @"""[^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }
}

public class Section : Event
{
    private readonly ID _classID = ID.Section;

    public override int classID { get { return (int)_classID; } }

    SectionController _controller = null;
    
    new public SectionController controller
    {
        get { return _controller; }
        set { _controller = value; base.controller = value; }
    }

    public Section(Song song, string _title, uint _position) : base(song, _title, _position) { }

    public override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = E \"section " + title + "\"" + Globals.LINE_ENDING;
    }

    new public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = E " + @"""section [^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }
}

public abstract class SyncTrack : SongObject
{
    public uint value;

    public SyncTrack (Song song, uint _position, uint _value) : base (song, _position)
    {
        value = _value;
    }
}

public class TimeSignature : SyncTrack
{
    private readonly ID _classID = ID.TimeSignature;

    public override int classID { get { return (int)_classID; } }

    public TimeSignature(Song song, uint _position = 0, uint _value = 4) : base (song, _position, _value) {}

    override public string GetSaveString()
    {
        //0 = TS 4
        return Globals.TABSPACE + position + " = TS " + value + Globals.LINE_ENDING;
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = TS \d+").IsMatch(line);
    }
}

public class BPM : SyncTrack
{
    private readonly int _classID = 1;

    public override int classID { get { return _classID; } }

    public BPM(Song song, uint _position = 0, uint _value = 120000) : base (song, _position, _value) { }

    override public string GetSaveString()
    {
        //0 = B 140000
        return Globals.TABSPACE + position + " = B " + value + Globals.LINE_ENDING;
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = B \d+").IsMatch(line);
    }
}
