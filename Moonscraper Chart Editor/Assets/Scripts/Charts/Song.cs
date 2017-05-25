//#define SONG_DEBUG
//#define TIMING_DEBUG
//#define LOAD_AUDIO_ASYNC
#define BASS_AUDIO

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using Un4seen.Bass;

public class Song {
    static int NUM_OF_DIFFICULTIES;
    public static bool streamAudio = true;

    const int MUSIC_STREAM_ARRAY_POS = 0;
    const int GUITAR_STREAM_ARRAY_POS = 1;
    const int RHYTHM_STREAM_ARRAY_POS = 2;

    const int TEXT_POS_TICK = 0;
    const int TEXT_POS_EVENT_TYPE = 2;
    const int TEXT_POS_DATA_1 = 3;

    // Song properties
    public string name = string.Empty, artist = string.Empty, charter = string.Empty;
    public string player2 = "Bass";
    public int difficulty = 0;
    public float offset = 0, previewStart = 0, previewEnd = 0, resolution = 192;
    public string genre = "rock", mediatype = "cd";
    public string year = string.Empty;
#if !BASS_AUDIO
    AudioClip[] audioStreams = new AudioClip[3];
    public AudioClip musicStream { get { return audioStreams[MUSIC_STREAM_ARRAY_POS]; } set { audioStreams[MUSIC_STREAM_ARRAY_POS] = value; } }
    public AudioClip guitarStream { get { return audioStreams[GUITAR_STREAM_ARRAY_POS]; } set { audioStreams[GUITAR_STREAM_ARRAY_POS] = value; } }
    public AudioClip rhythmStream { get { return audioStreams[RHYTHM_STREAM_ARRAY_POS]; } set { audioStreams[RHYTHM_STREAM_ARRAY_POS] = value; } }
#endif
    SampleData[] audioSampleData = new SampleData[3];
    public SampleData musicSample { get { return audioSampleData[MUSIC_STREAM_ARRAY_POS]; } private set { audioSampleData[MUSIC_STREAM_ARRAY_POS] = value; } }
    public SampleData guitarSample { get { return audioSampleData[GUITAR_STREAM_ARRAY_POS]; } private set { audioSampleData[GUITAR_STREAM_ARRAY_POS] = value; } }
    public SampleData rhythmSample { get { return audioSampleData[RHYTHM_STREAM_ARRAY_POS]; } private set { audioSampleData[RHYTHM_STREAM_ARRAY_POS] = value; } }

#if BASS_AUDIO
    int[] bassAudioStreams = new int[3];
    public int bassMusicStream
    {
        get
        {
            return bassAudioStreams[MUSIC_STREAM_ARRAY_POS];
        }
        set
        {
            if (bassAudioStreams[MUSIC_STREAM_ARRAY_POS] != 0)
            {
                if (Bass.BASS_StreamFree(bassAudioStreams[MUSIC_STREAM_ARRAY_POS]))
                    Debug.Log("Song audio stream successfully freed");
                else
                    Debug.LogError("Error while attempting to free song audio stream");
            }

            bassAudioStreams[MUSIC_STREAM_ARRAY_POS] = value;
        }
    }
    public int bassGuitarStream
    {
        get { return bassAudioStreams[GUITAR_STREAM_ARRAY_POS]; }
        set
        {
            if (bassAudioStreams[GUITAR_STREAM_ARRAY_POS] != 0)
            {
                if (Bass.BASS_StreamFree(bassAudioStreams[GUITAR_STREAM_ARRAY_POS]))
                    Debug.Log("Guitar audio stream successfully freed");
                else
                    Debug.LogError("Error while attempting to free guitar audio stream");
            }

            bassAudioStreams[GUITAR_STREAM_ARRAY_POS] = value;
        }
    }
    public int bassRhythmStream
    {
        get
        {
            return bassAudioStreams[RHYTHM_STREAM_ARRAY_POS];
        }
        set
        {
            if (bassAudioStreams[RHYTHM_STREAM_ARRAY_POS] != 0)
            {
                if (Bass.BASS_StreamFree(bassAudioStreams[RHYTHM_STREAM_ARRAY_POS]))
                    Debug.Log("Rhythm audio stream successfully freed");
                else
                    Debug.LogError("Error while attempting to free rhythm audio stream");
            }

            bassAudioStreams[RHYTHM_STREAM_ARRAY_POS] = value;
        }
    }
#endif
    float _length = 300;
    public float length
    {
        get
        {
            if (manualLength)
                return _length;
            else
            {
#if BASS_AUDIO
                if (bassMusicStream != 0)
                    return (float)Bass.BASS_ChannelBytes2Seconds(bassMusicStream, Bass.BASS_ChannelGetLength(bassMusicStream, BASSMode.BASS_POS_BYTES)) + offset;
#else
                if (musicStream)
                    return musicStream.length + offset;
#endif
                else
                    return 300;     // 5 minutes
            }
        }
        set
        {
            if (manualLength)
                _length = value;
        }
    }

    bool _manualLength = false;
    public bool manualLength
    {
        get
        {
            return _manualLength;
        }
        set
        {
            _manualLength = value;
            _length = length;
        }
    }

    string[] audioLocations = new string[3];

    public string musicSongName { get { return Path.GetFileName(audioLocations[MUSIC_STREAM_ARRAY_POS]); }
        set {
            if (File.Exists(value))
                audioLocations[MUSIC_STREAM_ARRAY_POS] = Path.GetFullPath(value);
        } }
    public string guitarSongName { get { return Path.GetFileName(audioLocations[GUITAR_STREAM_ARRAY_POS]); }
        set {
            if (File.Exists(value))
                audioLocations[GUITAR_STREAM_ARRAY_POS] = Path.GetFullPath(value); } }
    public string rhythmSongName { get { return Path.GetFileName(audioLocations[RHYTHM_STREAM_ARRAY_POS]); }
        set {
            if (File.Exists(value))
                audioLocations[RHYTHM_STREAM_ARRAY_POS] = Path.GetFullPath(value); } }

    public bool songAudioLoaded
    {
        get
        {
#if BASS_AUDIO
            return bassMusicStream != 0;
#else
            return musicStream ? true : false;
#endif
        }
    }

    public bool guitarAudioLoaded
    {
        get
        {
#if BASS_AUDIO
            return bassGuitarStream != 0;
#else
            return guitarStream ? true : false;
#endif
        }
    }

    public bool rhythmAudioLoaded
    {
        get
        {
#if BASS_AUDIO
            return bassRhythmStream != 0;
#else
            return rhythmStream ? true : false;
#endif
        }
    }

    // Charts
    Chart[] charts;// = new Chart[12];

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

    /// <summary>
    /// Is this song currently being saved asyncronously?
    /// </summary>
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
        NUM_OF_DIFFICULTIES = Enum.GetValues(typeof(Difficulty)).Length;

        _events = new List<Event>();
        _syncTrack = new List<SyncTrack>();

        events = new Event[0];
        sections = new Section[0];
        bpms = new BPM[0];
        timeSignatures = new TimeSignature[0];

        Add(new BPM());
        Add(new TimeSignature());

        // Chart initialisation
        charts = new Chart[Enum.GetNames(typeof(Instrument)).Length * Enum.GetNames(typeof(Difficulty)).Length];

        for (int i = 0; i < charts.Length; ++i)
        {
            charts[i] = new Chart(this);
        }

        // Set the name of the chart
        foreach (Instrument instrument in Enum.GetValues(typeof(Instrument)))
        {
            string instrumentName = string.Empty;
            switch (instrument)
            {
                case (Instrument.Guitar):
                    instrumentName += "Guitar - ";
                    break;
                case (Instrument.GuitarCoop):
                    instrumentName += "Guitar - Co-op - ";
                    break;
                case (Instrument.Bass):
                    instrumentName += "Bass - ";
                    break;
                case (Instrument.Keys):
                    instrumentName += "Keys - ";
                    break;
                default:
                    continue;
            }

            foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
            {
                GetChart(instrument, difficulty).name = instrumentName + difficulty.ToString();
            }
        }

        for (int i = 0; i < audioLocations.Length; ++i)
            audioLocations[i] = string.Empty;

        for (int i = 0; i < audioSampleData.Length; ++i)
            audioSampleData[i] = new SampleData(string.Empty);

        updateArrays();
    }

#if BASS_AUDIO
    ~Song()
    {
        FreeBassAudioStreams();
    }

    public void FreeBassAudioStreams()
    {
        for (int i = 0; i < bassAudioStreams.Length; ++i)
        {
            if (bassAudioStreams[i] != 0)
            {
                if (!Bass.BASS_StreamFree(bassAudioStreams[i]))
                    Debug.LogError("Error while freeing audio stream " + bassAudioStreams[i]);
                else
                    bassAudioStreams[i] = 0;
            }
        }

        foreach (SampleData sample in audioSampleData)
            sample.Free();
    }
#endif
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

    public Chart GetChart(Instrument instrument, Difficulty difficulty)
    {
        try
        {
            return charts[(int)instrument * NUM_OF_DIFFICULTIES + (int)difficulty];
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return charts[0];
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
        //string temp_wav_filepath = Application.persistentDataPath + "\\" + TEMP_MP3_TO_WAV_FILEPATH;
        string convertedFromMp3 = string.Empty;
#if !BASS_AUDIO
        if (audioStreams[audioStreamArrayPos])
        {
            audioStreams[audioStreamArrayPos].UnloadAudioData();
            GameObject.Destroy(audioStreams[audioStreamArrayPos]);
        }
#endif
        
        if (filepath != string.Empty && File.Exists(filepath))
        {
#if TIMING_DEBUG
            float time = Time.realtimeSinceStartup;
#endif
            // Check for valid extension
            if (!Utility.validateExtension(filepath, Globals.validAudioExtensions))
            {
                throw new System.Exception("Invalid file extension");
            }

            filepath = filepath.Replace('\\', '/');

            // Record the filepath
            audioLocations[audioStreamArrayPos] = Path.GetFullPath(filepath);
            ++audioLoads;
#if BASS_AUDIO
                   
            System.Threading.Thread streamCreateFileThread = new System.Threading.Thread(() =>
            {
                // Load Bass Audio Streams   
                audioSampleData[audioStreamArrayPos].Free();
                audioSampleData[audioStreamArrayPos] = new SampleData(filepath);
                audioSampleData[audioStreamArrayPos].ReadAudioFile();

                bassAudioStreams[audioStreamArrayPos] = Bass.BASS_StreamCreateFile(filepath, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                bassAudioStreams[audioStreamArrayPos] = Un4seen.Bass.AddOn.Fx.BassFx.BASS_FX_TempoCreate(bassAudioStreams[audioStreamArrayPos], BASSFlag.BASS_FX_FREESOURCE);
            });

            streamCreateFileThread.Start();

            while (streamCreateFileThread.ThreadState == System.Threading.ThreadState.Running)
                yield return null;

            --audioLoads;
#endif
#if TIMING_DEBUG
            Debug.Log("Audio load time: " + (Time.realtimeSinceStartup - time));
#endif
            Debug.Log("Finished loading audio");       
        }
        else
        {
            if (filepath != string.Empty)
                Debug.LogError("Unable to locate audio file");
        }

        GameObject.Destroy(coroutine);
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
    public uint TimeToChartPosition(float time, float resolution, bool capByLength = true)
    {
        if (time < 0)
            time = 0;
        else if (capByLength && time > length)
            time = length;

        uint position = 0;

        BPM prevBPM = bpms[0];

        // Search for the last bpm
        foreach (BPM bpmInfo in bpms)
        {
            if (bpmInfo.assignedTime >= time)
                break;
            else
                prevBPM = bpmInfo;
        }

        position = prevBPM.position;
        position += time_to_dis(prevBPM.assignedTime, time, resolution, prevBPM.value / 1000.0f);

        return position;
    }

    /// <summary>
    /// Finds the value of the first bpm that appears before or on the specified tick position.
    /// </summary>
    /// <param name="position">The tick position</param>
    /// <returns>Returns the value of the bpm that was found.</returns>
    public uint GetPrevBPM(uint position)
    {
        int closestPos = SongObject.FindClosestPosition(position, bpms);
        if (closestPos != SongObject.NOTFOUND)
        {
            // Select the smaller of the two
            if (bpms[closestPos].position <= position)
                return bpms[closestPos].position;
            else if (closestPos > 0)
                return bpms[closestPos - 1].position;
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
        int closestPos = SongObject.FindClosestPosition(position, timeSignatures);
        if (closestPos != SongObject.NOTFOUND)
        {
            // Select the smaller of the two
            if (timeSignatures[closestPos].position <= position)
                return timeSignatures[closestPos].position;
            else if (closestPos > 0)
                return timeSignatures[closestPos - 1].position;
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
    /// <returns>Returns the time in seconds.</returns>
    public float ChartPositionToTime(uint position)
    {
        return ChartPositionToTime(position, this.resolution);
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

    public static uint time_to_dis(float time_start, float time_end, float resolution, float bpm)
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
                //submitDataGlobals(stringData);
                //break;
            case ("[Events]"):
#if SONG_DEBUG
                Debug.Log("Loading events data");
#endif
                //submitDataEvents(stringData);
                submitDataGlobals(stringData);
                break;
            case ("[EasySingle]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasySingle");
#endif
                GetChart(Instrument.Guitar, Difficulty.Easy).Load(stringData);
                break;
            case ("[EasyDoubleGuitar]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                GetChart(Instrument.GuitarCoop, Difficulty.Easy).Load(stringData);
                break;
            case ("[EasyDoubleBass]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                GetChart(Instrument.Bass, Difficulty.Easy).Load(stringData);
                break;
            case ("[MediumSingle]"):
#if SONG_DEBUG
                Debug.Log("Loading chart MediumSingle");
#endif
                GetChart(Instrument.Guitar, Difficulty.Medium).Load(stringData);
                break;
            case ("[MediumDoubleGuitar]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                GetChart(Instrument.GuitarCoop, Difficulty.Medium).Load(stringData);
                break;
            case ("[MediumDoubleBass]"):
#if SONG_DEBUG
                Debug.Log("Loading chart MediumDoubleBass");
#endif
                GetChart(Instrument.Bass, Difficulty.Medium).Load(stringData);
                break;
            case ("[HardSingle]"):
#if SONG_DEBUG
                Debug.Log("Loading chart HardSingle");
#endif
                GetChart(Instrument.Guitar, Difficulty.Hard).Load(stringData);
                break;
            case ("[HardDoubleGuitar]"):
#if SONG_DEBUG
                Debug.Log("Loading chart EasyDoubleBass");
#endif
                GetChart(Instrument.GuitarCoop, Difficulty.Hard).Load(stringData);
                break;
            case ("[HardDoubleBass]"):
#if SONG_DEBUG
                Debug.Log("Loading chart HardDoubleBass");
#endif
                GetChart(Instrument.Bass, Difficulty.Hard).Load(stringData);
                break;
            case ("[ExpertSingle]"):
#if SONG_DEBUG
                Debug.Log("Loading chart ExpertSingle");
#endif
                GetChart(Instrument.Guitar, Difficulty.Expert).Load(stringData);
                break;
            case ("[ExpertDoubleGuitar]"):
#if SONG_DEBUG
                Debug.Log("Loading chart ExpertDoubleBass");
#endif
                GetChart(Instrument.GuitarCoop, Difficulty.Expert).Load(stringData);
                break;
            case ("[ExpertDoubleBass]"):
#if SONG_DEBUG
                Debug.Log("Loading chart ExpertDoubleBass");
#endif
                GetChart(Instrument.Bass, Difficulty.Expert).Load(stringData);
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
        Regex lengthRegex = new Regex(@"Length = " + FLOATSEARCH);
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
                    resolution = short.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
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

                // Length = 300
                else if (lengthRegex.IsMatch(line))
                {
                    manualLength = true;
                    length = float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
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
        if (manualLength)
            saveString += Globals.TABSPACE + "Length = " + _length + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "PreviewStart = " + previewStart + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "PreviewEnd = " + previewEnd + Globals.LINE_ENDING;
        if (genre != string.Empty)
            saveString += Globals.TABSPACE + "Genre = \"" + genre + "\"" + Globals.LINE_ENDING;
        if (mediatype != string.Empty)
            saveString += Globals.TABSPACE + "MediaType = \"" + mediatype + "\"" + Globals.LINE_ENDING;

        return saveString;
    }

    void submitDataGlobals(List<string> stringData)
    {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        foreach (string line in stringData)
        {
            string[] stringSplit = Regex.Split(line, @"\s+");
            uint position;
            string eventType;
            if (stringSplit.Length > TEXT_POS_DATA_1 && uint.TryParse(stringSplit[TEXT_POS_TICK], out position))
            {
                eventType = stringSplit[TEXT_POS_EVENT_TYPE];
                eventType = eventType.ToLower();
            }
            else
                continue;

            if (eventType == "ts")
            {
                uint numerator;
                uint denominator = 4;

                if (!uint.TryParse(stringSplit[TEXT_POS_DATA_1], out numerator))
                    continue;

                if (stringSplit.Length > TEXT_POS_DATA_1 + 1 && !uint.TryParse(stringSplit[TEXT_POS_DATA_1 + 1], out denominator))
                    continue;                   

                Add(new TimeSignature(position, numerator, denominator), false);
            }
            else if (eventType == "b")
            {
                uint value;
                if (!uint.TryParse(stringSplit[TEXT_POS_DATA_1], out value))
                    continue;

                Add(new BPM(position, value), false);
            }
            else if (eventType == "e")       // 0 = E "section Intro"
            {
                if (stringSplit.Length > TEXT_POS_DATA_1 + 1 && stringSplit[TEXT_POS_DATA_1] == "\"section")
                {
                    string title = string.Empty;// = stringSplit[TEXT_POS_DATA_1 + 1].Trim('"');
                    for (int i = TEXT_POS_DATA_1 + 1; i < stringSplit.Length; ++i)
                    {
                        title += stringSplit[i].Trim('"');
                        if (i < stringSplit.Length - 1)
                            title += " ";
                    }
                    Add(new Section(title, position), false);
                }
                else
                {
                    string title = stringSplit[TEXT_POS_DATA_1].Trim('"');
                    Add(new Event(title, position), false);
                }
            }
        }
#if TIMING_DEBUG
        Debug.Log("Synctrack load time: " + (Time.realtimeSinceStartup - time));
#endif
    }
    /*
    void submitDataEvents(List<string> stringData)
    {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        foreach (string line in stringData)
        {
            string[] stringSplit = Regex.Split(line, @"\s+");
            uint position;
            string eventType;
            if (stringSplit.Length > TEXT_POS_DATA_1 && uint.TryParse(stringSplit[TEXT_POS_TICK], out position))
            {
                eventType = stringSplit[TEXT_POS_EVENT_TYPE];
                eventType = eventType.ToLower();
            }
            else
                continue;

            if (Section.regexMatch(line))       // 0 = E "section Intro"
            {
                // Add a section
                string title = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"').Substring(8);
                Add(new Section(title, position), false);
            }
            else if (Event.regexMatch(line))    // 125952 = E "end"
            {
                // Add an event
                string title = Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
                Add(new Event(title, position), false);
            }
        }
#if TIMING_DEBUG
        Debug.Log("Events load time: " + (Time.realtimeSinceStartup - time));
#endif
    }*/

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
    /// Starts a thread that saves the song data in a .chart format to the specified path asynchonously. Can be monitored with the "IsSaving" parameter. 
    /// </summary>
    /// <param name="filepath">The path and filename to save to.</param>
    /// <param name="forced">Will the notes from each chart have their flag properties saved into the file?</param>
    public void SaveAsync(string filepath, bool forced = true)
    {
        saveThread = new System.Threading.Thread(() => Save(filepath, forced));
        saveThread.Start();
    }

    /// <summary>
    /// Saves the song data in a .chart format to the specified path.
    /// </summary>
    /// <param name="filepath">The path and filename to save to.</param>
    /// <param name="forced">Will the notes from each chart have their flag properties saved into the file?</param>
    public void Save(string filepath, bool forced = true)
    {
        string musicString = string.Empty;
        string guitarString = string.Empty;
        string rhythmString = string.Empty;

        // Check if the audio location is the same as the filepath. If so, we only have to save the name of the file, not the full path.
        if (songAudioLoaded && Path.GetDirectoryName(audioLocations[MUSIC_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(filepath).Replace("\\", "/"))
            musicString = Path.GetFileName(audioLocations[MUSIC_STREAM_ARRAY_POS]);
        else
            musicString = audioLocations[MUSIC_STREAM_ARRAY_POS];

        if (guitarAudioLoaded && Path.GetDirectoryName(audioLocations[GUITAR_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(filepath).Replace("\\", "/"))
            guitarString = Path.GetFileName(audioLocations[GUITAR_STREAM_ARRAY_POS]);
        else
            guitarString = audioLocations[GUITAR_STREAM_ARRAY_POS];

        if (rhythmAudioLoaded && Path.GetDirectoryName(audioLocations[RHYTHM_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(filepath).Replace("\\", "/"))
            rhythmString = Path.GetFileName(audioLocations[RHYTHM_STREAM_ARRAY_POS]);
        else
            rhythmString = audioLocations[RHYTHM_STREAM_ARRAY_POS];

        string saveString = string.Empty;

        // Song properties
        saveString += "[Song]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
        saveString += GetPropertiesStringWithoutAudio();

        // Song audio
        if (songAudioLoaded)
            saveString += Globals.TABSPACE + "MusicStream = \"" + musicString + "\"" + Globals.LINE_ENDING;

        if (guitarAudioLoaded)
            saveString += Globals.TABSPACE + "GuitarStream = \"" + guitarString + "\"" + Globals.LINE_ENDING;

        if (rhythmAudioLoaded)
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
        var difficulties = Enum.GetValues(typeof(Difficulty));

        foreach (Instrument instrument in Enum.GetValues(typeof(Instrument)))
        {
            string instrumentSaveString = string.Empty;
            switch (instrument)
            {
                case (Instrument.Guitar):
                    instrumentSaveString = "Single";
                    break;
                case (Instrument.GuitarCoop):
                    instrumentSaveString = "DoubleGuitar";
                    break;
                case (Instrument.Bass):
                    instrumentSaveString = "DoubleBass";
                    break;
                default:
                    continue;
            }

            foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
            {
                string difficultySaveString = difficulty.ToString();
                string chartString = GetChart(instrument, difficulty).GetChartString(forced);

                if (chartString == string.Empty)
                    continue;

                string seperator = "[" + difficultySaveString + instrumentSaveString + "]";
                saveString += seperator + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
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

    public void updateAllChartArrays()
    {
        foreach (Chart chart in charts)
            chart.updateArrays();
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

    public enum Difficulty
    {
        Expert = 0, Hard = 1, Medium = 2, Easy = 3
    }

    public enum Instrument
    {
        Guitar = 0, GuitarCoop = 1, Bass = 2, Keys = 3
    }
}

/// <summary>
/// Allows coroutines to be run by dynamically creating a MonoBehaviour derived instance by creating it with this class.
/// </summary>
class MonoWrapper : MonoBehaviour { }
