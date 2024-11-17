// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MiscUtil.Conversion;
using System.Linq;

namespace MoonscraperChartEditor.Song.IO
{
    public static class MidWriter
    {
        // http://www.somascape.org/midi/tech/mfile.html#:~:text=a%20MIDI%20file.-,Meta%20events,useful%20within%20a%20MIDI%20file).&text=type%20specifies%20the%20type%20of%20Meta%20event%20(0%20%2D%20127).
        enum MetaTextEventType
        {
            // All the events here follow the [0xFF type length] text structure
            Text = 1,
            Copyright = 2,
            TrackName = 3,
            InstrumentName = 4,
            Lyric = 5,
            Marker = 6,
            CuePoint = 7,
            ProgramName = 8,
            DeviceName = 9,
        }

        const byte ON_EVENT = 0x91;         // Note on channel 1
        const byte OFF_EVENT = 0x81;
        const byte VELOCITY = MidIOHelper.VELOCITY;

        const byte SYSEX_START = 0xF0;
        const byte SYSEX_END = 0xF7;

        const int SOLO_END_CORRECTION_OFFSET = 1;

        static readonly byte[] END_OF_TRACK = new byte[] { 0, 0xFF, 0x2F, 0x00 };

        struct VocalProcessingParams
        {
            public Song song;
            public IList<Event> eventList;
            public int eventListIndex;

            public List<SortableBytes> out_sortableBytes;
        }

        static readonly IReadOnlyDictionary<Song.Instrument, string> c_instrumentToTrackNameDict = new Dictionary<Song.Instrument, string>()
        {
            { Song.Instrument.Guitar,           MidIOHelper.GUITAR_TRACK },
            { Song.Instrument.GuitarCoop,       MidIOHelper.GUITAR_COOP_TRACK },
            { Song.Instrument.Bass,             MidIOHelper.BASS_TRACK },
            { Song.Instrument.Rhythm,           MidIOHelper.RHYTHM_TRACK },
            { Song.Instrument.Keys,             MidIOHelper.KEYS_TRACK },
            { Song.Instrument.Drums,            MidIOHelper.DRUMS_TRACK },
            { Song.Instrument.GHLiveGuitar,     MidIOHelper.GHL_GUITAR_TRACK },
            { Song.Instrument.GHLiveBass,       MidIOHelper.GHL_BASS_TRACK },
            { Song.Instrument.GHLiveRhythm,     MidIOHelper.GHL_RHYTHM_TRACK },
            { Song.Instrument.GHLiveCoop,       MidIOHelper.GHL_GUITAR_COOP_TRACK },

        };

        static readonly IReadOnlyDictionary<Song.Difficulty, int> c_difficultyToMidiNoteWriteDict = new Dictionary<Song.Difficulty, int>()
        {
            { Song.Difficulty.Easy,             60 },
            { Song.Difficulty.Medium,           72 },
            { Song.Difficulty.Hard,             84 },
            { Song.Difficulty.Expert,           96 },
        };

        static readonly IReadOnlyDictionary<int, int> c_guitarNoteMidiWriteOffsets = new Dictionary<int, int>()
        {
            { (int)Note.GuitarFret.Open,     0},     // Gets replaced by an sysex event
            { (int)Note.GuitarFret.Green,    0},
            { (int)Note.GuitarFret.Red,      1},
            { (int)Note.GuitarFret.Yellow,   2},
            { (int)Note.GuitarFret.Blue,     3},
            { (int)Note.GuitarFret.Orange,   4},
        };

        static readonly IReadOnlyDictionary<int, int> c_guitarNoteEnhancedOpensMidiWriteOffsets = new Dictionary<int, int>()
        {
            { (int)Note.GuitarFret.Open,     -1},
            { (int)Note.GuitarFret.Green,    0},
            { (int)Note.GuitarFret.Red,      1},
            { (int)Note.GuitarFret.Yellow,   2},
            { (int)Note.GuitarFret.Blue,     3},
            { (int)Note.GuitarFret.Orange,   4},
        };

        static readonly IReadOnlyDictionary<int, int> c_drumNoteMidiWriteOffsets = new Dictionary<int, int>()
        {
            { (int)Note.DrumPad.Kick,     0},
            { (int)Note.DrumPad.Red,      1},
            { (int)Note.DrumPad.Yellow,   2},
            { (int)Note.DrumPad.Blue,     3},
            { (int)Note.DrumPad.Orange,   4},
            { (int)Note.DrumPad.Green,    5},
        };

        static readonly IReadOnlyDictionary<int, int> c_ghlNoteMidiWriteOffsets = new Dictionary<int, int>()
        {
            { (int)Note.GHLiveGuitarFret.Open,   -2},
            { (int)Note.GHLiveGuitarFret.White1, -1},
            { (int)Note.GHLiveGuitarFret.White2, 0},
            { (int)Note.GHLiveGuitarFret.White3, 1},
            { (int)Note.GHLiveGuitarFret.Black1, 2},
            { (int)Note.GHLiveGuitarFret.Black2, 3},
            { (int)Note.GHLiveGuitarFret.Black3, 4},
        };

        static IReadOnlyDictionary<int, int> GetNoteWriteOffsetLookup(Chart.GameMode gameMode, bool enhancedOpens)
        {
            switch (gameMode)
            {
                case Chart.GameMode.Guitar:
                    {
                        if (enhancedOpens)
                        {
                            return c_guitarNoteEnhancedOpensMidiWriteOffsets;
                        }
                        else
                        {
                            return c_guitarNoteMidiWriteOffsets;
                        }
                    }
                case Chart.GameMode.Drums:
                    {
                        return c_drumNoteMidiWriteOffsets;
                    }
                case Chart.GameMode.GHLGuitar:
                    {
                        return c_ghlNoteMidiWriteOffsets;
                    }
                default:
                    {
                        throw new NotImplementedException($"Unhandled game mode {gameMode}, unable to get offset dictionary");
                    }
            }
        }

        static readonly IReadOnlyDictionary<Note.NoteType, int> c_forcingMidiWriteOffsets = new Dictionary<Note.NoteType, int>()
        {
            { Note.NoteType.Hopo, 5 },
            { Note.NoteType.Strum, 6 },
        };

        delegate void ProcessVocalEventBytesFn(in VocalProcessingParams processParams);
        static readonly IReadOnlyDictionary<string, ProcessVocalEventBytesFn> vocalPrefixProcessList = new Dictionary<string, ProcessVocalEventBytesFn>()
        {
            { MidIOHelper.LYRIC_EVENT_PREFIX, (in VocalProcessingParams processParams) => {
                const string prefix = MidIOHelper.LYRIC_EVENT_PREFIX;

                Event songEvent = processParams.eventList[processParams.eventListIndex];

                string currentEventTitle = songEvent.title;
                string metaTextEventStr = currentEventTitle.Substring(prefix.Length, currentEventTitle.Length - prefix.Length);

                SortableBytes bytes = new SortableBytes(songEvent.tick, MetaTextEvent(MetaTextEventType.Lyric, metaTextEventStr));
                InsertionSort(processParams.out_sortableBytes, bytes);
            }},

            { MidIOHelper.LYRICS_PHRASE_START_TEXT, (in VocalProcessingParams processParams) => {

                Event phraseStartEvent = processParams.eventList[processParams.eventListIndex];
                uint phraseEndEventTick = phraseStartEvent.tick;    // Find next phase end or the next phase start, whichever is first. 1 tick away as a backup. 

                for (int i = processParams.eventListIndex + 1; i < processParams.eventList.Count; ++i)
                {
                    Event nextEvent = processParams.eventList[i];
                    if (nextEvent.title.StartsWith(MidIOHelper.LYRICS_PHRASE_END_TEXT))
                    {
                        phraseEndEventTick = nextEvent.tick;
                        break;
                    }
                    else if(nextEvent.title.StartsWith(MidIOHelper.LYRICS_PHRASE_START_TEXT))
                    {
                        phraseEndEventTick = nextEvent.tick - 1;
                        break;
                    }
                }

                if (phraseEndEventTick == phraseStartEvent.tick)
                {
                    phraseEndEventTick = processParams.eventList[processParams.eventList.Count - 1].tick + 1;
                }

                // Make a note that has the length of the two phase events
                Note phraseNote = new Note(phraseStartEvent.tick, 0, phraseEndEventTick - phraseStartEvent.tick);
                SortableBytes onEvent = null;
                SortableBytes offEvent = null;
                GetNoteNumberBytes(MidIOHelper.LYRICS_PHRASE_1, phraseNote, VELOCITY, out onEvent, out offEvent);

                InsertionSort(processParams.out_sortableBytes, onEvent);
                InsertionSort(processParams.out_sortableBytes, offEvent);
            }},

            { MidIOHelper.LYRICS_PHRASE_END_TEXT, (in VocalProcessingParams processParams) => {
                // Do nothing, phrase start handles this. Still need to mark it here for it to be excluded from regular events track
            }},
        };

        public static void WriteToFile(string path, Song song, ExportOptions exportOptions)
        {
            short track_count = 1;

            float resolutionScaleRatio = song.ResolutionScaleRatio(exportOptions.targetResolution);
            List<SortableBytes> vocalsEvents = new List<SortableBytes>();

            byte[] track_sync = MakeTrack(GetSyncBytes(song, exportOptions, resolutionScaleRatio), song.name);
            byte[] track_events;
            {
                uint deltaTickSum;

                List<byte> eventBytes = new List<byte>(GetEventBytes(
                        song,
                        exportOptions,
                        vocalsEvents,
                        true,
                        resolutionScaleRatio,
                        out deltaTickSum));

                /*
                 * // Optional, not using this at the moment, use this if ever needed. Will need to pass the song length in manually though.
                uint musicEnd = song.TimeToTick(song.length + exportOptions.tickOffset, song.resolution * resolutionScaleRatio);

                if (musicEnd > deltaTickSum)
                    musicEnd -= deltaTickSum;
                else
                    musicEnd = deltaTickSum;

                // Add music_end and end text events.
                eventBytes.AddRange(TimedEvent(musicEnd, MetaTextEvent(TEXT_EVENT, "[music_end]")));
                eventBytes.AddRange(TimedEvent(0, MetaTextEvent(TEXT_EVENT, "[end]")));
                */
                track_events = MakeTrack(eventBytes.ToArray(), MidIOHelper.EVENTS_TRACK);
            }

            if (track_events.Length > 0)
                track_count++;

            //byte[] track_beat = MakeTrack(GenerateBeat(musicEnd, (uint)exportOptions.targetResolution), "BEAT");
            //song.GetChart(Song.Instrument.Guitar, Song.Difficulty.Expert).Add(new ChartEvent(0, "[idle_realtime]")); 

            List<byte[]> allTracks = new List<byte[]>();
            List<string> allTrackNames = new List<string>();
            foreach (KeyValuePair<Song.Instrument, string> entry in c_instrumentToTrackNameDict)
            {
                byte[] bytes = GetInstrumentBytes(song, entry.Key, exportOptions, resolutionScaleRatio);
                if (bytes.Length > 0)
                {
                    Debug.LogFormat("Saving {0} track", entry.Key.ToString());
                    allTracks.Add(bytes);
                    allTrackNames.Add(entry.Value);
                    track_count++;
                }
            }

            if (vocalsEvents.Count > 0)
            {
                // Make a vocals track
                Debug.Log("Vocals events found. Saving Vocals track.");
                byte[] bytes = SortableBytesToTimedEventBytes(vocalsEvents.ToArray(), song, exportOptions, resolutionScaleRatio);
                allTracks.Add(bytes);
                allTrackNames.Add(MidIOHelper.VOCALS_TRACK);
                track_count++;
            }

            byte[][] unrecognised_tracks = new byte[song.unrecognisedCharts.Count][];
            for (int i = 0; i < unrecognised_tracks.Length; ++i)
            {
                unrecognised_tracks[i] = GetUnrecognisedChartBytes(song.unrecognisedCharts[i], exportOptions, resolutionScaleRatio);
                track_count++;
            }

            byte[] header = GetMidiHeader(1, track_count, (short)(exportOptions.targetResolution));

            FileStream file = File.Open(path, FileMode.OpenOrCreate);
            BinaryWriter bw = new BinaryWriter(file);

            bw.Write(header);
            bw.Write(track_sync);
            //bw.Write(track_beat);
            bw.Write(track_events);

            for (int i = 0; i < allTracks.Count; ++i)
            {
                bw.Write(MakeTrack(allTracks[i], allTrackNames[i]));
            }

            for (int i = 0; i < unrecognised_tracks.Length; ++i)
            {
                if (unrecognised_tracks[i].Length > 0)
                    bw.Write(MakeTrack(unrecognised_tracks[i], song.unrecognisedCharts[i].name));
            }

            bw.Close();
            file.Close();
        }

        static byte[] GetSyncBytes(Song song, ExportOptions exportOptions, float resolutionScaleRatio)
        {
            List<byte> syncTrackBytes = new List<byte>();

            // Set default bpm and time signature
            if (exportOptions.tickOffset > 0)
            {
                syncTrackBytes.AddRange(TimedEvent(0, TempoEvent(new BPM())));
                syncTrackBytes.AddRange(TimedEvent(0, TimeSignatureEvent(new TimeSignature())));
            }

            // Loop through all synctrack events
            for (int i = 0; i < song.syncTrack.Count; ++i)
            {
                uint deltaTime = song.syncTrack[i].tick;
                if (i > 0)
                    deltaTime -= song.syncTrack[i - 1].tick;

                deltaTime = (uint)Mathf.Round(deltaTime * resolutionScaleRatio);

                if (i == 0)
                    deltaTime += exportOptions.tickOffset;

                var bpm = song.syncTrack[i] as BPM;
                if (bpm != null)
                    syncTrackBytes.AddRange(TimedEvent(deltaTime, TempoEvent(bpm)));

                var ts = song.syncTrack[i] as TimeSignature;
                if (ts != null)
                    syncTrackBytes.AddRange(TimedEvent(deltaTime, TimeSignatureEvent(ts)));
            }

            return syncTrackBytes.ToArray();
        }

        static byte[] GetEventBytes
            (Song song
            , ExportOptions exportOptions
            , List<SortableBytes> vocalsEvents
            , bool containInSquareBrackets
            , float resolutionScaleRatio
            )
        {
            uint deltaTickSum;
            return GetEventBytes(song, exportOptions, vocalsEvents, containInSquareBrackets, resolutionScaleRatio, out deltaTickSum);
        }

        static byte[] GetEventBytes
        (Song song
        , ExportOptions exportOptions
        , List<SortableBytes> vocalsEvents
        , bool containInSquareBrackets
        , float resolutionScaleRatio
        , out uint deltaTickSum
        )
        {
            VocalProcessingParams vocalProcesingParams = new VocalProcessingParams() { song = song, out_sortableBytes = vocalsEvents };

            var rbFormat = exportOptions.midiOptions.rbFormat;
            string section_id = MidIOHelper.SECTION_PREFIX_RB2;

            switch (rbFormat)
            {
                case ExportOptions.MidiOptions.RBFormat.RB3:
                    {
                        section_id = MidIOHelper.SECTION_PREFIX_RB3;
                        break;
                    }
                case ExportOptions.MidiOptions.RBFormat.RB2:
                default:
                    {
                        section_id = MidIOHelper.SECTION_PREFIX_RB2;
                        break;
                    }
            }

            List<byte> eventBytes = new List<byte>();

            //eventBytes.AddRange(TimedEvent(0, MetaTextEvent(TEXT_EVENT, "[music_start]")));

            deltaTickSum = 0;
            uint previousEventTick = 0;
            int eventCount = 0;

            vocalProcesingParams.eventList = song.eventsAndSections;
            for (int i = 0; i < song.eventsAndSections.Count; ++i)
            {
                vocalProcesingParams.eventListIndex = i;
                Event currentEvent = song.eventsAndSections[i];
                string currentEventTitle = currentEvent.title;
                bool allowedEvent = true;

                // Check if should be processed for the vocals track instead
                if (currentEvent as Section == null)
                {
                    foreach (var keyVal in vocalPrefixProcessList)
                    {
                        string prefix = keyVal.Key;
                        if (currentEventTitle.StartsWith(prefix))
                        {
                            keyVal.Value(vocalProcesingParams);
                            allowedEvent = false;
                            break;
                        }
                    }
                }

                if (!allowedEvent)
                    continue;

                string metaTextEventStr = currentEvent.title;
                MetaTextEventType metaTextEventType = MetaTextEventType.Text;

                if (currentEvent as Section != null)
                {
                    string prefix = section_id;
                    metaTextEventStr = prefix + currentEventTitle;
                }

                if (containInSquareBrackets)
                {
                    metaTextEventStr = "[" + metaTextEventStr + "]";
                }

                uint deltaTime = song.eventsAndSections[i].tick;
                if (eventCount > 0)
                    deltaTime -= previousEventTick;

                deltaTime = (uint)Mathf.Round(deltaTime * resolutionScaleRatio);

                if (eventCount == 0)
                    deltaTime += exportOptions.tickOffset;

                deltaTickSum += deltaTime;

                eventBytes.AddRange(TimedEvent(deltaTime, MetaTextEvent(metaTextEventType, metaTextEventStr)));

                previousEventTick = currentEvent.tick;
                ++eventCount;
            }

            return eventBytes.ToArray();
        }

        static byte[] GetInstrumentBytes(Song song, Song.Instrument instrument, ExportOptions exportOptions, float resolutionScaleRatio)
        {
            // Preprocess and determine if enhanced opens should be enabled or not
            bool enhancedOpens = false;
            if (Song.InstumentToChartGameMode(instrument) == Chart.GameMode.Guitar)
            {
                ChartFeatureChecker report = new ChartFeatureChecker();
                SongValidate.FeatureValidationOptions options = new SongValidate.FeatureValidationOptions(
                    openChordsAllowed: false,
                    openTapsAllowed: false,
                    sectionLimit: 0,
                    checkForTSPlacementErrors: false,
                    checkForMidiSoloSpMisread: false
                    );

                SongValidate.ValidateChart(song, instrument, Song.Difficulty.Easy, options, report);
                SongValidate.ValidateChart(song, instrument, Song.Difficulty.Medium, options, report);
                SongValidate.ValidateChart(song, instrument, Song.Difficulty.Hard, options, report);
                SongValidate.ValidateChart(song, instrument, Song.Difficulty.Expert, options, report);

                enhancedOpens = report.HasOpenChords;
            }

            if (enhancedOpens)
            {
                Debug.Log($"Enhanced opened enabled for midi track {instrument}");
            }

            // Collect all bytes from each difficulty of the instrument, assigning the position for each event unsorted
            SortableBytes[] easyBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Easy, enhancedOpens, exportOptions);
            SortableBytes[] mediumBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Medium, enhancedOpens, exportOptions);
            SortableBytes[] hardBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Hard, enhancedOpens, exportOptions);
            SortableBytes[] expertBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Expert, enhancedOpens, exportOptions);

            SortableBytes[] em = SortableBytes.MergeAlreadySorted(easyBytes, mediumBytes);
            SortableBytes[] he = SortableBytes.MergeAlreadySorted(hardBytes, expertBytes);

            List<SortableBytes> sortedEvents = new List<SortableBytes>(SortableBytes.MergeAlreadySorted(em, he));
           
            if (enhancedOpens && sortedEvents.Count > 0)
            {
                SortableBytes enhanceOpenFlagBytes = GetChartEventBytes(new ChartEvent(0, MidIOHelper.ENHANCED_OPENS_TEXT_BRACKET));
                Debug.Assert(enhanceOpenFlagBytes.tick <= sortedEvents[0].tick);
                sortedEvents.Insert(0, enhanceOpenFlagBytes);
            }

            // Strip out duplicate events. This may occur with cymbal flags across multiple difficulties, duplicate events across difficulties etc. 
            for (int i = sortedEvents.Count - 1; i >= 0; --i)
            {
                int next = i + 1;
                while (next < sortedEvents.Count && sortedEvents[i].tick == sortedEvents[next].tick)
                {
                    if (sortedEvents[i].bytes.SequenceEqual(sortedEvents[next].bytes))
                    {
                        sortedEvents.RemoveAt(next);
                    }

                    ++next;
                }
            }

            return SortableBytesToTimedEventBytes(sortedEvents.ToArray(), song, exportOptions, resolutionScaleRatio);
        }

        static byte[] SortableBytesToTimedEventBytes(SortableBytes[] sortedEvents, Song song, ExportOptions exportOptions, float resolutionScaleRatio)
        {
            List<byte> bytes = new List<byte>();

            for (int i = 0; i < sortedEvents.Length; ++i)
            {
                uint deltaTime = sortedEvents[i].tick;
                if (i > 0)
                    deltaTime -= sortedEvents[i - 1].tick;

                deltaTime = (uint)Mathf.Round(deltaTime * resolutionScaleRatio);

                if (i == 0)
                    deltaTime += exportOptions.tickOffset;

                // Apply time to the midi event
                bytes.AddRange(TimedEvent(deltaTime, sortedEvents[i].bytes));
            }

            return bytes.ToArray();
        }

        static void InsertionSort(IList<SortableBytes> eventList, SortableBytes sortableByte)
        {
            int index = eventList.Count - 1;

            while (index >= 0 && sortableByte.tick < eventList[index].tick)
                --index;

            eventList.Insert(index + 1, sortableByte);
        }

        static SortableBytes[] GetChartSortableBytes(Song song, Song.Instrument instrument, Song.Difficulty difficulty, bool enhancedOpens, ExportOptions exportOptions)
        {
            Chart chart = song.GetChart(instrument, difficulty);
            Chart.GameMode gameMode = chart.gameMode;
            bool writeGlobalTrackEvents = difficulty == exportOptions.midiOptions.difficultyToUseGlobalTrackEvents;

            if (exportOptions.copyDownEmptyDifficulty)
            {
                Song.Difficulty chartDiff = difficulty;
                while (chart.notes.Count <= 0)
                {
                    switch (chartDiff)
                    {
                        case (Song.Difficulty.Easy):
                            chartDiff = Song.Difficulty.Medium;
                            break;
                        case (Song.Difficulty.Medium):
                            chartDiff = Song.Difficulty.Hard;
                            break;
                        case (Song.Difficulty.Hard):
                            chartDiff = Song.Difficulty.Expert;
                            break;
                        case (Song.Difficulty.Expert):
                        default:
                            return new SortableBytes[0];
                    }

                    chart = song.GetChart(instrument, chartDiff);
                }
            }

            List<SortableBytes> eventList = new List<SortableBytes>();

            ChartEvent soloOnEvent = null;
            bool dynamicsFound = false;
            foreach (ChartObject chartObject in chart.chartObjects)
            {
                Note note = chartObject as Note;

                SortableBytes onEvent = null;
                SortableBytes offEvent = null;

                if (note != null)
                {
                    int noteNumber = GetMidiNoteNumber(note, gameMode, difficulty, enhancedOpens);

                    byte velocity = VELOCITY;

                    if (gameMode == Chart.GameMode.Drums)
                    {
                        if (note.flags.HasFlag(Note.Flags.ProDrums_Accent))
                        {
                            dynamicsFound = true;
                            velocity = MidIOHelper.VELOCITY_ACCENT;
                        }
                        else if (note.flags.HasFlag(Note.Flags.ProDrums_Ghost))
                        {
                            dynamicsFound = true;
                            velocity = MidIOHelper.VELOCITY_GHOST;
                        }
                    }

                    GetNoteNumberBytes(noteNumber, note, velocity, out onEvent, out offEvent);

                    if (exportOptions.forced)
                    {
                        Note.Flags bannedFlags = Note.GetBannedFlagsForGameMode(gameMode);
                        Note.Flags noteFlags = note.flags & ~bannedFlags;   // Last ditched error correction

                        // Forced notes               
                        if (noteFlags.HasFlag(Note.Flags.Forced) && note.type != Note.NoteType.Tap && (note.previous == null || (note.previous.tick != note.tick)))     // Don't overlap on chords
                        {
                            // Add a note
                            int difficultyNumber;
                            int forcingOffset;

                            if (!c_difficultyToMidiNoteWriteDict.TryGetValue(difficulty, out difficultyNumber))
                                throw new Exception("Unhandled difficulty");

                            if (!c_forcingMidiWriteOffsets.TryGetValue(note.type, out forcingOffset))
                                throw new Exception("Unhandled note type found when trying to write forcing flag");

                            int forcedNoteNumber = difficultyNumber + forcingOffset;

                            SortableBytes forceOnEvent = new SortableBytes(note.tick, new byte[] { ON_EVENT, (byte)forcedNoteNumber, VELOCITY });
                            SortableBytes forceOffEvent = new SortableBytes(note.tick + 1, new byte[] { OFF_EVENT, (byte)forcedNoteNumber, VELOCITY });

                            InsertionSort(eventList, forceOnEvent);
                            InsertionSort(eventList, forceOffEvent);
                        }

                        if (writeGlobalTrackEvents)
                        {
                            if (instrument == Song.Instrument.Drums && !noteFlags.HasFlag(Note.Flags.ProDrums_Cymbal))     // We want to write our flags if the cymbal is toggled OFF, as these notes are cymbals by default
                            {
                                int tomToggleNoteNumber;
                                if (MidIOHelper.PAD_TO_CYMBAL_LOOKUP.TryGetValue(note.drumPad, out tomToggleNoteNumber))
                                {
                                    SortableBytes tomToggleOnEvent = new SortableBytes(note.tick, new byte[] { ON_EVENT, (byte)tomToggleNoteNumber, VELOCITY });
                                    SortableBytes tomToggleOffEvent = new SortableBytes(note.tick + 1, new byte[] { OFF_EVENT, (byte)tomToggleNoteNumber, VELOCITY });

                                    InsertionSort(eventList, tomToggleOnEvent);
                                    InsertionSort(eventList, tomToggleOffEvent);
                                }
                            }

                            int openNote = gameMode == Chart.GameMode.GHLGuitar ? (int)Note.GHLiveGuitarFret.Open : (int)Note.GuitarFret.Open;
                            // Add tap sysex events
                            bool isStartOfTapRange = noteFlags.HasFlag(Note.Flags.Tap) && (note.previous == null || (note.previous.flags & Note.Flags.Tap) == 0);
                            if (isStartOfTapRange)  // This note is a tap while the previous one isn't as we're creating a range
                            {
                                // Find the next non-tap note
                                Note nextNonTap = note;
                                while (nextNonTap.next != null && nextNonTap.next.flags.HasFlag(Note.Flags.Tap))
                                    nextNonTap = nextNonTap.next;

                                SortableBytes tapOnEvent;
                                SortableBytes tapOffEvent;
                                GetPhaseShiftSysExEventBytes(note.tick, nextNonTap.tick + 1, MidIOHelper.SYSEX_CODE_GUITAR_TAP, out tapOnEvent, out tapOffEvent);

                                InsertionSort(eventList, tapOnEvent);
                                InsertionSort(eventList, tapOffEvent);
                            }
                        }
                    }

                    // Open notes on guitar tracks did not originally have an assigned number.
                    // The initial workaround was to apply an sysex event to flag open notes.
                    // Enhanced opens are the v2 of this system, which is required for open chords on guitar tracks to work.
                    if (!enhancedOpens && gameMode == Chart.GameMode.Guitar && note.guitarFret == Note.GuitarFret.Open && (note.previous == null || (note.previous.guitarFret != Note.GuitarFret.Open)))
                    {
                        // Find the next non-open note
                        Note nextNonOpen = note;
                        while (nextNonOpen.next != null && nextNonOpen.next.guitarFret == Note.GuitarFret.Open)
                            nextNonOpen = nextNonOpen.next;

                        SortableBytes openOnEvent;
                        SortableBytes openOffEvent;
                        GetPhaseShiftSysExEventBytes(note.tick, nextNonOpen.tick + 1, difficulty, MidIOHelper.SYSEX_CODE_GUITAR_OPEN, out openOnEvent, out openOffEvent);

                        InsertionSort(eventList, openOnEvent);
                        InsertionSort(eventList, openOffEvent);
                    }
                }

                if (writeGlobalTrackEvents)
                {
                    Starpower sp = chartObject as Starpower;
                    if (sp != null)     // Starpower cannot be split up between charts in a midi file
                    {
                        // Starpower notes marked as a drum roll are written as 5 notes instead of 1
                        // http://docs.c3universe.com/rbndocs/index.php?title=Drum_Authoring#Drum_Fills
                        var events = GetStarpowerBytes(sp);
                        if (events != null)
                        {
                            for (int i = 0; i < events.Length; ++i)
                            {
                                (var spOnEvent, var spOffEvent) = events[i];
                                Debug.Assert(spOnEvent != null && spOffEvent != null, "Invalid starpower event pair in MIDI export!");
                                if (spOnEvent == null || spOffEvent == null)
                                    continue;

                                InsertionSort(eventList, spOnEvent);

                                if (spOffEvent.tick == spOnEvent.tick)
                                    ++spOffEvent.tick;

                                InsertionSort(eventList, spOffEvent);
                            }
                        }
                    }

                    DrumRoll roll = chartObject as DrumRoll;
                    if (roll != null)
                        GetDrumRollBytes(roll, out onEvent, out offEvent);

                    ChartEvent chartEvent = chartObject as ChartEvent;
                    if (chartEvent != null)     // Text events cannot be split up in the file
                    {
                        if (soloOnEvent != null && chartEvent.eventName == MidIOHelper.SOLO_END_EVENT_TEXT)
                        {
                            // Note-off tick for a solo marker in midi does not count as part of the solo, however does in .chart
                            // Manually offset by 1 tick to make sure all notes in the solo are included
                            uint endTick = Math.Max(soloOnEvent.tick, chartEvent.tick + SOLO_END_CORRECTION_OFFSET);
                            GetSoloBytes(soloOnEvent, endTick, out onEvent, out offEvent);
                            soloOnEvent = null;
                        }
                        else if (chartEvent.eventName == MidIOHelper.SOLO_EVENT_TEXT)
                        {
                            soloOnEvent = chartEvent;
                        }
                        else
                        {
                            InsertionSort(eventList, GetChartEventBytes(chartEvent));
                        }
                    }
                }

                if (onEvent != null && offEvent != null)
                {
                    InsertionSort(eventList, onEvent);

                    if (offEvent.tick == onEvent.tick)
                        ++offEvent.tick;

                    InsertionSort(eventList, offEvent);
                }
            }

            if (soloOnEvent != null)        // Found a solo event with no end. Assume the solo lasts for the rest of the song
            {
                SortableBytes onEvent = null;
                SortableBytes offEvent = null;

                uint soloEndTick = chart.chartObjects[chart.chartObjects.Count - 1].tick;   // In order to get a solo event the chart objects needed to have some object in this container, no need to check size, hopefully...
                GetSoloBytes(soloOnEvent, soloEndTick, out onEvent, out offEvent);

                if (onEvent != null && offEvent != null)
                {
                    InsertionSort(eventList, onEvent);

                    if (offEvent.tick == onEvent.tick)
                        ++offEvent.tick;

                    InsertionSort(eventList, offEvent);
                }
            }

            if (dynamicsFound)
            {
                byte[] textEvent = MetaTextEvent(MetaTextEventType.Text, MidIOHelper.CHART_DYNAMICS_TEXT_BRACKET);
                SortableBytes dynamicsEvent = new SortableBytes(1, textEvent); // Place at the start of the track, just after the track name event

                InsertionSort(eventList, dynamicsEvent);
            }

            return eventList.ToArray();
        }

        static byte[] GetUnrecognisedChartBytes(Chart chart, ExportOptions exportOptions, float resolutionScaleRatio)
        {
            List<SortableBytes> eventList = new List<SortableBytes>();

            foreach (ChartObject chartObject in chart.chartObjects)
            {
                SortableBytes onEvent = null;
                SortableBytes offEvent = null;

                Note note = chartObject as Note;
                if (note != null)
                    GetUnrecognisedChartNoteBytes(note, out onEvent, out offEvent);

                Starpower sp = chartObject as Starpower;
                if (sp != null)     // Starpower cannot be split up between charts in a midi file
                {
                    var events = GetStarpowerBytes(sp);
                    if (events != null)
                    {
                        for (int i = 0; i < events.Length; ++i)
                        {
                            (var spOnEvent, var spOffEvent) = events[i];
                            Debug.Assert(spOnEvent != null && spOffEvent != null, "Invalid starpower event pair in MIDI export!");
                            if (spOnEvent == null || spOffEvent == null)
                                continue;

                            InsertionSort(eventList, spOnEvent);

                            if (spOffEvent.tick == spOnEvent.tick)
                                ++spOffEvent.tick;

                            InsertionSort(eventList, spOffEvent);
                        }
                    }
                }

                DrumRoll roll = chartObject as DrumRoll;
                if (roll != null)
                    GetDrumRollBytes(roll, out onEvent, out offEvent);

                ChartEvent chartEvent = chartObject as ChartEvent;
                if (chartEvent != null)     // Text events cannot be split up in the file
                {
                    SortableBytes bytes = GetChartEventBytes(chartEvent);
                    InsertionSort(eventList, bytes);
                }

                if (onEvent != null && offEvent != null)
                {
                    InsertionSort(eventList, onEvent);

                    if (offEvent.tick == onEvent.tick)
                        ++offEvent.tick;

                    InsertionSort(eventList, offEvent);
                }
            }

            return SortableBytesToTimedEventBytes(eventList.ToArray(), chart.song, exportOptions, resolutionScaleRatio);
        }

        static byte[] GetMidiHeader(short fileFormat, short trackCount, short resolution)
        {
            const string ID = "MThd";   // MThd, 6 bytes, still need to add file format, track count and resolution to the header
            const int headerSize = 6;

            byte[] header = new byte[14];
            byte[] sourceBytes;
            int offset = 0;

            Array.Copy(System.Text.Encoding.UTF8.GetBytes(ID.ToCharArray()), 0, header, offset, ID.Length);
            offset += ID.Length;

            sourceBytes = EndianBitConverter.Big.GetBytes(headerSize);
            Array.Copy(sourceBytes, 0, header, offset, sizeof(int));
            offset += sizeof(int);

            sourceBytes = EndianBitConverter.Big.GetBytes(fileFormat);
            Array.Copy(sourceBytes, 0, header, offset, sizeof(short));
            offset += sizeof(short);

            sourceBytes = EndianBitConverter.Big.GetBytes(trackCount);
            Array.Copy(sourceBytes, 0, header, offset, sizeof(short));
            offset += sizeof(short);

            sourceBytes = EndianBitConverter.Big.GetBytes(resolution);
            Array.Copy(sourceBytes, 0, header, offset, sizeof(short));

            return header;
        }

        static byte[] GetTrackHeader(int byteLength)
        {
            const string ID = "MTrk";

            byte[] header = new byte[ID.Length + sizeof(int)];
            int offset = 0;

            Array.Copy(System.Text.Encoding.UTF8.GetBytes(ID.ToCharArray()), 0, header, offset, ID.Length);
            offset += ID.Length;

            byte[] sourceBytes = EndianBitConverter.Big.GetBytes(byteLength);
            Array.Copy(sourceBytes, 0, header, offset, sizeof(int));

            return header;
        }

        static byte[] MakeTrack(byte[] trackEvents, string trackName)
        {
            byte[] trackNameEvent = TimedEvent(0, MetaTextEvent(MetaTextEventType.TrackName, trackName));

            byte[] header = GetTrackHeader(trackNameEvent.Length + trackEvents.Length + END_OF_TRACK.Length);
            byte[] fullTrack = new byte[header.Length + trackNameEvent.Length + trackEvents.Length + END_OF_TRACK.Length];

            int offset = 0;
            Array.Copy(header, fullTrack, header.Length);
            offset += header.Length;

            Array.Copy(trackNameEvent, 0, fullTrack, offset, trackNameEvent.Length);
            offset += trackNameEvent.Length;

            Array.Copy(trackEvents, 0, fullTrack, offset, trackEvents.Length);
            offset += trackEvents.Length;

            Array.Copy(END_OF_TRACK, 0, fullTrack, offset, END_OF_TRACK.Length);

            return fullTrack;
        }

        // Joins delta-time byte information onto the front of a byte array
        static byte[] TimedEvent(uint tick, byte[] midiEvent)
        {
            byte[] deltaTime = VLVCompressedBytes(tick);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(deltaTime);

            byte[] timedEvent = new byte[deltaTime.Length + midiEvent.Length];
            Array.Copy(deltaTime, 0, timedEvent, 0, deltaTime.Length);
            Array.Copy(midiEvent, 0, timedEvent, deltaTime.Length, midiEvent.Length);

            return timedEvent;
        }

        static byte[] MetaTextEvent(MetaTextEventType eventType, string text)
        {
            return MetaTextEvent((byte)eventType, text);
        }

        static byte[] MetaTextEvent(byte m_event, string text)
        {
            const int MaxTextLength = 127;
            if (text.Length > MaxTextLength)
            {
                Debug.LogWarning("Text cannot be longer than " + MaxTextLength + " characters, capping string " + text);
                text = text.Substring(0, MaxTextLength);
            }

            char[] chars = text.ToCharArray();

            byte[] header_event = EndianBitConverter.Big.GetBytes((short)(0xFF00 | (short)m_event));                // FF xx
            byte[] header_text = System.Text.Encoding.UTF8.GetBytes(chars);            // dd
            byte[] header_byte_length = EndianBitConverter.Big.GetBytes((sbyte)(header_text.Length));    // nn

            byte[] bytes = new byte[3 + (header_text.Length)];       // FF xx nn then whatever data

            int offset = 0;
            Array.Copy(header_event, 0, bytes, offset, sizeof(short));

            offset += sizeof(short);
            Array.Copy(header_byte_length, sizeof(sbyte), bytes, offset, sizeof(sbyte));

            offset += sizeof(sbyte);
            Array.Copy(header_text, 0, bytes, offset, header_text.Length);

            return bytes;
        }

        static byte[] TempoEvent(BPM bpm)
        {
            const byte TEMPO_EVENT = 0x51;
            byte[] bytes = new byte[6];

            bytes[0] = 0xFF;
            bytes[1] = TEMPO_EVENT;
            bytes[2] = 0x03;            // Size

            // Microseconds per quarter note for the last 3 bytes stored as a 24-bit binary
            byte[] microPerSec = EndianBitConverter.Big.GetBytes((uint)(6.0f * Mathf.Pow(10, 10) / bpm.value));

            Array.Copy(microPerSec, 1, bytes, 3, 3);        // Offset of 1 and length of 3 cause 24 bit

            return bytes;
        }

        static byte[] TimeSignatureEvent(TimeSignature ts)
        {
            const byte TIME_SIGNATURE_EVENT = 0x58;
            byte[] bytes = new byte[7];

            bytes[0] = 0xFF;
            bytes[1] = TIME_SIGNATURE_EVENT;
            bytes[2] = 0x04;            // Size
            bytes[3] = EndianBitConverter.Big.GetBytes((short)ts.numerator)[1];
            bytes[4] = EndianBitConverter.Big.GetBytes((short)(Mathf.Log(ts.denominator, 2)))[1];
            bytes[5] = 0x18; // 24, 24 clock ticks in metronome click, so once every quater note. I doubt this is important, but I'm sure irony will strike.
            bytes[6] = 0x08; // 8, a quater note should happen every quarter note.

            return bytes;
        }

        static byte[] GenerateBeat(uint end, uint resolution)
        {
            const byte BEAT_ON_EVENT = 0x97;         // Note on channel 7
            const byte BEAT_OFF_EVENT = 0x87;
            const int BEAT_VELOCITY = 100;
            const int MEASURE_NOTE = 12;
            const int BEAT_NOTE = 13;

            uint length = resolution / 4;
            uint measure = resolution * 4;

            uint tick = 0;

            List<byte> beatBytes = new List<byte>();
            // Add inital beats
            byte[] onEvent = new byte[] { BEAT_ON_EVENT, (byte)MEASURE_NOTE, BEAT_VELOCITY };
            byte[] offEvent = new byte[] { BEAT_OFF_EVENT, (byte)MEASURE_NOTE, BEAT_VELOCITY };

            beatBytes.AddRange(TimedEvent(0, onEvent));
            beatBytes.AddRange(TimedEvent(length, offEvent));

            tick += resolution;

            while (tick < end)
            {
                tick += resolution;

                int noteNumber = BEAT_NOTE;
                if (tick % measure == 0)
                    noteNumber = MEASURE_NOTE;

                onEvent = new byte[] { BEAT_ON_EVENT, (byte)noteNumber, BEAT_VELOCITY };
                offEvent = new byte[] { BEAT_OFF_EVENT, (byte)noteNumber, BEAT_VELOCITY };

                beatBytes.AddRange(TimedEvent(resolution - length, onEvent));
                beatBytes.AddRange(TimedEvent(length, offEvent));
            }

            return beatBytes.ToArray();
        }

        static byte[] VLVCompressedBytes(uint value)
        {
            List<byte> vlvEncodedBytesList = new List<byte>();

            bool first = true;
            while (first || value > 0)
            {
                byte lower7bits;

                lower7bits = (byte)(value & 0x7F);

                if (!first)
                    lower7bits |= 128;      // Change lsb to a value of 1
                value >>= 7;

                first = false;
                vlvEncodedBytesList.Add(lower7bits);
            }

            return vlvEncodedBytesList.ToArray();
        }

        /* CHART EVENT BYTE DETERMINING 
        ***********************************************************************************************/

        static (SortableBytes, SortableBytes)[] GetStarpowerBytes(Starpower sp)
        {
            uint startTick = sp.tick;
            uint endTick = sp.tick + sp.length;
            bool isDrumFill = sp.flags.HasFlag(Starpower.Flags.ProDrums_Activation);
            // Drum fills are 5 notes instead of one
            // http://docs.c3universe.com/rbndocs/index.php?title=Drum_Authoring#Drum_Fills
            if (isDrumFill)
            {
                return new (SortableBytes, SortableBytes)[] {
                    (new SortableBytes(startTick, new byte[] { ON_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_0, VELOCITY }),
                    new SortableBytes(endTick, new byte[] { OFF_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_0, VELOCITY })),

                    (new SortableBytes(startTick, new byte[] { ON_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_1, VELOCITY }),
                    new SortableBytes(endTick, new byte[] { OFF_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_1, VELOCITY })),

                    (new SortableBytes(startTick, new byte[] { ON_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_2, VELOCITY }),
                    new SortableBytes(endTick, new byte[] { OFF_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_2, VELOCITY })),

                    (new SortableBytes(startTick, new byte[] { ON_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_3, VELOCITY }),
                    new SortableBytes(endTick, new byte[] { OFF_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_3, VELOCITY })),

                    (new SortableBytes(startTick, new byte[] { ON_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_4, VELOCITY }),
                    new SortableBytes(endTick, new byte[] { OFF_EVENT, MidIOHelper.STARPOWER_DRUM_FILL_4, VELOCITY }))
                };
            }
            else
            {
                return new (SortableBytes, SortableBytes)[] {
                    (new SortableBytes(startTick, new byte[] { ON_EVENT, MidIOHelper.STARPOWER_NOTE, VELOCITY }),
                    new SortableBytes(endTick, new byte[] { OFF_EVENT, MidIOHelper.STARPOWER_NOTE, VELOCITY }))
                };
            }
        }

        static void GetDrumRollBytes(DrumRoll roll, out SortableBytes onEvent, out SortableBytes offEvent)
        {
            byte note = roll.type == DrumRoll.Type.Standard ? MidIOHelper.DRUM_ROLL_STANDARD : MidIOHelper.DRUM_ROLL_SPECIAL;

            onEvent = new SortableBytes(roll.tick, new byte[] { ON_EVENT, note, VELOCITY });
            offEvent = new SortableBytes(roll.tick + roll.length, new byte[] { OFF_EVENT, note, VELOCITY });
        }

        static void GetSoloBytes(ChartEvent solo, uint soloEndTick, out SortableBytes onEvent, out SortableBytes offEvent)
        {
            onEvent = new SortableBytes(solo.tick, new byte[] { ON_EVENT, MidIOHelper.SOLO_NOTE, VELOCITY });
            offEvent = new SortableBytes(soloEndTick, new byte[] { OFF_EVENT, MidIOHelper.SOLO_NOTE, VELOCITY });
        }

        static SortableBytes GetChartEventBytes(ChartEvent chartEvent)
        {
            byte[] textEvent = MetaTextEvent(MetaTextEventType.Text, chartEvent.eventName);
            return new SortableBytes(chartEvent.tick, textEvent);
        }

        static void GetPhaseShiftSysExEventBytes(uint startTick, uint endTick, byte code, out SortableBytes startEvent, out SortableBytes endEvent)
        {
            startEvent = GetPhaseShiftSysExEventBytes(startTick, code, start: true);
            endEvent = GetPhaseShiftSysExEventBytes(endTick, code, start: false);
        }

        static void GetPhaseShiftSysExEventBytes(uint startTick, uint endTick, Song.Difficulty difficulty, byte code, out SortableBytes startEvent, out SortableBytes endEvent)
        {
            startEvent = GetPhaseShiftSysExEventBytes(startTick, difficulty, code, start: true);
            endEvent = GetPhaseShiftSysExEventBytes(endTick, difficulty, code, start: false);
        }

        static SortableBytes GetPhaseShiftSysExEventBytes(uint tick, byte code, bool start)
        {
            return GetPhaseShiftSysExEventBytes(tick, MidIOHelper.SYSEX_DIFFICULTY_ALL, code, start);
        }

        static SortableBytes GetPhaseShiftSysExEventBytes(uint tick, Song.Difficulty difficulty, byte code, bool start)
        {
            return GetPhaseShiftSysExEventBytes(tick, MidIOHelper.MS_TO_SYSEX_DIFF_LOOKUP[difficulty], code, start);
        }

        static SortableBytes GetPhaseShiftSysExEventBytes(uint tick, byte difficulty, byte code, bool start)
        {
            if (!MidIOHelper.MS_TO_SYSEX_DIFF_LOOKUP.ContainsValue(difficulty) && difficulty != MidIOHelper.SYSEX_DIFFICULTY_ALL)
                throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "The given difficulty is not valid.");

            byte[] eventBytes = new byte[] {
                SYSEX_START,
                MidIOHelper.SYSEX_LENGTH,
                (byte)MidIOHelper.SYSEX_HEADER_1,
                (byte)MidIOHelper.SYSEX_HEADER_2,
                (byte)MidIOHelper.SYSEX_HEADER_3,
                MidIOHelper.SYSEX_TYPE_PHRASE,
                difficulty,
                code,
                start ? MidIOHelper.SYSEX_VALUE_PHRASE_START : MidIOHelper.SYSEX_VALUE_PHRASE_END,
                SYSEX_END
            };

            return new SortableBytes(tick, eventBytes);
        }

        static void GetUnrecognisedChartNoteBytes(Note note, out SortableBytes onEvent, out SortableBytes offEvent)
        {
            GetNoteNumberBytes(note.rawNote, note, VELOCITY, out onEvent, out offEvent);
        }

        static void GetNoteNumberBytes(int noteNumber, Note note, byte velocity, out SortableBytes onEvent, out SortableBytes offEvent)
        {
            onEvent = new SortableBytes(note.tick, new byte[] { ON_EVENT, (byte)noteNumber, velocity });
            offEvent = new SortableBytes(note.tick + note.length, new byte[] { OFF_EVENT, (byte)noteNumber, velocity });
        }

        static int GetMidiNoteNumber(Note note, Chart.GameMode gameMode, Song.Difficulty difficulty, bool enhancedOpens)
        {
            IReadOnlyDictionary<int, int> noteToMidiOffsetDict;
            int difficultyNumber;
            int offset;

            // Double kick, ala instrument+, kinda hacky
            if (gameMode == Chart.GameMode.Drums && (note.flags & Note.Flags.DoubleKick) != 0 && NoteFunctions.AllowedToBeDoubleKick(note, difficulty))
            {
                return MidIOHelper.DOUBLE_KICK_NOTE;
            }

            noteToMidiOffsetDict = GetNoteWriteOffsetLookup(gameMode, enhancedOpens);

            if (!noteToMidiOffsetDict.TryGetValue(note.rawNote, out offset))
                throw new NotImplementedException($"Unhandled note {note.rawNote}, unable to get offset");

            if (!c_difficultyToMidiNoteWriteDict.TryGetValue(difficulty, out difficultyNumber))
                throw new NotImplementedException($"Unhandled difficulty {difficulty}");

            return difficultyNumber + offset;
        }

        class ChartFeatureChecker : ISongValidateReport
        {
            bool _hasOpenChords = false;
            bool _hasOpenTaps = false;

            public bool HasOpenChords => _hasOpenChords;
            public bool HasOpenTaps => _hasOpenTaps;

            public void NotifyOpenChordFound(Note note)
            {
                _hasOpenChords = true;
            }

            public void NotifyOpenTapFound(Note note)
            {
                _hasOpenTaps = true;
            }

            public void NotifyRockBandMidiSoloStarpowerMisRead(Song.Instrument instrument)
            {
            }

            public void NotifySectionLimitError(Song song, int sectionLimit)
            {
            }

            public void NotifySongObjectBeyondExpectedLength(SongObject song)
            {
            }

            public void NotifyTimeSignaturePlacementError(TimeSignature ts)
            {
            }
        }
    }
}
