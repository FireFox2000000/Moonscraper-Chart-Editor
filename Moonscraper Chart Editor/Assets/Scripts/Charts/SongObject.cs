using UnityEngine;
using System.Collections.Generic;

public abstract class SongObject
{
    /// <summary>
    /// The song this object is connected to.
    /// </summary>
    public Song song;
    /// <summary>
    /// The tick position of the object
    /// </summary>
    public uint position;
    /// <summary>
    /// Unity only.
    /// </summary>
    public SongObjectController controller;
    public const int NOTFOUND = -1;

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
            {
                Debug.Log("null");
            }
            return song.ChartPositionToWorldYPosition(position);
        }
    }

    /// <summary>
    /// Automatically converts the object's tick position into the time it will appear in the song.
    /// </summary>
    public float time
    {
        get
        {
            return song.ChartPositionToTime(position, song.resolution);
        }
    }

    internal abstract string GetSaveString();

    /// <summary>
    /// Removes this object from it's song/chart
    /// </summary>
    /// <param name="update">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when deleting multiple objects as it increases performance dramatically.</param>
    public virtual void Delete(bool update = true)
    {
        if (controller)
        {
            controller.gameObject.SetActive(false);
        }
    }
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

    /// <summary>
    /// Searches through the array and finds the array position of item most similar to the one provided.
    /// </summary>
    /// <typeparam name="T">Only objects that extend from the SongObject class.</typeparam>
    /// <param name="searchItem">The item you want to search for.</param>
    /// <param name="objects">The items you want to search through.</param>
    /// <returns>Returns the array position of the object most similar to the search item provided in the 'objects' parameter. 
    /// Returns SongObject.NOTFOUND if there are no objects provided. </returns>
    public static int FindClosestPosition<T>(T searchItem, T[] objects) where T : SongObject
    {
        int lowerBound = 0;
        int upperBound = objects.Length - 1;
        int index = NOTFOUND;

        int midPoint = NOTFOUND;

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

    /// <summary>
    /// Searches through the array and finds the array position of item with the closest position to the one provided.
    /// </summary>
    /// <typeparam name="T">Only objects that extend from the SongObject class.</typeparam>
    /// <param name="searchItem">The item you want to search for.</param>
    /// <param name="objects">The items you want to search through.</param>
    /// <returns>Returns the array position of the closest object located at the specified tick position. 
    /// Returns SongObject.NOTFOUND if there are no objects provided. </returns>
    public static int FindClosestPosition<T>(uint position, T[] objects) where T : SongObject
    {
        int lowerBound = 0;
        int upperBound = objects.Length - 1;
        int index = NOTFOUND;

        int midPoint = NOTFOUND;

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

    /// <summary>
    /// Searches through the array to collect all the items found at the specified position.
    /// </summary>
    /// <typeparam name="T">Only objects that extend from the SongObject class.</typeparam>
    /// <param name="position">The tick position of the items.</param>
    /// <param name="objects">The list you want to search through.</param>
    /// <returns>Returns an array of items located at the specified tick position. 
    /// Returns an empty array if no items are at that exact tick position. </returns>
    public static T[] FindObjectsAtPosition<T>(uint position, T[] objects) where T : SongObject
    {
        int index = FindClosestPosition(position, objects);

        if (index != NOTFOUND && objects[index].position == position)
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

    /// <summary>
    /// Searches through the provided array to find the item specified.  
    /// </summary>
    /// <typeparam name="T">Only objects that extend from the SongObject class.</typeparam>
    /// <param name="searchItem">The item you want to search for.</param>
    /// <param name="objects">The items you want to search through.</param>
    /// <returns>Returns the array position that the search item was found at within the objects array. 
    /// Returns SongObject.NOTFOUND if the item does not exist in the objects array. </returns>
    public static int FindObjectPosition<T>(T searchItem, T[] objects) where T : SongObject
    {      
        int pos = FindClosestPosition(searchItem, objects);

        if (pos != NOTFOUND && objects[pos] != searchItem)
        {
            pos = NOTFOUND;
        }

        return pos;
    }

    static int FindPreviousPosition<T>(System.Type type, int startPosition, List<T> list) where T : SongObject
    {
        // Linear search
        if (startPosition < 0 || startPosition > list.Count - 1)
            return NOTFOUND;
        else
        {
            --startPosition;

            while (startPosition >= 0)
            {
                if (list[startPosition].GetType() == type)
                    return startPosition;
                --startPosition;
            }

            return NOTFOUND;
        }
    }

    static T FindPreviousOfType<T>(System.Type type, int startPosition, List<T> list) where T : SongObject
    {
        int pos = FindPreviousPosition(type, startPosition, list);

        if (pos == NOTFOUND)
            return null;
        else
            return list[pos];
    }

    static int FindNextPosition<T>(System.Type type, int startPosition, List<T> list) where T : SongObject
    {
        // Linear search
        if (startPosition < 0 || startPosition > list.Count - 1)
            return NOTFOUND;
        else
        {
            ++startPosition;

            while (startPosition < list.Count)
            {
                if (list[startPosition].GetType() == type)
                    return startPosition;
                ++startPosition;
            }

            return NOTFOUND;
        }
    }

    static T FindNextOfType<T>(System.Type type, int startPosition, List<T> list) where T : SongObject
    {
        int pos = FindNextPosition(type, startPosition, list);
        if (pos == NOTFOUND)
            return null;
        else
            return list[pos];
    }

    /// <summary>
    /// Adds the item into a sorted position into the specified list and updates the note linked list if a note is inserted. 
    /// </summary>
    /// <typeparam name="T">Only objects that extend from the SongObject class.</typeparam>
    /// <param name="item">The item to be inserted.</param>
    /// <param name="list">The list in which the item will be inserted.</param>
    /// <returns>Returns the list position it was inserted into.</returns>
    public static int Insert<T>(T item, List<T> list) where T : SongObject
    {
        ChartEditor.editOccurred = true;

        int insertionPos = NOTFOUND;

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

                if (insertionPos != NOTFOUND)
                {
                    if (list[insertionPos] == item && item.classID == list[insertionPos].classID)
                    {
                        // Overwrite 
                        if (list[insertionPos].controller != null)
                        {
                            list[insertionPos].controller.gameObject.SetActive(false);
                            //GameObject.Destroy(list[insertionPos].controller.gameObject);
                        }

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

        if (insertionPos == NOTFOUND)
        {
            // Adding the first note
            list.Add(item);
            insertionPos = list.Count - 1;
        }

        if ((ID)item.classID == ID.Note)
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

    /// <summary>
    /// Removes the item from the specified list and updates the note linked list if a note is removed. 
    /// </summary>
    /// <typeparam name="T">Only objects that extend from the SongObject class.</typeparam>
    /// <param name="item">The item to be remove.</param>
    /// <param name="list">The list in which the item will be removed from.</param>
    /// <returns>Returns whether the item was successfully removed or not (may not be removed if the objects was not found).</returns>
    public static bool Remove<T>(T item, List<T> list, bool uniqueData = true) where T : SongObject
    {
        ChartEditor.editOccurred = true;
        int pos = FindObjectPosition(item, list.ToArray());

        if (pos != NOTFOUND)
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

    /// <summary>
    /// Gets a collection of items between a minimum and maximum tick position range.
    /// </summary>
    /// <typeparam name="T">Only objects that extend from the SongObject class.</typeparam>
    /// <param name="list">The list to search through.</param>
    /// <param name="minPos">The minimum range (inclusive).</param>
    /// <param name="maxPos">The maximum range (inclusive).</param>
    /// <returns>Returns all the objects found between the minimum and maximum tick positions specified.</returns>
    public static T[] GetRange<T>(T[] list, uint minPos, uint maxPos) where T : SongObject
    {
        if (minPos > maxPos || list.Length < 1)
            return new T[0];

        int minArrayPos = FindClosestPosition(minPos, list);
        int maxArrayPos = FindClosestPosition(maxPos, list);

        if (minArrayPos == NOTFOUND || maxArrayPos == NOTFOUND)
            return new T[0];
        else
        {
            // Find position may return an object located at a lower position than the minimum position
            while (minArrayPos < list.Length && list[minArrayPos].position < minPos)
            {
                ++minArrayPos;
            }

            if (minArrayPos > list.Length - 1)
                return new T[0];

            // Iterate to the very first object at a greater position, as there may be multiple objects located at the same position
            while (minArrayPos - 1 >= 0 && list[minArrayPos - 1].position >= minPos)
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
            while (maxArrayPos + 1 < list.Length && list[maxArrayPos + 1].position <= maxPos)
            {
                ++maxArrayPos;
            }

            if (minArrayPos > maxArrayPos)
                return new T[0];

            T[] rangedList = new T[maxArrayPos - minArrayPos + 1];
            System.Array.Copy(list, minArrayPos, rangedList, 0, rangedList.Length);

            return rangedList;
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

    /// <summary>
    /// Allows different classes to be sorted and grouped together in arrays by giving each class a comparable numeric value that is greater or less than other classes.
    /// </summary>
    public enum ID
    {
        TimeSignature, BPM, Event, Section, Note, Starpower, ChartEvent
    }
}
