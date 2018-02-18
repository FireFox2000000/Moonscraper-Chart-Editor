// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

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

public class Metadata
{
    public string name, artist, charter, player2, genre, mediatype, album, year;
    public int difficulty;
    public float previewStart, previewEnd;

    public Metadata()
    {
        name = artist = charter = album = year = string.Empty;
        player2 = "Bass";
        difficulty = 0;
        previewStart = previewEnd = 0;
        genre = "rock";
        mediatype = "cd";
    }

    public Metadata(Metadata metaData)
    {
        name = metaData.name;
        artist = metaData.artist;
        charter = metaData.charter;
        album = metaData.artist;
        year = metaData.year;
        player2 = metaData.player2;
        difficulty = metaData.difficulty;
        previewStart = metaData.previewStart;
        previewEnd = metaData.previewEnd;
        genre = metaData.genre;
        mediatype = metaData.mediatype;
    }
}

public class Song {
    public static readonly float STANDARD_BEAT_RESOLUTION = 192.0f;
    public const uint FULL_STEP = 768;
    public bool saveError = false;

    static int NUM_OF_DIFFICULTIES;
    static int NUM_OF_AUDIO_STREAMS = 5;
    public static bool streamAudio = true;

    public const int    MUSIC_STREAM_ARRAY_POS  = 0, 
                        GUITAR_STREAM_ARRAY_POS = 1, 
                        BASS_STREAM_ARRAY_POS   = 2, 
                        RHYTHM_STREAM_ARRAY_POS = 3, 
                        DRUM_STREAM_ARRAY_POS   = 4;

    // Song properties
    public Metadata metaData = new Metadata();
    public string name
    {
        get
        {
            return metaData.name;
        }
        set
        {
            metaData.name = value;
        }
    }
    public float resolution = 192, offset = 0;

#if !BASS_AUDIO
    AudioClip[] audioStreams = new AudioClip[3];
    public AudioClip musicStream { get { return audioStreams[MUSIC_STREAM_ARRAY_POS]; } set { audioStreams[MUSIC_STREAM_ARRAY_POS] = value; } }
    public AudioClip guitarStream { get { return audioStreams[GUITAR_STREAM_ARRAY_POS]; } set { audioStreams[GUITAR_STREAM_ARRAY_POS] = value; } }
    public AudioClip rhythmStream { get { return audioStreams[RHYTHM_STREAM_ARRAY_POS]; } set { audioStreams[RHYTHM_STREAM_ARRAY_POS] = value; } }
#endif
    SampleData[] audioSampleData = new SampleData[NUM_OF_AUDIO_STREAMS];
    public SampleData musicSample { get { return audioSampleData[MUSIC_STREAM_ARRAY_POS]; } private set { audioSampleData[MUSIC_STREAM_ARRAY_POS] = value; } }
    public SampleData guitarSample { get { return audioSampleData[GUITAR_STREAM_ARRAY_POS]; } private set { audioSampleData[GUITAR_STREAM_ARRAY_POS] = value; } }
    public SampleData bassSample { get { return audioSampleData[BASS_STREAM_ARRAY_POS]; } private set { audioSampleData[BASS_STREAM_ARRAY_POS] = value; } }
    public SampleData rhythmSample { get { return audioSampleData[RHYTHM_STREAM_ARRAY_POS]; } private set { audioSampleData[RHYTHM_STREAM_ARRAY_POS] = value; } }
    public SampleData drumSample { get { return audioSampleData[DRUM_STREAM_ARRAY_POS]; } private set { audioSampleData[DRUM_STREAM_ARRAY_POS] = value; } }

#if BASS_AUDIO
    public int[] bassAudioStreams = new int[NUM_OF_AUDIO_STREAMS];
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
    public int bassBassStream
    {
        get
        {
            return bassAudioStreams[BASS_STREAM_ARRAY_POS];
        }
        set
        {
            if (bassAudioStreams[BASS_STREAM_ARRAY_POS] != 0)
            {
                if (Bass.BASS_StreamFree(bassAudioStreams[BASS_STREAM_ARRAY_POS]))
                    Debug.Log("Rhythm audio stream successfully freed");
                else
                    Debug.LogError("Error while attempting to free rhythm audio stream");
            }

            bassAudioStreams[BASS_STREAM_ARRAY_POS] = value;
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
    public int bassDrumStream
    {
        get
        {
            return bassAudioStreams[DRUM_STREAM_ARRAY_POS];
        }
        set
        {
            if (bassAudioStreams[DRUM_STREAM_ARRAY_POS] != 0)
            {
                if (Bass.BASS_StreamFree(bassAudioStreams[DRUM_STREAM_ARRAY_POS]))
                    Debug.Log("Drum audio stream successfully freed");
                else
                    Debug.LogError("Error while attempting to free rhythm audio stream");
            }

            bassAudioStreams[DRUM_STREAM_ARRAY_POS] = value;
        }
    }
#endif

    public ExportOptions defaultExportOptions
    {
        get
        {
            ExportOptions exportOptions = default(ExportOptions);

            exportOptions.forced = true;
            exportOptions.copyDownEmptyDifficulty = false;
            exportOptions.format = ExportOptions.Format.Chart;
            exportOptions.targetResolution = this.resolution;
            exportOptions.tickOffset = 0;

            return exportOptions;
        }
    }

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
                else
                {
                    foreach (int stream in bassAudioStreams)
                    {
                        if (stream != 0)
                        {
                            float length = (float)Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream, BASSMode.BASS_POS_BYTES)) + offset;
                            return length;
                        }
                    }

                    return 300;     // 5 minutes
                }
#else
                if (musicStream)
                    return musicStream.length + offset;
                else
                    return 300;
#endif  
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

    public string[] audioLocations = new string[NUM_OF_AUDIO_STREAMS];

    public string musicSongName { get { return Path.GetFileName(audioLocations[MUSIC_STREAM_ARRAY_POS]); }
        set {
            if (File.Exists(value))
                audioLocations[MUSIC_STREAM_ARRAY_POS] = Path.GetFullPath(value);
        } }
    public string guitarSongName { get { return Path.GetFileName(audioLocations[GUITAR_STREAM_ARRAY_POS]); }
        set {
            if (File.Exists(value))
                audioLocations[GUITAR_STREAM_ARRAY_POS] = Path.GetFullPath(value); } }
    public string bassSongName
    {
        get { return Path.GetFileName(audioLocations[BASS_STREAM_ARRAY_POS]); }
        set
        {
            if (File.Exists(value))
                audioLocations[BASS_STREAM_ARRAY_POS] = Path.GetFullPath(value);
        }
    }
    public string rhythmSongName { get { return Path.GetFileName(audioLocations[RHYTHM_STREAM_ARRAY_POS]); }
        set {
            if (File.Exists(value))
                audioLocations[RHYTHM_STREAM_ARRAY_POS] = Path.GetFullPath(value); } }

    public string drumSongName
    {
        get { return Path.GetFileName(audioLocations[DRUM_STREAM_ARRAY_POS]); }
        set
        {
            if (File.Exists(value))
                audioLocations[DRUM_STREAM_ARRAY_POS] = Path.GetFullPath(value);
        }
    }

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

    public bool bassAudioLoaded
    {
        get
        {
#if BASS_AUDIO
            return bassBassStream != 0;
#else
            return bassStream ? true : false;
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

    public bool drumAudioLoaded
    {
        get
        {

            return bassDrumStream != 0;
        }
    }

    // Charts
    Chart[] charts;
    public List<Chart> unrecognisedCharts = new List<Chart>();

    public List<Event> _events;
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
    public Event[] eventsAndSections { get { return _events.ToArray(); } }

    /// <summary>
    /// Read only list of a song's bpm changes.
    /// </summary>
    public BPM[] bpms { get; private set; }
    /// <summary>
    /// Read only list of a song's time signature changes.
    /// </summary>
    public TimeSignature[] timeSignatures { get; private set; }

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
        int numberOfInstruments = Enum.GetNames(typeof(Instrument)).Length - 1;     // Don't count the "Unused" instrument
        charts = new Chart[numberOfInstruments * Enum.GetNames(typeof(Difficulty)).Length];

        for (int i = 0; i < charts.Length; ++i)
        {
            charts[i] = new Chart(this);
        }

        // Set the name of the chart
        foreach (Instrument instrument in Enum.GetValues(typeof(Instrument)))
        {
            if (instrument == Instrument.Unrecognised)
                continue;

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
                case (Instrument.Rhythm):
                    instrumentName += "Rhythm - ";
                    break;
                case (Instrument.Keys):
                    instrumentName += "Keys - ";
                    break;
                case (Instrument.Drums):
                    instrumentName += "Drums - ";
                    break;
                case (Instrument.GHLiveGuitar):
                    instrumentName += "GHLive Guitar - ";
                    break;
                case (Instrument.GHLiveBass):
                    instrumentName += "GHLive Bass - ";
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

        UpdateCache();
    }

    public Song(Song song)
    {
        metaData = new Metadata(song.metaData);
        offset = song.offset;
        resolution = song.resolution;

        for (int i = 0; i < audioLocations.Length; ++i)
        {
            audioLocations[i] = song.audioLocations[i];
        }

        _events = new List<Event>();
        _syncTrack = new List<SyncTrack>();

        _events.AddRange(song._events);
        _syncTrack.AddRange(song._syncTrack);

        charts = new Chart[song.charts.Length];
        for (int i = 0; i < charts.Length; ++i)
        {
            charts[i] = new Chart(song.charts[i], this);
        }
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
        LoadBassStream(audioLocations[BASS_STREAM_ARRAY_POS]);
        LoadRhythmStream(audioLocations[RHYTHM_STREAM_ARRAY_POS]);
        LoadDrumStream(audioLocations[DRUM_STREAM_ARRAY_POS]);
#if TIMING_DEBUG
        Debug.Log("Total audio files load time: " + (Time.realtimeSinceStartup - time));
#endif
    }

    public void LoadMusicStream(string filepath)
    {
        GameObject loadAudioObject = new GameObject("Load Audio");
        MonoWrapper coroutine = loadAudioObject.AddComponent<MonoWrapper>();

        coroutine.StartCoroutine(LoadAudio(filepath, MUSIC_STREAM_ARRAY_POS, loadAudioObject));
    }

    public void LoadGuitarStream(string filepath)
    {
        GameObject loadAudioObject = new GameObject("Load Audio");
        MonoWrapper coroutine = loadAudioObject.AddComponent<MonoWrapper>();

        coroutine.StartCoroutine(LoadAudio(filepath, GUITAR_STREAM_ARRAY_POS, loadAudioObject));
    }

    public void LoadBassStream(string filepath)
    {
        GameObject loadAudioObject = new GameObject("Load Audio");
        MonoWrapper coroutine = loadAudioObject.AddComponent<MonoWrapper>();

        coroutine.StartCoroutine(LoadAudio(filepath, BASS_STREAM_ARRAY_POS, loadAudioObject));
    }

    public void LoadRhythmStream(string filepath)
    {
        GameObject loadAudioObject = new GameObject("Load Audio");
        MonoWrapper coroutine = loadAudioObject.AddComponent<MonoWrapper>();

        coroutine.StartCoroutine(LoadAudio(filepath, RHYTHM_STREAM_ARRAY_POS, loadAudioObject));
    }

    public void LoadDrumStream(string filepath)
    {
        GameObject loadAudioObject = new GameObject("Load Drum Audio");
        MonoWrapper coroutine = loadAudioObject.AddComponent<MonoWrapper>();

        coroutine.StartCoroutine(LoadAudio(filepath, DRUM_STREAM_ARRAY_POS, loadAudioObject));
    }

    IEnumerator LoadAudio(string filepath, int audioStreamArrayPos, GameObject coroutine)
    {
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

                bassAudioStreams[audioStreamArrayPos] = Bass.BASS_StreamCreateFile(filepath, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_ASYNCFILE | BASSFlag.BASS_STREAM_PRESCAN);
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
        return TickFunctions.TimeToWorldYPosition(ChartPositionToTime(position, resolution));
    }

    public float ChartPositionToWorldYPosition(uint position, float resolution)
    {
        return TickFunctions.TimeToWorldYPosition(ChartPositionToTime(position, resolution));
    }

    public uint WorldYPositionToChartPosition(float worldYPos)
    {
        return TimeToChartPosition(TickFunctions.WorldYPositionToTime(worldYPos), resolution);
    }

    public uint WorldYPositionToChartPosition(float worldYPos, float resolution)
    {
        return TimeToChartPosition(TickFunctions.WorldYPositionToTime(worldYPos), resolution);
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
        position += TickFunctions.TimeToDis(prevBPM.assignedTime, time, resolution, prevBPM.value / 1000.0f);

        return position;
    }

    /// <summary>
    /// Finds the value of the first bpm that appears before or on the specified tick position.
    /// </summary>
    /// <param name="position">The tick position</param>
    /// <returns>Returns the value of the bpm that was found.</returns>
    public BPM GetPrevBPM(uint position)
    {
        int closestPos = SongObjectHelper.FindClosestPosition(position, bpms);
        if (closestPos != SongObjectHelper.NOTFOUND)
        {
            // Select the smaller of the two
            if (bpms[closestPos].position <= position)
                return bpms[closestPos];
            else if (closestPos > 0)
                return bpms[closestPos - 1];
        }

        return bpms[0];
    }

    /// <summary>
    /// Finds the value of the first time signature that appears before the specified tick position.
    /// </summary>
    /// <param name="position">The tick position</param>
    /// <returns>Returns the value of the time signature that was found.</returns>
    public TimeSignature GetPrevTS(uint position)
    {
        int closestPos = SongObjectHelper.FindClosestPosition(position, timeSignatures);
        if (closestPos != SongObjectHelper.NOTFOUND)
        {
            // Select the smaller of the two
            if (timeSignatures[closestPos].position <= position)
                return timeSignatures[closestPos];
            else if (closestPos > 0)
                return timeSignatures[closestPos - 1];
        }

        return timeSignatures[0];
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
        int previousBPMPos = SongObjectHelper.FindClosestPosition(position, bpms);
        if (bpms[previousBPMPos].position > position)
            --previousBPMPos;

        BPM prevBPM = bpms[previousBPMPos];
        float time = prevBPM.assignedTime;
        time += (float)TickFunctions.DisToTime(prevBPM.position, position, resolution, prevBPM.value / 1000.0f);

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
        SongObjectHelper.Insert(syncTrackObject, _syncTrack);

        if (autoUpdate)
            UpdateCache();

        ChartEditor.isDirty = true;
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
            success = SongObjectHelper.Remove(syncTrackObject, _syncTrack);
        }

        if (success)
        {
            syncTrackObject.song = null;
            ChartEditor.isDirty = true;
        }

        if (autoUpdate)
            UpdateCache();

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
        SongObjectHelper.Insert(eventObject, _events);

        if (autoUpdate)
            UpdateCache();

        ChartEditor.isDirty = true;
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
        success = SongObjectHelper.Remove(eventObject, _events);

        if (success)
        {
            eventObject.song = null;
            ChartEditor.isDirty = true;
        }

        if (autoUpdate)
            UpdateCache();

        return success;
    }

    /// <summary>
    /// Starts a thread that saves the song data in a .chart format to the specified path asynchonously. Can be monitored with the "IsSaving" parameter. 
    /// </summary>
    /// <param name="filepath">The path and filename to save to.</param>
    /// <param name="forced">Will the notes from each chart have their flag properties saved into the file?</param>
    public void SaveAsync(string filepath, ExportOptions exportOptions)
    {

#if false
        Song songCopy = new Song(this);
        songCopy.Save(filepath, exportOptions);

#if !UNITY_EDITOR
        This is for debugging only you moron
#endif
#else
        if (!IsSaving)
        {
            Song songCopy = new Song(this);

            saveThread = new System.Threading.Thread(() => songCopy.Save(filepath, exportOptions));
            saveThread.Start();
        }
#endif
    }

    /// <summary>
    /// Saves the song data in a .chart format to the specified path.
    /// </summary>
    /// <param name="filepath">The path and filename to save to.</param>
    /// <param name="forced">Will the notes from each chart have their flag properties saved into the file?</param>
    public void Save(string filepath, ExportOptions exportOptions)
    {
        string saveErrorMessage;
        try
        {
            new ChartWriter(filepath).Write(this, exportOptions, out saveErrorMessage);

            Debug.Log("Save complete!");

            if (saveErrorMessage != string.Empty)
            {
                saveError = true;
                ErrorMessage.errorMessage = "Save completed with the following errors: " + Globals.LINE_ENDING + saveErrorMessage;
            }
        }
        catch (System.Exception e)
        {
            saveError = true;
            ErrorMessage.errorMessage = "Save FAILED: " + e.Message;
        }
    }

    /// <summary>
    /// Updates all read-only values and bpm assigned time values. 
    /// </summary>
    public void UpdateCache()
    {
        events = _events.ToArray();
        sections = _events.OfType<Section>().ToArray();
        bpms = _syncTrack.OfType<BPM>().ToArray();
        timeSignatures = _syncTrack.OfType<TimeSignature>().ToArray();
        UpdateBPMTimeValues();

        //ChartEditor.FindCurrentEditor().FixUpBPMAnchors();
    }

    public void UpdateAllChartCaches()
    {
        foreach (Chart chart in charts)
            chart.UpdateCache();
    }

    /// <summary>
    /// Dramatically speeds up calculations of songs with lots of bpm changes.
    /// </summary>
    void UpdateBPMTimeValues()
    {
        foreach (BPM bpm in bpms)
        {
            bpm.assignedTime = LiveChartPositionToTime(bpm.position, resolution);
        }
    }

    public float LiveChartPositionToTime(uint position, float resolution)
    {
        double time = 0;
        BPM prevBPM = bpms[0];

        foreach (SyncTrack syncTrack in _syncTrack)
        {
            BPM bpmInfo = syncTrack as BPM;

            if (bpmInfo == null)
                continue;

            if (bpmInfo.position > position)
            {
                break;
            }
            else
            {
                time += TickFunctions.DisToTime(prevBPM.position, bpmInfo.position, resolution, prevBPM.value / 1000.0f);
                prevBPM = bpmInfo;
            }
        }

        time += TickFunctions.DisToTime(prevBPM.position, position, resolution, prevBPM.value / 1000.0f);

        return (float)time;
    }

    public float ResolutionScaleRatio (float targetResoltion)
    {
        return (targetResoltion / resolution);
    }

    public enum Difficulty
    {
        Expert = 0, Hard = 1, Medium = 2, Easy = 3
    }

    public enum Instrument
    {
        Guitar = 0, GuitarCoop = 1, Bass = 2, Rhythm = 3, Keys = 4, Drums = 5, GHLiveGuitar = 6, GHLiveBass = 7, Unrecognised = 99,
    }
}

/// <summary>
/// Allows coroutines to be run by dynamically creating a MonoBehaviour derived instance by creating it with this class.
/// </summary>
class MonoWrapper : MonoBehaviour { }
