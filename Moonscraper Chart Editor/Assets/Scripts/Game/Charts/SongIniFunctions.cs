using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public static class SongIniFunctions
{
    public const string INI_SECTION_HEADER = "Song";
    public const string INI_FILENAME = "song.ini";

    public static void PopulateIniWithSongMetadata(Song song, INIParser ini, float songLengthSeconds)
    {
        Metadata metaData = song.metaData;

        ini.WriteValue(INI_SECTION_HEADER, "name", song.name);
        ini.WriteValue(INI_SECTION_HEADER, "artist", metaData.artist);
        ini.WriteValue(INI_SECTION_HEADER, "album", metaData.artist);
        ini.WriteValue(INI_SECTION_HEADER, "genre", metaData.genre);
        ini.WriteValue(INI_SECTION_HEADER, "year", metaData.year);
        ini.WriteValue(INI_SECTION_HEADER, "song_length", (int)(songLengthSeconds * 1000));
        ini.WriteValue(INI_SECTION_HEADER, "charter", metaData.charter);
        ini.WriteValue(INI_SECTION_HEADER, "name", song.name);
        ini.WriteValue(INI_SECTION_HEADER, "name", song.name);
    }
}
