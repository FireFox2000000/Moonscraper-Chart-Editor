using System;
using MoonscraperChartEditor.Song;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public interface ISongValidateReport
{
    void NotifySongObjectBeyondExpectedLength(SongObject song);
    void NotifySectionLimitError(Song song, int sectionLimit);
    void NotifyTimeSignaturePlacementError(TimeSignature ts);
    void NotifyRockBandMidiSoloStarpowerMisRead(Song.Instrument instrument);

    void NotifyOpenChordFound(Note note);
    void NotifyOpenTapFound(Note note);
}

public class ValidationMenuSongValidateReport : ISongValidateReport
{
    bool hasErrors = false;
    StringBuilder sb = new StringBuilder();

    bool hasMoonscraperErrors = false;
    StringBuilder moonscraperErrorsSb = new StringBuilder();

    public bool HasErrors => hasErrors || hasMoonscraperErrors;

    static string PrintObjectTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return time.ToString("mm':'ss'.'ff");
    }

    public void NotifySongObjectBeyondExpectedLength(SongObject songObject)
    {
        if (songObject is SyncTrack st)
        {
            moonscraperErrorsSb.AppendFormat("\tFound synctrack object beyond the length of the song-\n");
            moonscraperErrorsSb.AppendFormat("\t\tType = {0}, time = {2}, position = {1}\n", st.GetType(), st.tick, PrintObjectTime(st.time));
        }
        else if (songObject is MoonscraperChartEditor.Song.Event eventObject)
        {
            moonscraperErrorsSb.AppendFormat("\tFound event object beyond the length of the song-\n");
            moonscraperErrorsSb.AppendFormat("\t\tType = {0}, time = {2}, position = {1}\n", eventObject.GetType(), eventObject.tick, PrintObjectTime(eventObject.time));
        }
        else if (songObject is ChartObject co)
        {
            moonscraperErrorsSb.AppendFormat("\tFound chart object beyond the length of the song-\n");
            moonscraperErrorsSb.AppendFormat("\t\tType = {0}, time = {2}, position = {1}\n", co.GetType(), co.tick, PrintObjectTime(co.time));
        }

        hasMoonscraperErrors |= true;
    }

    public void NotifyOpenChordFound(Note note)
    {
        sb.AppendFormat("Found Open chord at time {1}, position {0}.\n",
            note.tick, PrintObjectTime(note.time));

        hasErrors |= true;
    }

    public void NotifyOpenTapFound(Note note)
    {
        sb.AppendFormat("Found Tap Open note at time {1}, position {0}.\n",
            note.tick, PrintObjectTime(note.time));

        hasErrors |= true;
    }

    public void NotifyRockBandMidiSoloStarpowerMisRead(Song.Instrument instrument)
    {
        sb.AppendFormat("Track {0} has no starpower and more than 1 solo section. If exported to the midi format, Clone Hero will interpret this chart as an older style midi, and will misinterpret solo markers as starpower.\n", instrument);
        hasErrors |= true;
    }

    public void NotifySectionLimitError(Song song, int sectionLimit)
    {
        sb.AppendFormat("Section count has exceeded limit of {0} sections\n", sectionLimit);
        sb.AppendFormat("Affected sections:\n");

        for (int i = sectionLimit; i < song.sections.Count; ++i)
        {
            Section section = song.sections[i];
            sb.AppendFormat("\tTime = {2}, Position = {0}, Title = {1}\n", section.tick, section.title, PrintObjectTime(section.time));
        }
        hasErrors |= true;
    }

    public void NotifyTimeSignaturePlacementError(TimeSignature ts)
    {
        sb.AppendFormat("Found misaligned Time Signature at time {1}, position {0}. Time signatures must be aligned to the measure set by the previous time signature.\n",
            ts.tick
            , PrintObjectTime(ts.time)
        );
        hasErrors |= true;
    }

    public override string ToString()
    {
        StringBuilder finalReport = new StringBuilder();

        if (!hasMoonscraperErrors && !hasErrors)
        {
            finalReport.AppendLine("No errors detected");
        }
        else
        {
            if (hasMoonscraperErrors)
            {
                finalReport.AppendLine("Moonscraper validation report: ");
                finalReport.AppendLine(moonscraperErrorsSb.ToString());
            }

            finalReport.AppendLine(sb.ToString());
        }
        return finalReport.ToString();
    }
}