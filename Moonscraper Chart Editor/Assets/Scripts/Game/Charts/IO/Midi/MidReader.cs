// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MoonscraperEngine;

namespace MoonscraperChartEditor.Song.IO
{
    using NoteEventQueue = List<(NoteEvent note, long tick)>;
    using SysExEventQueue = List<(PhaseShiftSysEx sysex, long tick)>;

    public static class MidReader
    {
        const int SOLO_END_CORRECTION_OFFSET = -1;

        public enum CallbackState
        {
            None,
            WaitingForExternalInformation,
        }

        static readonly IReadOnlyDictionary<string, Song.Instrument> c_trackNameToInstrumentMap = new Dictionary<string, Song.Instrument>()
        {
            { MidIOHelper.GUITAR_TRACK,        Song.Instrument.Guitar },
            { MidIOHelper.GH1_GUITAR_TRACK,    Song.Instrument.Guitar },
            { MidIOHelper.GUITAR_COOP_TRACK,   Song.Instrument.GuitarCoop },
            { MidIOHelper.BASS_TRACK,          Song.Instrument.Bass },
            { MidIOHelper.RHYTHM_TRACK,        Song.Instrument.Rhythm },
            { MidIOHelper.KEYS_TRACK,          Song.Instrument.Keys },
            { MidIOHelper.DRUMS_TRACK,         Song.Instrument.Drums },
            { MidIOHelper.DRUMS_REAL_TRACK,    Song.Instrument.Drums },
            { MidIOHelper.GHL_GUITAR_TRACK,    Song.Instrument.GHLiveGuitar },
            { MidIOHelper.GHL_BASS_TRACK,      Song.Instrument.GHLiveBass },
            { MidIOHelper.GHL_RHYTHM_TRACK,    Song.Instrument.GHLiveRhythm },
            { MidIOHelper.GHL_GUITAR_COOP_TRACK, Song.Instrument.GHLiveCoop },
        };

        static readonly IReadOnlyDictionary<string, bool> c_trackExcludesMap = new Dictionary<string, bool>()
        {
            { MidIOHelper.BEAT_TRACK,       true },
            { MidIOHelper.VENUE_TRACK,      true },
        };

        static readonly List<Song.Instrument> c_legacyStarPowerFixupWhitelist = new List<Song.Instrument>()
        {
            Song.Instrument.Guitar,
            Song.Instrument.GuitarCoop,
            Song.Instrument.Bass,
            Song.Instrument.Rhythm,
        };

        static readonly IReadOnlyDictionary<Song.AudioInstrument, string[]> c_audioStreamLocationOverrideDict = new Dictionary<Song.AudioInstrument, string[]>()
        {
            // String list is ordered in priority. If it finds a file names with the first string it'll skip over the rest.
            // Otherwise just does a ToString on the AudioInstrument enum
            { Song.AudioInstrument.Drum, new string[] { "drums", "drums_1" } },
        };

        struct TimedMidiEvent
        {
            public MidiEvent midiEvent;
            public long startTick;
            public long endTick;

            public long length => endTick - startTick;
        }

        struct EventProcessParams
        {
            public Song song;
            public Song.Instrument instrument;
            public Chart currentUnrecognisedChart;
            public TimedMidiEvent timedEvent;
            public IReadOnlyDictionary<int, EventProcessFn> noteProcessMap;
            public IReadOnlyDictionary<string, ProcessModificationProcessFn> textProcessMap;
            public IReadOnlyDictionary<byte, EventProcessFn> sysexProcessMap;
            public List<EventProcessFn> forcingProcessList;
            public List<EventProcessFn> sysexProcessList;
        }

        // Delegate for functions that parse something into the chart
        delegate void EventProcessFn(in EventProcessParams eventProcessParams);
        // Delegate for functions that modify how the chart should be parsed
        delegate void ProcessModificationProcessFn(ref EventProcessParams eventProcessParams);

        // These dictionaries map the NoteNumber of each midi note event to a specific function of how to process them
        static readonly IReadOnlyDictionary<int, EventProcessFn> GuitarMidiNoteNumberToProcessFnMap = BuildGuitarMidiNoteNumberToProcessFnDict();
        static readonly IReadOnlyDictionary<int, EventProcessFn> GuitarMidiNoteNumberToProcessFnMap_EnhancedOpens = BuildGuitarMidiNoteNumberToProcessFnDict(enhancedOpens: true);
        static readonly IReadOnlyDictionary<int, EventProcessFn> GhlGuitarMidiNoteNumberToProcessFnMap = BuildGhlGuitarMidiNoteNumberToProcessFnDict();
        static readonly IReadOnlyDictionary<int, EventProcessFn> DrumsMidiNoteNumberToProcessFnMap = BuildDrumsMidiNoteNumberToProcessFnDict();
        static readonly IReadOnlyDictionary<int, EventProcessFn> DrumsMidiNoteNumberToProcessFnMap_Velocity = BuildDrumsMidiNoteNumberToProcessFnDict(enableVelocity: true);

        // These dictionaries map the text of a MIDI text event to a specific function that processes them
        static readonly IReadOnlyDictionary<string, ProcessModificationProcessFn> GuitarTextEventToProcessFnMap = new Dictionary<string, ProcessModificationProcessFn>()
        {
            { MidIOHelper.ENHANCED_OPENS_TEXT, SwitchToGuitarEnhancedOpensProcessMap },
            { MidIOHelper.ENHANCED_OPENS_TEXT_BRACKET, SwitchToGuitarEnhancedOpensProcessMap }
        };

        static readonly IReadOnlyDictionary<string, ProcessModificationProcessFn> GhlGuitarTextEventToProcessFnMap = new Dictionary<string, ProcessModificationProcessFn>()
        {
        };

        static readonly IReadOnlyDictionary<string, ProcessModificationProcessFn> DrumsTextEventToProcessFnMap = new Dictionary<string, ProcessModificationProcessFn>()
        {
            { MidIOHelper.CHART_DYNAMICS_TEXT, SwitchToDrumsVelocityProcessMap },
            { MidIOHelper.CHART_DYNAMICS_TEXT_BRACKET, SwitchToDrumsVelocityProcessMap },
        };

        static readonly Dictionary<byte, EventProcessFn> GuitarSysExEventToProcessFnMap = new Dictionary<byte, EventProcessFn>()
        {
            { MidIOHelper.SYSEX_CODE_GUITAR_OPEN, ProcessSysExEventPairAsOpenNoteModifier },
            { MidIOHelper.SYSEX_CODE_GUITAR_TAP, (in EventProcessParams eventProcessParams) => {
                ProcessSysExEventPairAsForcedType(eventProcessParams, Note.NoteType.Tap);
            }},
        };

        static readonly Dictionary<byte, EventProcessFn> GhlGuitarSysExEventToProcessFnMap = new Dictionary<byte, EventProcessFn>()
        {
            { MidIOHelper.SYSEX_CODE_GUITAR_OPEN, ProcessSysExEventPairAsOpenNoteModifier },
            { MidIOHelper.SYSEX_CODE_GUITAR_TAP, (in EventProcessParams eventProcessParams) => {
                ProcessSysExEventPairAsForcedType(eventProcessParams, Note.NoteType.Tap);
            }},
        };

        static readonly Dictionary<byte, EventProcessFn> DrumsSysExEventToProcessFnMap = new Dictionary<byte, EventProcessFn>()
        {
        };

        // For handling things that require user intervention
        delegate void MessageProcessFn(MessageProcessParams processParams);
        struct MessageProcessParams
        {
            public string title;
            public string message;
            public Song currentSong;
            public Song.Instrument instrument;
            public TrackChunk track;
            public MessageProcessFn processFn;
            public bool executeInEditor;
        }

        private static readonly ReadingSettings ReadSettings = new ReadingSettings() {
            InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
            NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
            NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore,
            InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid,
        };

        public static Song ReadMidi(string path, ref CallbackState callBackState)
        {
            // Initialize new song
            Song song = new Song();
            string directory = Path.GetDirectoryName(path);

            // Make message list
            var messageList = new List<MessageProcessParams>();

            foreach (Song.AudioInstrument audio in EnumX<Song.AudioInstrument>.Values)
            {
                // First try any specific filenames for the instrument, then try the instrument name
                List<string> filenamesToTry = new List<string>();

                if (c_audioStreamLocationOverrideDict.ContainsKey(audio)) {
                    filenamesToTry.AddRange(c_audioStreamLocationOverrideDict[audio]);
                }

                filenamesToTry.Add(audio.ToString());

                // Search for each combination of filenamesToTry + audio extension until we find a file
                string audioFilepath = null;

                foreach (string testFilename in filenamesToTry)
                {
                    foreach (string extension in Globals.validAudioExtensions) {
                        string testFilepath = Path.Combine(directory, testFilename.ToLower() + extension);

                        if (File.Exists(testFilepath))
                        {
                            audioFilepath = testFilepath;
                            break;
                        }
                    }
                }

                // If we didn't find a file, assign a default value to the audio path
                if (audioFilepath == null) {
                    audioFilepath = Path.Combine(directory, audio.ToString().ToLower() + ".ogg");
                }

                Debug.Log(audioFilepath);
                song.SetAudioLocation(audio, audioFilepath);
            }

            MidiFile midi;

            try
            {
                midi = MidiFile.Read(path, ReadSettings);
            }
            catch (Exception e)
            {
                throw new SystemException("Bad or corrupted midi file- " + e.Message);
            }

            if (midi.Chunks == null || midi.Chunks.Count < 1)
            {
                throw new InvalidOperationException("MIDI file has no tracks, unable to parse.");
            }

            if (!(midi.TimeDivision is TicksPerQuarterNoteTimeDivision ticks))
                throw new InvalidOperationException("MIDI file has no beat resolution set!");

            song.resolution = ticks.TicksPerQuarterNote;

            // Read all bpm data in first. This will also allow song.TimeToTick to function properly.
            ReadSync(midi.GetTempoMap(), song);

            foreach (var track in midi.GetTrackChunks())
            {
                if (track.Events.Count < 1)
                {
                    Debug.LogWarning("Encountered an empty track!");
                    continue;
                }

                if (!(track.Events[0] is SequenceTrackNameEvent trackName))
                    continue;
                Debug.Log("Found midi track " + trackName.Text);

                string trackNameKey = trackName.Text.ToUpper();
                if (c_trackExcludesMap.ContainsKey(trackNameKey))
                {
                    continue;
                }

                switch (trackNameKey)
                {
                    case MidIOHelper.EVENTS_TRACK:
                        ReadSongGlobalEvents(track, song);
                        break;

                    case MidIOHelper.VOCALS_TRACK:
                        messageList.Add(new MessageProcessParams()
                        {
                            message = "A vocals track was found in the file. Would you like to import the text events as global lyrics and phrase events?",
                            title = "Vocals Track Found",
                            executeInEditor = true,
                            currentSong = song,
                            track = track,
                            processFn = (MessageProcessParams processParams) => {
                                Debug.Log("Loading lyrics from Vocals track");
                                ReadTextEventsIntoGlobalEventsAsLyrics(track, processParams.currentSong);
                            }
                        });
                        break;

                    default:
                        Song.Instrument instrument;
                        if (!c_trackNameToInstrumentMap.TryGetValue(trackNameKey, out instrument))
                        {
                            instrument = Song.Instrument.Unrecognised;
                        }

                        if ((instrument != Song.Instrument.Unrecognised) && song.ChartExistsForInstrument(instrument))
                        {
                            messageList.Add(new MessageProcessParams()
                            {
                                message = $"A track was already loaded for instrument {instrument}, but another track was found for this instrument: {trackNameKey}\nWould you like to overwrite the currently loaded track?",
                                title = "Duplicate Instrument Track Found",
                                executeInEditor = false,
                                currentSong = song,
                                track = track,
                                processFn = (MessageProcessParams processParams) => {
                                    Debug.Log($"Overwriting already-loaded part {processParams.instrument}");
                                    foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
                                    {
                                        var chart = processParams.currentSong.GetChart(processParams.instrument, difficulty);
                                        chart.Clear();
                                        chart.UpdateCache();
                                    }

                                    ReadNotes(track, messageList, processParams.currentSong, processParams.instrument);
                                }
                            });
                        }
                        else
                        {
                            Debug.LogFormat("Loading midi track {0}", instrument);
                            ReadNotes(track, messageList, song, instrument);
                        }
                        break;
                }
            }

            // Display messages to user
            ProcessPendingUserMessages(messageList, ref callBackState);

            return song;
        }

        static void ProcessPendingUserMessages(IList<MessageProcessParams> messageList, ref CallbackState callBackState)
        {
            if (messageList == null)
            {
                Debug.Assert(false, $"No message list provided to {nameof(ProcessPendingUserMessages)}!");
                return;
            }

            foreach (var processParams in messageList)
            {
#if UNITY_EDITOR
                // The editor freezes when its message box API is used during parsing,
                // we use the params to determine whether or not to execute actions instead
                if (!processParams.executeInEditor)
                {
                    Debug.Log("Auto-skipping action for message: " + processParams.message);
                    continue;
                }
                else
                {
                    Debug.Log("Auto-executing action for message: " + processParams.message);
                    processParams.processFn(processParams);
                }
#else
                callBackState = CallbackState.WaitingForExternalInformation;
                NativeMessageBox.Result result = NativeMessageBox.Show(processParams.message, processParams.title, NativeMessageBox.Type.YesNo, null);
                callBackState = CallbackState.None;
                if (result == NativeMessageBox.Result.Yes)
                {
                    processParams.processFn(processParams);
                }
#endif
            }
        }

        private static void ReadSync(TempoMap tempoMap, Song song)
        {
            foreach (var tempo in tempoMap.GetTempoChanges())
            {
                song.Add(new BPM((uint)tempo.Time, (uint)(tempo.Value.BeatsPerMinute * 1000)), false);
            }
            foreach (var timesig in tempoMap.GetTimeSignatureChanges())
            {
                song.Add(new TimeSignature((uint)timesig.Time, (uint)timesig.Value.Numerator, (uint)timesig.Value.Denominator), false);
            }

            song.UpdateCache();
        }

        private static void ReadSongGlobalEvents(TrackChunk track, Song song)
        {
            const string rb2SectionPrefix = "[" + MidIOHelper.SECTION_PREFIX_RB2;
            const string rb3SectionPrefix = "[" + MidIOHelper.SECTION_PREFIX_RB3;

            if (track.Events.Count < 1)
                return;

            // Skip track name event
            long absoluteTick = track.Events[0].DeltaTime;
            for (int i = 1; i < track.Events.Count; i++)
            {
                var trackEvent = track.Events[i];
                absoluteTick += trackEvent.DeltaTime;

                if (trackEvent is BaseTextEvent text)
                {
                    if (text.Text.Contains(rb2SectionPrefix))
                    {
                        song.Add(new Section(text.Text.Substring(9, text.Text.Length - 10), (uint)absoluteTick), false);
                    }
                    else if (text.Text.Contains(rb3SectionPrefix) && text.Text.Length > 1)
                    {
                        string sectionText = string.Empty;
                        char lastChar = text.Text[text.Text.Length - 1];
                        if (lastChar == ']')
                        {
                            sectionText = text.Text.Substring(5, text.Text.Length - 6);
                        }
                        else if (lastChar == '"')
                        {
                            // Is in the format [prc_intro] "Intro". Strip for just the quoted section
                            int startIndex = text.Text.IndexOf('"') + 1;
                            sectionText = text.Text.Substring(startIndex, text.Text.Length - (startIndex + 1));
                        }
                        else
                        {
                            Debug.LogError("Found section name in an unknown format: " + text.Text);
                        }

                        song.Add(new Section(sectionText, (uint)absoluteTick), false);
                    }
                    else
                    {
                        song.Add(new Event(text.Text.Trim(new char[] { '[', ']' }), (uint)absoluteTick), false);
                    }
                }
            }

            song.UpdateCache();
        }

        private static void ReadTextEventsIntoGlobalEventsAsLyrics(TrackChunk track, Song song)
        {
            if (track.Events.Count < 1)
                return;

            // Skip track name event
            long absoluteTick = track.Events[0].DeltaTime;
            for (int i = 1; i < track.Events.Count; i++)
            {
                var trackEvent = track.Events[i];
                absoluteTick += trackEvent.DeltaTime;

                if (trackEvent is LyricEvent text && text.Text.Length > 0)
                {
                    string lyricEvent = MidIOHelper.LYRIC_EVENT_PREFIX + text.Text;
                    song.Add(new Event(lyricEvent, (uint)absoluteTick), false);
                }

                if (trackEvent is NoteEvent note &&
                    ((byte)note.NoteNumber == MidIOHelper.LYRICS_PHRASE_1 || (byte)note.NoteNumber == MidIOHelper.LYRICS_PHRASE_2))
                {
                    if (note.EventType == MidiEventType.NoteOn)
                        song.Add(new Event(MidIOHelper.LYRICS_PHRASE_START_TEXT, (uint)absoluteTick), false);
                    else if (note.EventType == MidiEventType.NoteOff)
                        song.Add(new Event(MidIOHelper.LYRICS_PHRASE_END_TEXT, (uint)absoluteTick), false);
                }
            }

            song.UpdateCache();
        }

        private static void ReadNotes(TrackChunk track, IList<MessageProcessParams> messageList, Song song, Song.Instrument instrument)
        {
            if (track == null || track.Events.Count < 1)
            {
                Debug.LogError($"Attempted to load null or empty track.");
                return;
            }

            Debug.Assert(messageList != null, $"No message list provided to {nameof(ReadNotes)}!");

            var unpairedNoteQueue = new NoteEventQueue();
            var unpairedSysexQueue = new SysExEventQueue();

            Chart unrecognised = new Chart(song, Song.Instrument.Unrecognised);
            Chart.GameMode gameMode = Song.InstumentToChartGameMode(instrument);

            EventProcessParams processParams = new EventProcessParams()
            {
                song = song,
                currentUnrecognisedChart = unrecognised,
                instrument = instrument,
                noteProcessMap = GetNoteProcessDict(gameMode),
                textProcessMap = GetTextEventProcessDict(gameMode),
                sysexProcessMap = GetSysExEventProcessDict(gameMode),
                forcingProcessList = new List<EventProcessFn>(),
                sysexProcessList = new List<EventProcessFn>(),
            };

            if (instrument == Song.Instrument.Unrecognised)
            {
                if (track.Events[0] is SequenceTrackNameEvent unrecognizedTrackName)
                    unrecognised.name = unrecognizedTrackName.Text;
                song.unrecognisedCharts.Add(unrecognised);
            }

            // Load all the notes
            long absoluteTick = track.Events[0].DeltaTime;
            for (int i = 1; i < track.Events.Count; i++)
            {
                var trackEvent = track.Events[i];
                absoluteTick += trackEvent.DeltaTime;

                processParams.timedEvent = new TimedMidiEvent()
                {
                    midiEvent = trackEvent,
                    startTick = absoluteTick
                };

                if (trackEvent is NoteEvent note)
                {
                    ProcessNoteEvent(ref processParams, unpairedNoteQueue, note, absoluteTick);
                }
                else if (trackEvent is BaseTextEvent text)
                {
                    ProcessTextEvent(ref processParams, text, absoluteTick);
                }
                else if (trackEvent is SysExEvent sysex)
                {
                    ProcessSysExEvent(ref processParams, unpairedSysexQueue, sysex, absoluteTick);
                }
            }

            Debug.Assert(unpairedNoteQueue.Count == 0, $"Note queue was not fully processed! Remaining event count: {unpairedNoteQueue.Count}");
            Debug.Assert(unpairedSysexQueue.Count == 0, $"SysEx event queue was not fully processed! Remaining event count: {unpairedSysexQueue.Count}");

            // Update all chart arrays
            if (instrument != Song.Instrument.Unrecognised)
            {
                foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
                    song.GetChart(instrument, diff).UpdateCache();
            }
            else
                unrecognised.UpdateCache();

            // Apply SysEx events first
            //
            // These are separate to prevent force marker issues on open notes marked via SysEx:
            // combining them or processing forcing first could result in the natural HOPO state of the note being
            // wrong, since the note hadn't been turned into an open note yet.
            //
            // This would cause the wrong flags to be applied when processing forcing, resulting in one of two things:
            // - An open note that followed a green note would ignore any forcing, since at force time it would still
            //   be green and already appear to be the correct note type, due to not being a natural HOPO.
            // - Consecutive open notes would be marked as forced and become all HOPOs, since at force time they were
            //   still green, but in-between forcing the previous and forcing this one, the previous was then marked
            //   as an open note, causing it to have the wrong force state and also causing the current note to have
            //   the wrong natural HOPO state.
            //
            // Tap marking is not affected, as that trumps all other forcing, and opens can't be marked as taps
            // (nor are they converted to HOPOs, as we don't allow consecutive HOPOs of the same fret).
            //
            // TL;DR, ensure fret changes occur *before* applying forcing, or else natural HOPO states can be wrong.
            foreach (var process in processParams.sysexProcessList)
            {
                process(processParams);
            }

            // Apply forcing events
            foreach (var process in processParams.forcingProcessList)
            {
                process(processParams);
            }

            // Legacy star power fixup
            if (track.Events[0] is SequenceTrackNameEvent trackName)
                FixupStarPowerIfNeeded(ref processParams, messageList, trackName);
        }

        static void FixupStarPowerIfNeeded(ref EventProcessParams processParams, IList<MessageProcessParams> messageList,
            SequenceTrackNameEvent trackName)
        {
            Debug.Assert(trackName != null, "Track name not given when processing legacy starpower fixups");
            if (trackName == null)
                return;

            // Check if instrument is allowed to be fixed up
            if (!c_legacyStarPowerFixupWhitelist.Contains(processParams.instrument))
                return;

            // Only need to check one difficulty since Star Power gets copied to all difficulties
            var chart = processParams.song.GetChart(processParams.instrument, Song.Difficulty.Expert);
            if (chart.starPower.Count > 0 || !ContainsTextEvent(chart.events, MidIOHelper.SOLO_EVENT_TEXT) || !ContainsTextEvent(chart.events, MidIOHelper.SOLO_END_EVENT_TEXT))
                return;

            messageList?.Add(new MessageProcessParams()
            {
                message = $"No Star Power phrases were found on track {trackName.Text}. However, solo phrases were found. These may be legacy star power phrases.\nImport these solo phrases as Star Power?",
                title = "Legacy Star Power Detected",
                executeInEditor = true,
                currentSong = processParams.song,
                instrument = processParams.instrument,
                processFn = (messageParams) => {
                    Debug.Log("Loading solo events as Star Power");
                    ProcessTextEventPairAsStarpower(
                        new EventProcessParams()
                        {
                            song = messageParams.currentSong,
                            instrument = messageParams.instrument
                        },
                        MidIOHelper.SOLO_EVENT_TEXT,
                        MidIOHelper.SOLO_END_EVENT_TEXT,
                        Starpower.Flags.None
                    );
                    // Update instrument's caches
                    foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
                        messageParams.currentSong.GetChart(messageParams.instrument, diff).UpdateCache();
                }
            });
        }

        static void ProcessNoteEvent(ref EventProcessParams processParams, NoteEventQueue unpairedNotes,
            NoteEvent note, long absoluteTick)
        {
            if (note.EventType == MidiEventType.NoteOn)
            {
                // Check for duplicates
                if (TryFindMatchingNote(unpairedNotes, note, out _, out _, out _))
                    Debug.LogWarning($"Found duplicate note on at tick {absoluteTick}!");
                else
                    unpairedNotes.Add((note, absoluteTick));
            }
            else if (note.EventType == MidiEventType.NoteOff)
            {
                if (!TryFindMatchingNote(unpairedNotes, note, out var noteStart, out long startTick, out int startIndex))
                {
                    Debug.LogWarning($"Found note off with no corresponding note on at tick {absoluteTick}!");
                    return;
                }
                unpairedNotes.RemoveAt(startIndex);

                if (processParams.instrument == Song.Instrument.Unrecognised)
                {
                    var tick = (uint)absoluteTick;
                    var sus = SanitiseSustainLength(processParams.song, (uint)(absoluteTick - startTick));

                    int rawNote = noteStart.NoteNumber;
                    Note newNote = new Note(tick, rawNote, sus);
                    processParams.currentUnrecognisedChart.Add(newNote);
                    return;
                }

                processParams.timedEvent.midiEvent = noteStart;
                processParams.timedEvent.startTick = startTick;
                processParams.timedEvent.endTick = absoluteTick;

                if (processParams.noteProcessMap.TryGetValue(noteStart.NoteNumber, out var processFn))
                {
                    processFn(processParams);
                }
            }
        }

        static void ProcessTextEvent(ref EventProcessParams processParams, BaseTextEvent text, long absoluteTick)
        {
            var tick = (uint)absoluteTick;
            var eventName = text.Text;

            ChartEvent chartEvent = new ChartEvent(tick, eventName);

            if (processParams.instrument == Song.Instrument.Unrecognised)
            {
                processParams.currentUnrecognisedChart.Add(chartEvent);
            }
            else
            {
                if (processParams.textProcessMap.TryGetValue(eventName, out var processFn))
                {
                    // This text event affects parsing of the .mid file, run its function and don't parse it into the chart
                    processFn(ref processParams);
                }
                else
                {
                    // Copy text event to all difficulties so that .chart format can store these properly. Midi writer will strip duplicate events just fine anyway.
                    foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
                    {
                        processParams.song.GetChart(processParams.instrument, difficulty).Add(chartEvent);
                    }
                }
            }
        }

        static void ProcessSysExEvent(ref EventProcessParams processParams, SysExEventQueue unpairedSysex,
            SysExEvent sysex, long absoluteTick)
        {
            if (!PhaseShiftSysEx.TryParse(sysex, out var psEvent))
            {
                // SysEx event is not a Phase Shift SysEx event
                Debug.LogWarning($"Encountered unknown SysEx event: {BitConverter.ToString(sysex.Data)}");
                return;
            }

            if (psEvent.type != MidIOHelper.SYSEX_TYPE_PHRASE)
            {
                Debug.LogWarning($"Encountered unknown Phase Shift SysEx event type {psEvent.type}");
                return;
            }

            if (psEvent.value == MidIOHelper.SYSEX_VALUE_PHRASE_START)
            {
                // Check for duplicates
                if (TryFindMatchingSysEx(unpairedSysex, psEvent, out _, out _, out _))
                    Debug.LogWarning($"Found duplicate SysEx start event at tick {absoluteTick}!");
                else
                    unpairedSysex.Add((psEvent, absoluteTick));
            }
            else if (psEvent.value == MidIOHelper.SYSEX_VALUE_PHRASE_END)
            {
                if (!TryFindMatchingSysEx(unpairedSysex, psEvent, out var sysexStart, out long startTick, out int startIndex))
                {
                    Debug.LogWarning($"Found PS SysEx end with no corresponding start at tick {absoluteTick}!");
                    return;
                }
                unpairedSysex.RemoveAt(startIndex);

                processParams.timedEvent.midiEvent = sysexStart;
                processParams.timedEvent.startTick = startTick;
                processParams.timedEvent.endTick = absoluteTick;

                if (processParams.sysexProcessMap.TryGetValue(psEvent.code, out var processFn))
                {
                    processFn(processParams);
                }
            }
        }

        static bool TryFindMatchingNote(NoteEventQueue unpairedNotes, NoteEvent noteToMatch,
            out NoteEvent matchingNote, out long matchTick, out int matchIndex)
        {
            for (int i = 0; i < unpairedNotes.Count; i++)
            {
                var queued = unpairedNotes[i];
                if (queued.note.NoteNumber == noteToMatch.NoteNumber && queued.note.Channel == noteToMatch.Channel)
                {
                    (matchingNote, matchTick) = queued;
                    matchIndex = i;
                    return true;
                }
            }

            matchingNote = null;
            matchTick = -1;
            matchIndex = -1;
            return false;
        }

        static bool TryFindMatchingSysEx(SysExEventQueue unpairedSysex, PhaseShiftSysEx sysexToMatch,
            out PhaseShiftSysEx matchingSysex, out long matchTick, out int matchIndex)
        {
            for (int i = 0; i < unpairedSysex.Count; i++)
            {
                var queued = unpairedSysex[i];
                if (queued.sysex.MatchesWith(sysexToMatch))
                {
                    (matchingSysex, matchTick) = queued;
                    matchIndex = i;
                    return true;
                }
            }

            matchingSysex = null;
            matchTick = -1;
            matchIndex = -1;
            return false;
        }

        static bool ContainsTextEvent(IList<ChartEvent> events, string text)
        {
            foreach (var textEvent in events)
            {
                if (textEvent.eventName == text)
                {
                    return true;
                }
            }

            return false;
        }

        static IReadOnlyDictionary<int, EventProcessFn> GetNoteProcessDict(Chart.GameMode gameMode)
        {
            switch (gameMode)
            {
                case Chart.GameMode.GHLGuitar:
                    {
                        return GhlGuitarMidiNoteNumberToProcessFnMap;
                    }
                case Chart.GameMode.Drums:
                    {
                        return DrumsMidiNoteNumberToProcessFnMap;
                    }

                default: break;
            }

            return GuitarMidiNoteNumberToProcessFnMap;
        }

        static IReadOnlyDictionary<string, ProcessModificationProcessFn> GetTextEventProcessDict(Chart.GameMode gameMode)
        {
            switch (gameMode)
            {
                case Chart.GameMode.GHLGuitar:
                    return GhlGuitarTextEventToProcessFnMap;
                case Chart.GameMode.Drums:
                    return DrumsTextEventToProcessFnMap;

                default:
                    return GuitarTextEventToProcessFnMap;
            }
        }

        static IReadOnlyDictionary<byte, EventProcessFn> GetSysExEventProcessDict(Chart.GameMode gameMode)
        {
            switch (gameMode)
            {
                case Chart.GameMode.Guitar:
                    return GuitarSysExEventToProcessFnMap;
                case Chart.GameMode.GHLGuitar:
                    return GhlGuitarSysExEventToProcessFnMap;
                case Chart.GameMode.Drums:
                    return DrumsSysExEventToProcessFnMap;

                // Don't process any SysEx events on unrecognized tracks
                default:
                    return new Dictionary<byte, EventProcessFn>();
            }
        }

        static void SwitchToGuitarEnhancedOpensProcessMap(ref EventProcessParams processParams)
        {
            var gameMode = Song.InstumentToChartGameMode(processParams.instrument);
            if (gameMode != Chart.GameMode.Guitar)
            {
                Debug.LogWarning($"Attempted to apply guitar enhanced opens process map to non-guitar instrument: {processParams.instrument}");
                return;
            }

            // Switch process map to guitar enhanced opens process map
            processParams.noteProcessMap = GuitarMidiNoteNumberToProcessFnMap_EnhancedOpens;
        }

        static void SwitchToDrumsVelocityProcessMap(ref EventProcessParams processParams)
        {
            if (processParams.instrument != Song.Instrument.Drums)
            {
                Debug.LogWarning($"Attempted to apply drums velocity process map to non-drums instrument: {processParams.instrument}");
                return;
            }

            // Switch process map to drums velocity process map
            processParams.noteProcessMap = DrumsMidiNoteNumberToProcessFnMap_Velocity;
        }

        static IReadOnlyDictionary<int, EventProcessFn> BuildGuitarMidiNoteNumberToProcessFnDict(bool enhancedOpens = false)
        {
            var processFnDict = new Dictionary<int, EventProcessFn>()
            {
                { MidIOHelper.STARPOWER_NOTE, ProcessTimedEventAsStarpower },
                { MidIOHelper.TAP_NOTE_CH, (in EventProcessParams eventProcessParams) => {
                    ProcessTimedEventAsForcedType(eventProcessParams, Note.NoteType.Tap);
                }},
                { MidIOHelper.SOLO_NOTE, (in EventProcessParams eventProcessParams) => {
                    ProcessTimedEventAsEvent(eventProcessParams, MidIOHelper.SOLO_EVENT_TEXT, 0, MidIOHelper.SOLO_END_EVENT_TEXT, SOLO_END_CORRECTION_OFFSET);
                }},
            };

            var FretToMidiKey = new Dictionary<Note.GuitarFret, int>()
            {
                { Note.GuitarFret.Green, 0 },
                { Note.GuitarFret.Red, 1 },
                { Note.GuitarFret.Yellow, 2 },
                { Note.GuitarFret.Blue, 3 },
                { Note.GuitarFret.Orange, 4 },
            };

            if (enhancedOpens)
                FretToMidiKey.Add(Note.GuitarFret.Open, -1);

            foreach (var difficulty in EnumX<Song.Difficulty>.Values)
            {
                int difficultyStartRange = MidIOHelper.GUITAR_DIFF_START_LOOKUP[difficulty];
                foreach (var guitarFret in EnumX<Note.GuitarFret>.Values)
                {
                    int fretOffset;
                    if (FretToMidiKey.TryGetValue(guitarFret, out fretOffset))
                    {
                        int key = fretOffset + difficultyStartRange;
                        int fret = (int)guitarFret;

                        processFnDict.Add(key, (in EventProcessParams eventProcessParams) =>
                        {
                            ProcessTimedEventAsNote(eventProcessParams, difficulty, fret);
                        });
                    }
                }

                // Process forced hopo or forced strum
                {
                    int flagKey = difficultyStartRange + 5;
                    processFnDict.Add(flagKey, (in EventProcessParams eventProcessParams) =>
                    {
                        ProcessTimedEventAsForcedType(eventProcessParams, difficulty, Note.NoteType.Hopo);
                    });
                }
                {
                    int flagKey = difficultyStartRange + 6;
                    processFnDict.Add(flagKey, (in EventProcessParams eventProcessParams) =>
                    {
                        ProcessTimedEventAsForcedType(eventProcessParams, difficulty, Note.NoteType.Strum);
                    });
                }
            };

            return processFnDict;
        }

        static IReadOnlyDictionary<int, EventProcessFn> BuildGhlGuitarMidiNoteNumberToProcessFnDict()
        {
            var processFnDict = new Dictionary<int, EventProcessFn>()
            {
                { MidIOHelper.STARPOWER_NOTE, ProcessTimedEventAsStarpower },
                { MidIOHelper.TAP_NOTE_CH, (in EventProcessParams eventProcessParams) => {
                    ProcessTimedEventAsForcedType(eventProcessParams, Note.NoteType.Tap);
                }},
                { MidIOHelper.SOLO_NOTE, (in EventProcessParams eventProcessParams) => {
                    ProcessTimedEventAsEvent(eventProcessParams, MidIOHelper.SOLO_EVENT_TEXT, 0, MidIOHelper.SOLO_END_EVENT_TEXT, SOLO_END_CORRECTION_OFFSET);
                }},
            };

            IReadOnlyDictionary<Note.GHLiveGuitarFret, int> FretToMidiKey = new Dictionary<Note.GHLiveGuitarFret, int>()
            {
                { Note.GHLiveGuitarFret.Open, 0 },
                { Note.GHLiveGuitarFret.White1, 1 },
                { Note.GHLiveGuitarFret.White2, 2 },
                { Note.GHLiveGuitarFret.White3, 3 },
                { Note.GHLiveGuitarFret.Black1, 4 },
                { Note.GHLiveGuitarFret.Black2, 5 },
                { Note.GHLiveGuitarFret.Black3, 6 },
            };

            foreach (var difficulty in EnumX<Song.Difficulty>.Values)
            {
                int difficultyStartRange = MidIOHelper.GHL_GUITAR_DIFF_START_LOOKUP[difficulty];
                foreach (var guitarFret in EnumX<Note.GHLiveGuitarFret>.Values)
                {
                    int fretOffset;
                    if (FretToMidiKey.TryGetValue(guitarFret, out fretOffset))
                    {
                        int key = fretOffset + difficultyStartRange;
                        int fret = (int)guitarFret;

                        processFnDict.Add(key, (in EventProcessParams eventProcessParams) =>
                        {
                            ProcessTimedEventAsNote(eventProcessParams, difficulty, fret);
                        });
                    }
                }

                // Process forced hopo or forced strum
                {
                    int flagKey = difficultyStartRange + 7;
                    processFnDict.Add(flagKey, (in EventProcessParams eventProcessParams) =>
                    {
                        ProcessTimedEventAsForcedType(eventProcessParams, difficulty, Note.NoteType.Hopo);
                    });
                }
                {
                    int flagKey = difficultyStartRange + 8;
                    processFnDict.Add(flagKey, (in EventProcessParams eventProcessParams) =>
                    {
                        ProcessTimedEventAsForcedType(eventProcessParams, difficulty, Note.NoteType.Strum);
                    });
                }
            };

            return processFnDict;
        }

        static IReadOnlyDictionary<int, EventProcessFn> BuildDrumsMidiNoteNumberToProcessFnDict(bool enableVelocity = false)
        {
            var processFnDict = new Dictionary<int, EventProcessFn>()
            {
                { MidIOHelper.STARPOWER_NOTE, ProcessTimedEventAsStarpower },
                { MidIOHelper.SOLO_NOTE, (in EventProcessParams eventProcessParams) => {
                    ProcessTimedEventAsEvent(eventProcessParams, MidIOHelper.SOLO_EVENT_TEXT, 0, MidIOHelper.SOLO_END_EVENT_TEXT, SOLO_END_CORRECTION_OFFSET);
                }},
                { MidIOHelper.DOUBLE_KICK_NOTE, (in EventProcessParams eventProcessParams) => {
                    ProcessTimedEventAsNote(eventProcessParams, Song.Difficulty.Expert, (int)Note.DrumPad.Kick, Note.Flags.InstrumentPlus);
                }},

                { MidIOHelper.STARPOWER_DRUM_FILL_0, ProcessTimedEventAsDrumFill },
                { MidIOHelper.STARPOWER_DRUM_FILL_1, ProcessTimedEventAsDrumFill },
                { MidIOHelper.STARPOWER_DRUM_FILL_2, ProcessTimedEventAsDrumFill },
                { MidIOHelper.STARPOWER_DRUM_FILL_3, ProcessTimedEventAsDrumFill },
                { MidIOHelper.STARPOWER_DRUM_FILL_4, ProcessTimedEventAsDrumFill },
                { MidIOHelper.DRUM_ROLL_STANDARD, (in EventProcessParams eventProcessParams) => {
                    ProcessTimedEventAsDrumRoll(eventProcessParams, DrumRoll.Type.Standard);
                }},
                { MidIOHelper.DRUM_ROLL_SPECIAL, (in EventProcessParams eventProcessParams) => {
                    ProcessTimedEventAsDrumRoll(eventProcessParams, DrumRoll.Type.Special);
                }},
            };

            IReadOnlyDictionary<Note.DrumPad, int> DrumPadToMidiKey = new Dictionary<Note.DrumPad, int>()
            {
                { Note.DrumPad.Kick, 0 },
                { Note.DrumPad.Red, 1 },
                { Note.DrumPad.Yellow, 2 },
                { Note.DrumPad.Blue, 3 },
                { Note.DrumPad.Orange, 4 },
                { Note.DrumPad.Green, 5 },
            };

            IReadOnlyDictionary<Note.DrumPad, Note.Flags> DrumPadDefaultFlags = new Dictionary<Note.DrumPad, Note.Flags>()
            {
                { Note.DrumPad.Yellow, Note.Flags.ProDrums_Cymbal },
                { Note.DrumPad.Blue, Note.Flags.ProDrums_Cymbal },
                { Note.DrumPad.Orange, Note.Flags.ProDrums_Cymbal },
            };

            foreach (var difficulty in EnumX<Song.Difficulty>.Values)
            {
                int difficultyStartRange = MidIOHelper.DRUMS_DIFF_START_LOOKUP[difficulty];
                foreach (var pad in EnumX<Note.DrumPad>.Values)
                {
                    int padOffset;
                    if (DrumPadToMidiKey.TryGetValue(pad, out padOffset))
                    {
                        int key = padOffset + difficultyStartRange;
                        int fret = (int)pad;
                        Note.Flags defaultFlags = Note.Flags.None;
                        DrumPadDefaultFlags.TryGetValue(pad, out defaultFlags);

                        if (enableVelocity && pad != Note.DrumPad.Kick)
                        {
                            processFnDict.Add(key, (in EventProcessParams eventProcessParams) =>
                            {
                                var noteEvent = eventProcessParams.timedEvent.midiEvent as NoteEvent;
                                Debug.Assert(noteEvent != null, $"Wrong note event type passed to drums note process. Expected: {typeof(NoteEvent)}, Actual: {eventProcessParams.timedEvent.midiEvent.GetType()}");

                                var flags = defaultFlags;
                                switch (noteEvent.Velocity)
                                {
                                    case MidIOHelper.VELOCITY_ACCENT:
                                        {
                                            flags |= Note.Flags.ProDrums_Accent;
                                            break;
                                        }
                                    case MidIOHelper.VELOCITY_GHOST:
                                        {
                                            flags |= Note.Flags.ProDrums_Ghost;
                                            break;
                                        }
                                    default: break;
                                }

                                ProcessTimedEventAsNote(eventProcessParams, difficulty, fret, flags);
                            });
                        }
                        else
                        {
                            processFnDict.Add(key, (in EventProcessParams eventProcessParams) =>
                            {
                                ProcessTimedEventAsNote(eventProcessParams, difficulty, fret, defaultFlags);
                            });
                        }
                    }
                }
            };

            foreach (var keyVal in MidIOHelper.PAD_TO_CYMBAL_LOOKUP)
            {
                int pad = (int)keyVal.Key;
                int midiKey = keyVal.Value;

                processFnDict.Add(midiKey, (in EventProcessParams eventProcessParams) =>
                {
                    ProcessTimedEventAsFlagToggle(eventProcessParams, Note.Flags.ProDrums_Cymbal, pad);
                });
            }

            return processFnDict;
        }

        static void ProcessTimedEventAsNote(in EventProcessParams eventProcessParams, Song.Difficulty diff, int ingameFret, Note.Flags defaultFlags = Note.Flags.None)
        {
            Chart chart;
            if (eventProcessParams.instrument == Song.Instrument.Unrecognised)
            {
                chart = eventProcessParams.currentUnrecognisedChart;
            }
            else
            {
                chart = eventProcessParams.song.GetChart(eventProcessParams.instrument, diff);
            }

            var timedEvent = eventProcessParams.timedEvent;
            uint tick = (uint)timedEvent.startTick;
            uint sus = SanitiseSustainLength(eventProcessParams.song, (uint)timedEvent.length);

            Note newNote = new Note(tick, ingameFret, sus, defaultFlags);
            chart.Add(newNote, false);
        }

        static void ProcessTimedEventAsStarpower(in EventProcessParams eventProcessParams)
        {
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            var timedEvent = eventProcessParams.timedEvent;
            uint tick = (uint)timedEvent.startTick;
            uint sus = SanitiseSustainLength(eventProcessParams.song, (uint)timedEvent.length);

            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                song.GetChart(instrument, diff).Add(new Starpower(tick, sus), false);
            }
        }

        static void ProcessTimedEventAsDrumFill(in EventProcessParams eventProcessParams)
        {
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            var timedEvent = eventProcessParams.timedEvent;
            uint tick = (uint)timedEvent.startTick;
            uint sus = SanitiseSustainLength(eventProcessParams.song, (uint)timedEvent.length);

            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                song.GetChart(instrument, diff).Add(new Starpower(tick, sus, Starpower.Flags.ProDrums_Activation), false);
            }
        }

        static void ProcessTimedEventAsDrumRoll(in EventProcessParams eventProcessParams, DrumRoll.Type type)
        {
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            var timedEvent = eventProcessParams.timedEvent;
            uint tick = (uint)timedEvent.startTick;
            uint sus = SanitiseSustainLength(eventProcessParams.song, (uint)timedEvent.length);

            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                song.GetChart(instrument, diff).Add(new DrumRoll(tick, sus, type), false);
            }
        }

        static void ProcessTimedEventAsForcedType(in EventProcessParams eventProcessParams, Note.NoteType noteType)
        {
            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                ProcessTimedEventAsForcedType(eventProcessParams, diff, noteType);
            }
        }

        static void ProcessTimedEventAsForcedType(in EventProcessParams eventProcessParams, Song.Difficulty difficulty, Note.NoteType noteType)
        {
            var timedEvent = eventProcessParams.timedEvent;
            uint startTick = (uint)timedEvent.startTick;
            uint endTick = (uint)timedEvent.endTick;
            // Exclude the last tick of the phrase
            if (endTick > startTick)
                --endTick;

            // Delay the actual processing once all the notes are actually in
            eventProcessParams.forcingProcessList.Add((in EventProcessParams processParams) =>
            {
                ProcessEventAsForcedTypePostDelay(processParams, startTick, endTick, difficulty, noteType);
            });
        }

        static void ProcessEventAsForcedTypePostDelay(in EventProcessParams eventProcessParams, uint startTick, uint endTick, Song.Difficulty difficulty, Note.NoteType noteType)
        {
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            Chart chart;
            if (instrument != Song.Instrument.Unrecognised)
                chart = song.GetChart(instrument, difficulty);
            else
                chart = eventProcessParams.currentUnrecognisedChart;

            int index, length;
            SongObjectHelper.GetRange(chart.notes, startTick, endTick, out index, out length);

            uint lastChordTick = uint.MaxValue;
            bool expectedForceFailure = true; // Whether or not it is expected that the actual type will not match the expected type
            bool shouldBeForced = false;

            for (int i = index; i < index + length; ++i)
            {
                // Tap marking overrides all other forcing
                if ((chart.notes[i].flags & Note.Flags.Tap) != 0)
                    continue;

                Note note = chart.notes[i];

                // Check if the chord has changed
                if (lastChordTick != note.tick)
                {
                    expectedForceFailure = false;
                    shouldBeForced = false;

                    switch (noteType)
                    {
                        case (Note.NoteType.Strum):
                        {
                            if (!note.isChord && note.isNaturalHopo)
                            {
                                shouldBeForced = true;
                            }
                            break;
                        }

                        case (Note.NoteType.Hopo):
                        {
                            // Forcing consecutive same-fret HOPOs is possible in charts, but we do not allow it
                            // (see RB2's chart of Steely Dan - Bodhisattva)
                            if (!note.isNaturalHopo && note.cannotBeForced)
                            {
                                expectedForceFailure = true;
                            }

                            if (!note.cannotBeForced && (note.isChord || !note.isNaturalHopo))
                            {
                                shouldBeForced = true;
                            }
                            break;
                        }

                        case (Note.NoteType.Tap):
                        {
                            if (!note.IsOpenNote())
                            {
                                note.flags |= Note.Flags.Tap;
                                // Forced flag will be removed shortly after here
                            }
                            else
                            {
                                // Open notes cannot become taps
                                // CH handles this by turning them into open HOPOs, we'll do the same here for consistency with them
                                expectedForceFailure = true;
                                // In the case that consecutive open notes are marked as taps, only the first will become a HOPO
                                if (!note.cannotBeForced && !note.isNaturalHopo)
                                {
                                    shouldBeForced = true;
                                }
                            }
                            break;
                        }

                        default:
                            Debug.Assert(false, $"Unhandled note type {noteType} in .mid forced type processing");
                            continue; // Unhandled
                    }

                    if (shouldBeForced)
                    {
                        note.flags |= Note.Flags.Forced;
                    }
                    else
                    {
                        note.flags &= ~Note.Flags.Forced;
                    }

                    note.ApplyFlagsToChord();
                }

                lastChordTick = note.tick;

                Debug.Assert(note.type == noteType || expectedForceFailure, $"Failed to set forced type! Expected: {noteType}  Actual: {note.type}  Natural HOPO: {note.isNaturalHopo}  Chord: {note.isChord}  Forceable: {!note.cannotBeForced}\non {difficulty} {instrument} at tick {note.tick} ({TimeSpan.FromSeconds(note.time):mm':'ss'.'ff})");
            }
        }

        static uint SanitiseSustainLength(Song song, uint length)
        {
            // Apply sustain cutoff (notes less than 1/12th note in length will not become sustains)
            int susCutoff = (int)(SongConfig.MIDI_SUSTAIN_CUTOFF_THRESHOLD * song.resolution / SongConfig.STANDARD_BEAT_RESOLUTION);
            if (length <= susCutoff)
                length = 0;

            return length;
        }

        static void ProcessTimedEventAsEvent(EventProcessParams eventProcessParams, string eventStartText, int tickStartOffset, string eventEndText, int tickEndOffset)
        {
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            var timedEvent = eventProcessParams.timedEvent;
            uint tick = (uint)timedEvent.startTick;
            uint sus = SanitiseSustainLength(eventProcessParams.song, (uint)timedEvent.length);
            if (sus >= tickEndOffset)
            {
                sus = (uint)(sus + tickEndOffset);
            }

            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                Chart chart = song.GetChart(instrument, diff);
                chart.Add(new ChartEvent(tick, eventStartText));
                chart.Add(new ChartEvent(tick + sus, eventEndText));
            }
        }

        static void ProcessTimedEventAsFlagToggle(in EventProcessParams eventProcessParams, Note.Flags flags, int individualNoteSpecifier)
        {
            var timedEvent = eventProcessParams.timedEvent;
            uint startTick = (uint)timedEvent.startTick;
            uint endTick = (uint)timedEvent.endTick;
            // Exclude the last tick of the phrase
            if (endTick > startTick)
                --endTick;

            // Delay the actual processing once all the notes are actually in
            eventProcessParams.forcingProcessList.Add((in EventProcessParams processParams) =>
            {
                ProcessTimedEventAsFlagTogglePostDelay(processParams, startTick, endTick, flags, individualNoteSpecifier);
            });
        }

        static void ProcessTimedEventAsFlagTogglePostDelay(in EventProcessParams eventProcessParams, uint startTick, uint endTick, Note.Flags flags, int individualNoteSpecifier)   // individualNoteSpecifier as -1 to apply to the whole chord
        {
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
            {
                Chart chart = song.GetChart(instrument, difficulty);

                int index, length;
                SongObjectHelper.GetRange(chart.notes, startTick, endTick, out index, out length);

                for (int i = index; i < index + length; ++i)
                {
                    Note note = chart.notes[i];

                    if (individualNoteSpecifier < 0 || note.rawNote == individualNoteSpecifier)
                    {
                        // Toggle flag
                        note.flags ^= flags;
                    }
                }
            }
        }

        static void ProcessTextEventPairAsStarpower(in EventProcessParams eventProcessParams, string startText, string endText, Starpower.Flags flags)
        {
            foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
            {
                var song = eventProcessParams.song;
                var instrument = eventProcessParams.instrument;
                var chart = song.GetChart(instrument, difficulty);

                // Convert start and end events into phrases
                uint? currentStartTick = null;
                for (int i = 0; i < chart.events.Count; ++i)
                {
                    var textEvent = chart.events[i];
                    if (textEvent.eventName == startText)
                    {
                        // Remove text event
                        chart.Remove(textEvent, false);

                        uint startTick = textEvent.tick;
                        // Only one start event can be active at a time
                        if (currentStartTick != null)
                        {
                            Debug.LogError($"A previous start event at tick {currentStartTick.Value} is interrupted by another start event at tick {startTick}!");
                            continue;
                        }

                        currentStartTick = startTick;
                    }
                    else if (textEvent.eventName == endText)
                    {
                        // Remove text event
                        chart.Remove(textEvent, false);

                        uint endTick = textEvent.tick;
                        // Events must pair up
                        if (currentStartTick == null)
                        {
                            Debug.LogError($"End event at tick {endTick} does not have a corresponding start event!");
                            continue;
                        }

                        uint startTick = currentStartTick.GetValueOrDefault();
                        // Current start must occur before the current end
                        if (currentStartTick > textEvent.tick)
                        {
                            Debug.LogError($"Start event at tick {endTick} occurs before end event at {endTick}!");
                            continue;
                        }

                        chart.Add(new Starpower(startTick, endTick - startTick), false);
                        currentStartTick = null;
                    }
                }
            }
        }

        static void ProcessSysExEventPairAsForcedType(in EventProcessParams eventProcessParams, Note.NoteType noteType)
        {
            var timedEvent = eventProcessParams.timedEvent;
            var startEvent = eventProcessParams.timedEvent.midiEvent as PhaseShiftSysEx;
            Debug.Assert(startEvent != null, $"Wrong note event type passed to {nameof(ProcessSysExEventPairAsForcedType)}. Expected: {typeof(PhaseShiftSysEx)}, Actual: {eventProcessParams.timedEvent.midiEvent.GetType()}");

            uint startTick = (uint)timedEvent.startTick;
            uint endTick = (uint)timedEvent.endTick;
            // Exclude the last tick of the phrase
            if (endTick > startTick)
                --endTick;

            if (startEvent.difficulty == MidIOHelper.SYSEX_DIFFICULTY_ALL)
            {
                foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
                {
                    eventProcessParams.sysexProcessList.Add((in EventProcessParams processParams) =>
                    {
                        ProcessEventAsForcedTypePostDelay(processParams, startTick, endTick, diff, noteType);
                    });
                }
            }
            else
            {
                var diff = MidIOHelper.SYSEX_TO_MS_DIFF_LOOKUP[startEvent.difficulty];
                eventProcessParams.sysexProcessList.Add((in EventProcessParams processParams) =>
                {
                    ProcessEventAsForcedTypePostDelay(processParams, startTick, endTick, diff, noteType);
                });
            }
        }

        static void ProcessSysExEventPairAsOpenNoteModifier(in EventProcessParams eventProcessParams)
        {
            var timedEvent = eventProcessParams.timedEvent;
            var startEvent = timedEvent.midiEvent as PhaseShiftSysEx;
            Debug.Assert(startEvent != null, $"Wrong note event type passed to {nameof(ProcessSysExEventPairAsOpenNoteModifier)}. Expected: {typeof(PhaseShiftSysEx)}, Actual: {eventProcessParams.timedEvent.midiEvent.GetType()}");

            uint startTick = (uint)timedEvent.startTick;
            uint endTick = (uint)timedEvent.endTick;
            // Exclude the last tick of the phrase
            if (endTick > startTick)
                --endTick;

            if (startEvent.difficulty == MidIOHelper.SYSEX_DIFFICULTY_ALL)
            {
                foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
                {
                    eventProcessParams.sysexProcessList.Add((in EventProcessParams processParams) =>
                    {
                        ProcessEventAsOpenNoteModifierPostDelay(processParams, startTick, endTick, diff);
                    });
                }
            }
            else
            {
                var diff = MidIOHelper.SYSEX_TO_MS_DIFF_LOOKUP[startEvent.difficulty];
                eventProcessParams.sysexProcessList.Add((in EventProcessParams processParams) =>
                {
                    ProcessEventAsOpenNoteModifierPostDelay(processParams, startTick, endTick, diff);
                });
            }
        }

        static void ProcessEventAsOpenNoteModifierPostDelay(in EventProcessParams processParams, uint startTick, uint endTick, Song.Difficulty difficulty)
        {
            var instrument = processParams.instrument;
            var gameMode = Song.InstumentToChartGameMode(instrument);
            var song = processParams.song;

            Chart chart;
            if (instrument == Song.Instrument.Unrecognised)
                chart = processParams.currentUnrecognisedChart;
            else
                chart = song.GetChart(instrument, difficulty);

            int index, length;
            SongObjectHelper.GetRange(chart.notes, startTick, endTick, out index, out length);
            for (int i = index; i < index + length; ++i)
            {
                switch (gameMode)
                {
                    case (Chart.GameMode.Guitar):
                        chart.notes[i].guitarFret = Note.GuitarFret.Open;
                        break;

                    // Usually not used, but in the case that it is it should work properly
                    case (Chart.GameMode.GHLGuitar):
                        chart.notes[i].ghliveGuitarFret = Note.GHLiveGuitarFret.Open;
                        break;

                    default:
                        Debug.Assert(false, $"Unhandled game mode for open note modifier: {gameMode} (instrument: {instrument})");
                        break;
                }
            }

            chart.UpdateCache();
        }
    }
}
