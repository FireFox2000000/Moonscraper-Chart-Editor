// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Text;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public class SongValidate
{
    [System.Flags]
    public enum ValidationOptions
    {
        None            = 0,
        GuitarHero3     = 1 << 0,
        CloneHero       = 1 << 1,
    }

    public struct ValidationParameters
    {
        public bool checkMidiIssues;
        public float songLength;
    }

    public static string GenerateReport(ValidationOptions validationOptions, Song song, ValidationParameters validationParams, out bool hasErrors)
    {
        StringBuilder sb = new StringBuilder();
        hasErrors = false;

        sb.AppendFormat("{0}\n", CheckForErrorsMoonscraper(song, validationParams, ref hasErrors));

        if ((validationOptions & ValidationOptions.GuitarHero3) != 0)
        {
            sb.AppendFormat("{0}\n", CheckForErrorsGuitarHero3(song, validationParams, ref hasErrors));
        }

        if ((validationOptions & ValidationOptions.CloneHero) != 0)
        {
            sb.AppendFormat("{0}\n", CheckForErrorsCloneHero(song, validationParams, ref hasErrors));
        }

        return sb.ToString();
    }

    static string CheckForErrorsMoonscraper(Song song, ValidationParameters validationParams, ref bool hasErrors)
    {
        bool hasErrorsLocal = false;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Moonscraper validation report: ");

        // Check if any objects have exceeded the max length
        {
            uint tick = song.TimeToTick(validationParams.songLength, song.resolution);

            // Song objects
            {
                // Synctrack
                {
                    int index, length;
                    SongObjectHelper.GetRange(song.syncTrack, tick, uint.MaxValue, out index, out length);

                    for (int i = index; i < length; ++i)
                    {
                        hasErrorsLocal |= true;

                        SyncTrack st = song.syncTrack[i];

                        sb.AppendFormat("\tFound synctrack object beyond the length of the song-\n");
                        sb.AppendFormat("\t\tType = {0}, position = {1}\n", st.GetType(), st.tick);
                    }
                }

                // Events
                {
                    int index, length;
                    SongObjectHelper.GetRange(song.eventsAndSections, tick, uint.MaxValue, out index, out length);

                    for (int i = index; i < length; ++i)
                    {
                        hasErrorsLocal |= true;

                        MoonscraperChartEditor.Song.Event eventObject = song.eventsAndSections[i];

                        sb.AppendFormat("\tFound event object beyond the length of the song-\n");
                        sb.AppendFormat("\t\tType = {0}, position = {1}\n", eventObject.GetType(), eventObject.tick);
                    }
                }
            }

            // Chart objects
            foreach (Song.Instrument instrument in EnumX<Song.Instrument>.Values)
            {
                if (instrument == Song.Instrument.Unrecognised)
                    continue;

                foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
                {
                    Chart chart = song.GetChart(instrument, difficulty);

                    int index, length;
                    SongObjectHelper.GetRange(chart.chartObjects, tick, uint.MaxValue, out index, out length);

                    for (int i = index; i < length; ++i)
                    {
                        hasErrorsLocal |= true;

                        ChartObject co = chart.chartObjects[i];

                        sb.AppendFormat("\tFound chart object beyond the length of the song-\n");
                        sb.AppendFormat("\t\tType = {0}, position = {1}\n", co.GetType(), co.tick);
                    }
                }
            }
        }

        if (!hasErrorsLocal)
        {
            sb.AppendLine("\tNo errors detected");
        }

        hasErrors |= hasErrorsLocal;

        return sb.ToString();
    }

    static string CheckForErrorsGuitarHero3(Song song, ValidationParameters validationParams, ref bool hasErrors)
    {
        const int SECTION_LIMIT = 100;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Guitar Hero 3 validation report: ");

        bool hasErrorsLocal = false;

        // Check that we haven't exceeded the section count in GH3
        if (song.sections.Count > 100)
        {
            hasErrorsLocal |= true;
            sb.AppendFormat("\tSection count has exceeded limit of {0} sections\n", SECTION_LIMIT);
            sb.AppendFormat("\tAffected sections:\n");

            for (int i = SECTION_LIMIT; i < song.sections.Count; ++i)
            {
                Section section = song.sections[i];
                sb.AppendFormat("\t\tPosition = {0}, Title = {1}\n", section.tick, section.title);
            }
        }

        if (!hasErrorsLocal)
        {
            sb.AppendLine("\tNo errors detected");
        }

        hasErrors |= hasErrorsLocal;

        return sb.ToString();
    }

    static string CheckForErrorsCloneHero(Song song, ValidationParameters validationParams, ref bool hasErrors)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Clone Hero validation report: ");

        bool hasErrorsLocal = false;

        // Check that time signature positions line up on the measure from the previous time signature
        {
            var timeSignatures = song.timeSignatures;
            for (int tsIndex = 1; tsIndex < timeSignatures.Count; ++tsIndex)
            {
                TimeSignature previousTs = timeSignatures[tsIndex - 1];
                TimeSignature.MeasureInfo measureInfo = previousTs.GetMeasureInfo();

                TimeSignature tsToTest = timeSignatures[tsIndex];
                uint deltaTick = tsToTest.tick - previousTs.tick;

                var measureLine = measureInfo.measureLine;
                if (((float)deltaTick % measureLine.tickGap) != 0)      // Doesn't line up on a measure
                {
                    hasErrorsLocal |= true;
                    sb.AppendFormat("\tFound misaligned Time Signature at position {0}. Time signatures must be aligned to the measure set by the previous time signature.\n", tsToTest.tick);
                }
            }
        }

        // If we have no starpower but more than 1 solo section then CH will interpret this as an RB1 style midi, and misinterpret the solo markers as starpower
        if (validationParams.checkMidiIssues)
        {
            foreach (Song.Instrument instrument in EnumX<Song.Instrument>.Values)
            {
                if (instrument == Song.Instrument.Unrecognised)
                    continue;

                foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
                {
                    Chart chart = song.GetChart(instrument, difficulty);

                    if (chart.starPower.Count <= 0)
                    {
                        // Check for solo markers
                        int soloMarkerCount = 0;
                        foreach (ChartEvent ce in chart.events)
                        {
                            if (ce.eventName == MoonscraperChartEditor.Song.IO.MidIOHelper.SoloEventText)
                            {
                                ++soloMarkerCount;

                                if (soloMarkerCount > 1)
                                {
                                    hasErrorsLocal |= true;
                                    sb.AppendFormat("\tTrack {0} has no starpower and more than 1 solo section. If exported to the midi format, Clone Hero will interpret this chart as an older style midi, and will misinterpret solo markers as starpower.\n", instrument);

                                    goto NewInstrument;
                                }
                            }
                        }
                    }
                }

            NewInstrument:;
            }
        }

        if (!hasErrorsLocal)
        {
            sb.AppendLine("\tNo errors detected");
        }

        hasErrors |= hasErrorsLocal;

        return sb.ToString();
    }
}
