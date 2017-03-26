//#define SONG_DEBUG
//#define TIMING_DEBUG
//#define LOAD_AUDIO_ASYNC

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using NAudio.Midi;
using System;

public class Song {
    public static bool streamAudio = true;

    const int MUSIC_STREAM_ARRAY_POS = 0;
    const int GUITAR_STREAM_ARRAY_POS = 1;
    const int RHYTHM_STREAM_ARRAY_POS = 2;
    public const string TEMP_MP3_TO_WAV_FILEPATH = "moonscraper_mp3_to_wav_conversion_temp.wav";

    // Song properties
    public string name = string.Empty, artist = string.Empty, charter = string.Empty;
    public string player2 = "Bass";
    public int difficulty = 0;
    public float offset = 0, resolution = 192, previewStart = 0, previewEnd = 0;
    public string genre = "rock", mediatype = "cd";
    public string year = string.Empty;
    AudioClip[] audioStreams = new AudioClip[3];
    SampleData[] audioSampleData = new SampleData[3];

    public AudioClip musicStream { get { return audioStreams[MUSIC_STREAM_ARRAY_POS]; }
        set { audioStreams[MUSIC_STREAM_ARRAY_POS] = value; }
    }
    public AudioClip guitarStream { get { return audioStreams[GUITAR_STREAM_ARRAY_POS]; } set { audioStreams[GUITAR_STREAM_ARRAY_POS] = value; } }
    public AudioClip rhythmStream { get { return audioStreams[RHYTHM_STREAM_ARRAY_POS]; } set { audioStreams[RHYTHM_STREAM_ARRAY_POS] = value; } }

    public SampleData musicSample { get { return audioSampleData[MUSIC_STREAM_ARRAY_POS]; } private set { audioSampleData[MUSIC_STREAM_ARRAY_POS] = value; } }
    public SampleData guitarSample { get { return audioSampleData[GUITAR_STREAM_ARRAY_POS]; } private set { audioSampleData[GUITAR_STREAM_ARRAY_POS] = value; } }
    public SampleData rhythmSample { get { return audioSampleData[RHYTHM_STREAM_ARRAY_POS]; } private set { audioSampleData[RHYTHM_STREAM_ARRAY_POS] = value; } }

    public float length = 0;

    //string audioLocation = string.Empty;
    string[] audioLocations = new string[3];

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

    /// <summary>
    /// Read only list of song events.
    /// </summary>
    public Event[] events { get; private set; }
    /// <summary>
    /// Read only list of song sections.
    /// </summary>
    public Section[] sections { get; private set; }

    public SyncTrack[] syncTrack { get { return _syncTrack.ToArray(); } }
    /// <summary>
    /// Read only list of a song's bpm changes.
    /// </summary>
    public BPM[] bpms { get; private set; }
    /// <summary>
    /// Read only list of a song's time signature changes.
    /// </summary>
    public TimeSignature[] timeSignatures { get; private set; }

    // For regexing
    const string QUOTEVALIDATE = @"""[^""\\]*(?:\\.[^""\\]*)*""";
    const string QUOTESEARCH = "\"([^\"]*)\"";
    const string FLOATSEARCH = @"[\-\+]?\d+(\.\d+)?";  

    public readonly string[] instrumentTypes = { "Bass", "Rhythm" };

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
    public bool IsAudioLoading
    {
        get
        {
            if (audioLoads > 0)
                return true;
            else
                return false;
        }
    }

    System.Threading.Thread saveThread;

    int audioLoads = 0;

    /// <summary>
    /// Default constructor for a new chart. Initialises all lists and adds locked bpm and timesignature objects.
    /// </summary>
    public Song()
    { 
        _events = new List<Event>();
        _syncTrack = new List<SyncTrack>();

        events = new Event[0];
        sections = new Section[0];
        bpms = new BPM[0];
        timeSignatures = new TimeSignature[0];

        Add(new BPM());
        Add(new TimeSignature());

        // Chart initialisation
        for (int i = 0; i < charts.Length; ++i)
        {
            string name;

            switch (i)
            {
                case(0):
                    name = "Easy Single";
                    break;
                case (1):
                    name = "Easy Double Guitar";
                    break;
                case (2):
                    name = "Easy Double Bass";
                    break;
                case (3):
                    name = "Medium Single";
                    break;
                case (4):
                    name = "Medium Double Guitar";
                    break;
                case (5):
                    name = "Medium Double Bass";
                    break;
                case (6):
                    name = "Hard Single";
                    break;
                case (7):
                    name = "Hard Double Guitar";
                    break;
                case (8):
                    name = "Hard Double Bass";
                    break;
                case (9):
                    name = "Expert Single";
                    break;
                case (10):
                    name = "Expert Double Guitar";
                    break;
                case (11):
                    name = "Expert Double Bass";
                    break;
                default:
                    name = string.Empty;
                    break;
            }

            charts[i] = new Chart(this, name);
        }

        for (int i = 0; i < audioLocations.Length; ++i)
            audioLocations[i] = string.Empty;

        for (int i = 0; i < audioSampleData.Length; ++i)
            audioSampleData[i] = new SampleData(string.Empty);

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

    void LoadChartFile(string filepath)
    {
        bool open = false;
        string dataName = string.Empty;

        List<string> dataStrings = new List<string>();
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        string[] fileLines = File.ReadAllLines(filepath);

        // Gather lines between {} brackets and submit data
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
        
#if TIMING_DEBUG
        Debug.Log("Chart file load time: " + (Time.realtimeSinceStartup - time));
        time = Time.realtimeSinceStartup;

        LoadAllAudioClips();
#endif

        updateArrays();
    }

    /// <summary>
    /// Generates a song object loaded from a .chart file.
    /// </summary>
    /// <param name="filepath">The path to the .chart file you want to load.</param>
    public Song(string filepath) : this()
    {
        try
        {
            if (!File.Exists(filepath))
                throw new System.Exception("File does not exist");

            if (Path.GetExtension(filepath) == ".chart")
                LoadChartFile(filepath);
            else
            {
                throw new System.Exception("Bad file type");
            }
            
        }
        catch
        {
            throw new System.Exception("Could not open file");
        }
    }

    /// <summary>
    /// Unity context only. Loads the audio provided from the .chart file into AudioClips for song, guitar and rhythm tracks.
    /// </summary>
    public void LoadAllAudioClips()
    {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        LoadMusicStream(audioLocations[MUSIC_STREAM_ARRAY_POS]);
        LoadGuitarStream(audioLocations[GUITAR_STREAM_ARRAY_POS]);
        LoadRhythmStream(audioLocations[RHYTHM_STREAM_ARRAY_POS]);
#if TIMING_DEBUG
        Debug.Log("Total audio files load time: " + (Time.realtimeSinceStartup - time));
#endif
    }

    public void LoadMusicStream(string filepath)
    {
        GameObject loadAudioObject = new GameObject("Load Rhythm Audio");
        MonoWrapper coroutine = loadAudioObject.AddComponent<MonoWrapper>();

        coroutine.StartCoroutine(LoadAudio(filepath, MUSIC_STREAM_ARRAY_POS, loadAudioObject));
    }

    public void LoadGuitarStream(string filepath)
    {
        GameObject loadAudioObject = new GameObject("Load Rhythm Audio");
        MonoWrapper coroutine = loadAudioObject.AddComponent<MonoWrapper>();

        coroutine.StartCoroutine(LoadAudio(filepath, GUITAR_STREAM_ARRAY_POS, loadAudioObject));
    }

    public void LoadRhythmStream(string filepath)
    {
        GameObject loadAudioObject = new GameObject("Load Rhythm Audio");
        MonoWrapper coroutine = loadAudioObject.AddComponent<MonoWrapper>();

        coroutine.StartCoroutine(LoadAudio(filepath, RHYTHM_STREAM_ARRAY_POS, loadAudioObject));
    }

    IEnumerator LoadAudio(string filepath, int audioStreamArrayPos, GameObject coroutine)
    {
        string temp_wav_filepath = Globals.realWorkingDirectory + "\\" + TEMP_MP3_TO_WAV_FILEPATH;
        string convertedFromMp3 = string.Empty;

        if (audioStreams[audioStreamArrayPos])
        {
            audioStreams[audioStreamArrayPos].UnloadAudioData();
            GameObject.Destroy(audioStreams[audioStreamArrayPos]);
        }

        audioSampleData[audioStreamArrayPos].Stop();
        audioSampleData[audioStreamArrayPos] = new SampleData(filepath);
        audioSampleData[audioStreamArrayPos].ReadAudioFile();

        filepath = filepath.Replace('\\', '/');
        
        if (filepath != string.Empty && File.Exists(filepath))
        {
#if TIMING_DEBUG
            float time = Time.realtimeSinceStartup;
#endif
            if (!Utility.validateExtension(filepath, Globals.validAudioExtensions))
            {
                throw new System.Exception("Invalid file extension");
            }
            
            audioLocations[audioStreamArrayPos] = Path.GetFullPath(filepath);
            ++audioLoads;

            if (Path.GetExtension(filepath) == ".mp3")
            {
                Debug.Log("Converting Mp3 to wav...");
                System.Threading.Thread wavConversionThread = new System.Threading.Thread(() => { ConvertMp3ToWav(filepath, temp_wav_filepath); });
                wavConversionThread.Start();

                while (wavConversionThread.ThreadState == System.Threading.ThreadState.Running)
                    yield return null;

                File.SetAttributes(temp_wav_filepath, FileAttributes.Hidden);
                convertedFromMp3 = filepath;
                filepath = temp_wav_filepath;

                Debug.Log("Mp3 to wav conversion complete!");            
            }

            WWW www = new WWW("file://" + filepath);

            while (!www.isDone)
            {
                yield return null;
            }

            if (Path.GetExtension(filepath) == ".mp3")
            {
                Debug.Log("Still scanning for mp3 for whatever reason");
                WAV wav = null;
                float[] interleavedData = null;

                byte[] bytes = www.bytes;
                System.Threading.Thread wavConversionThread = new System.Threading.Thread(() => { wav = NAudioPlayer.WAVFromMp3Data(bytes, out interleavedData); });
                wavConversionThread.Start();

                while (wavConversionThread.ThreadState == System.Threading.ThreadState.Running)
                    yield return null;            

                audioStreams[audioStreamArrayPos] = AudioClip.Create("testSound", wav.SampleCount, 2, wav.Frequency, false);
                audioStreams[audioStreamArrayPos].SetData(interleavedData, 0);

                audioSampleData[audioStreamArrayPos].SetData(interleavedData);
                //audioStreams[audioStreamArrayPos] = NAudioPlayer.FromMp3Data(www.bytes);
            }
            else
            {
                audioStreams[audioStreamArrayPos] = www.GetAudioClip(false, streamAudio);
            }

            --audioLoads;

            if (convertedFromMp3 == string.Empty)
                audioStreams[audioStreamArrayPos].name = Path.GetFileName(filepath);
            else
                audioStreams[audioStreamArrayPos].name = Path.GetFileName(convertedFromMp3);

            while (audioStreams[audioStreamArrayPos] != null && audioStreams[audioStreamArrayPos].loadState != AudioDataLoadState.Loaded) ;

            if (audioStreamArrayPos == MUSIC_STREAM_ARRAY_POS)
                length = musicStream.length;

#if TIMING_DEBUG
            Debug.Log("Audio load time: " + (Time.realtimeSinceStartup - time));
#endif     
            Debug.Log("Finished loading audio");       
        }
        else
        {
            audioStreams[audioStreamArrayPos] = null;

            if (filepath != string.Empty)
                Debug.LogError("Unable to locate audio file");
        }

        GameObject.Destroy(coroutine);
    }

    private static void ConvertMp3ToWav(string _inPath_, string _outPath_)
    {
        using (NAudio.Wave.Mp3FileReader mp3 = new NAudio.Wave.Mp3FileReader(_inPath_))
        {
            using (NAudio.Wave.WaveStream pcm = NAudio.Wave.WaveFormatConversionStream.CreatePcmStream(mp3))
            {
                NAudio.Wave.WaveFileWriter.CreateWaveFile(_outPath_, pcm);
            }
        }
    }

    public void FreeAudioClips()
    {
        foreach (AudioClip clip in audioStreams)
        {
            if (clip)
            {
                clip.UnloadAudioData();

                GameObject.Destroy(clip);
            }
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

    /// <summary>
    /// Converts a time value into a tick position value. May be inaccurate due to interger rounding.
    /// </summary>
    /// <param name="time">The time (in seconds) to convert.</param>
    /// <param name="resolution">Ticks per beat, usually provided from the resolution song of a Song class.</param>
    /// <returns>Returns the calculated tick position.</returns>
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

    /// <summary>
    /// Finds the value of the first bpm that appears before the specified tick position.
    /// </summary>
    /// <param name="position">The tick position</param>
    /// <returns>Returns the value of the bpm that was found.</returns>
    public uint GetPrevBPM(uint position)
    {
        for (int i = 0; i < bpms.Length; ++i)
        {
            if (i + 1 >= bpms.Length)
                return bpms[i].value;
            else if (bpms[i + 1].position > position)
                return bpms[i].value;
        }

        return bpms[0].value;
    }

    /// <summary>
    /// Finds the value of the first time signature that appears before the specified tick position.
    /// </summary>
    /// <param name="position">The tick position</param>
    /// <returns>Returns the value of the time signature that was found.</returns>
    public uint GetPrevTS(uint position)
    {
        for (int i = 0; i < timeSignatures.Length; ++i)
        {
            if (i + 1 >= timeSignatures.Length)
                return timeSignatures[i].numerator;
            else if (timeSignatures[i + 1].position > position)
                return timeSignatures[i].numerator;
        }

        return timeSignatures[0].numerator;
    }

    public static float WorldYPositionToTime (float worldYPosition)
    {
        return worldYPosition / (Globals.hyperspeed / Globals.gameSpeed);
    }

    public static float TimeToWorldYPosition(float time)
    {
        return time * Globals.hyperspeed / Globals.gameSpeed;
    }

    /// <summary>
    /// Converts a tick position into the time it will appear in the song.
    /// </summary>
    /// <param name="position">Tick position.</param>
    /// <param name="resolution">Ticks per beat, usually provided from the resolution song of a Song class.</param>
    /// <returns>Returns the time in seconds.</returns>
    public float ChartPositionToTime(uint position, float resolution)
    {
        int previousBPMPos = SongObject.FindClosestPosition(position, bpms);
        if (bpms[previousBPMPos].position > position)
            --previousBPMPos;

        BPM prevBPM = bpms[previousBPMPos];
        float time = prevBPM.assignedTime;
        time += (float)Song.dis_to_time(prevBPM.position, position, resolution, prevBPM.value / 1000.0f);

        return time;
    }

    /// <summary>
    /// Adds a synctrack object (bpm or time signature) into the song.
    /// </summary>
    /// <param name="syncTrackObject">Item to add.</param>
    /// <param name="autoUpdate">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when adding multiple objects as it increases performance dramatically.</param>
    public void Add(SyncTrack syncTrackObject, bool autoUpdate = true)
    {
        syncTrackObject.song = this;
        SongObject.Insert(syncTrackObject, _syncTrack);

        if (autoUpdate)
            updateArrays();

        ChartEditor.editOccurred = true;
    }

    /// <summary>
    /// Removes a synctrack object (bpm or time signature) from the song.
    /// </summary>
    /// <param name="autoUpdate">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when removing multiple objects as it increases performance dramatically.</param>
    /// <returns>Returns whether the removal was successful or not (item may not have been found if false).</returns>
    public bool Remove(SyncTrack syncTrackObject, bool autoUpdate = true)
    {
        bool success = false;

        if (syncTrackObject.position > 0)
        {
            success = SongObject.Remove(syncTrackObject, _syncTrack);
        }

        if (success)
        {
            syncTrackObject.song = null;
            ChartEditor.editOccurred = true;
        }

        if (autoUpdate)
            updateArrays();

        return success;
    }

    /// <summary>
    /// Adds an event object (section or event) into the song.
    /// </summary>
    /// <param name="syncTrackObject">Item to add.</param>
    /// <param name="autoUpdate">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when adding multiple objects as it increases performance dramatically.</param>
    public void Add(Event eventObject, bool autoUpdate = true)
    {
        eventObject.song = this;
        SongObject.Insert(eventObject, _events);

        if (autoUpdate)
            updateArrays();

        ChartEditor.editOccurred = true;
    }

    /// <summary>
    /// Removes an event object (section or event) from the song.
    /// </summary>
    /// <param name="autoUpdate">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when removing multiple objects as it increases performance dramatically.</param>
    /// <returns>Returns whether the removal was successful or not (item may not have been found if false).</returns>
    public bool Remove(Event eventObject, bool autoUpdate = true)
    {
        bool success = false;
        success = SongObject.Remove(eventObject, _events);

        if (success)
        {
            eventObject.song = null;
            ChartEditor.editOccurred = true;
        }

        if (autoUpdate)
            updateArrays();

        return success;
    }

    /// <summary>
    /// Calculates the amount of time elapsed between 2 tick positions.
    /// </summary>
    /// <param name="pos_start">Initial tick position.</param>
    /// <param name="pos_end">Final tick position.</param>
    /// <param name="resolution">Ticks per beat, usually provided from the resolution song of a Song class.</param>
    /// <param name="bpm">The beats per minute value. BPMs provided from a BPM object need to be divded by 1000 as it is stored as the value read from a .chart file.</param>
    /// <returns></returns>
    public static double dis_to_time(uint pos_start, uint pos_end, float resolution, float bpm)
    {
        return (pos_end - pos_start) / resolution * 60.0f / bpm;
    }

    static uint time_to_dis(float time_start, float time_end, float resolution, float bpm)
    {
        return (uint)Mathf.Round((time_end - time_start) * bpm / 60.0f * resolution);
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
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif

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
        Regex yearRegex = new Regex(@"Year = " + QUOTEVALIDATE);
        Regex mediaTypeRegex = new Regex(@"MediaType = " + QUOTEVALIDATE);
        Regex musicStreamRegex = new Regex(@"MusicStream = " + QUOTEVALIDATE);
        Regex guitarStreamRegex = new Regex(@"GuitarStream = " + QUOTEVALIDATE);
        Regex rhythmStreamRegex = new Regex(@"RhythmStream = " + QUOTEVALIDATE);

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

                else if (yearRegex.IsMatch(line))
                    year = Regex.Replace(Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"'), @"\D", "");

                // MusicStream = "ENDLESS REBIRTH.ogg"
                else if (musicStreamRegex.IsMatch(line))
                {
                    string audioFilepath = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');

                    // Check if it's already the full path. If not, make it relative to the chart file.
                    if (!File.Exists(audioFilepath))
                        audioFilepath = audioDirectory + "\\" + audioFilepath;

                    if (File.Exists(audioFilepath) && Utility.validateExtension(audioFilepath, Globals.validAudioExtensions))
                        audioLocations[MUSIC_STREAM_ARRAY_POS] = Path.GetFullPath(audioFilepath);
                }
                else if (guitarStreamRegex.IsMatch(line))
                {
                    string audioFilepath = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');

                    // Check if it's already the full path. If not, make it relative to the chart file.
                    if (!File.Exists(audioFilepath))
                        audioFilepath = audioDirectory + "\\" + audioFilepath;

                    if (File.Exists(audioFilepath) && Utility.validateExtension(audioFilepath, Globals.validAudioExtensions))
                        audioLocations[GUITAR_STREAM_ARRAY_POS] = Path.GetFullPath(audioFilepath);
                }
                else if (rhythmStreamRegex.IsMatch(line))
                {
                    string audioFilepath = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');

                    // Check if it's already the full path. If not, make it relative to the chart file.
                    if (!File.Exists(audioFilepath))
                        audioFilepath = audioDirectory + "\\" + audioFilepath;

                    if (File.Exists(audioFilepath) && Utility.validateExtension(audioFilepath, Globals.validAudioExtensions))
                        audioLocations[RHYTHM_STREAM_ARRAY_POS] = Path.GetFullPath(audioFilepath);
                }
            }

#if TIMING_DEBUG
            Debug.Log("Song properties load time: " + (Time.realtimeSinceStartup - time));
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
        
    }

    string GetPropertiesStringWithoutAudio()
    {
        string saveString = string.Empty;

        // Song properties  
        if (name != string.Empty)      
            saveString += Globals.TABSPACE + "Name = \"" + name + "\"" + Globals.LINE_ENDING;
        if (artist != string.Empty)
            saveString += Globals.TABSPACE + "Artist = \"" + artist + "\"" + Globals.LINE_ENDING;
        if (charter != string.Empty)
            saveString += Globals.TABSPACE + "Charter = \"" + charter + "\"" + Globals.LINE_ENDING;
        if (year != string.Empty)
            saveString += Globals.TABSPACE + "Year = \", " + year + "\"" + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Offset = " + offset + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Resolution = " + resolution + Globals.LINE_ENDING;
        if (player2 != string.Empty)
            saveString += Globals.TABSPACE + "Player2 = \"" + player2.ToLower() + Globals.LINE_ENDING;       
        saveString += Globals.TABSPACE + "Difficulty = " + difficulty + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "PreviewStart = " + previewStart + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "PreviewEnd = " + previewEnd + Globals.LINE_ENDING;
        if (genre != string.Empty)
            saveString += Globals.TABSPACE + "Genre = \"" + genre + "\"" + Globals.LINE_ENDING;
        if (mediatype != string.Empty)
            saveString += Globals.TABSPACE + "MediaType = \"" + mediatype + "\"" + Globals.LINE_ENDING;

        return saveString;
    }

    void submitDataSyncTrack(List<string> stringData)
    {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        foreach (string line in stringData)
        {     
            if (TimeSignature.regexMatch(line))
            {
                MatchCollection matches = Regex.Matches(line, @"\d+");
                uint position = uint.Parse(matches[0].ToString());
                uint value = uint.Parse(matches[1].ToString());

                Add(new TimeSignature(position, value), false);
            }
            else if (BPM.regexMatch(line))
            {
                MatchCollection matches = Regex.Matches(line, @"\d+");
                uint position = uint.Parse(matches[0].ToString());
                uint value = uint.Parse(matches[1].ToString());

                Add(new BPM(position, value), false);
            }
        }
#if TIMING_DEBUG
        Debug.Log("Synctrack load time: " + (Time.realtimeSinceStartup - time));
#endif
    }

    void submitDataEvents(List<string> stringData)
    {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        foreach (string line in stringData)
        {
            if (Section.regexMatch(line))       // 0 = E "section Intro"
            {
                // Add a section
                string title = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"').Substring(8);
                uint position = uint.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                Add(new Section(title, position), false);
            }
            else if (Event.regexMatch(line))    // 125952 = E "end"
            {
                // Add an event
                string title = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                uint position = uint.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                Add(new Event(title, position), false);
            }
        }
#if TIMING_DEBUG
        Debug.Log("Events load time: " + (Time.realtimeSinceStartup - time));
#endif
    }

    string GetSaveString<T>(T[] list) where T : SongObject
    {
        string saveString = string.Empty;

        foreach (T item in list)
        {
            saveString += item.GetSaveString();
        }

        return saveString;
    }

    /// <summary>
    /// Saves the song data in a .chart format to the specified path asynchonously (starts a thread).
    /// </summary>
    /// <param name="filepath">The path and filename to save to.</param>
    /// <param name="forced">Will the notes from each chart have their flags properties saved into the file?</param>
    public void Save(string filepath, bool forced = true)
    {
        saveThread = new System.Threading.Thread(() => SongSave(filepath, forced));
        saveThread.Start();
    }

    void SongSave(string filepath, bool forced = true)
    {
        string musicString = string.Empty;
        string guitarString = string.Empty;
        string rhythmString = string.Empty;

        // Check if the audio location is the same as the filepath. If so, we only have to save the name of the file, not the full path.
        if (musicStream && Path.GetDirectoryName(audioLocations[MUSIC_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(filepath).Replace("\\", "/"))
            //musicString = musicStream.name;
            musicString = Path.GetFileName(audioLocations[MUSIC_STREAM_ARRAY_POS]);
        else
            musicString = audioLocations[MUSIC_STREAM_ARRAY_POS];

        if (guitarStream && Path.GetDirectoryName(audioLocations[GUITAR_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(filepath).Replace("\\", "/"))
            //guitarString = guitarStream.name;
            guitarString = Path.GetFileName(audioLocations[GUITAR_STREAM_ARRAY_POS]);
        else
            guitarString = audioLocations[GUITAR_STREAM_ARRAY_POS];

        if (rhythmStream && Path.GetDirectoryName(audioLocations[RHYTHM_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(filepath).Replace("\\", "/"))
            //rhythmString = rhythmStream.name;
            rhythmString = Path.GetFileName(audioLocations[RHYTHM_STREAM_ARRAY_POS]);
        else
            rhythmString = audioLocations[RHYTHM_STREAM_ARRAY_POS];

        string saveString = string.Empty;

        // Song properties
        saveString += "[Song]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
        saveString += GetPropertiesStringWithoutAudio();

        // Song audio
        if (musicStream != null)
            saveString += Globals.TABSPACE + "MusicStream = \"" + musicString + "\"" + Globals.LINE_ENDING;

        if (guitarStream != null)
            saveString += Globals.TABSPACE + "GuitarStream = \"" + guitarString + "\"" + Globals.LINE_ENDING;

        if (rhythmStream != null)
            saveString += Globals.TABSPACE + "RhythmStream = \"" + rhythmString + "\"" + Globals.LINE_ENDING;

        saveString += "}" + Globals.LINE_ENDING;

        // SyncTrack
        saveString += "[SyncTrack]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
        saveString += GetSaveString(_syncTrack.ToArray());
        saveString += "}" + Globals.LINE_ENDING;

        // Events
        saveString += "[Events]" + Globals.LINE_ENDING +"{" + Globals.LINE_ENDING;
        saveString += GetSaveString(_events.ToArray());
        saveString += "}" + Globals.LINE_ENDING;

        // Charts      
        for(int i = 0; i < charts.Length; ++i)
        {
            string chartString = string.Empty;
            chartString = charts[i].GetChartString(forced);

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

                saveString += chartString;
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

    /// <summary>
    /// Updates all read-only values and bpm assigned time values. 
    /// </summary>
    public void updateArrays()
    {
        events = _events.ToArray();
        sections = _events.OfType<Section>().ToArray();
        bpms = _syncTrack.OfType<BPM>().ToArray();
        timeSignatures = _syncTrack.OfType<TimeSignature>().ToArray();
        updateBPMTimeValues();
    }

    /// <summary>
    /// Dramatically speeds up calculations of songs with lots of bpm changes.
    /// </summary>
    void updateBPMTimeValues()
    {
        foreach (BPM bpm in bpms)
        {
            bpm.assignedTime = LiveChartPositionToTime(bpm.position, resolution);
        }
    }

    float LiveChartPositionToTime(uint position, float resolution)
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
}

class MonoWrapper : MonoBehaviour { }
