using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Song { 
    // Song properties
    public string name = string.Empty, artist = string.Empty, charter = string.Empty;
    public string player2 = "Bass";
    public int difficulty = 0;
    public float offset = 0, resolution = 192, previewStart = 0, previewEnd = 0;
    public string genre = "rock", mediatype = "cd";
    public AudioClip musicStream;
    string audioLocation = string.Empty;

    // Charts
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
    public List<SyncTrack> syncTrack;

    // For regexing
    const string QUOTEVALIDATE = @"""[^""\\]*(?:\\.[^""\\]*)*""";
    const string QUOTESEARCH = "\"([^\"]*)\"";
    const string FLOATSEARCH = @"[\-\+]?\d+(\.\d+)?";

    public readonly string[] instrumentTypes = { "Bass", "Rhythm" };
    public readonly string[] validAudioExtensions = { ".ogg", ".wav", ".mp3" };

    void Init()
    {
        events = new List<Event>();
        syncTrack = new List<SyncTrack>();

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
            if (!File.Exists(filepath))
                throw new System.Exception("File does not exist");

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
                    dataName = trimmedLine;
                }
                else if (trimmedLine == "{")
                {
                    open = true;
                }
                else if (trimmedLine == "}")
                {
                    open = false;

                    // Submit data
                    submitChartData(dataName, dataStrings, filepath);

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
                        submitChartData(dataName, dataStrings, filepath);

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

    public void LoadAudio(string filepath)
    {
        // Need to check extension
        if (filepath != string.Empty && File.Exists(filepath))
        {
            audioLocation = filepath;

            Debug.Log("Loading audio");

            WWW www = new WWW("file://" + filepath);
            musicStream = www.GetAudioClip(false, false);

            musicStream.name = Path.GetFileName(filepath);
            Debug.Log(musicStream.name);
        }
        else
        {
            Debug.LogError("Unable to locate audio file");
        }
    }

    public float ChartPositionToWorldYPosition(int position)
    {
        return positionToTime(position) * Globals.hyperspeed;
    }

    // TODO - Will be used for snapping
    public int WorldYPositionToChartPosition(float worldYPos)
    {
        return 0;
    }

    public float WorldYPositionToTime (float worldYPosition)
    {
        return worldYPosition / Globals.hyperspeed;
    }

    public float positionToTime(int position)
    {
        double time = 0;
        BPM prevBPM = new BPM ();

        foreach (BPM bpmInfo in syncTrack.OfType<BPM>())
        {
            if (bpmInfo.position > position)
            {
                break;
            }
            else
            {
                time += dis_to_time(prevBPM.position, bpmInfo.position, prevBPM.value / 1000.0);
                prevBPM = bpmInfo;
            }
        }

        time += dis_to_time(prevBPM.position, position, prevBPM.value / 1000.0) + offset;

        return (float)time;
    }

    // Calculates the amount of time elapsed between the 2 positions at a set bpm
    static double dis_to_time(int pos_start, int pos_end, double bpm)
    {
        return (pos_end - pos_start) / 192.0 * 60.0 / bpm;
    }

    // Returns the distance from the strikeline a note should be
    static float note_distance(float highway_speed, float elapsed_time, float note_time)
    {
        return highway_speed * (note_time - elapsed_time);
    }

    void submitChartData(string dataName, List<string> stringData, string filePath = "")
    {
        switch (dataName)
        {
            case ("[Song]"):
                Debug.Log("Loading chart properties");
                submitDataSong(stringData, new FileInfo(filePath).Directory.FullName);
                break;
            case ("[SyncTrack]"):
                Debug.Log("Loading sync data");
                submitDataSyncTrack(stringData);
                break;
            case ("[Events]"):
                Debug.Log("Loading events data");
                submitDataEvents(stringData);
                break;
            case ("[EasySingle]"):
                Debug.Log("Loading chart EasySingle");
                easy_single.Load(stringData);
                break;
            case ("[EasyDoubleBass]"):
                Debug.Log("Loading chart EasyDoubleBass");
                easy_double_bass.Load(stringData);
                break;
            case ("[MediumSingle]"):
                Debug.Log("Loading chart MediumSingle");
                medium_single.Load(stringData);
                break;
            case ("[MediumDoubleBass]"):
                Debug.Log("Loading chart MediumDoubleBass");
                medium_double_bass.Load(stringData);
                break;
            case ("[HardSingle]"):
                Debug.Log("Loading chart HardSingle");
                hard_single.Load(stringData);
                break;
            case ("[HardDoubleBass]"):
                Debug.Log("Loading chart HardDoubleBass");
                hard_double_bass.Load(stringData);
                break;
            case ("[ExpertSingle]"):
                Debug.Log("Loading chart ExpertSingle");
                expert_single.Load(stringData);
                break;
            case ("[ExpertDoubleBass]"):
                Debug.Log("Loading chart ExpertDoubleBass");
                expert_double_bass.Load(stringData);
                break;
            default:
                return;
        }
    }

    void submitDataSong(List<string> stringData, string audioDirectory = "")
    {
        Debug.Log("Loading song properties");

        string audioFilepath = string.Empty;

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

                // MusicStream = "ENDLESS REBIRTH.ogg"
                else if (musicStreamRegex.IsMatch(line))
                {
                    audioFilepath = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');

                    // Check if it's already the full path. If not, make it relative to the chart file.
                    if (!File.Exists(audioFilepath))
                        audioFilepath = audioDirectory + "\\" + audioFilepath;
                }
            }

            Debug.Log(GetPropertiesString());
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }

        // Load audio
        LoadAudio(audioFilepath);
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
        foreach (string line in stringData)
        {
            if (TimeSignature.regexMatch(line))
            {
                MatchCollection matches = Regex.Matches(line, @"\d+");
                int position = int.Parse(matches[0].ToString());
                int value = int.Parse(matches[1].ToString());

                SongObject.SortedInsert(new TimeSignature(position, value), syncTrack);
            }
            else if (BPM.regexMatch(line))
            {
                MatchCollection matches = Regex.Matches(line, @"\d+");
                int position = int.Parse(matches[0].ToString());
                int value = int.Parse(matches[1].ToString());

                SongObject.SortedInsert(new BPM(position, value), syncTrack);
            }
        }

        // Validate that there are base values
        SyncTrack[] initSync = SongObject.FindObjectsAtPosition(0, syncTrack.ToArray());
        bool bpmInit = false, timeScaleInit = false;

        foreach (SyncTrack sync in initSync)
        {
            if (sync.GetType() == typeof(BPM))
                bpmInit = true;
            else if (sync.GetType() == typeof(TimeSignature))
                timeScaleInit = true;
        }

        if (bpmInit == false)
            SongObject.SortedInsert(new BPM(), syncTrack);
        if (timeScaleInit == false)
            SongObject.SortedInsert(new TimeSignature(), syncTrack);
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

    string GetSaveString<T>(List<T> list) where T : SongObject
    {
        string saveString = string.Empty;

        foreach (T item in list)
        {
            saveString += item.GetSaveString();
        }

        return saveString;
    }

    public void Save(string filepath)
    {
        string saveString = string.Empty;

        // Song
        saveString += "[Song]\n{\n";
        saveString += Globals.TABSPACE + "Name = \"" + name + "\"\n";
        saveString += Globals.TABSPACE + "Artist = \"" + artist + "\"\n";
        saveString += Globals.TABSPACE + "Charter = \"" + charter + "\"\n";
        saveString += Globals.TABSPACE + "Offset = " + offset + "\n";
        saveString += Globals.TABSPACE + "Resolution = " + resolution + "\n";
        saveString += Globals.TABSPACE + "Player2 = " + player2.ToLower() + "\n";
        saveString += Globals.TABSPACE + "Difficulty = " + difficulty + "\n";
        saveString += Globals.TABSPACE + "PreviewStart = " + previewStart + "\n";
        saveString += Globals.TABSPACE + "PreviewEnd = " + previewEnd + "\n";
        saveString += Globals.TABSPACE + "Genre = \"" + genre + "\"\n";
        saveString += Globals.TABSPACE + "MediaType = \"" + mediatype + "\"\n";

        if (musicStream != null)
        {
            string musicString;

            // Check if the audio location is the same as the filepath. If so, we only have to save the name of the file, not the full path.
            if (Path.GetDirectoryName(audioLocation) == Path.GetDirectoryName(filepath))
                musicString = musicStream.name;
            else
                musicString = audioLocation;

            saveString += Globals.TABSPACE + "MusicStream = \"" + musicString + "\"\n";
        }
        else
            saveString += Globals.TABSPACE + "MusicStream = \"\"\n";

        saveString += "}\n";

        // SyncTrack
        saveString += "[SyncTrack]\n{\n";
        saveString += GetSaveString(syncTrack);
        saveString += "}\n";

        // Events
        saveString += "[Events]\n{\n";
        saveString += GetSaveString(events);
        saveString += "}\n";

        // Charts
        string chartString = string.Empty;
        for(int i = 0; i < charts.Length; ++i)
        {
            chartString = charts[i].GetChartString();

            if (chartString != string.Empty)
            {
                switch(i)
                {
                    case (0):
                        saveString += "[EasySingle]\n{\n";
                        break;
                    case (1):
                        saveString += "[EasyDoubleBass]\n{\n";
                        break;
                    case (2):
                        saveString += "[MediumSingle]\n{\n";
                        break;
                    case (3):
                        saveString += "[MediumDoubleBass]\n{\n";
                        break;
                    case (4):
                        saveString += "[HardSingle]\n{\n";
                        break;
                    case (5):
                        saveString += "[HardDoubleBass]\n{\n";
                        break;
                    case (6):
                        saveString += "[ExpertSingle]\n{\n";
                        break;
                    case (7):
                        saveString += "[ExpertDoubleBass]\n{\n";
                        break;
                    default:
                        break;
                }

                saveString += charts[i].GetChartString();
                saveString += "}\n";
            }
        }

        try {
            // Save to file
            File.WriteAllText(filepath, saveString);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
