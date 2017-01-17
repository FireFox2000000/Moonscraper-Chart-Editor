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

    public SongObject (uint _position)
    {
        position = _position;
    }
    
    public float worldYPosition
    {
        get
        {
            if (song == null)
                Debug.Log("null");
            return song.ChartPositionToWorldYPosition(position);
        }
    }

    float time
    {
        get
        {
            return song.ChartPositionToTime(position, song.resolution);
        }
    }

    public abstract string GetSaveString();

    public abstract SongObject Clone();
    public abstract bool AllValuesCompare<T>(T songObject) where T : SongObject;
    
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

    public static bool operator <=(SongObject a, SongObject b)
    {
        return (a < b || a == b);
    }

    public static bool operator >=(SongObject a, SongObject b)
    {
        return (a > b || a == b);
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
    /*
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

    static T FindPreviousOfType<T>(System.Type type, int startPosition, T[] list) where T : SongObject
    {
        int pos = FindPreviousPosition(type, startPosition, list);

        if (pos == Globals.NOTFOUND)
            return null;
        else
            return list[pos];
    }*/

    static int FindPreviousPosition<T>(System.Type type, int startPosition, List<T> list) where T : SongObject
    {
        // Linear search
        if (startPosition < 0 || startPosition > list.Count - 1)
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

    static T FindPreviousOfType<T>(System.Type type, int startPosition, List<T> list) where T : SongObject
    {
        int pos = FindPreviousPosition(type, startPosition, list);

        if (pos == Globals.NOTFOUND)
            return null;
        else
            return list[pos];
    }
    /*
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

    static T FindNextOfType<T>(System.Type type, int startPosition, T[] list) where T : SongObject
    {
        int pos = FindNextPosition(type, startPosition, list);
        if (pos == Globals.NOTFOUND)
            return null;
        else
            return list[pos];
    }*/

    static int FindNextPosition<T>(System.Type type, int startPosition, List<T> list) where T : SongObject
    {
        // Linear search
        if (startPosition < 0 || startPosition > list.Count - 1)
            return Globals.NOTFOUND;
        else
        {
            ++startPosition;

            while (startPosition < list.Count)
            {
                if (list[startPosition].GetType() == type)
                    return startPosition;
                ++startPosition;
            }

            return Globals.NOTFOUND;
        }
    }

    static T FindNextOfType<T>(System.Type type, int startPosition, List<T> list) where T : SongObject
    {
        int pos = FindNextPosition(type, startPosition, list);
        if (pos == Globals.NOTFOUND)
            return null;
        else
            return list[pos];
    }

    public static int Insert<T>(T item, List<T> list, bool uniqueData = true) where T : SongObject
    {
        ChartEditor.editOccurred = true;
#if false
        int insertionPos = FindClosestPosition(item, list.ToArray());
        
        if (list.Count > 0 && insertionPos != Globals.NOTFOUND)
        {
            if (list[insertionPos] == item && item.classID == list[insertionPos].classID)
            {
                // Overwrite 
                if (uniqueData && list[insertionPos].controller != null)
                    list[insertionPos].controller.DestroyGameObject();

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
#else
        int insertionPos = Globals.NOTFOUND;

        if (list.Count > 0)
        {
            if (list[list.Count - 1] < item)
            {
                insertionPos = list.Count;
                list.Insert(insertionPos, item);
            }
            else
            {
                insertionPos = FindClosestPosition(item, list.ToArray());

                if (insertionPos != Globals.NOTFOUND)
                {
                    if (list[insertionPos] == item && item.classID == list[insertionPos].classID)
                    {
                        // Overwrite 
                        if (uniqueData && list[insertionPos].controller != null)
                            list[insertionPos].controller.DestroyGameObject();

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
            }
        }

        if (insertionPos == Globals.NOTFOUND)
        {
            // Adding the first note
            list.Add(item);
            insertionPos = list.Count - 1;
        }
#endif

        if (uniqueData && (ID)item.classID == ID.Note)
        {
            // Update linked list
            Note current = list[insertionPos] as Note;
            
            Note previous = FindPreviousOfType(typeof(Note), insertionPos, list) as Note;
            Note next = FindNextOfType(typeof(Note), insertionPos, list) as Note;

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

            Note openNote = null;
            //bool openFound = false;
            bool standardFound = false;
            
            // Collect all the flags
            while (previous != null && previous.position == current.position)
            {
                if (previous.fret_type == Note.Fret_Type.OPEN)
                    openNote = previous;
                else
                    standardFound = true;

                flags |= previous.flags;
                previous = previous.previous;
            }

            while (next != null && next.position == current.position)
            {
                if (next.fret_type == Note.Fret_Type.OPEN)
                    openNote = next;
                else
                    standardFound = true;

                flags |= next.flags;
                next = next.next;
            }

            // Apply flags
            if (current.fret_type != Note.Fret_Type.OPEN && openNote != null)
            {
                //openNote.controller.Delete();
            }
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
                    next = next.next;
                }
            }
        }

        return insertionPos;
    }

    public static bool Remove<T>(T item, List<T> list, bool uniqueData = true) where T : SongObject
    {
        ChartEditor.editOccurred = true;
        int pos = FindObjectPosition(item, list.ToArray());

        if (pos != Globals.NOTFOUND)
        {
            if (uniqueData && item.GetType() == typeof(Note))
            {
                // Update linked list
                Note previous = FindPreviousOfType(item.GetType(), pos, list) as Note;
                Note next = FindNextOfType(item.GetType(), pos, list) as Note;

                if (previous != null)
                    previous.next = next;
                if (next != null)
                    next.previous = previous;
            }
            list.RemoveAt(pos);
            
            return true;
        }

        return false;
    }

    public static T[] GetRange<T>(T[] list, uint minPos, uint maxPos) where T : SongObject
    {
        if (minPos > maxPos || list.Length < 1)
            return new T[0];

        int minArrayPos = FindClosestPosition(minPos, list);
        int maxArrayPos = FindClosestPosition(maxPos, list);

        if (minArrayPos == Globals.NOTFOUND || maxArrayPos == Globals.NOTFOUND)
            return new T[0];
        else
        {
            // Find position may return an object locationed at a lower position than the minimum position
            while (minArrayPos < list.Length && list[minArrayPos].position < minPos)
            {
                ++minArrayPos;
            }

            if (minArrayPos > list.Length - 1)
                return new T[0];

            // Iterate to the very first object at a greater position, as there may be multiple objects located at the same position
            while (minArrayPos - 1 >= 0 && list[minArrayPos - 1].position > minPos)
            {
                --minArrayPos;
            }

            // Find position may return an object locationed at a greater position than the maximum position
            while (maxArrayPos >= 0 && list[maxArrayPos].position > maxPos)
            {
                --maxArrayPos;
            }

            if (maxArrayPos < 0)
                return new T[0];

            // Iterate to the very last object at a lesser position, as there may be multiple objects located at the same position
            while (maxArrayPos + 1 < list.Length && list[maxArrayPos + 1].position < maxPos)
            {
                ++maxArrayPos;
            }

            if (minArrayPos > maxArrayPos)
                return new T[0];

            return list.Skip(minArrayPos).Take(maxArrayPos - minArrayPos + 1).ToArray();
        }
    }

    public static void sort<T>(T[] songObjects) where T : SongObject
    {
        int j;
        T temp;
        for (int i = 1; i < songObjects.Length; i++)
        {
            temp = songObjects[i];
            j = i - 1;

            while (j >= 0 && songObjects[j] > temp)
            {
                songObjects[j + 1] = songObjects[j];
                j--;
            }

            songObjects[j + 1] = temp;
        }
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

    public Event(string _title, uint _position) : base(_position)
    {
        title = _title;
    }

    public Event(Event songEvent) : base (songEvent.position)
    {
        position = songEvent.position;
        title = songEvent.title;
    }

    public override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = E \"" + title + "\"" + Globals.LINE_ENDING;
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = E " + @"""[^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }

    public override SongObject Clone()
    {
        return new Event(this);
    }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as Event).title == title)
            return true;
        else
            return false;
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

    public Section(Song song, string _title, uint _position) : base(_title, _position) { }

    public Section(Section section) : base(section.title, section.position) { }

    public override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = E \"section " + title + "\"" + Globals.LINE_ENDING;
    }

    new public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = E " + @"""section [^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }

    public override SongObject Clone()
    {
        return new Section(this);
    }
}

public abstract class SyncTrack : SongObject
{
    public uint value;

    public SyncTrack (uint _position, uint _value) : base (_position)
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

public class TimeSignature : SyncTrack
{
    private readonly ID _classID = ID.TimeSignature;

    public override int classID { get { return (int)_classID; } }

    public TimeSignature(uint _position = 0, uint _value = 4) : base (_position, _value) {}

    public TimeSignature(TimeSignature ts) : base(ts.position, ts.value) { }

    override public string GetSaveString()
    {
        //0 = TS 4
        return Globals.TABSPACE + position + " = TS " + value + Globals.LINE_ENDING;
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = TS \d+").IsMatch(line);
    }

    public override SongObject Clone()
    {
        return new TimeSignature(this);
    }
}
