using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public static class SongIniFunctions
{
    public const string INI_SECTION_HEADER = "Song";
    public const string INI_FILENAME = "song.ini";

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

        AddTagIfNonExistant("count", "0");
        AddTagIfNonExistant("diff_band", "-1");
        AddTagIfNonExistant("diff_guitar", "-1");
        AddTagIfNonExistant("diff_bass", "-1");
        AddTagIfNonExistant("diff_drums", "-1");
        AddTagIfNonExistant("diff_keys", "-1");
        AddTagIfNonExistant("diff_guitarghl", "-1");
        AddTagIfNonExistant("diff_bassghl", "-1");
        AddTagIfNonExistant("preview_start_time", "0");
        AddTagIfNonExistant("frets", "0");
        AddTagIfNonExistant("icon", "0");
        AddTagIfNonExistant("playlist_track", "");
        AddTagIfNonExistant("track", "");
        AddTagIfNonExistant("album_track", "");
        AddTagIfNonExistant("delay", "0");
        AddTagIfNonExistant("loading_phrase", "");
    }
}
