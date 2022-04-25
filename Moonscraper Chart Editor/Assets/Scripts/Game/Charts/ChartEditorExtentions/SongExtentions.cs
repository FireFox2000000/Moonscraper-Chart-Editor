using MoonscraperChartEditor.Song;

public static class SongExtentions
{
    public static float TickToWorldYPosition(this Song song, uint position)
    {
        return ChartEditor.TimeToWorldYPosition(song.TickToTime(position, song.resolution));
    }

    public static float TickToWorldYPosition(this Song song, uint position, float resolution)
    {
        return ChartEditor.TimeToWorldYPosition(song.TickToTime(position, resolution));
    }

    public static uint WorldYPositionToTick(this Song song, float worldYPos)
    {
        return song.TimeToTick(ChartEditor.WorldYPositionToTime(worldYPos), song.resolution);
    }

    public static uint WorldYPositionToTick(this Song song, float worldYPos, float resolution)
    {
        return song.TimeToTick(ChartEditor.WorldYPositionToTime(worldYPos), resolution);
    }

    public static uint WorldPositionToSnappedTick(this Song song, float worldYPos, int step)
    {
        uint chartPos = song.WorldYPositionToTick(worldYPos);

        return Snapable.TickToSnappedTick(chartPos, step, song);
    }
}
