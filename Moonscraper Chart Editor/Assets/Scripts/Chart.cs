using UnityEngine;
using System.Collections.Generic;

public class Chart  {
    public const int NOTFOUND = -1;

    public List<Note> notes;

    public Note this[int i]
    {
        get { return notes[i]; }
        set { notes[i] = value; }
    }

    public int Length { get { return notes.Count; } }

    public Chart ()
    {
        notes = new List<Note>();
    }

    // Insert into a sorted position
    // Return the position it was inserted into
    public int Add (Note note)
    {
        int insertionPos = BinarySearchChartClosestNote(note);
        
        if (notes.Count > 0 && insertionPos != NOTFOUND)
        {
            // TODO Insert into sorted position
            if (note > notes[insertionPos])
            {
                ++insertionPos;
            }
            notes.Insert(insertionPos, note);     
        }
        else
        {
            // Adding the first note
            notes.Add(note);
            insertionPos = notes.Count - 1;
        }

        return insertionPos;
    }

    public bool Remove (Note note)
    {
        int pos = BinarySearchChartExactNote(note);

        if (pos == NOTFOUND)
            return false;
        else
        {
            notes.RemoveAt(pos);
            return true;
        }
    }

    public Note[] ToArray()
    {
        return notes.ToArray();
    }

    public int BinarySearchChartClosestNote(Note searchItem)
    {
        int lowerBound = 0;
        int upperBound = notes.Count - 1;
        int index = NOTFOUND;

        int midPoint = NOTFOUND;

        while (lowerBound <= upperBound)
        {
            midPoint = (lowerBound + upperBound) / 2;
            index = midPoint;

            if (notes[midPoint] == searchItem)
            {
                break;
            }
            else
            {
                if (notes[midPoint] < searchItem)
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

    public int BinarySearchChartExactNote(Note searchItem)
    {
        int pos = BinarySearchChartClosestNote(searchItem);

        if (pos != NOTFOUND && notes[pos] != searchItem)
        { 
            pos = NOTFOUND;
        }

        return pos;
    }

    // Returns all the notes found at the specified position, i.e. chords
    public Note[] GetNotes(int pos)
    {
        return new Note[0];
    }

    public Note searchPreviousNote (Note note)
    {
        int pos = BinarySearchChartExactNote(note);
        if (pos != NOTFOUND && pos > 0)
            return notes[pos - 1];
        else
            return null;
    }

    public Note searchNextNote (Note note)
    {
        int pos = BinarySearchChartExactNote(note);
        if (pos != NOTFOUND && pos < notes.Count - 1)
            return notes[pos + 1];
        else
            return null;
    }
}
