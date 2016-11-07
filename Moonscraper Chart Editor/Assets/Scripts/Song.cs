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

    const string QUOTEVALIDATE = @"""[^""\\]*(?:\\.[^""\\]*)*""";
    const string QUOTESEARCH = "\"([^\"]*)\"";
    const string FLOATSEARCH = @"[\-\+]?\d+(\.\d+)?";

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
        switch(dataName)
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
                submitDataChart(easy_single, stringData);
                break;
            case ("[EasyDoubleBass]"):
                submitDataChart(easy_double_bass, stringData);
                break;
            case ("[MediumSingle]"):
                submitDataChart(medium_single, stringData);
                break;
            case ("[MediumDoubleBass]"):
                submitDataChart(medium_double_bass, stringData);
                break;
            case ("[HardSingle]"):
                submitDataChart(hard_single, stringData);
                break;
            case ("[HardDoubleBass]"):
                submitDataChart(hard_double_bass, stringData);
                break;
            case ("[ExpertSingle]"):
                submitDataChart(expert_single, stringData);
                break;
            case ("[ExpertDoubleBass]"):
                submitDataChart(expert_double_bass, stringData);
                break;
            default:
                return;
        }
    }

    void submitDataSong(List<string> stringData)
    {
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

        foreach (string line in stringData)
        {
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
        }
    }

    void submitDataSyncTrack(List<string> stringData)
    {
        /*
        0 = B 140000
        0 = TS 4
        13824 = B 280000
        */
    }

    void submitDataEvents(List<string> stringData)
    {
        foreach(string line in stringData)
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

    void submitDataChart(Chart chart, List<string> stringData)
    {
        Regex noteRegex = new Regex(@"^\s*\d+ = N \d \d+$");            // 48 = N 2 0
        Regex starPowerRegex = new Regex(@"^\s*\d+ = S \d \d+$");       // 768 = S 2 768
        Regex noteEventRegex = new Regex(@"^\s*\d+ = E \S");            // 1728 = E T

        foreach (string line in stringData)
        {  
            //Debug.Log(line);
            if (noteRegex.IsMatch(line))
            {
                // Split string to get note information
                string[] digits = Regex.Split(line.Trim(), @"\D+");

                if (digits.Length == 3)
                {
                    try
                    {
                        int position = int.Parse(digits[0]);
                        Note.Fret_Type fret_type = (Note.Fret_Type)int.Parse(digits[1]);
                        int length = int.Parse(digits[2]);
                        
                        chart.Add(new Note(position, fret_type, length));
                    }
                    catch
                    {
                        // Probably hit N 5 0 or N 6 0
                    }
                }
            }
            else if (starPowerRegex.IsMatch(line))
            {

            }
            else if (noteEventRegex.IsMatch(line))
            {

            }
        }
    }

    public void Save (string filepath)
    {

    }
}
