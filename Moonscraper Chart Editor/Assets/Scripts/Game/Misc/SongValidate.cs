// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Text;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;
using System;

public class SongValidate
{
    // Will crash GH3 if this section limit is exceeded
    const int GH3_SECTION_LIMIT = 100;

    [Flags]
    public enum PlatformValidationFlags
    {
        None            = 0,
        GuitarHero3     = 1 << 0,
        CloneHero       = 1 << 1,
        Yarg            = 1 << 2,
    }

    public readonly struct FeatureValidationOptions
    {
        public readonly bool openChordsAllowed;
        public readonly bool openTapsAllowed;
        public readonly int sectionLimit;
        public readonly bool checkForTSPlacementErrors;
        public readonly bool checkForMidiSoloSpMisread;

        public FeatureValidationOptions(
            bool openChordsAllowed
            , bool openTapsAllowed
            , int sectionLimit
            , bool checkForTSPlacementErrors
            , bool checkForMidiSoloSpMisread
            )
        {
            this.openChordsAllowed = openChordsAllowed;
            this.openTapsAllowed = openTapsAllowed;
            this.sectionLimit = sectionLimit;
            this.checkForTSPlacementErrors = checkForTSPlacementErrors;
            this.checkForMidiSoloSpMisread = checkForMidiSoloSpMisread;
        }
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

    public static string GenerateReport(PlatformValidationFlags validationOptions, Song song, ValidationParameters validationParams, out bool hasErrors)
    {
        ValidationMenuSongValidateReport report = new ValidationMenuSongValidateReport();
        hasErrors = false;

        CheckForErrorsMoonscraper(song, validationParams, report);

        FeatureValidationOptions featureValidationOptions = new FeatureValidationOptions(
            openChordsAllowed: (validationOptions & PlatformValidationFlags.Yarg) != 0,
            openTapsAllowed: (validationOptions & PlatformValidationFlags.Yarg) != 0,
            sectionLimit: (validationOptions & PlatformValidationFlags.GuitarHero3) != 0 ? GH3_SECTION_LIMIT : int.MaxValue,
            checkForTSPlacementErrors: (validationOptions & (PlatformValidationFlags.CloneHero | PlatformValidationFlags.Yarg)) != 0,
            checkForMidiSoloSpMisread: validationParams.checkMidiIssues && (validationOptions & (PlatformValidationFlags.CloneHero | PlatformValidationFlags.Yarg)) != 0
        );

        if (featureValidationOptions.sectionLimit < int.MaxValue)
        {
            CheckForSectionLimitErrors(song, featureValidationOptions.sectionLimit, report);
        }

        if (featureValidationOptions.checkForTSPlacementErrors)
        {
            CheckForTimeSignaturePlacementErrors(song, report);
        }

        if (featureValidationOptions.checkForMidiSoloSpMisread)
        {
            CheckForRockBandMidiSoloStarpowerMisRead(song, report);
        }

        foreach (var instrument in EnumX<Song.Instrument>.Values)
        {
            if (instrument != Song.Instrument.Drums && instrument != Song.Instrument.Unrecognised)
            {
                foreach (var difficulty in EnumX<Song.Difficulty>.Values)
                {
                    ValidateChart(song, instrument, difficulty, featureValidationOptions, report);
                }
            }
        }

        hasErrors = report.HasErrors;
        return report.ToString();
    }

    public static void ValidateChart(
        Song song
        , Song.Instrument instrument
        , Song.Difficulty difficulty
        , in FeatureValidationOptions featureValidationOptions
        , ISongValidateReport report
        )
    {
        if (instrument != Song.Instrument.Drums && instrument != Song.Instrument.Unrecognised)
        {
            var chart = song.GetChart(instrument, difficulty);

            if (!featureValidationOptions.openChordsAllowed)
            {
                CheckForOpenChords(chart, report);
            }

            if (!featureValidationOptions.openTapsAllowed)
            {
                CheckForOpenTaps(chart, report);
            }
        }
    }

    static void CheckForErrorsMoonscraper(Song song, ValidationParameters validationParams, ISongValidateReport report)
    {
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
                        report.NotifySongObjectBeyondExpectedLength(song.syncTrack[i]);
                    }
                }

                // Events
                {
                    int index, length;
                    SongObjectHelper.GetRange(song.eventsAndSections, tick, uint.MaxValue, out index, out length);

                    for (int i = index; i < length; ++i)
                    {
                        report.NotifySongObjectBeyondExpectedLength(song.eventsAndSections[i]);
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
                        report.NotifySongObjectBeyondExpectedLength(chart.chartObjects[i]);
                    }
                }
            }
        }
    }

    static void CheckForSectionLimitErrors(Song song, int sectionLimit, ISongValidateReport report)
    {
        if (song.sections.Count > 100)
        {
            report.NotifySectionLimitError(song, sectionLimit);
        }
    }
    static void CheckForTimeSignaturePlacementErrors(Song song, ISongValidateReport report)
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
                report.NotifyTimeSignaturePlacementError(tsToTest);
            }
        }
    }

    static void CheckForRockBandMidiSoloStarpowerMisRead(Song song, ISongValidateReport report)
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
                                report.NotifyRockBandMidiSoloStarpowerMisRead(instrument);

                                goto NewInstrument;
                            }
                        }
                    }
                }
            }

            NewInstrument:;
        }
    }

    static void CheckForOpenChords(Chart chart, ISongValidateReport report)
    {
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
                report.NotifyOpenChordFound(note);
            }
        }
    }

    static void CheckForOpenTaps(Chart chart, ISongValidateReport report)
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
                report.NotifyOpenTapFound(note);
            }
        }
    }
}
