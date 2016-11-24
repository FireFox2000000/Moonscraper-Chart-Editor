//#define SONG_DEBUG

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
    public AudioClip musicStream = null;
    public float length = 0;

    string audioLocation = string.Empty;

    // Charts
    Chart[] charts = new Chart[12];
    public Chart easy_single { get { return charts[0]; } }
    public Chart easy_double_guitar { get { return charts[1]; } }
    public Chart easy_double_bass { get { return charts[2]; } }
    public Chart medium_single { get { return charts[3]; } }
    public Chart medium_double_guitar { get { return charts[4]; } }
    public Chart medium_double_bass { get { return charts[5]; } }
    public Chart hard_single { get { return charts[6]; } }
    public Chart hard_double_guitar { get { return charts[7]; } }
    public Chart hard_double_bass { get { return charts[8]; } }
    public Chart expert_single { get { return charts[9]; } }
    public Chart expert_double_guitar { get { return charts[10]; } }
    public Chart expert_double_bass { get { return charts[11]; } }

    List<Event> _events;
    List<SyncTrack> _syncTrack;

    public Event[] events { get; private set; }
    public Section[] sections { get; private set; }

    public SyncTrack[] syncTrack { get { return _syncTrack.ToArray(); } }
    public BPM[] bpms { get; private set; }
    public TimeSignature[] timeSignatures { get; private set; }

    // For regexing
    const string QUOTEVALIDATE = @"""[^""\\]*(?:\\.[^""\\]*)*""";
    const string QUOTESEARCH = "\"([^\"]*)\"";
    const string FLOATSEARCH = @"[\-\+]?\d+(\.\d+)?";  

    public readonly string[] instrumentTypes = { "Bass", "Rhythm" };
    public readonly string[] validAudioExtensions = { ".ogg", ".wav", ".mp3" };

    public bool IsSaving
    {
        get
        {
            if (saveThread != null && saveThread.IsAlive)
                return true;
            else
                return false;
        }
    }
    System.Threading.Thread saveThread;

    // Constructor for a new chart
    public Song()
    { 
        _events = new List<Event>();
        _syncTrack = new List<SyncTrack>();

        events = new Event[0];
        sections = new Section[0];
        bpms = new BPM[0];
        timeSignatures = new TimeSignature[0];

        Add(new BPM(this));
        Add(new TimeSignature(this));

        // Chart initialisation
        for (int i = 0; i < charts.Length; ++i)
        {
            charts[i] = new Chart(this);
        }

        audioLocation = string.Empty;
        musicStream = null;
        length = 60 * 5;

        updateArrays();
    }

    // Creating a new song from audio
    public Song(AudioClip _musicStream) : this()
    {
        musicStream = _musicStream;
#if SONG_DEBUG
        Debug.Log("Complete");
#endif
    }

    // Loading a chart file
    public Song(string filepath) : this()
    {
        try
        {
            if (!File.Exists(filepath))
                throw new System.Exception("File does not exist");

            bool open = false;
            string dataName = string.Empty;

            List<string> dataStrings = new List<string>();

            string[] fileLines = File.ReadAllLines(filepath);
#if SONG_DEBUG
            Debug.Log("Loading file");
#endif

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

            updateArrays();

#if SONG_DEBUG
            Debug.Log("Complete");
#endif
        }
        catch
        {
            throw new System.Exception("Could not open file");
        }
    }

    public void LoadAudio(string filepath)
    {      
        filepath = filepath.Replace('\\', '/');
        
        if (filepath != string.Empty && File.Exists(filepath))
        {
            if (!Utility.validateExtension(filepath, validAudioExtensions))
            {
                throw new System.Exception("Invalid file extension");
            }

            audioLocation = filepath;
#if SONG_DEBUG
            Debug.Log("Loading audio");
#endif

            WWW www = new WWW("file://" + filepath);

            while (!www.isDone) ;

            if (Path.GetExtension(filepath) == ".mp3")
                musicStream = NAudioPlayer.FromMp3Data(www.bytes);
            else
                musicStream = www.GetAudioClip(false, false);
            Debug.Log(www.url);
            musicStream.name = Path.GetFileName(filepath);

            while (musicStream != null && musicStream.loadState != AudioDataLoadState.Loaded)
            {
#if SONG_DEBUG
                Debug.Log("Loading audio...");
#endif
            }

            length = musicStream.length;
        }
        else
        {
            Debug.LogError("Unable to locate audio file");
        }
    }

    public uint WorldPositionToSnappedChartPosition(float worldYPos, int step)
    {
        uint chartPos = WorldYPositionToChartPosition(worldYPos);

        return Snapable.ChartPositionToSnappedChartPosition(chartPos, step, resolution);
    }

    public float ChartPositionToWorldYPosition(uint position)
    {
        return TimeToWorldYPosition(ChartPositionToTime(position, resolution));
    }

    public float ChartPositionToWorldYPosition(uint position, float resolution)
    {
        return TimeToWorldYPosition(ChartPositionToTime(position, resolution));
    }

    public uint WorldYPositionToChartPosition(float worldYPos)
    {
        return TimeToChartPosition(WorldYPositionToTime(worldYPos), resolution);
    }

    public uint WorldYPositionToChartPosition(float worldYPos, float resolution)
    {
        return TimeToChartPosition(WorldYPositionToTime(worldYPos), resolution);
    }

    // Used for snapping
    public uint TimeToChartPosition(float time, float resolution)
    {
        if (time < 0)
            time = 0;
        else if (time > length)
            time = length;

        uint position = 0;

        BPM prevBPM = bpms[0];

        // Search for the last bpm
        foreach (BPM bpmInfo in bpms)
        {
            if (ChartPositionToTime(bpmInfo.position, resolution) >= time)
            {
                break;
            }
            else
            {
                prevBPM = bpmInfo;
            }
        }

        position = prevBPM.position;
        position += time_to_dis(ChartPositionToTime(prevBPM.position, resolution), time, resolution, prevBPM.value / 1000.0f);

        return position;
    }

    public static float WorldYPositionToTime (float worldYPosition)
    {
        return worldYPosition / Globals.hyperspeed;
    }

    public static float TimeToWorldYPosition(float time)
    {
        return time * Globals.hyperspeed;
    }

    public float ChartPositionToTime(uint position, float resolution)
    {
        double time = 0;
        BPM prevBPM = bpms[0];

        foreach (BPM bpmInfo in bpms)
        {
            if (bpmInfo.position > position)
            {
                break;
            }
            else
            {
                time += dis_to_time(prevBPM.position, bpmInfo.position, resolution, prevBPM.value / 1000.0f);
                prevBPM = bpmInfo;
            }
        }

        time += dis_to_time(prevBPM.position, position, resolution, prevBPM.value / 1000.0f);

        return (float)time;
    }

    public void Add(SyncTrack syncTrackObject, bool update = true)
    {
        SongObject.Insert(syncTrackObject, _syncTrack);

        if (update)
            updateArrays();
    }

    public bool Remove(SyncTrack syncTrackObject, bool update = true)
    {
        bool success = false;

        if (syncTrackObject.position > 0)
        {
            success = SongObject.Remove(syncTrackObject, _syncTrack);
        }

        if (update)
            updateArrays();

        return success;
    }

    public void Add(Event eventObject, bool update = true)
    {
        SongObject.Insert(eventObject, _events);

        if (update)
            updateArrays();
    }

    public bool Remove(Event eventObject, bool update = true)
    {
        bool success = false;
        success = SongObject.Remove(eventObject, _events);

        if (update)
            updateArrays();

        return success;
    }

    // Calculates the amount of time elapsed between the 2 positions at a set bpm
    static double dis_to_time(uint pos_start, uint pos_end, float resolution, float bpm)
    {
        return (pos_end - pos_start) / resolution * 60.0f / bpm;
    }

    static uint time_to_dis(float time_start, float time_end, float resolution, float bpm)
    {
        return (uint)((time_end - time_start) * bpm / 60.0f * resolution);
    }

    void submitChartData(string dataName, List<string> stringData, string filePath = "")
    {
        switch (dataName)
        {
            case ("[Song]"):
#if SONG_DEBUG
                Debug.Log("Loading chart properties");
#endif
                submitDataSong(stringData, new FileInfo(filePath).Directory.FullName);
                break;
            case ("[SyncTrack]"):
#if SONG_DEBUG
                Debug.Log("Loading sync data");
#endif
                submitDataSyncTrack(stringData);
                break;
            case ("[Events]"):
#if SONG_DEBUG
                Debug.Log("Loading events data");
#endif
                submitDataEvents(stringData);
                break;
            case ("[EasySingle]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasySingle");
#endif
                easy_single.Load(stringData);
                break;
            case ("[EasyDoubleGuitar]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                easy_double_guitar.Load(stringData);
                break;
            case ("[EasyDoubleBass]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                easy_double_bass.Load(stringData);
                break;
            case ("[MediumSingle]"):
#if SONG_DEBUG
                Debug.Log("Loading chart MediumSingle");
#endif
                medium_single.Load(stringData);
                break;
            case ("[MediumDoubleGuitar]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                medium_double_guitar.Load(stringData);
                break;
            case ("[MediumDoubleBass]"):
#if SONG_DEBUG
                Debug.Log("Loading chart MediumDoubleBass");
#endif
                medium_double_bass.Load(stringData);
                break;
            case ("[HardSingle]"):
#if SONG_DEBUG
                Debug.Log("Loading chart HardSingle");
#endif
                hard_single.Load(stringData);
                break;
            case ("[HardDoubleGuitar]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                hard_double_guitar.Load(stringData);
                break;
            case ("[HardDoubleBass]"):
#if SONG_DEBUG
                Debug.Log("Loading chart HardDoubleBass");
#endif
                hard_double_bass.Load(stringData);
                break;
            case ("[ExpertSingle]"):
#if SONG_DEBUG
                Debug.Log("Loading chart ExpertSingle");
#endif
                expert_single.Load(stringData);
                break;
            case ("[ExpertDoubleGuitar]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                expert_double_guitar.Load(stringData);
                break;
            case ("[ExpertDoubleBass]"):
#if SONG_DEBUG
                Debug.Log("Loading chart ExpertDoubleBass");
#endif
                expert_double_bass.Load(stringData);
                break;
            default:
                return;
        }
    }

    void submitDataSong(List<string> stringData, string audioDirectory = "")
    {
#if SONG_DEBUG
        Debug.Log("Loading song properties");
#endif

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
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }

        // Load audio
        try {
            LoadAudio(audioFilepath);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    string GetPropertiesString()
    {
        return name + Globals.LINE_ENDING +
                artist + Globals.LINE_ENDING +
                charter + Globals.LINE_ENDING +
                offset + Globals.LINE_ENDING +
                resolution + Globals.LINE_ENDING +
                player2 + Globals.LINE_ENDING +
                difficulty + Globals.LINE_ENDING +
                previewStart + Globals.LINE_ENDING +
                previewEnd + Globals.LINE_ENDING +
                genre + Globals.LINE_ENDING +
                mediatype;
    }

    void submitDataSyncTrack(List<string> stringData)
    {
        foreach (string line in stringData)
        {
            
            if (TimeSignature.regexMatch(line))
            {
                MatchCollection matches = Regex.Matches(line, @"\d+");
                uint position = uint.Parse(matches[0].ToString());
                uint value = uint.Parse(matches[1].ToString());

                Add(new TimeSignature(this, position, value), false);
            }
            else if (BPM.regexMatch(line))
            {
                MatchCollection matches = Regex.Matches(line, @"\d+");
                uint position = uint.Parse(matches[0].ToString());
                uint value = uint.Parse(matches[1].ToString());

                Add(new BPM(this, position, value), false);
            }
        }
    }

    void submitDataEvents(List<string> stringData)
    {
        foreach (string line in stringData)
        {
            if (Section.regexMatch(line))       // 0 = E "section Intro"
            {
                // Add a section
                string title = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"').Substring(8);
                uint position = uint.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                Add(new Section(this, title, position), false);
            }
            else if (Event.regexMatch(line))    // 125952 = E "end"
            {
                // Add an event
                string title = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                uint position = uint.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                Add(new Event(this, title, position), false);
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
        string musicString;

        // Check if the audio location is the same as the filepath. If so, we only have to save the name of the file, not the full path.
        if (Path.GetDirectoryName(audioLocation).Replace("\\", "/") == Path.GetDirectoryName(filepath).Replace("\\", "/"))
             musicString = musicStream.name;
        else
            musicString = audioLocation;

        saveThread = new System.Threading.Thread(() => SongSave(filepath, musicString));
        saveThread.Start();
    }

    void SongSave(string filepath, string musicString)
    {
        string saveString = string.Empty;

        // Song
        saveString += "[Song]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Name = \"" + name + "\"" + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Artist = \"" + artist + "\"" + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Charter = \"" + charter + "\"" + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Offset = " + offset + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Resolution = " + resolution + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Player2 = " + player2.ToLower() + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Difficulty = " + difficulty + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "PreviewStart = " + previewStart + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "PreviewEnd = " + previewEnd + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Genre = \"" + genre + "\"" + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "MediaType = \"" + mediatype + "\"" + Globals.LINE_ENDING;

        if (musicStream != null)
        {
            saveString += Globals.TABSPACE + "MusicStream = \"" + musicString + "\"" + Globals.LINE_ENDING;
        }
        else
            saveString += Globals.TABSPACE + "MusicStream = \"\"" + Globals.LINE_ENDING;

        saveString += "}" + Globals.LINE_ENDING;

        // SyncTrack
        saveString += "[SyncTrack]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
        saveString += GetSaveString(_syncTrack);
        saveString += "}" + Globals.LINE_ENDING;

        // Events
        saveString += "[Events]" + Globals.LINE_ENDING +"{" + Globals.LINE_ENDING;
        saveString += GetSaveString(_events);
        saveString += "}" + Globals.LINE_ENDING;

        // Charts      
        for(int i = 0; i < charts.Length; ++i)
        {
            string chartString = string.Empty;
            chartString = charts[i].GetChartString();

            if (chartString != string.Empty)
            {
                switch(i)
                {
                    case (0):
                        saveString += "[EasySingle]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (1):
                        saveString += "[EasyDoubleBass]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (2):
                        saveString += "[EasyDoubleGuitar]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (3):
                        saveString += "[MediumSingle]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (4):
                        saveString += "[MediumDoubleGuitar]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (5):
                        saveString += "[MediumDoubleBass]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (6):
                        saveString += "[HardSingle]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (7):
                        saveString += "[HardDoubleGuitar]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (8):
                        saveString += "[HardDoubleBass]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (9):
                        saveString += "[ExpertSingle]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (10):
                        saveString += "[ExpertDoubleGuitar]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    case (11):
                        saveString += "[ExpertDoubleBass]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                        break;
                    default:
                        break;
                }

                saveString += charts[i].GetChartString();
                saveString += "}" + Globals.LINE_ENDING;
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

    void updateArrays()
    {
        events = _events.ToArray();
        sections = _events.OfType<Section>().ToArray();
        bpms = _syncTrack.OfType<BPM>().ToArray();
        timeSignatures = _syncTrack.OfType<TimeSignature>().ToArray();
    }
}
