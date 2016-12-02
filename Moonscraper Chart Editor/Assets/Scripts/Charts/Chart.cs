#define TIMING_DEBUG

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Chart  {
    Song song;
    List<ChartObject> _chartObjects;

    public Note[] notes { get; private set; }
    public StarPower[] starPower { get; private set; }
    public ChartEvent[] events { get; private set; }

    public ChartObject[] chartObjects { get { return _chartObjects.ToArray(); } }

    public Chart (Song _song)
    {
        song = _song;
        _chartObjects = new List<ChartObject>();

        notes = new Note[0];
        starPower = new StarPower[0];
        events = new ChartEvent[0];
    }

    private void updateArrays()
    {
        notes = _chartObjects.OfType<Note>().ToArray();
        starPower = _chartObjects.OfType<StarPower>().ToArray();
        events = _chartObjects.OfType<ChartEvent>().ToArray();
    }

    // Insert into a sorted position
    // Return the position it was inserted into
    public int Add(ChartObject chartObject, bool update = true)
    {
        int pos = SongObject.Insert(chartObject, _chartObjects);

        if (update)
            updateArrays();

        return pos;
    }

    public bool Remove(ChartObject chartObject, bool update = true)
    {
        bool success = SongObject.Remove(chartObject, _chartObjects);

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
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        Regex noteRegex = new Regex(@"^\s*\d+ = N \d \d+$");            // 48 = N 2 0
        Regex starPowerRegex = new Regex(@"^\s*\d+ = S 2 \d+$");        // 768 = S 2 768
        Regex noteEventRegex = new Regex(@"^\s*\d+ = E \S");            // 1728 = E T

        List<string> flags = new List<string>();

        _chartObjects.Capacity = data.Length;

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

                        switch (fret_type)
                        {
                            case (0):
                            case (1):
                            case (2):
                            case (3):
                            case (4):
                                // Add note to the data
                                Note newStandardNote = new Note(song, this, position, (Note.Fret_Type)fret_type, length);
                                Add(newStandardNote, false);
                                break;
                            case (5):
                            case (6):
                                flags.Add(line);
                                break;
                            case (7):
                                Note newOpenNote = new Note(song, this, position, Note.Fret_Type.OPEN, length);
                                Add(newOpenNote, false);
                                break;
                            default:
                                break;
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
            updateArrays();

            // Load flags
            foreach (string line in flags)
            {
                if (noteRegex.IsMatch(line))
                {
                    // Split string to get note information
                    string[] digits = Regex.Split(line.Trim(), @"\D+");

                    if (digits.Length == 3)
                    {
                        uint position = uint.Parse(digits[0]);
                        int fret_type = int.Parse(digits[1]);

                        Note[] notesToAddFlagTo = SongObject.FindObjectsAtPosition(position, notes);
                        switch (fret_type)
                        {
                            case (5):
                                Note.groupAddFlags(notesToAddFlagTo, Note.Flags.FORCED);
                                break;
                            case (6):
                                Note.groupAddFlags(notesToAddFlagTo, Note.Flags.TAP);
                                break;
                            default:
                                break;
                        }       
                    }
                }
            }
#if TIMING_DEBUG
            Debug.Log("Chart load time: " + (Time.realtimeSinceStartup - time));
#endif
        }
        catch (System.Exception e)
        {
            // Bad load, most likely a parsing error
            Debug.LogError(e.Message);
            _chartObjects.Clear();
        }
    }

    public string GetChartString()
    {
        string chart = string.Empty;

        for(int i = 0; i < _chartObjects.Count; ++i)
        {
            chart += _chartObjects[i].GetSaveString();

            if (_chartObjects[i].GetType() == typeof(Note))
            {
                // if the next note is not at the same position, add flags into the string
                Note currentNote = (Note)_chartObjects[i];    

                if (currentNote.next != null && currentNote.next.position != currentNote.position)
                    chart += currentNote.GetFlagsSaveString();
            }
        }

        return chart;
    }
}
