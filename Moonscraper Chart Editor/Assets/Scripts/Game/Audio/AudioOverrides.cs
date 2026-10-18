// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;
using MoonscraperChartEditor.Song.IO;

/// <summary>
/// Remembers the audio a user chose for a song's directory where it differs from the files Clone Hero would find there.
/// Games only find audio by file name, so this is kept in Moonscraper's own settings rather than in the chart.
/// </summary>
public static class AudioOverrides
{
    [Serializable]
    class AudioOverride
    {
        public string directory;
        public string audioInstrument;
        public string path;     // Empty if the user cleared this audio instrument
    }

    [Serializable]
    class AudioOverrideList
    {
        public List<AudioOverride> overrides = new List<AudioOverride>();
    }

    // Used by the loading thread as well as the main thread
    static readonly object s_lock = new object();
    static string s_filepath = string.Empty;
    static AudioOverrideList s_overrideList = new AudioOverrideList();

    public static void Load(string filepath)
    {
        lock (s_lock)
        {
            s_filepath = filepath;
            s_overrideList = new AudioOverrideList();

            try
            {
                if (File.Exists(filepath))
                    s_overrideList = JsonUtility.FromJson<AudioOverrideList>(File.ReadAllText(filepath)) ?? s_overrideList;
            }
            catch (Exception e)
            {
                Logger.LogException(e, "Unable to load audio overrides");
            }
        }
    }

    /// <summary>
    /// Sets the song's audio to the files the user chose for this directory. Files that no longer exist are skipped.
    /// </summary>
    public static void Apply(Song song, string directory)
    {
        directory = Path.GetFullPath(directory);

        lock (s_lock)
        {
            foreach (AudioOverride audioOverride in s_overrideList.overrides)
            {
                Song.AudioInstrument audio;
                if (IsSameDirectory(audioOverride.directory, directory)
                    && EnumX<Song.AudioInstrument>.GenericTryParse(audioOverride.audioInstrument, out audio)
                    && (audioOverride.path == string.Empty || File.Exists(audioOverride.path)))
                {
                    song.SetAudioLocation(audio, audioOverride.path);
                }
            }
        }
    }

    /// <summary>
    /// Remembers each of the song's audio files that differs from what Clone Hero would find in the directory, and forgets the rest.
    /// </summary>
    public static void Record(Song song, string directory)
    {
        directory = Path.GetFullPath(directory);
        var cloneHeroAudioFiles = CloneHeroAudioFiles.FindAudioFiles(directory);

        lock (s_lock)
        {
            int changeCount = s_overrideList.overrides.RemoveAll(audioOverride => IsSameDirectory(audioOverride.directory, directory));

            foreach (Song.AudioInstrument audio in EnumX<Song.AudioInstrument>.Values)
            {
                string cloneHeroAudioFile;
                if (!cloneHeroAudioFiles.TryGetValue(audio, out cloneHeroAudioFile))
                    cloneHeroAudioFile = string.Empty;

                string audioFile = song.GetAudioLocation(audio);
                if (!string.Equals(audioFile, cloneHeroAudioFile, StringComparison.OrdinalIgnoreCase))
                {
                    s_overrideList.overrides.Add(new AudioOverride() { directory = directory, audioInstrument = audio.ToString(), path = audioFile });
                    ++changeCount;
                }
            }

            if (changeCount > 0)
                Save();
        }
    }

    static bool IsSameDirectory(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    static void Save()
    {
        try
        {
            File.WriteAllText(s_filepath, JsonUtility.ToJson(s_overrideList, true), System.Text.Encoding.UTF8);
        }
        catch (Exception e)
        {
            Logger.LogException(e, "Unable to save audio overrides");
        }
    }
}
