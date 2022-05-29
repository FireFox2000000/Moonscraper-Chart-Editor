// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

public class SongAudioManager
{
    public SampleData[] audioSampleData { get; private set; }
    public TempoStream[] bassAudioStreams = new TempoStream[EnumX<Song.AudioInstrument>.Count];
    int audioLoads = 0;
    string audioFilesPath = string.Empty;
    const string AudioCacheFolderId = "TempSongAudio";

    public SongAudioManager()
    {
        audioSampleData = new SampleData[EnumX<Song.AudioInstrument>.Count];
        audioFilesPath = Path.Combine(Application.persistentDataPath, string.Format("{0}-{1}", AudioCacheFolderId, Guid.NewGuid().ToString()));
        Directory.CreateDirectory(audioFilesPath);

        Debug.Log(string.Format("Audio cache created at {0}", audioFilesPath));
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

        if (filepath == string.Empty) { return false; }

        filepath = filepath.Replace('\\', '/');

        // Check for valid extension
        if (!Utility.validateExtension(filepath, Globals.validAudioExtensions))
        {
            throw new System.Exception("Invalid file extension");
        }

        if (!File.Exists(filepath)) {
            Debug.LogError("Unable to locate audio file: " + filepath);
            return false;
        }

#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif

        ++audioLoads;

        try {
            string copiedFilepath = Path.Combine(audioFilesPath, audio.ToString() + Path.GetExtension(filepath));

            File.Copy(filepath, copiedFilepath, true);

            // Load sample data from waveform. This creates a thread on it's own.
            if (audioSampleData[audioStreamArrayPos] != null)
                audioSampleData[audioStreamArrayPos].Dispose();
            audioSampleData[audioStreamArrayPos] = new SampleData(copiedFilepath);

            // Load Audio Streams
            if (bassAudioStreams[audioStreamArrayPos] != null)
                bassAudioStreams[audioStreamArrayPos].Dispose();
            bassAudioStreams[audioStreamArrayPos] = AudioManager.LoadTempoStream(copiedFilepath);
        } catch (Exception e) {
            Logger.LogException(e, "Could not open audio");
            return false;
        } finally {
            --audioLoads;
        }

#if TIMING_DEBUG
        Debug.Log("Audio load time: " + (Time.realtimeSinceStartup - time));
#endif

        Debug.Log("Finished loading audio " + audio.ToString());

        return true;
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

    public void Dispose()
    {
        FreeAudioStreams();

        try
        {
            // Cleanup Temp Audio Files
            Directory.Delete(audioFilesPath, true);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    // In the case of a crash or shutdown failure, song cache's may still remain. 
    public static void ClearOldAudioCaches()
    {
        DateTime currentDate = DateTime.Now;

        string[] directories = Directory.GetDirectories(Application.persistentDataPath);
        foreach(string directory in directories)
        {
            if (directory.Contains(AudioCacheFolderId))
            {
                DateTime creationDate = Directory.GetCreationTime(directory);
                var delta = currentDate - creationDate;
                if (delta.Days > 2)
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch(UnauthorizedAccessException e)
                    {
                        Debug.LogErrorFormat("Skipping deletion of temp audio directory due to UnauthorizedAccessException: {0}", e.Message);
                    }
                }
            }
        }
    }
}
