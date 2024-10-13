// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Text;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;
using System;
using System.Linq;
using Game.Misc;

public class SongValidate
{
    // Will crash GH3 if this section limit is exceeded
    const int GH3_SECTION_LIMIT = 100;

    [System.Flags]
    public enum ValidationOptions
    {
        None            = 0,
        GuitarHero3     = 1 << 0,
        CloneHero       = 1 << 1,
        Yarg            = 1 << 2,
    }

    public struct ValidationParameters
    {
        public bool checkMidiIssues;
        public float songLength;
    }

    static string PrintObjectTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return time.ToString("mm':'ss'.'ff");
    }

    public static string GenerateReport(ValidationOptions validationOptions, Song song, ValidationParameters validationParams, out bool hasErrors)
    {
        StringBuilder sb = new StringBuilder();
        hasErrors = false;

        sb.AppendFormat("{0}\n", CheckForErrorsMoonscraper(song, validationParams, ref hasErrors));

        bool openChordsAllowed = (validationOptions & ValidationOptions.Yarg) != 0;
        bool openTapsAllowed = (validationOptions & ValidationOptions.Yarg) != 0;
        int sectionLimit = (validationOptions & ValidationOptions.GuitarHero3) != 0 ? GH3_SECTION_LIMIT : int.MaxValue;
        bool checkForTSPlacementErrors = (validationOptions & (ValidationOptions.CloneHero | ValidationOptions.Yarg)) != 0;
        bool checkForMidiSoloSpMisread = validationParams.checkMidiIssues && (validationOptions & (ValidationOptions.CloneHero | ValidationOptions.Yarg)) != 0;

        if (sectionLimit < int.MaxValue)
        {
            StringBuilder errors = new StringBuilder();
            CheckForSectionLimitErrors(song, sectionLimit, errors);
            hasErrors |= errors.Length > 0;
            sb.Append(errors.ToString());
        }

        if (checkForTSPlacementErrors)
        {
            StringBuilder errors = new StringBuilder();
            CheckForTimeSignaturePlacementErrors(song, errors);
            hasErrors |= errors.Length > 0;
            sb.Append(errors.ToString());
        }

        if (checkForMidiSoloSpMisread)
        {
            StringBuilder errors = new StringBuilder();
            CheckForRockBandMidiSoloStarpowerMisRead(song, errors);
            hasErrors |= errors.Length > 0;
            sb.Append(errors.ToString());
        }

        foreach (var instrument in EnumX<Song.Instrument>.Values)
        {
            if (instrument != Song.Instrument.Drums && instrument != Song.Instrument.Unrecognised)
            {
                foreach (var difficulty in EnumX<Song.Difficulty>.Values)
                {
                    var chart = song.GetChart(instrument, difficulty);

                    if (!openChordsAllowed)
                    {
                        var errors = CheckForOpenChords(chart);
                        hasErrors |= errors.Length > 0;
                        sb.Append(errors.ToString());
                    }

                    if (!openTapsAllowed)
                    {
                        var errors = CheckForOpenTaps(chart);
                        hasErrors |= errors.Length > 0;
                        sb.Append(errors.ToString());
                    }
                }
            }
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
                        sb.AppendFormat("\t\tType = {0}, time = {2}, position = {1}\n", st.GetType(), st.tick, PrintObjectTime(st.time));
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
                        sb.AppendFormat("\t\tType = {0}, time = {2}, position = {1}\n", eventObject.GetType(), eventObject.tick, PrintObjectTime(eventObject.time));
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
                        sb.AppendFormat("\t\tType = {0}, time = {2}, position = {1}\n", co.GetType(), co.tick, PrintObjectTime(co.time));
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

    static void CheckForSectionLimitErrors(Song song, int sectionLimit, StringBuilder sb)
    {
        if (song.sections.Count > 100)
        {
            sb.AppendFormat("Section count has exceeded limit of {0} sections\n", sectionLimit);
            sb.AppendFormat("Affected sections:\n");

            for (int i = sectionLimit; i < song.sections.Count; ++i)
            {
                Section section = song.sections[i];
                sb.AppendFormat("\tTime = {2}, Position = {0}, Title = {1}\n", section.tick, section.title, PrintObjectTime(section.time));
            }
        }
    }
    static void CheckForTimeSignaturePlacementErrors(Song song, StringBuilder sb)
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
                sb.AppendFormat("Found misaligned Time Signature at time {1}, position {0}. Time signatures must be aligned to the measure set by the previous time signature.\n",
                    tsToTest.tick
                    , PrintObjectTime(tsToTest.time)
                );
            }
        }
    }

    static void CheckForRockBandMidiSoloStarpowerMisRead(Song song, StringBuilder sb)
    {
        // If we have no starpower but more than 1 solo section then CH will interpret this as an RB1 style midi, and misinterpret the solo markers as starpower
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
                        if (ce.eventName == MoonscraperChartEditor.Song.IO.MidIOHelper.SOLO_EVENT_TEXT)
                        {
                            ++soloMarkerCount;

                            if (soloMarkerCount > 1)
                            {
                                sb.AppendFormat("Track {0} has no starpower and more than 1 solo section. If exported to the midi format, Clone Hero will interpret this chart as an older style midi, and will misinterpret solo markers as starpower.\n", instrument);

                                goto NewInstrument;
                            }
                        }
                    }
                }
            }

            NewInstrument:;
        }
    }

    static StringBuilder CheckForOpenChords(Chart chart)
    {
        StringBuilder sb = new StringBuilder();

        for (var i = 0; i < chart.notes.Count; i++)
        {
            var note = chart.notes[i];

            if (!note.IsOpenNote())
            {
                continue;
            }

            bool previousSameTick = note.previous != null && note.tick == note.previous.tick;
            bool nextSameTick = note.next != null && note.tick == note.next.tick;

            // Open chords are not supported in Clone Hero (yet)
            if (previousSameTick || nextSameTick)
            {
                sb.AppendFormat("Found Open chord at time {1}, position {0}.\n",
                    note.tick, PrintObjectTime(note.time));
            }
        }

        return sb;
    }

    static StringBuilder CheckForOpenTaps(Chart chart)
    {
        StringBuilder sb = new StringBuilder();

        for (var i = 0; i < chart.notes.Count; i++)
        {
            var note = chart.notes[i];

            if (!note.IsOpenNote())
            {
                continue;
            }

            // Neither are Tap opens
            if ((note.flags & Note.Flags.Tap) != 0)
            {
                sb.AppendFormat("Found Tap Open note at time {1}, position {0}.\n",
                    note.tick, PrintObjectTime(note.time));
            }
        }

        return sb;
    }
}
