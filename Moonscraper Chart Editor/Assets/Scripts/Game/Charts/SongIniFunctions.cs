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
    public static void AddCloneHeroIniTags(Song song, INIParser ini, float songLengthSeconds)
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

        foreach(var tag in chTags)
        {
            AddTagIfNonExistant(tag.Key, tag.Value);
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
}
