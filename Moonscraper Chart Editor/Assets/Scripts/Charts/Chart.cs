using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Chart  {
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
        return ChartObject.SortedInsert(note, notes);
    }

    public bool Remove (Note note)
    {
        int pos = ChartObject.FindObjectPosition(note, notes.ToArray()); //BinarySearchChartExactNote(note);

        if (pos == Globals.NOTFOUND)
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

    public Note searchPreviousNote (Note note)
    {
        int pos = ChartObject.FindObjectPosition(note, notes.ToArray());
        if (pos != Globals.NOTFOUND && pos > 0)
            return notes[pos - 1];
        else
            return null;
    }

    public Note searchNextNote (Note note)
    {
        int pos = ChartObject.FindObjectPosition(note, notes.ToArray());
        if (pos != Globals.NOTFOUND && pos < notes.Count - 1)
            return notes[pos + 1];
        else
            return null;
    }

    public void Load(List<string> data)
    {
        Load(data.ToArray());
    }

    public void Load(string[] data)
    {
        Regex noteRegex = new Regex(@"^\s*\d+ = N \d \d+$");            // 48 = N 2 0
        Regex starPowerRegex = new Regex(@"^\s*\d+ = S \d \d+$");       // 768 = S 2 768
        Regex noteEventRegex = new Regex(@"^\s*\d+ = E \S");            // 1728 = E T

        List<string> flags = new List<string>();

        try
        {
            // Load notes, collect flags
            foreach (string line in data)
            {
                if (noteRegex.IsMatch(line))
                {
                    // Split string to get note information
                    string[] digits = Regex.Split(line.Trim(), @"\D+");

                    if (digits.Length == 3)
                    {
                        int position = int.Parse(digits[0]);
                        int fret_type = int.Parse(digits[1]);
                        int length = int.Parse(digits[2]);

                        // Collect flags
                        if (fret_type > 4 || fret_type < 0)
                        {
                            flags.Add(line);
                        }
                        else
                        {
                            // Add note to the data
                            Note newNote = new Note(position, (Note.Fret_Type)fret_type, length);
                            int pos = Add(newNote);
                        }
                    }
                }
                // TODO
                else if (starPowerRegex.IsMatch(line))
                {

                }
                // TODO
                else if (noteEventRegex.IsMatch(line))
                {

                }
            }

            // Load flags
            foreach (string line in flags)
            {
                if (noteRegex.IsMatch(line))
                {
                    // Split string to get note information
                    string[] digits = Regex.Split(line.Trim(), @"\D+");

                    if (digits.Length == 3)
                    {
                        int position = int.Parse(digits[0]);
                        int fret_type = int.Parse(digits[1]);
                        int length = int.Parse(digits[2]);

                        Note[] notesToFlag = ChartObject.FindObjectsAtPosition(position, notes.ToArray());

                        // TODO
                        if (fret_type > 4 || fret_type < 0)
                        {
                            switch (fret_type)
                            {
                                case (5):
                                    Note.groupAddFlags(notesToFlag, Note.Flags.FORCED);
                                    break;
                                case (6):
                                    Note.groupAddFlags(notesToFlag, Note.Flags.TAP);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            // Bad load, most likely a parsing error
            Debug.LogError(e.Message);
            notes.Clear();
        }
    }

    // TODO
    public string GetChartString()
    {
        string chart = string.Empty;

        foreach(Note note in notes)
        {

        }

        return chart;
    }
}
