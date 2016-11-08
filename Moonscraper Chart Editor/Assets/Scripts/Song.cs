using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Song { 
    public string name = string.Empty, artist = string.Empty, charter = string.Empty;
    public string player2 = "Bass";
    public int difficulty = 0;
    public float offset = 0, resolution = 192, previewStart = 0, previewEnd = 0;
    string genre = "rock", mediatype = "cd";
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

    const string QUOTEVALIDATE = @"""[^""\\]*(?:\\.[^""\\]*)*""";
    const string QUOTESEARCH = "\"([^\"]*)\"";
    const string FLOATSEARCH = @"[\-\+]?\d+(\.\d+)?";

    public readonly string[] instrumentTypes = { "Bass", "Rhythm" };

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
        try
        {
            bool open = false;
            string dataName = string.Empty;

            List<string> dataStrings = new List<string>();

            string[] fileLines = File.ReadAllLines(filepath);
            Debug.Log("Loading");

            Init();

            for (int i = 0; i < fileLines.Length; ++i)
            {
                string trimmedLine = fileLines[i].Trim();

                if (new Regex(@"\[.+\]").IsMatch(trimmedLine))
                {
                    dataName = trimmedLine;//.Trim(new char[] { '[', ']' });
                }
                else if (trimmedLine == "{")
                {
                    open = true;
                }
                else if (trimmedLine == "}")
                {
                    open = false;

                    // Submit data
                    submitChartData(dataName, dataStrings);

                    dataName = string.Empty;
                    dataStrings.Clear();
                }
                else
                {
                    if (open)
                    {
                        // Add data into the array
                        dataStrings.Add(trimmedLine);
                    }
                    else if (dataStrings.Count > 0 && dataName != string.Empty)
                    {
                        // Submit data
                        submitChartData(dataName, dataStrings);

                        dataName = string.Empty;
                        dataStrings.Clear();
                    }
                }
            }

            Debug.Log("Complete");
        }
        catch
        {
            throw new System.Exception("Could not open file");
        }
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

    void submitChartData(string dataName, List<string> stringData)
    {
        switch (dataName)
        {
            case ("[Song]"):
                submitDataSong(stringData);
                break;
            case ("[SyncTrack]"):
                submitDataSyncTrack(stringData);
                break;
            case ("[Events]"):
                submitDataEvents(stringData);
                break;
            case ("[EasySingle]"):
                easy_single.Load(stringData);
                break;
            case ("[EasyDoubleBass]"):
                easy_double_bass.Load(stringData);
                break;
            case ("[MediumSingle]"):
                medium_single.Load(stringData);
                break;
            case ("[MediumDoubleBass]"):
                medium_double_bass.Load(stringData);
                break;
            case ("[HardSingle]"):
                hard_single.Load(stringData);
                break;
            case ("[HardDoubleBass]"):
                hard_double_bass.Load(stringData);
                break;
            case ("[ExpertSingle]"):
                expert_single.Load(stringData);
                break;
            case ("[ExpertDoubleBass]"):
                expert_double_bass.Load(stringData);
                break;
            default:
                return;
        }
    }

    void submitDataSong(List<string> stringData)
    {
        Regex nameRegex = new Regex(@"Name = " + QUOTEVALIDATE);
        Regex artistRegex = new Regex(@"Artist = " + QUOTEVALIDATE);
        Regex charterRegex = new Regex(@"Charter = " + QUOTEVALIDATE);
        Regex offsetRegex = new Regex(@"Offset = " + FLOATSEARCH);
        Regex resolutionRegex = new Regex(@"Resolution = " + FLOATSEARCH);
        Regex player2TypeRegex = new Regex(@"Player2 = \w+");
        Regex difficultyRegex = new Regex(@"Difficulty = \d+");
        Regex previewStartRegex = new Regex(@"PreviewStart = " + FLOATSEARCH);
        Regex previewEndRegex = new Regex(@"PreviewEnd = " + FLOATSEARCH);
        Regex genreRegex = new Regex(@"Genre = " + QUOTEVALIDATE);
        Regex mediaTypeRegex = new Regex(@"MediaType = " + QUOTEVALIDATE);
        Regex musicStreamRegex = new Regex(@"MusicStream = " + QUOTEVALIDATE);

        try
        {
            foreach (string line in stringData)
            {
                // Name = "5000 Robots"
                if (nameRegex.IsMatch(line))
                {
                    name = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                }

                // Artist = "TheEruptionOffer"
                else if (artistRegex.IsMatch(line))
                {
                    artist = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                }

                // Charter = "TheEruptionOffer"
                else if (charterRegex.IsMatch(line))
                {
                    charter = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                }

                // Offset = 0
                else if (offsetRegex.IsMatch(line))
                {
                    offset = float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
                }

                // Resolution = 192
                else if (resolutionRegex.IsMatch(line))
                {
                    resolution = float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
                }

                // Player2 = bass
                else if (player2TypeRegex.IsMatch(line))
                {
                    string split = line.Split('=')[1].Trim();

                    foreach (string instrument in instrumentTypes)
                    {
                        if (split.Equals(instrument, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            player2 = instrument;
                            break;
                        }
                    }
                }

                // Difficulty = 0
                else if (difficultyRegex.IsMatch(line))
                {
                    difficulty = int.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                }

                // PreviewStart = 0.00
                else if (previewStartRegex.IsMatch(line))
                {
                    previewStart = float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
                }

                // PreviewEnd = 0.00
                else if (previewEndRegex.IsMatch(line))
                {
                    previewEnd = float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
                }

                // Genre = "rock"
                else if (genreRegex.IsMatch(line))
                {
                    genre = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                }

                // MediaType = "cd"
                else if (mediaTypeRegex.IsMatch(line))
                {
                    mediatype = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                }

                else if (musicStreamRegex.IsMatch(line))
                {

                }
            }

            Debug.Log(GetPropertiesString());
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    string GetPropertiesString()
    {
        return name + "\n" +
                artist + "\n" +
                charter + "\n" +
                offset + "\n" +
                resolution + "\n" +
                player2 + "\n" +
                difficulty + "\n" +
                previewStart + "\n" +
                previewEnd + "\n" +
                genre + "\n" +
                mediatype;
    }

    void submitDataSyncTrack(List<string> stringData)
    {
        /*
        0 = B 140000
        0 = TS 4
        13824 = B 280000
        */
    }

    string GetSyncTrackString()
    {
        return string.Empty;
    }

    void submitDataEvents(List<string> stringData)
    {
        foreach (string line in stringData)
        {
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
        }
    }

    string GetEventsString()
    {
        return string.Empty;
    }

    public void Save(string filepath)
    {

    }
}
