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

    public Chart(Chart chart, Song song)
    {
        _song = song;
        name = chart.name;

        _chartObjects = new List<ChartObject>();
        _chartObjects.AddRange(chart._chartObjects);

        this.name = chart.name;
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

        int pos = SongObjectHelper.Insert(chartObject, _chartObjects);

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
        bool success = SongObjectHelper.Remove(chartObject, _chartObjects);

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
                        case ("n"):
                            // Split string to get note information
                            string[] digits = splitString;

                            int fret_type = int.Parse(digits[SPLIT_VALUE]);
                            uint length = uint.Parse(digits[SPLIT_LENGTH]);

                            if (instrument == Song.Instrument.Unrecognised)
                            {
                                Note newNote = new Note(position, fret_type, length);
                                Add(newNote, false);
                            }
                            else if (instrument == Song.Instrument.Drums)
                                LoadDrumNote(position, fret_type, length);
                            else if (instrument == Song.Instrument.GHLiveGuitar || instrument == Song.Instrument.GHLiveBass)
                                LoadGHLiveNote(line, position, fret_type, length, flags);
                            else
                                LoadStandardNote(line, position, fret_type, length, flags);                      
                            break;

                        case ("s"):
                            fret_type = int.Parse(splitString[SPLIT_VALUE]);

                            if (fret_type != 2)
                                continue;

                            length = uint.Parse(splitString[SPLIT_LENGTH]);

                            Add(new Starpower(position, length), false);
                            break;

                        case ("e"):
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

                        Note[] notesToAddFlagTo = SongObjectHelper.FindObjectsAtPosition(position, notes);
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

    void LoadStandardNote(string line, uint position, int noteNumber, uint length, List<string> flagsList)
    {
        Note.Fret_Type? noteFret = null;
        switch (noteNumber)
        {
            case (0):
                noteFret = Note.Fret_Type.GREEN;
                break;
            case (1):
                noteFret = Note.Fret_Type.RED;
                break;
            case (2):
                noteFret = Note.Fret_Type.YELLOW;
                break;
            case (3):
                noteFret = Note.Fret_Type.BLUE;
                break;
            case (4):
                noteFret = Note.Fret_Type.ORANGE;
                break;
            case (5):
            case (6):
                flagsList.Add(line);
                break;
            case (7):
                noteFret = Note.Fret_Type.OPEN;
                break;
            default:
                return;
        }

        if (noteFret != null)
        {
            Note newNote = new Note(position, (int)noteFret, length);
            Add(newNote, false);
        }
    }

    void LoadDrumNote(uint position, int noteNumber, uint length)
    {
        Note.Drum_Fret_Type? noteFret = null;
        switch (noteNumber)
        {
            case (0):
                noteFret = Note.Drum_Fret_Type.KICK;
                break;
            case (1):
                noteFret = Note.Drum_Fret_Type.RED;
                break;
            case (2):
                noteFret = Note.Drum_Fret_Type.YELLOW;
                break;
            case (3):
                noteFret = Note.Drum_Fret_Type.BLUE;
                break;
            case (4):
                noteFret = Note.Drum_Fret_Type.ORANGE;
                break;
            case (5):
                noteFret = Note.Drum_Fret_Type.GREEN;
                break;
            default:
                return;
        }

        if (noteFret != null)
        {
            Note newNote = new Note(position, (int)noteFret, length);
            Add(newNote, false);
        }
    }

    void LoadGHLiveNote(string line, uint position, int noteNumber, uint length, List<string> flagsList)
    {
        Note.GHLive_Fret_Type? noteFret = null;
        switch (noteNumber)
        {
            case (0):
                noteFret = Note.GHLive_Fret_Type.WHITE_1;
                break;
            case (1):
                noteFret = Note.GHLive_Fret_Type.WHITE_2;
                break;
            case (2):
                noteFret = Note.GHLive_Fret_Type.WHITE_3;
                break;
            case (3):
                noteFret = Note.GHLive_Fret_Type.BLACK_1;
                break;
            case (4):
                noteFret = Note.GHLive_Fret_Type.BLACK_2;
                break;
            case (5):
            case (6):
                flagsList.Add(line);
                break;
            case (7):
                noteFret = Note.GHLive_Fret_Type.OPEN;
                break;
            case (8):
                noteFret = Note.GHLive_Fret_Type.BLACK_3;
                break;
            default:
                return;
        }

        if (noteFret != null)
        {
            Note newNote = new Note(position, (int)noteFret, length);
            Add(newNote, false);
        }
    }
}
