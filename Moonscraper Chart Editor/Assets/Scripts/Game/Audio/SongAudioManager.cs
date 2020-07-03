// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.IO;
using UnityEngine;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

public class SongAudioManager
{
    public SampleData[] audioSampleData { get; private set; }
    public TempoStream[] bassAudioStreams = new TempoStream[EnumX<Song.AudioInstrument>.Count];
    int audioLoads = 0;

    public SongAudioManager()
    {
        audioSampleData = new SampleData[EnumX<Song.AudioInstrument>.Count];
    }

    ~SongAudioManager()
    {
        FreeAudioStreams();
    }

    public bool isAudioLoading
    {
        get
        {
            if (audioLoads > 0)
                return true;
            else
                return false;
        }
    }

    public void FreeAudioStreams()
    {
        foreach (var audio in EnumX<Song.AudioInstrument>.Values)
        {
            Clear(audio);
        }
    }

    public void Clear(Song.AudioInstrument audio)
    {
        {
            var stream = GetAudioStream(audio);
            if (stream != null)
                stream.Dispose();

            bassAudioStreams[(int)audio] = null;
        }

        {
            var sampleData = GetSampleData(audio);
            if (sampleData != null)
                sampleData.Dispose();

            audioSampleData[(int)audio] = null;
        }
    }

    public SampleData GetSampleData(Song.AudioInstrument audio)
    {
        return audioSampleData[(int)audio];
    }

    public TempoStream GetAudioStream(Song.AudioInstrument audio)
    {
        return bassAudioStreams[(int)audio];
    }

    public void SetBassAudioStream(Song.AudioInstrument audio, TempoStream stream)
    {
        int arrayPos = (int)audio;

        if (bassAudioStreams[arrayPos] != null)
            bassAudioStreams[arrayPos].Dispose();

        bassAudioStreams[arrayPos] = stream;
    }

    public AudioStream mainSongAudio
    {
        get
        {
            if (AudioManager.StreamIsValid(GetAudioStream(Song.AudioInstrument.Song)))
            {
                return GetAudioStream(Song.AudioInstrument.Song);
            }

            for (int i = 0; i < EnumX<Song.AudioInstrument>.Count; ++i)
            {
                Song.AudioInstrument audio = (Song.AudioInstrument)i;
                if (AudioManager.StreamIsValid(GetAudioStream(audio)))
                {
                    return GetAudioStream(audio);
                }
            }

            return null;
        }
    }

    public bool GetAudioIsLoaded(Song.AudioInstrument audio)
    {
        TempoStream stream = GetAudioStream(audio);
        return AudioManager.StreamIsValid(stream);
    }

    public bool LoadAudio(string filepath, Song.AudioInstrument audio)
    {
        int audioStreamArrayPos = (int)audio;

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

            ++audioLoads;

            // Load sample data from waveform. This creates a thread on it's own.
            if (audioSampleData[audioStreamArrayPos] != null)
                audioSampleData[audioStreamArrayPos].Dispose();
            audioSampleData[audioStreamArrayPos] = new SampleData(filepath);

            // Load Audio Streams   
            if (bassAudioStreams[audioStreamArrayPos] != null)
                bassAudioStreams[audioStreamArrayPos].Dispose();
            bassAudioStreams[audioStreamArrayPos] = AudioManager.LoadTempoStream(filepath);

            --audioLoads;
#if TIMING_DEBUG
            Debug.Log("Audio load time: " + (Time.realtimeSinceStartup - time));
#endif
            Debug.Log("Finished loading audio");

            return true;
        }
        else
        {
            if (filepath != string.Empty)
                Debug.LogError("Unable to locate audio file: " + filepath);
        }

        return false;
    }

    public void LoadAllAudioClips(Song song)
    {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif

        foreach (Song.AudioInstrument audio in EnumX<Song.AudioInstrument>.Values)
        {
            LoadAudio(song.GetAudioLocation(audio), audio);
        }
#if TIMING_DEBUG
        Debug.Log("Total audio files load time: " + (Time.realtimeSinceStartup - time));
#endif
    }
}
