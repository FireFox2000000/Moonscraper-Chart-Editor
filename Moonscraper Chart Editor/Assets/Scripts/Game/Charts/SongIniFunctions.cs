using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MoonscraperChartEditor.Song;

public static class SongIniFunctions
{
    public const string INI_SECTION_HEADER = "Song";
    public const string INI_FILENAME = "song.ini";

    const string CH_TAG_FILE = "clone_hero_ini_tags.txt";

    static KeyValuePair<string, string>[] s_chTags = null;
    static KeyValuePair<string, string>[] chTags
    {
        get
        {
            if (s_chTags == null)
            {
                // Only need to load this once, but need to wait for the working directory to be initialised correctly
                s_chTags = LoadTags(Path.Combine(Globals.CONFIG_FOLDER, CH_TAG_FILE));
                Debug.Assert(s_chTags != null);
            }

            return s_chTags;
        }
    }

    // song.ini is compatible with more games if there is a space before and after the '=' character in ini files. Ty Phase Shift.
    static string PrefixSpaceToINIValue(string val)
    {
        return " " + val.Trim();
    }

    public static void PopulateIniWithSongMetadata(Song song, INIParser ini, float songLengthSeconds)
    {
        Metadata metaData = song.metaData;

        int songLength = (int)(songLengthSeconds * 1000);

        ini.WriteValue(INI_SECTION_HEADER, "name ", PrefixSpaceToINIValue(song.name));
        ini.WriteValue(INI_SECTION_HEADER, "artist ", PrefixSpaceToINIValue(metaData.artist));
        ini.WriteValue(INI_SECTION_HEADER, "album ", PrefixSpaceToINIValue(metaData.album));
        ini.WriteValue(INI_SECTION_HEADER, "genre ", PrefixSpaceToINIValue(metaData.genre));
        ini.WriteValue(INI_SECTION_HEADER, "year ", PrefixSpaceToINIValue(metaData.year));
        ini.WriteValue(INI_SECTION_HEADER, "song_length ", PrefixSpaceToINIValue(songLength.ToString()));
        ini.WriteValue(INI_SECTION_HEADER, "charter ", PrefixSpaceToINIValue(metaData.charter));
    }

    delegate void AddTagFn(string key, string defaultVal);

    public static void AddDefaultIniTags(Song song, INIParser ini, float songLengthSeconds)
    {
        Metadata metaData = song.metaData;
        AddTagFn AddTagIfNonExistant = (string key, string defaultVal) => {
            ini.WriteValue(INI_SECTION_HEADER, key.Trim() + " ", ini.ReadValue(INI_SECTION_HEADER, key, PrefixSpaceToINIValue(defaultVal)));
        };

        AddTagIfNonExistant("name", song.name);
        AddTagIfNonExistant("artist", metaData.artist);
        AddTagIfNonExistant("album", metaData.album);
        AddTagIfNonExistant("genre", metaData.genre);
        AddTagIfNonExistant("year", metaData.year);
        AddTagIfNonExistant("song_length", ((int)(songLengthSeconds * 1000)).ToString());
        AddTagIfNonExistant("charter", metaData.charter);
    }

    static string GetCHDifficultyTagForInstrument(Song.Instrument instrument)
    {
        switch (instrument)
        {
            case Song.Instrument.Guitar:
                return "diff_guitar";

            case Song.Instrument.Rhythm:
                return "diff_rhythm";

            case Song.Instrument.Bass:
                return "diff_bass";

            case Song.Instrument.Drums:
                return "diff_drums";

            case Song.Instrument.Keys:
                return "diff_keys";

            case Song.Instrument.GHLiveGuitar:
                return "diff_guitarghl";

            case Song.Instrument.GHLiveBass:
                return "diff_bassghl";
        }

        return string.Empty;
    }

    static void AddTagIfAlreadyExistantAndDefault(INIParser ini, string key, string newValue, string defaultVal)
    {
        string realKey = key.Trim() + " ";
        if (ini.IsKeyExists(INI_SECTION_HEADER, realKey))
        {
            string currentValue = ini.ReadValue(INI_SECTION_HEADER, realKey, PrefixSpaceToINIValue(defaultVal));

            if (currentValue == PrefixSpaceToINIValue(defaultVal))
            {
                ini.WriteValue(INI_SECTION_HEADER, realKey, PrefixSpaceToINIValue(newValue));
            }
        }
    }

    public static void AddCloneHeroIniTags(Song song, INIParser ini, float songLengthSeconds)
    {
        AddTagFn AddTagIfNonExistant = (string key, string defaultVal) => {
            string realKey = key.Trim() + " ";
            ini.WriteValue(INI_SECTION_HEADER, realKey, ini.ReadValue(INI_SECTION_HEADER, realKey, PrefixSpaceToINIValue(defaultVal)));
        };
 
        AddDefaultIniTags(song, ini, songLengthSeconds);

        foreach (var tag in chTags)
        {
            AddTagIfNonExistant(tag.Key, tag.Value);
        }

        // Fill out difficulty tags with metadata difficulty if it has not been initilised
        {
            const string UninitialisedDifficultyValue = "-1";
            Metadata metaData = song.metaData;
            AddTagIfAlreadyExistantAndDefault(ini, "diff_band", metaData.difficulty.ToString(), UninitialisedDifficultyValue);

            foreach (Song.Instrument instrument in MoonscraperEngine.EnumX<Song.Instrument>.Values)
            {
                if (instrument == Song.Instrument.Unrecognised)
                    continue;

                string tag = GetCHDifficultyTagForInstrument(instrument);

                if (!string.IsNullOrEmpty(tag))
                {
                    AddTagIfAlreadyExistantAndDefault(ini, tag, metaData.difficulty.ToString(), UninitialisedDifficultyValue);
                }
            }
        }
    }

    static KeyValuePair<string, string>[] LoadTags(string filename)
    {
        List<KeyValuePair<string, string>> tags = new List<KeyValuePair<string, string>>();

        if (string.IsNullOrEmpty(Globals.realWorkingDirectory))
        {
            Debug.Assert(false, "Working directory has not been initialised yet.");
            return null;
        }

        string filepath = Path.Combine(Globals.realWorkingDirectory, filename);
        Debug.Log("Loading ini tags from " + Path.GetFullPath(filepath));

        if (File.Exists(filepath))
        {
            StreamReader ifs = null;
            try
            {
                ifs = File.OpenText(filepath);

                while (true)
                {
                    string line = ifs.ReadLine();
                    if (line == null)
                        break;

                    line.Replace('"', '\0');
                    if (line != string.Empty)
                    {
                        string[] splitLines = line.Split('=');
                        string key = splitLines[0];
                        string val = splitLines.Length > 1 ? splitLines[1] : string.Empty;

                        tags.Add(new KeyValuePair<string, string>(key.Trim(), val.Trim()));
                    }
                }

                Debug.Log(tags.Count + " tag strings loaded");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error: unable to load events- " + e.Message);
            }
            finally
            {
                if (ifs != null)
                    ifs.Close();
            }
        }

        return tags.ToArray();
    }

    public static string IniTextFromSongProperties(INIParser iniProperties)
    {
        string str = iniProperties.GetSectionValues(new string[] { INI_SECTION_HEADER, "song" }, INIParser.Formatting.Whitespaced);
        str = str.Replace("\r\n", "\n");
        return str;
    }

    public static INIParser SongIniFromString(string str)
    {
        INIParser newProperties = new INIParser();

        string[] seperatingTags = { System.Environment.NewLine.ToString(), "\n" };
        string[] customIniLines = str.Split(seperatingTags, System.StringSplitOptions.None);

        foreach (string line in customIniLines)
        {
            string[] keyVal = line.Split(new char[] { '=' }, 2);
            if (keyVal.Length >= 1)
            {
                string key = keyVal[0].Trim();
                string val = keyVal.Length > 1 ? keyVal[1].Trim() : string.Empty;

                if (!string.IsNullOrEmpty(key))
                    newProperties.WriteValue(INI_SECTION_HEADER, key + " ", " " + val);
            }
        }

        return newProperties;
    }

    public static INIParser FixupSongIniWhitespace(INIParser iniProperties)
    {
        return SongIniFromString(IniTextFromSongProperties(iniProperties));
    }
}
