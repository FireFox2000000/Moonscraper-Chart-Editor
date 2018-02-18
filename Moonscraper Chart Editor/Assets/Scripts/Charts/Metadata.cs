
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
