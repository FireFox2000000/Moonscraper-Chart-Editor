using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Chart  {
    Song song;
    List<ChartObject> chartObjects;

    public Note[] notes { get; private set; }
    public StarPower[] starPower { get; private set; }
    public ChartEvent[] events { get; private set; }

    public float endTime
    {
        get
        {
            /*
            SongObject finalObject = chartObjects[chartObjects.Count - 1];
            finalObject = finalObject > song.events[song.events.Length - 1] ? finalObject : song.events[song.events.Length - 1];
            finalObject = finalObject > song.bpms[song.bpms.Length - 1] ? finalObject : song.bpms[song.bpms.Length - 1];
            finalObject = finalObject > song.timeSignatures[song.timeSignatures.Length - 1] ? finalObject : song.timeSignatures[song.timeSignatures.Length - 1];

            float objectTime = finalObject.time;
            */
            return song.length;// > objectTime ? song.length : objectTime;
        }
    }

    public Chart (Song _song)
    {
        song = _song;
        chartObjects = new List<ChartObject>();

        notes = new Note[0];
        starPower = new StarPower[0];
        events = new ChartEvent[0];
    }

    private void updateArrays()
    {
        notes = chartObjects.OfType<Note>().ToArray();
        starPower = chartObjects.OfType<StarPower>().ToArray();
        events = chartObjects.OfType<ChartEvent>().ToArray();
    }

    // Insert into a sorted position
    // Return the position it was inserted into
    public int Add(ChartObject chartObject, bool update = true)
    {
        int pos = SongObject.Insert(chartObject, chartObjects);

        if (update)
            updateArrays();

        return pos;
    }

    public bool Remove(ChartObject chartObject, bool update = true)
    {
        bool success = SongObject.Remove(chartObject, chartObjects);

        if (update)
            updateArrays();

        return success;
    }

    public void Load(List<string> data)
    {
        Load(data.ToArray());
    }

    public void Load(string[] data)
    {
        Regex noteRegex = new Regex(@"^\s*\d+ = N \d \d+$");            // 48 = N 2 0
        Regex starPowerRegex = new Regex(@"^\s*\d+ = S 2 \d+$");        // 768 = S 2 768
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
                        uint position = uint.Parse(digits[0]);
                        int fret_type = int.Parse(digits[1]);
                        uint length = uint.Parse(digits[2]);

                        // Collect flags
                        if (fret_type > 4 || fret_type < 0)
                        {
                            flags.Add(line);
                        }
                        else
                        {
                            // Add note to the data
                            Note newNote = new Note(song, this, position, (Note.Fret_Type)fret_type, length);
                            Add(newNote, false);
                        }
                    }
                }
                
                else if (starPowerRegex.IsMatch(line))
                {
                    string[] digits = Regex.Split(line.Trim(), @"\D+");

                    uint position = uint.Parse(digits[0]);
                    uint length = uint.Parse(digits[2]);

                    Add(new StarPower(song, this, position, length), false);
                }
                
                else if (noteEventRegex.IsMatch(line))
                {
                    string[] strings = Regex.Split(line.Trim(), @"\s+");

                    uint position = uint.Parse(strings[0]);
                    string eventName = strings[3];

                    Add(new ChartEvent(song, this, position, eventName), false);
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

                        Note[] notesToFlag = SongObject.FindObjectsAtPosition(position, chartObjects.OfType<Note>().ToArray());

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

            updateArrays();
        }
        catch (System.Exception e)
        {
            // Bad load, most likely a parsing error
            Debug.LogError(e.Message);
            chartObjects.Clear();
        }
    }

    public string GetChartString()
    {
        string chart = string.Empty;

        for(int i = 0; i < chartObjects.Count; ++i)
        {
            chart += chartObjects[i].GetSaveString();

            if (chartObjects[i].GetType() == typeof(Note))
            {
                // if the next note is not at the same position, add flags into the string
                Note currentNote = (Note)chartObjects[i];    

                if (currentNote.next != null && currentNote.next.position != currentNote.position)
                    chart += currentNote.GetFlagsSaveString();
            }
        }

        return chart;
    }
}
