// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

namespace MoonscraperChartEditor.Song
{
    public class Metadata
    {
        // The difficulty of a song that has none set. Earlier versions wrote a difficulty of 0 into every chart, so 0 is also none.
        public const int NoDifficulty = -1;

        string m_name, m_artist, m_charter, m_player2, m_genre, m_mediatype, m_album;
        int m_difficulty;

        public string name { get { return m_name; } set { m_name = MakeValidMetadataString(value); } }
        public string artist { get { return m_artist; } set { m_artist = MakeValidMetadataString(value); } }
        public string charter { get { return m_charter; } set { m_charter = MakeValidMetadataString(value); } }
        public string player2 { get { return m_player2; } set { m_player2 = MakeValidMetadataString(value); } }
        public string genre { get { return m_genre; } set { m_genre = MakeValidMetadataString(value); } }
        public string mediatype { get { return m_mediatype; } set { m_mediatype = MakeValidMetadataString(value); } }
        public string album { get { return m_album; } set { m_album = MakeValidMetadataString(value); } }
        public string year;

        public int difficulty { get { return m_difficulty; } set { m_difficulty = value > 0 ? value : NoDifficulty; } }
        public bool hasDifficulty { get { return m_difficulty != NoDifficulty; } }

        // Preview times are only written when a loaded chart has them, since games choose a preview when there is none
        public float? previewStart, previewEnd;

        public Metadata()
        {
            name = artist = charter = album = year = genre = string.Empty;
            player2 = "Bass";
            difficulty = NoDifficulty;
            previewStart = previewEnd = null;
            mediatype = "cd";
        }

        public Metadata(Metadata metaData)
        {
            name = metaData.name;
            artist = metaData.artist;
            charter = metaData.charter;
            album = metaData.album;
            year = metaData.year;
            player2 = metaData.player2;
            difficulty = metaData.difficulty;
            previewStart = metaData.previewStart;
            previewEnd = metaData.previewEnd;
            genre = metaData.genre;
            mediatype = metaData.mediatype;
        }

        string MakeValidMetadataString(string v)
        {
            return v.Replace("\"", string.Empty);
        }
    }
}
