// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

//#define TIMING_DEBUG

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Chart  {
    Song _song;
    List<ChartObject> _chartObjects;

    /// <summary>
    /// Read only list of notes.
    /// </summary>
    public Note[] notes { get; private set; }
    /// <summary>
    /// Read only list of starpower.
    /// </summary>
    public Starpower[] starPower { get; private set; }
    /// <summary>
    /// Read only list of local events.
    /// </summary>
    public ChartEvent[] events { get; private set; }
    /// <summary>
    /// The song this chart is connected to.
    /// </summary>
    public Song song { get { return _song; } }

    /// <summary>
    /// Read only list containing all chart notes, starpower and events.
    /// </summary>
    public ChartObject[] chartObjects { get { return _chartObjects.ToArray(); } }

    int _note_count;
    /// <summary>
    /// The total amount of notes in the chart, counting chord (notes sharing the same tick position) as a single note.
    /// </summary>
    public int note_count { get { return _note_count; } }

    public string name = string.Empty;

    /// <summary>
    /// Creates a new chart object.
    /// </summary>
    /// <param name="song">The song to associate this chart with.</param>
    /// <param name="name">The name of the chart (easy single, expert double guitar, etc.</param>
    public Chart (Song song, string name = "")
    {
        _song = song;
        _chartObjects = new List<ChartObject>();

        notes = new Note[0];
        starPower = new Starpower[0];
        events = new ChartEvent[0];

        _note_count = 0;

        this.name = name;
    }

    /// <summary>
    /// Updates all read-only values and the total note count.
    /// </summary>
    public void UpdateCache()
    {
        notes = _chartObjects.OfType<Note>().ToArray();
        starPower = _chartObjects.OfType<Starpower>().ToArray();
        events = _chartObjects.OfType<ChartEvent>().ToArray();

        _note_count = GetNoteCount();
    }

    int GetNoteCount()
    {
        if (notes.Length > 0)
        {
            int count = 1;

            uint previousPos = notes[0].position;
            for (int i = 1; i < notes.Length; ++i)
            {
                if (notes[i].position > previousPos)
                {
                    ++count;
                    previousPos = notes[i].position;
                }
            }

            return count;
        }
        else
            return 0;
    }

    /// <summary>
    /// Adds a series of chart objects (note, starpower and/or chart events) into the chart.
    /// </summary>
    /// <param name="chartObjects">Items to add.</param>
    public void Add(ChartObject[] chartObjects)
    {
        foreach (ChartObject chartObject in chartObjects)
        {
            Add(chartObject, false);        
        }

        UpdateCache();
        ChartEditor.isDirty = true;
    }

    /// <summary>
    /// Adds a chart object (note, starpower and/or chart event) into the chart.
    /// </summary>
    /// <param name="chartObject">The item to add</param>
    /// <param name="update">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when adding multiple objects as it increases performance dramatically.</param>
    public int Add(ChartObject chartObject, bool update = true)
    {
        chartObject.chart = this;
        chartObject.song = this._song;

        int pos = SongObject.Insert(chartObject, _chartObjects);

        if (update)
            UpdateCache();

        ChartEditor.isDirty = true;

        return pos;
    }

    /// <summary>
    /// Removes a series of chart objects (note, starpower and/or chart events) from the chart.
    /// </summary>
    /// <param name="chartObjects">Items to add.</param>
    public void Remove(ChartObject[] chartObjects)
    {
        foreach (ChartObject chartObject in chartObjects)
        {
            Remove(chartObject, false);
        }

        UpdateCache();
        ChartEditor.isDirty = true;
    }

    /// <summary>
    /// Removes a chart object (note, starpower and/or chart event) from the chart.
    /// </summary>
    /// <param name="chartObject">Item to add.</param>
    /// <param name="update">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when removing multiple objects as it increases performance dramatically.</param>
    /// <returns>Returns whether the removal was successful or not (item may not have been found if false).</returns>
    public bool Remove(ChartObject chartObject, bool update = true)
    {
        bool success = SongObject.Remove(chartObject, _chartObjects);

        if (success)
        {
            chartObject.chart = null;
            chartObject.song = null;
            ChartEditor.isDirty = true;
        }

        if (update)
            UpdateCache();

        return success;
    }

    public void Load(List<string> data, Song.Instrument instrument = Song.Instrument.Guitar)
    {
        Load(data.ToArray(), instrument);
    }

    public void Load(string[] data, Song.Instrument instrument = Song.Instrument.Guitar)
    {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif

        Regex noteRegex = new Regex(@"^\s*\d+ = N \d \d+$");            // 48 = N 2 0
        Regex starPowerRegex = new Regex(@"^\s*\d+ = S 2 \d+$");        // 768 = S 2 768
        Regex noteEventRegex = new Regex(@"^\s*\d+ = E \S");            // 1728 = E T

        List<string> flags = new List<string>();

        _chartObjects.Capacity = data.Length;

        const int SPLIT_POSITION = 0;
        const int SPLIT_EQUALITY = 1;
        const int SPLIT_TYPE = 2;
        const int SPLIT_VALUE = 3;
        const int SPLIT_LENGTH = 4; 

        try
        {
            // Load notes, collect flags
            foreach (string line in data)
            {
                try
                {
                    string[] splitString = line.Split(' ');
                    uint position = uint.Parse(splitString[SPLIT_POSITION]);
                    string type = splitString[SPLIT_TYPE].ToLower();

                    switch (type)
                    {
                        case ("n"):// noteRegex.IsMatch(line))
                            // Split string to get note information
                            string[] digits = splitString;

                            int fret_type = int.Parse(digits[SPLIT_VALUE]);
                            uint length = uint.Parse(digits[SPLIT_LENGTH]);

                            if (instrument != Song.Instrument.Unrecognised)
                            {
                                switch (fret_type)
                                {
                                    case (0):
                                    case (1):
                                    case (2):
                                    case (3):
                                    case (4):
                                        // Add note to the data
                                        Note newStandardNote = new Note(position, (Note.Fret_Type)fret_type, length);
                                        if (instrument == Song.Instrument.Drums)
                                            newStandardNote.fret_type = Note.LoadDrumNoteToGuitarNote(newStandardNote.fret_type);
                                        Add(newStandardNote, false);
                                        break;
                                    case (5):
                                        if (instrument == Song.Instrument.Drums)
                                        {
                                            Note drumNote = new Note(position, Note.Fret_Type.ORANGE, length);
                                            Add(drumNote, false);
                                            break;
                                        }
                                        else
                                            goto case (6);
                                    case (6):
                                        flags.Add(line);
                                        break;
                                    case (7):
                                        Note newOpenNote = new Note(position, Note.Fret_Type.OPEN, length);
                                        Add(newOpenNote, false);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                Note newNote = new Note(position, Note.Fret_Type.GREEN, length);
                                newNote.rawNote = fret_type;
                                Add(newNote, false);
                            }
                            
                            break;

                        case ("s")://(starPowerRegex.IsMatch(line))
                            length = uint.Parse(splitString[SPLIT_LENGTH]);

                            Add(new Starpower(position, length), false);
                            break;

                        case ("e"): //(noteEventRegex.IsMatch(line))
                            string[] strings = splitString;
                            string eventName = strings[SPLIT_VALUE];
                            Add(new ChartEvent(position, eventName), false);
                            break;
                        default:
                            break;
                    }

                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing line \"" + line + "\": " + e);
                }
            }
            UpdateCache();

            // Load flags
            foreach (string line in flags)
            {
                try
                {
                    // Split string to get note information
                    string[] digits = line.Split(' ');

                    if (digits.Length == 5)
                    {
                        uint position = uint.Parse(digits[SPLIT_POSITION]);
                        int fret_type = int.Parse(digits[SPLIT_VALUE]);

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
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing line \"" + line + "\": " + e);
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

    public string GetChartString(bool forced = true)
    {
        string chart = string.Empty;
        ChartObject[] chartObjects = _chartObjects.ToArray();

        for (int i = 0; i < chartObjects.Length; ++i)
        {
            chart += chartObjects[i].GetSaveString();

            if (forced && chartObjects[i].GetType() == typeof(Note))
            {
                // if the next note is not at the same position, add flags into the string
                Note currentNote = (Note)chartObjects[i];    

                // Only add the flags of the last note at that position
                if (currentNote.next == null || (currentNote.next != null && currentNote.next.position != currentNote.position))
                    chart += currentNote.GetFlagsSaveString();
            }
        }

        return chart;
    }
}
