// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace MoonscraperChartEditor.Song.IO
{
    /// <summary>
    /// Finds a song's audio the way Clone Hero does, by file name in the song's directory. YARG uses the same names.
    /// Neither game reads audio file locations from the chart.
    /// </summary>
    public static class CloneHeroAudioFiles
    {
        // Ordered by priority, the first file that exists is used
        static readonly string[] c_extensions = { ".opus", ".ogg", ".mp3", ".wav" };

        // Ordered by priority, the first file that exists is used
        static readonly IReadOnlyDictionary<Song.AudioInstrument, string[]> c_audioInstrumentToFilenames = new Dictionary<Song.AudioInstrument, string[]>()
        {
            { Song.AudioInstrument.Song,    new string[] { "song" } },
            { Song.AudioInstrument.Guitar,  new string[] { "guitar" } },
            { Song.AudioInstrument.Bass,    new string[] { "bass" } },
            { Song.AudioInstrument.Rhythm,  new string[] { "rhythm" } },
            { Song.AudioInstrument.Drum,    new string[] { "drums", "drums_1" } },
            { Song.AudioInstrument.Drums_2, new string[] { "drums_2" } },
            { Song.AudioInstrument.Drums_3, new string[] { "drums_3" } },
            { Song.AudioInstrument.Drums_4, new string[] { "drums_4" } },
            { Song.AudioInstrument.Vocals,  new string[] { "vocals", "vocals_1" } },
            { Song.AudioInstrument.Keys,    new string[] { "keys" } },
            { Song.AudioInstrument.Crowd,   new string[] { "crowd" } },
        };

        /// <summary>
        /// Finds the audio file for each audio instrument in a song's directory.
        /// Letter case is ignored, so the result is the same on every platform.
        /// </summary>
        /// <returns>The full path of each file found. Audio instruments without a file are left out.</returns>
        public static Dictionary<Song.AudioInstrument, string> FindAudioFiles(string directory)
        {
            var audioFiles = new Dictionary<Song.AudioInstrument, string>();
            var filesByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    string filename = Path.GetFileName(file);
                    if (!filesByName.ContainsKey(filename))
                        filesByName.Add(filename, Path.GetFullPath(file));
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e, "Unable to search for audio files in " + directory);
                return audioFiles;
            }

            foreach (var instrumentFilenames in c_audioInstrumentToFilenames)
            {
                string audioFile = FindFirstFile(filesByName, instrumentFilenames.Value);
                if (audioFile != null)
                    audioFiles.Add(instrumentFilenames.Key, audioFile);
            }

            return audioFiles;
        }

        /// <summary>
        /// The name, without extension, that Clone Hero looks for first for an audio instrument.
        /// </summary>
        public static string GetFilename(Song.AudioInstrument audio)
        {
            return c_audioInstrumentToFilenames[audio][0];
        }

        static string FindFirstFile(Dictionary<string, string> filesByName, string[] filenames)
        {
            foreach (string filename in filenames)
            {
                foreach (string extension in c_extensions)
                {
                    string file;
                    if (filesByName.TryGetValue(filename + extension, out file))
                        return file;
                }
            }

            return null;
        }
    }
}
