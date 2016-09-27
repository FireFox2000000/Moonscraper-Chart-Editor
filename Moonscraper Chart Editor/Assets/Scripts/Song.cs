using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Song {
    public string name = string.Empty, artist = string.Empty, charter = string.Empty;
    public float offset = 0, resolution = 192, bpm = 120;
    string genre = string.Empty, mediatype = string.Empty;
    public readonly AudioClip musicStream;

    // Charts
    //List<Note>[] charts = new List<Note>[8];
    Chart[] charts = new Chart[8];
    public Chart easy_single { get { return charts[0]; } }
    public Chart easy_double_bass { get { return charts[1]; } }
    public Chart medium_single { get { return charts[2]; } }
    public Chart medium_double_bass { get { return charts[3]; } }
    public Chart hard_single { get { return charts[4]; } }
    public Chart hard_double_bass { get { return charts[5]; } }
    public Chart expert_single { get { return charts[6]; } }
    public Chart expert_double_bass { get { return charts[7]; } }

    public List<Event> events;

    void Init()
    {
        events = new List<Event>();

        // Chart initialisation
        for (int i = 0; i < charts.Length; ++i)     
        {
            charts[i] = new Chart();
        }
    }

    // Constructor for a new chart
    public Song()
    {
        Init();
    }

    // Constructor for loading a chart
    public Song(string filepath)
    {
        string[] chartFileLines;

        // Open file
        try
        {
            chartFileLines = File.ReadAllLines(filepath);
        }
        catch
        {
            throw new System.Exception("Could not open file");
        }

        Debug.Log("Loading");

        // Initialisation
        LoadingPoint loadPoint = LoadingPoint.None;
        bool open = false;
        Init();

        foreach (string line in chartFileLines)
        {
            // Remove leadning and trailing whitespace
            line.Trim();

            // Update which section of the chart we're reading data from
            switch (line)
            {
                case ("[Song]"):
                    loadPoint = LoadingPoint.Song;
                    break;
                case ("[SyncTrack]"):
                    loadPoint = LoadingPoint.SyncTrack;
                    break;
                case ("[Events]"):
                    loadPoint = LoadingPoint.Events;
                    break;
                case ("[EasySingle]"):
                    loadPoint = LoadingPoint.EasySingle;
                    break;
                case ("[EasyDoubleBass]"):
                    loadPoint = LoadingPoint.EasyDoubleBass;
                    break;
                case ("[MediumSingle]"):
                    loadPoint = LoadingPoint.MediumSingle;
                    break;
                case ("[MediumDoubleBass]"):
                    loadPoint = LoadingPoint.MediumDoubleBass;
                    break;
                case ("[HardSingle]"):
                    loadPoint = LoadingPoint.HardSingle;
                    break;
                case ("[HardDoubleBass]"):
                    loadPoint = LoadingPoint.HardDoubleBass;
                    break;
                case ("[ExpertSingle]"):
                    loadPoint = LoadingPoint.ExpertSingle;
                    break;
                case ("[ExpertDoubleBass]"):
                    loadPoint = LoadingPoint.ExpertDoubleBass;
                    break;
                case ("{"):
                    open = true;
                    break;
                case ("}"):
                    loadPoint = LoadingPoint.None;
                    open = false;
                    break;
                default:
                    break;
            }

            // Make sure we're inbetween these things { }
            if (open)
            {
                const string QUOTEVALIDATE = @"""[^""\\]*(?:\\.[^""\\]*)*""";
                const string QUOTESEARCH = "\"([^\"]*)\"";
                const string FLOATSEARCH = @"[\-\+]?\d+(\.\d+)?";

                switch (loadPoint)
                {

                    case (LoadingPoint.Song):
                        /*
                        Name = "5000 Robots"
                        Artist = "TheEruptionOffer"
                        Charter = "TheEruptionOffer"
                        Offset = 0
                        Resolution = 192
                        Player2 = bass
                        Difficulty = 0
                        PreviewStart = 0.00
                        PreviewEnd = 0.00
                        Genre = "rock"
                        MediaType = "cd"
                        MusicStream = "5000 Robots.ogg"
                        */

                        // Name = "5000 Robots"
                        if (new Regex(@"Name = " + QUOTEVALIDATE).IsMatch(line))
                        {                            
                            name = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                        }

                        // Artist = "TheEruptionOffer"
                        else if (new Regex(@"Artist = " + QUOTEVALIDATE).IsMatch(line))
                        {
                            artist = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                        }

                        // Charter = "TheEruptionOffer"
                        else if (new Regex(@"Charter = " + QUOTEVALIDATE).IsMatch(line))
                        {
                            charter = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                        }

                        // Offset = 0
                        else if (new Regex(@"Offset = " + FLOATSEARCH).IsMatch(line))
                        {
                            offset = float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
                        }

                        // Resolution = 192
                        else if (new Regex(@"Resolution = " + FLOATSEARCH).IsMatch(line))
                        {
                            resolution = float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
                        }

                        // Player2 = bass
                        // Difficulty = 0
                        // PreviewStart = 0.00
                        // PreviewEnd = 0.00

                        // Genre = "rock"
                        else if (new Regex(@"Genre = " + QUOTEVALIDATE).IsMatch(line))
                        {
                            genre = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                        }

                        // MediaType = "cd"
                        else if (new Regex(@"MediaType = " + QUOTEVALIDATE).IsMatch(line))
                        {
                            mediatype = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                        }

                        break;

                    case (LoadingPoint.SyncTrack):
                        /*
                        0 = B 140000
	                    0 = TS 4
	                    13824 = B 280000
                        */
                        break;
                    case (LoadingPoint.Events):
                        if (Section.regexMatch(line))       // 0 = E "section Intro"
                        {
                            // Add a section
                            string title = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"').Substring(8);                         
                            int position = int.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                            events.Add(new Section(title, position));
                        }
                        else if (Event.regexMatch(line))    // 125952 = E "end"
                        {
                            // Add an event
                            string title = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                            int position = int.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                            events.Add(new Event(title, position));
                        }

                        break;

                    case (LoadingPoint.EasySingle):
                        addDataToChart(easy_single, line);
                        break;
                    case (LoadingPoint.EasyDoubleBass):
                        addDataToChart(easy_double_bass, line);
                        break;
                    case (LoadingPoint.MediumSingle):
                        addDataToChart(medium_single, line);
                        break;
                    case (LoadingPoint.MediumDoubleBass):
                        addDataToChart(medium_double_bass, line);
                        break;
                    case (LoadingPoint.HardSingle):
                        addDataToChart(hard_single, line);
                        break;
                    case (LoadingPoint.HardDoubleBass):
                        addDataToChart(hard_double_bass, line);
                        break;
                    case (LoadingPoint.ExpertSingle):
                        addDataToChart(expert_single, line);
                        break;
                    case (LoadingPoint.ExpertDoubleBass):
                        addDataToChart(expert_double_bass, line);
                        break;
                }
            }
        }

        Debug.Log("Complete");
    }

    // Calculates the amount of time elapsed between the 2 positions at a set bpm
    static float dis_to_time(int pos_start, int pos_end, float bpm, float offset)
    {
        return (pos_end - pos_start) / 192 * 60 / bpm + offset;
    }

    // Returns the distance from the strikeline a note should be
    static float note_distance(float highway_speed, float elapsed_time, float note_time)
    {
        return highway_speed * (note_time - elapsed_time);
    }

    void addDataToChart (Chart chart, string line)
    {
        Regex noteRegex = new Regex(@"^\s+\d+ = N \d \d+$");      // 48 = N 2 0
        Regex starPowerRegex = new Regex(@"^\s+\d+ = S \d \d+$");      // 768 = S 2 768
        Regex noteEventRegex = new Regex(@"^\s+\d+ = E \S");      // 1728 = E T

        if (noteRegex.IsMatch(line))
        {
            // Split string to get note information
            string[] digits = Regex.Split(line.Trim(), @"\D+");

            if (digits.Length == 3)
            {
                try
                {
                    int position = int.Parse(digits[0]);
                    Note.Fret_Type fret_type = Note.NoteNumberToFretType(int.Parse(digits[1]));
                    int length = int.Parse(digits[2]);

                    chart.Add(new Note(position, fret_type, length));
                }
                catch
                {
                    // Probably hit N 5 0 or N 6 0
                }
            }
        }
    }

    public void Save (string filepath)
    {

    }

    public class Event
    {
        public string title;
        public int position;

        public Event(string _title, int _position)
        {
            title = _title;
            position = _position;
        }

        public string GetSaveString()
        {
            const string TABSPACE = "  ";
            return TABSPACE + position + " = E \"" + title + "\"\n";
        }

        public static bool regexMatch(string line)
        {
            return new Regex(@"\d+ = E " + @"""[^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
        }
    }

    public class Section : Event
    {
        public Section(string _title, int _position) : base(_title, _position) {}

        new public string GetSaveString()
        {
            const string TABSPACE = "  ";
            return TABSPACE + position + " = E \"section " + title + "\"\n";
        }

        new public static bool regexMatch(string line)
        {
            return new Regex(@"\d+ = E " + @"""section [^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
        }
    }

    enum LoadingPoint
    {
        None, Song, SyncTrack, Events, EasySingle, EasyDoubleBass, MediumSingle, MediumDoubleBass, HardSingle, HardDoubleBass, ExpertSingle, ExpertDoubleBass
    }
}
