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
        const byte SYSEX_ON = 0x01;
        const byte SYSEX_OFF = 0x00;

        static readonly byte[] END_OF_TRACK = new byte[] { 0, 0xFF, 0x2F, 0x00 };

        struct VocalProcessingParams
        {
            public Song song;
            public IList<Event> eventList;
            public int eventListIndex;

            public List<SortableBytes> out_sortableBytes;
        }

        static readonly Dictionary<Song.Instrument, string> c_instrumentToTrackNameDict = new Dictionary<Song.Instrument, string>()
    {
        { Song.Instrument.Guitar,           MidIOHelper.GUITAR_TRACK },
        { Song.Instrument.GuitarCoop,       MidIOHelper.GUITAR_COOP_TRACK },
        { Song.Instrument.Bass,             MidIOHelper.BASS_TRACK },
        { Song.Instrument.Rhythm,           MidIOHelper.RHYTHM_TRACK },
        { Song.Instrument.Keys,             MidIOHelper.KEYS_TRACK },
        { Song.Instrument.Drums,            MidIOHelper.DRUMS_TRACK },
        { Song.Instrument.GHLiveGuitar,     MidIOHelper.GHL_GUITAR_TRACK },
        { Song.Instrument.GHLiveBass,       MidIOHelper.GHL_BASS_TRACK },
    };

        static readonly Dictionary<Song.Difficulty, int> c_difficultyToMidiNoteWriteDict = new Dictionary<Song.Difficulty, int>()
    {
        { Song.Difficulty.Easy,             60 },
        { Song.Difficulty.Medium,           72 },
        { Song.Difficulty.Hard,             84 },
        { Song.Difficulty.Expert,           96 },
    };

        static readonly Dictionary<int, int> c_guitarNoteMidiWriteOffsets = new Dictionary<int, int>()
    {
        { (int)Note.GuitarFret.Open,     0},     // Gets replaced by an sysex event
        { (int)Note.GuitarFret.Green,    0},
        { (int)Note.GuitarFret.Red,      1},
        { (int)Note.GuitarFret.Yellow,   2},
        { (int)Note.GuitarFret.Blue,     3},
        { (int)Note.GuitarFret.Orange,   4},
    };

        static readonly Dictionary<int, int> c_drumNoteMidiWriteOffsets = new Dictionary<int, int>()
    {
        { (int)Note.DrumPad.Kick,     0},
        { (int)Note.DrumPad.Red,      1},
        { (int)Note.DrumPad.Yellow,   2},
        { (int)Note.DrumPad.Blue,     3},
        { (int)Note.DrumPad.Orange,   4},
        { (int)Note.DrumPad.Green,    5},
    };

        static readonly Dictionary<int, int> c_ghlNoteMidiWriteOffsets = new Dictionary<int, int>()
    {
        { (int)Note.GHLiveGuitarFret.Open,   -2},
        { (int)Note.GHLiveGuitarFret.White1, -1},
        { (int)Note.GHLiveGuitarFret.White2, 0},
        { (int)Note.GHLiveGuitarFret.White3, 1},
        { (int)Note.GHLiveGuitarFret.Black1, 2},
        { (int)Note.GHLiveGuitarFret.Black2, 3},
        { (int)Note.GHLiveGuitarFret.Black3, 4},
    };

        static readonly Dictionary<Chart.GameMode, Dictionary<int, int>> c_gameModeNoteWriteOffsetDictLookup = new Dictionary<Chart.GameMode, Dictionary<int, int>>()
    {
        { Chart.GameMode.Guitar,    c_guitarNoteMidiWriteOffsets },
        { Chart.GameMode.Drums,     c_drumNoteMidiWriteOffsets },
        { Chart.GameMode.GHLGuitar, c_ghlNoteMidiWriteOffsets },
    };

        static readonly Dictionary<Note.NoteType, int> c_forcingMidiWriteOffsets = new Dictionary<Note.NoteType, int>()
    {
        { Note.NoteType.Hopo, 5 },
        { Note.NoteType.Strum, 6 },
    };

        delegate void ProcessVocalEventBytesFn(in VocalProcessingParams processParams);
        static readonly Dictionary<string, ProcessVocalEventBytesFn> vocalPrefixProcessList = new Dictionary<string, ProcessVocalEventBytesFn>()
        {
            { MidIOHelper.LYRIC_EVENT_PREFIX, (in VocalProcessingParams processParams) => {
                const string prefix = MidIOHelper.LYRIC_EVENT_PREFIX;

                Event songEvent = processParams.eventList[processParams.eventListIndex];

                string currentEventTitle = songEvent.title;
                string metaTextEventStr = currentEventTitle.Substring(prefix.Length, currentEventTitle.Length - prefix.Length);

                SortableBytes bytes = new SortableBytes(songEvent.tick, MetaTextEvent(MetaTextEventType.Lyric, metaTextEventStr));
                InsertionSort(processParams.out_sortableBytes, bytes);
            }},

            { MidIOHelper.PhraseStartText, (in VocalProcessingParams processParams) => {

                Event phraseStartEvent = processParams.eventList[processParams.eventListIndex];
                uint phraseEndEventTick = phraseStartEvent.tick;    // Find next phase end or the next phase start, whichever is first. 1 tick away as a backup. 

                for (int i = processParams.eventListIndex + 1; i < processParams.eventList.Count; ++i)
                {
                    Event nextEvent = processParams.eventList[i];
                    if (nextEvent.title.StartsWith(MidIOHelper.PhraseEndText))
                    {
                        phraseEndEventTick = nextEvent.tick;
                        break;
                    }
                    else if(nextEvent.title.StartsWith(MidIOHelper.PhraseStartText))
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
                GetNoteNumberBytes(MidIOHelper.PhraseMarker, phraseNote, VELOCITY, out onEvent, out offEvent);

                InsertionSort(processParams.out_sortableBytes, onEvent);
                InsertionSort(processParams.out_sortableBytes, offEvent);
            }},

            { MidIOHelper.PhraseEndText, (in VocalProcessingParams processParams) => {
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
            string section_id = MidIOHelper.Rb2SectionPrefix;

            switch (rbFormat)
            {
                case ExportOptions.MidiOptions.RBFormat.RB3:
                    {
                        section_id = MidIOHelper.Rb3SectionPrefix;
                        break;
                    }
                case ExportOptions.MidiOptions.RBFormat.RB2:
                default:
                    {
                        section_id = MidIOHelper.Rb2SectionPrefix;
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
            // Collect all bytes from each difficulty of the instrument, assigning the position for each event unsorted
            //List<SortableBytes> byteEvents = new List<SortableBytes>();
            SortableBytes[] easyBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Easy, exportOptions);
            SortableBytes[] mediumBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Medium, exportOptions);
            SortableBytes[] hardBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Hard, exportOptions);
            SortableBytes[] expertBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Expert, exportOptions);

            SortableBytes[] em = SortableBytes.MergeAlreadySorted(easyBytes, mediumBytes);
            SortableBytes[] he = SortableBytes.MergeAlreadySorted(hardBytes, expertBytes);
            List<SortableBytes> sortedEvents = new List<SortableBytes>(SortableBytes.MergeAlreadySorted(em, he));

            // Perform merge sort to re-order everything correctly
            //SortableBytes[] sortedEvents = new SortableBytes[easyBytes.Length + mediumBytes.Length + hardBytes.Length + expertBytes.Length];//byteEvents.ToArray();
            //SortableBytes.Sort(sortedEvents);

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

        static SortableBytes[] GetChartSortableBytes(Song song, Song.Instrument instrument, Song.Difficulty difficulty, ExportOptions exportOptions)
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
            foreach (ChartObject chartObject in chart.chartObjects)
            {
                Note note = chartObject as Note;

                SortableBytes onEvent = null;
                SortableBytes offEvent = null;

                if (note != null)
                {
                    int noteNumber = GetMidiNoteNumber(note, gameMode, difficulty);

                    byte velocity = VELOCITY;

                    if (gameMode == Chart.GameMode.Drums)
                    {
                        if (note.flags.HasFlag(Note.Flags.ProDrums_Accent))
                        {
                            velocity = MidIOHelper.VELOCITY_ACCENT;
                        }
                        else if (note.flags.HasFlag(Note.Flags.ProDrums_Ghost))
                        {
                            velocity = MidIOHelper.VELOCITY_GHOST;
                        }
                    }

                    GetNoteNumberBytes(noteNumber, note, velocity, out onEvent, out offEvent);

                    if (exportOptions.forced)
                    {
                        Note.Flags bannedFlags = Note.GetBannedFlagsForGameMode(gameMode);
                        Note.Flags noteFlags = note.flags & ~bannedFlags;   // Last ditched error correction

                        // Forced notes               
                        if ((noteFlags & Note.Flags.Forced) != 0 && note.type != Note.NoteType.Tap && (note.previous == null || (note.previous.tick != note.tick)))     // Don't overlap on chords
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
                            if (instrument == Song.Instrument.Drums && ((noteFlags & Note.Flags.ProDrums_Cymbal) == 0))     // We want to write our flags if the cymbal is toggled OFF, as these notes are cymbals by default
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
                            bool isStartOfTapRange = note.rawNote != openNote && (noteFlags & Note.Flags.Tap) != 0 && (note.previous == null || (note.previous.flags & Note.Flags.Tap) == 0);
                            if (isStartOfTapRange)  // This note is a tap while the previous one isn't as we're creating a range
                            {
                                // Find the next non-tap note
                                Note nextNonTap = note;
                                while (nextNonTap.next != null && nextNonTap.rawNote != openNote && (nextNonTap.next.flags & Note.Flags.Tap) != 0)
                                    nextNonTap = nextNonTap.next;

                                // Tap event = 08-50-53-00-00-FF-04-01, end with 01 for On, 00 for Off
                                byte[] tapOnEventBytes = new byte[] { SYSEX_START, 0x08, 0x50, 0x53, 0x00, 0x00, 0xFF, 0x04, SYSEX_ON, SYSEX_END };
                                byte[] tapOffEventBytes = new byte[] { SYSEX_START, 0x08, 0x50, 0x53, 0x00, 0x00, 0xFF, 0x04, SYSEX_OFF, SYSEX_END };

                                SortableBytes tapOnEvent = new SortableBytes(note.tick, tapOnEventBytes);
                                SortableBytes tapOffEvent = new SortableBytes(nextNonTap.tick + 1, tapOffEventBytes);

                                InsertionSort(eventList, tapOnEvent);
                                InsertionSort(eventList, tapOffEvent);
                            }
                        }
                    }

                    if (gameMode != Chart.GameMode.Drums && gameMode != Chart.GameMode.GHLGuitar && note.guitarFret == Note.GuitarFret.Open && (note.previous == null || (note.previous.guitarFret != Note.GuitarFret.Open)))
                    {
                        // Find the next non-open note
                        Note nextNonOpen = note;
                        while (nextNonOpen.next != null && nextNonOpen.next.guitarFret == Note.GuitarFret.Open)
                            nextNonOpen = nextNonOpen.next;

                        byte diff;

                        switch (difficulty)
                        {
                            case (Song.Difficulty.Easy):
                                diff = 0;
                                break;
                            case (Song.Difficulty.Medium):
                                diff = 1;
                                break;
                            case (Song.Difficulty.Hard):
                                diff = 2;
                                break;
                            case (Song.Difficulty.Expert):
                                diff = 3;
                                break;
                            default:
                                continue;
                        }

                        byte[] openOnEventBytes = new byte[] { SYSEX_START, 0x08, 0x50, 0x53, 0x00, 0x00, diff, 0x01, SYSEX_ON, SYSEX_END };
                        byte[] openOffEventBytes = new byte[] { SYSEX_START, 0x08, 0x50, 0x53, 0x00, 0x00, diff, 0x01, SYSEX_OFF, SYSEX_END };

                        SortableBytes openOnEvent = new SortableBytes(note.tick, openOnEventBytes);
                        SortableBytes openOffEvent = new SortableBytes(nextNonOpen.tick + 1, openOffEventBytes);

                        InsertionSort(eventList, openOnEvent);
                        InsertionSort(eventList, openOffEvent);
                    }
                }

                if (writeGlobalTrackEvents)
                {
                    Starpower sp = chartObject as Starpower;
                    if (sp != null)     // Starpower cannot be split up between charts in a midi file
                        GetStarpowerBytes(sp, out onEvent, out offEvent);

                    ChartEvent chartEvent = chartObject as ChartEvent;
                    if (chartEvent != null)     // Text events cannot be split up in the file
                    {
                        if (soloOnEvent != null && chartEvent.eventName == MidIOHelper.SoloEndEventText)
                        {
                            GetSoloBytes(soloOnEvent, chartEvent.tick, out onEvent, out offEvent);
                            soloOnEvent = null;
                        }
                        else if (chartEvent.eventName == MidIOHelper.SoloEventText)
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
                    GetStarpowerBytes(sp, out onEvent, out offEvent);

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

        static void GetStarpowerBytes(Starpower sp, out SortableBytes onEvent, out SortableBytes offEvent)
        {
            bool isDrumFill = sp.flags.HasFlag(Starpower.Flags.ProDrums_Activation);
            byte spNote = isDrumFill ? MidIOHelper.STARPOWER_DRUM_FILL_0 : MidIOHelper.STARPOWER_NOTE;

            onEvent = new SortableBytes(sp.tick, new byte[] { ON_EVENT, spNote, VELOCITY });
            offEvent = new SortableBytes(sp.tick + sp.length, new byte[] { OFF_EVENT, spNote, VELOCITY });
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

        static void GetUnrecognisedChartNoteBytes(Note note, out SortableBytes onEvent, out SortableBytes offEvent)
        {
            GetNoteNumberBytes(note.rawNote, note, VELOCITY, out onEvent, out offEvent);
        }

        static void GetNoteNumberBytes(int noteNumber, Note note, byte velocity, out SortableBytes onEvent, out SortableBytes offEvent)
        {
            onEvent = new SortableBytes(note.tick, new byte[] { ON_EVENT, (byte)noteNumber, velocity });
            offEvent = new SortableBytes(note.tick + note.length, new byte[] { OFF_EVENT, (byte)noteNumber, velocity });
        }

        static int GetMidiNoteNumber(Note note, Chart.GameMode gameMode, Song.Difficulty difficulty)
        {
            Dictionary<int, int> noteToMidiOffsetDict;
            int difficultyNumber;
            int offset;

            // Double kick, ala instrument+, kinda hacky
            if (gameMode == Chart.GameMode.Drums && (note.flags & Note.Flags.DoubleKick) != 0 && NoteFunctions.AllowedToBeDoubleKick(note, difficulty))
            {
                return MidIOHelper.DOUBLE_KICK_NOTE;
            }

            if (!c_gameModeNoteWriteOffsetDictLookup.TryGetValue(gameMode, out noteToMidiOffsetDict))
                throw new System.Exception("Unhandled game mode, unable to get offset dictionary");

            if (!noteToMidiOffsetDict.TryGetValue(note.rawNote, out offset))
                throw new System.Exception("Unhandled note, unable to get offset");

            if (!c_difficultyToMidiNoteWriteDict.TryGetValue(difficulty, out difficultyNumber))
                throw new System.Exception("Unhandled difficulty");

            return difficultyNumber + offset;
        }
    }
}
