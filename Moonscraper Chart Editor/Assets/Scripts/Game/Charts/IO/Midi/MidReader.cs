﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;
using MoonscraperEngine;

namespace MoonscraperChartEditor.Song.IO
{
    public static class MidReader
    {

        public enum CallbackState
        {
            None,
            WaitingForExternalInformation,
        }

        static readonly Dictionary<string, Song.Instrument> c_trackNameToInstrumentMap = new Dictionary<string, Song.Instrument>()
    {
        { MidIOHelper.GUITAR_TRACK,        Song.Instrument.Guitar },
        { MidIOHelper.GUITAR_COOP_TRACK,   Song.Instrument.GuitarCoop },
        { MidIOHelper.BASS_TRACK,          Song.Instrument.Bass },
        { MidIOHelper.RHYTHM_TRACK,        Song.Instrument.Rhythm },
        { MidIOHelper.KEYS_TRACK,          Song.Instrument.Keys },
        { MidIOHelper.DRUMS_TRACK,         Song.Instrument.Drums },
        { MidIOHelper.GHL_GUITAR_TRACK,    Song.Instrument.GHLiveGuitar },
        { MidIOHelper.GHL_BASS_TRACK,      Song.Instrument.GHLiveBass },
    };

        static readonly Dictionary<string, bool> c_trackExcludesMap = new Dictionary<string, bool>()
    {
        { "t1 gems",    true },
        { "beat",       true }
    };

        static readonly Dictionary<Song.AudioInstrument, string[]> c_audioStreamLocationOverrideDict = new Dictionary<Song.AudioInstrument, string[]>()
    {
        // String list is ordered in priority. If it finds a file names with the first string it'll skip over the rest.
        // Otherwise just does a ToString on the AudioInstrument enum
        { Song.AudioInstrument.Drum, new string[] { "drums", "drums_1" } },
    };

        struct EventProcessParams
        {
            public Song song;
            public Song.Instrument instrument;
            public Chart currentUnrecognisedChart;
            public MidiEvent midiEvent;
            public List<EventProcessFn> delayedProcessesList;
        }

        delegate void EventProcessFn(in EventProcessParams eventProcessParams);

        // These dictionaries map the NoteNumber of each midi note event to a specific function of how to process them
        static readonly Dictionary<int, EventProcessFn> GuitarMidiNoteNumberToProcessFnMap = new Dictionary<int, EventProcessFn>()
    {
        { MidIOHelper.STARPOWER_NOTE, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsStarpower(eventProcessParams);
        }},
        { MidIOHelper.SOLO_NOTE, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsEvent(eventProcessParams, MidIOHelper.SoloEventText, MidIOHelper.SoloEndEventText);
        }},
    };

        static readonly Dictionary<int, EventProcessFn> GhlGuitarMidiNoteNumberToProcessFnMap = new Dictionary<int, EventProcessFn>()
    {
        { MidIOHelper.STARPOWER_NOTE, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsStarpower(eventProcessParams);
        }},
        { MidIOHelper.SOLO_NOTE, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsEvent(eventProcessParams, MidIOHelper.SoloEventText, MidIOHelper.SoloEndEventText);
        }},
    };

        static readonly Dictionary<int, EventProcessFn> DrumsMidiNoteNumberToProcessFnMap = new Dictionary<int, EventProcessFn>()
    {
        { MidIOHelper.STARPOWER_NOTE, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsStarpower(eventProcessParams);
        }},
        { MidIOHelper.SOLO_NOTE, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsEvent(eventProcessParams, MidIOHelper.SoloEventText, MidIOHelper.SoloEndEventText);
        }},
        { MidIOHelper.DOUBLE_KICK_NOTE, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsNote(eventProcessParams, Song.Difficulty.Expert, (int)Note.DrumPad.Kick, Note.Flags.InstrumentPlus);
        }},

        { MidIOHelper.STARPOWER_DRUM_FILL_0, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsStarpower(eventProcessParams, Starpower.Flags.ProDrums_Activation);
        }},
        { MidIOHelper.STARPOWER_DRUM_FILL_1, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsStarpower(eventProcessParams, Starpower.Flags.ProDrums_Activation);
        }},
        { MidIOHelper.STARPOWER_DRUM_FILL_2, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsStarpower(eventProcessParams, Starpower.Flags.ProDrums_Activation);
        }},
        { MidIOHelper.STARPOWER_DRUM_FILL_3, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsStarpower(eventProcessParams, Starpower.Flags.ProDrums_Activation);
        }},
        { MidIOHelper.STARPOWER_DRUM_FILL_4, (in EventProcessParams eventProcessParams) => {
            ProcessNoteOnEventAsStarpower(eventProcessParams, Starpower.Flags.ProDrums_Activation);
        }},
    };

        // These dictionaries map the text of a MIDI text event to a specific function that processes them
        static readonly Dictionary<string, EventProcessFn> GuitarTextEventToProcessFnMap = new Dictionary<string, EventProcessFn>()
    {
    };

        static readonly Dictionary<string, EventProcessFn> GhlGuitarTextEventToProcessFnMap = new Dictionary<string, EventProcessFn>()
    {
    };

        static readonly Dictionary<string, EventProcessFn> DrumsTextEventToProcessFnMap = new Dictionary<string, EventProcessFn>()
    {
    };

        static MidReader()
        {
            BuildGuitarMidiNoteNumberToProcessFnDict();
            BuildGhlGuitarMidiNoteNumberToProcessFnDict();
            BuildDrumsMidiNoteNumberToProcessFnDict();
        }

        public static Song ReadMidi(string path, ref CallbackState callBackState)
        {
            Song song = new Song();
            string directory = Path.GetDirectoryName(path);

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
                midi = new MidiFile(path);
            }
            catch (SystemException e)
            {
                throw new SystemException("Bad or corrupted midi file- " + e.Message);
            }

            song.resolution = (short)midi.DeltaTicksPerQuarterNote;

            // Read all bpm data in first. This will also allow song.TimeToTick to function properly.
            ReadSync(midi.Events[0], song);

            for (int i = 1; i < midi.Tracks; ++i)
            {
                var trackName = midi.Events[i][0] as TextEvent;
                if (trackName == null)
                    continue;
                Debug.Log("Found midi track " + trackName.Text);

                string trackNameKey = trackName.Text.ToUpper();
                if (trackNameKey == MidIOHelper.EVENTS_TRACK)
                {
                    ReadSongGlobalEvents(midi.Events[i], song);
                }
                else if (!c_trackExcludesMap.ContainsKey(trackNameKey))
                {
                    bool importTrackAsVocalsEvents = trackNameKey == MidIOHelper.VOCALS_TRACK;

#if !UNITY_EDITOR
                    if (importTrackAsVocalsEvents)
                    {
                        callBackState = CallbackState.WaitingForExternalInformation;
                        NativeMessageBox.Result result = NativeMessageBox.Show("A vocals track was found in the file. Would you like to import the text events as global lyrics and phrase events?", "Vocals Track Found", NativeMessageBox.Type.YesNo, null);
                        callBackState = CallbackState.None;
                        importTrackAsVocalsEvents = result == NativeMessageBox.Result.Yes;
                    }
#endif
                    if (importTrackAsVocalsEvents)
                    {
                        Debug.Log("Loading lyrics from Vocals track");
                        ReadTextEventsIntoGlobalEventsAsLyrics(midi.Events[i], song);
                    }
                    else
                    {
                        Song.Instrument instrument;
                        if (!c_trackNameToInstrumentMap.TryGetValue(trackNameKey, out instrument))
                        {
                            instrument = Song.Instrument.Unrecognised;
                        }

                        Debug.LogFormat("Loading midi track {0}", instrument);
                        ReadNotes(midi.Events[i], song, instrument);
                    }
                }
            }

            return song;
        }

        static void ReadTrack(IList<MidiEvent> track)
        {
            foreach (var me in track)
            {
                var note = me as NoteOnEvent;
                if (note != null)
                {
                    Debug.Log("Note: " + note.NoteNumber + ", Pos: " + note.AbsoluteTime + ", Vel: " + note.Velocity + ", Channel: " + note.Channel + ", Off pos: " + note.OffEvent.AbsoluteTime);
                }

                var text = me as TextEvent;
                if (text != null)
                {
                    Debug.Log(text.Text + " " + text.AbsoluteTime);
                }
            }
        }

        private static void ReadSync(IList<MidiEvent> track, Song song)
        {
            foreach (var me in track)
            {
                var ts = me as TimeSignatureEvent;
                if (ts != null)
                {
                    var tick = me.AbsoluteTime;

                    song.Add(new TimeSignature((uint)tick, (uint)ts.Numerator, (uint)(Mathf.Pow(2, ts.Denominator))), false);
                    continue;
                }
                var tempo = me as TempoEvent;
                if (tempo != null)
                {
                    var tick = me.AbsoluteTime;
                    song.Add(new BPM((uint)tick, (uint)(tempo.Tempo * 1000)), false);
                    continue;
                }

                // Read the song name
                var text = me as TextEvent;
                if (text != null)
                {
                    song.name = text.Text;
                }
            }

            song.UpdateCache();
        }

        private static void ReadSongGlobalEvents(IList<MidiEvent> track, Song song)
        {
            const string rb2SectionPrefix = "[" + MidIOHelper.Rb2SectionPrefix;
            const string rb3SectionPrefix = "[" + MidIOHelper.Rb3SectionPrefix;

            for (int i = 1; i < track.Count; ++i)
            {
                var text = track[i] as TextEvent;

                if (text != null)
                {
                    if (text.Text.Contains(rb2SectionPrefix))
                    {
                        song.Add(new Section(text.Text.Substring(9, text.Text.Length - 10), (uint)text.AbsoluteTime), false);
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

                        song.Add(new Section(sectionText, (uint)text.AbsoluteTime), false);
                    }
                    else
                    {
                        song.Add(new Event(text.Text.Trim(new char[] { '[', ']' }), (uint)text.AbsoluteTime), false);
                    }
                }
            }

            song.UpdateCache();
        }

        private static void ReadTextEventsIntoGlobalEventsAsLyrics(IList<MidiEvent> track, Song song)
        {
            for (int i = 1; i < track.Count; ++i)
            {
                var text = track[i] as TextEvent;
                if (text != null && text.Text.Length > 0 && text.MetaEventType == MetaEventType.Lyric)
                {
                    string lyricEvent = MidIOHelper.LYRIC_EVENT_PREFIX + text.Text;
                    song.Add(new Event(lyricEvent, (uint)text.AbsoluteTime), false);
                }

                var phrase = track[i] as NoteOnEvent;
                if (phrase != null && phrase.OffEvent != null &&
                    (phrase.NoteNumber == MidIOHelper.PhraseMarker || phrase.NoteNumber == MidIOHelper.PhraseMarker2))
                {
                    string phraseStartEvent = MidIOHelper.PhraseStartText;
                    song.Add(new Event(phraseStartEvent, (uint)phrase.AbsoluteTime), false);

                    string phraseEndEvent = MidIOHelper.PhraseEndText;
                    song.Add(new Event(phraseEndEvent, (uint)phrase.OffEvent.AbsoluteTime), false);
                }
            }

            song.UpdateCache();
        }

        private static void ReadNotes(IList<MidiEvent> track, Song song, Song.Instrument instrument)
        {
            List<NoteOnEvent> forceNotesList = new List<NoteOnEvent>();
            List<NoteOnEvent> proDrumsNotesList = new List<NoteOnEvent>();
            List<SysexEvent> tapAndOpenEvents = new List<SysexEvent>();

            Chart unrecognised = new Chart(song, Song.Instrument.Unrecognised);
            Chart.GameMode gameMode = Song.InstumentToChartGameMode(instrument);

            EventProcessParams processParams = new EventProcessParams()
            {
                song = song,
                currentUnrecognisedChart = unrecognised,
                instrument = instrument,
                delayedProcessesList = new List<EventProcessFn>(),
            };

            var noteProcessDict = GetNoteProcessDict(gameMode);
            var textEventProcessDict = GetTextEventProcessDict(gameMode);

            if (instrument == Song.Instrument.Unrecognised)
                song.unrecognisedCharts.Add(unrecognised);

            int rbSustainFixLength = (int)(64 * song.resolution / SongConfig.STANDARD_BEAT_RESOLUTION);

            // Load all the notes
            for (int i = 0; i < track.Count; i++)
            {
                var text = track[i] as TextEvent;
                if (text != null)
                {
                    if (i == 0)
                    {
                        if (instrument == Song.Instrument.Unrecognised)
                            unrecognised.name = text.Text;
                        continue;           // We don't want the first event because that is the name of the track
                    }

                    var tick = (uint)text.AbsoluteTime;
                    var eventName = text.Text;

                    ChartEvent chartEvent = new ChartEvent(tick, eventName);

                    if (instrument == Song.Instrument.Unrecognised)
                    {
                        unrecognised.Add(chartEvent);
                    }
                    else
                    {
                        EventProcessFn processFn;
                        if (textEventProcessDict.TryGetValue(eventName, out processFn))
                        {
                            // This text event affects parsing of the .mid file, run its function and don't parse it into the chart
                            processParams.midiEvent = text;
                            processFn(processParams);
                        }
                        else
                        {
                            // Copy text event to all difficulties so that .chart format can store these properly. Midi writer will strip duplicate events just fine anyway. 
                            foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
                            {
                                song.GetChart(instrument, difficulty).Add(chartEvent);
                            }
                        }
                    }
                }

                var note = track[i] as NoteOnEvent;
                if (note != null && note.OffEvent != null)
                {
                    if (instrument == Song.Instrument.Unrecognised)
                    {
                        var tick = (uint)note.AbsoluteTime;
                        var sus = CalculateSustainLength(song, note);

                        int rawNote = note.NoteNumber;
                        Note newNote = new Note(tick, rawNote, sus);
                        unrecognised.Add(newNote);
                        continue;
                    }

                    processParams.midiEvent = note;

                    EventProcessFn processFn;
                    if (noteProcessDict.TryGetValue(note.NoteNumber, out processFn))
                    {
                        processFn(processParams);
                    }
                }

                var sysexEvent = track[i] as SysexEvent;
                if (sysexEvent != null)
                {
                    tapAndOpenEvents.Add(sysexEvent);
                }
            }

            // Update all chart arrays
            if (instrument != Song.Instrument.Unrecognised)
            {
                foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
                    song.GetChart(instrument, diff).UpdateCache();
            }
            else
                unrecognised.UpdateCache();

            // Run delayed processes
            foreach (var process in processParams.delayedProcessesList)
            {
                process(processParams);
            }

            // Apply tap and open note events
            Chart[] chartsOfInstrument;

            if (instrument == Song.Instrument.Unrecognised)
            {
                chartsOfInstrument = new Chart[] { unrecognised };
            }
            else
            {
                chartsOfInstrument = new Chart[EnumX<Song.Difficulty>.Count];

                int difficultyCount = 0;
                foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
                    chartsOfInstrument[difficultyCount++] = song.GetChart(instrument, difficulty);
            }

            for (int i = 0; i < tapAndOpenEvents.Count; ++i)
            {
                var se1 = tapAndOpenEvents[i];
                byte[] bytes = se1.GetData();

                // Check for tap event
                if (bytes.Length == 8 && bytes[5] == 255 && bytes[7] == 1)
                {
                    // Identified a tap section
                    // 8 total bytes, 5th byte is FF, 7th is 1 to start, 0 to end
                    uint tick = (uint)se1.AbsoluteTime;
                    uint endPos = 0;

                    // Find the end of the tap section
                    for (int j = i; j < tapAndOpenEvents.Count; ++j)
                    {
                        var se2 = tapAndOpenEvents[j];
                        var bytes2 = se2.GetData();
                        /// Check for tap section end
                        if (bytes2.Length == 8 && bytes2[5] == 255 && bytes2[7] == 0)
                        {
                            endPos = (uint)(se2.AbsoluteTime - tick);

                            if (endPos > 0)
                                --endPos;

                            break;
                        }

                    }

                    // Apply tap property
                    foreach (Chart chart in chartsOfInstrument)
                    {
                        int index, length;
                        SongObjectHelper.GetRange(chart.notes, tick, tick + endPos, out index, out length);
                        for (int k = index; k < index + length; ++k)
                        {
                            if (!chart.notes[k].IsOpenNote())
                            {
                                chart.notes[k].flags = Note.Flags.Tap;
                            }
                        }
                    }
                }

                // Check for open notes
                // 5th byte determines the difficulty to apply to
                else if (bytes.Length == 8 && bytes[5] >= 0 && bytes[5] < 4 && bytes[7] == 1)
                {
                    uint tick = (uint)se1.AbsoluteTime;
                    Song.Difficulty difficulty;
                    switch (bytes[5])
                    {
                        case 0: difficulty = Song.Difficulty.Easy; break;
                        case 1: difficulty = Song.Difficulty.Medium; break;
                        case 2: difficulty = Song.Difficulty.Hard; break;
                        case 3: difficulty = Song.Difficulty.Expert; break;
                        default: continue;
                    }

                    uint endPos = 0;
                    for (int j = i; j < tapAndOpenEvents.Count; ++j)
                    {
                        var se2 = tapAndOpenEvents[j] as SysexEvent;
                        if (se2 != null)
                        {
                            var b2 = se2.GetData();
                            if (b2.Length == 8 && b2[5] == bytes[5] && b2[7] == 0)
                            {
                                endPos = (uint)(se2.AbsoluteTime - tick);

                                if (endPos > 0)
                                    --endPos;

                                break;
                            }
                        }
                    }

                    int index, length;
                    SongObjectCache<Note> notes;
                    if (instrument == Song.Instrument.Unrecognised)
                        notes = unrecognised.notes;
                    else
                        notes = song.GetChart(instrument, difficulty).notes;
                    SongObjectHelper.GetRange(notes, tick, tick + endPos, out index, out length);
                    for (int k = index; k < index + length; ++k)
                    {
                        switch (gameMode)
                        {
                            case (Chart.GameMode.Guitar):
                                notes[k].guitarFret = Note.GuitarFret.Open;
                                break;

                            case (Chart.GameMode.GHLGuitar):
                                notes[k].ghliveGuitarFret = Note.GHLiveGuitarFret.Open;
                                break;

                            case (Chart.GameMode.Drums):
                                notes[k].guitarFret = LoadDrumNoteToGuitarNote(notes[k].guitarFret);
                                break;
                        }
                    }
                }
            }

            foreach (var flagEvent in proDrumsNotesList)
            {
                uint tick = (uint)flagEvent.AbsoluteTime;
                uint endPos = (uint)(flagEvent.OffEvent.AbsoluteTime - tick);
                if (endPos > 0)
                    --endPos;

                Debug.Assert(instrument == Song.Instrument.Drums);

                foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
                {
                    Chart chart = song.GetChart(instrument, difficulty);

                    int index, length;
                    SongObjectHelper.GetRange(chart.notes, tick, tick + endPos, out index, out length);

                    Note.DrumPad drumPadForFlag;
                    if (!MidIOHelper.CYMBAL_TO_PAD_LOOKUP.TryGetValue(flagEvent.NoteNumber, out drumPadForFlag))
                    {
                        Debug.Assert(false, "Unknown note number flag " + flagEvent.NoteNumber);
                        continue;
                    }

                    for (int i = index; i < index + length; ++i)
                    {
                        Note note = chart.notes[i];

                        if (note.drumPad == drumPadForFlag)
                        {
                            // Reverse cymbal flag
                            note.flags ^= Note.Flags.ProDrums_Cymbal;
                        }
                    }
                }
            }
        }

        static Dictionary<int, EventProcessFn> GetNoteProcessDict(Chart.GameMode gameMode)
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

        static Dictionary<string, EventProcessFn> GetTextEventProcessDict(Chart.GameMode gameMode)
        {
            switch (gameMode)
            {
                case Chart.GameMode.GHLGuitar:
                    {
                        return GhlGuitarTextEventToProcessFnMap;
                    }
                case Chart.GameMode.Drums:
                    {
                        return DrumsTextEventToProcessFnMap;
                    }

                default: break;
            }

            return GuitarTextEventToProcessFnMap;
        }

        delegate void BuildPerDifficultyFn(int difficultyStartRange, Song.Difficulty difficulty);
        static void BuildGuitarMidiNoteNumberToProcessFnDict()
        {
            Dictionary<Note.GuitarFret, int> FretToMidiKey = new Dictionary<Note.GuitarFret, int>()
        {
            // { Note.GuitarFret.Open, 0 }, // Handled by sysex event
            { Note.GuitarFret.Green, 0 },
            { Note.GuitarFret.Red, 1 },
            { Note.GuitarFret.Yellow, 2 },
            { Note.GuitarFret.Blue, 3 },
            { Note.GuitarFret.Orange, 4 },
        };

            BuildPerDifficultyFn BuildPerDifficulty = (int difficultyStartRange, Song.Difficulty difficulty) =>
            {
                foreach (var guitarFret in EnumX<Note.GuitarFret>.Values)
                {
                    int fretOffset;
                    if (FretToMidiKey.TryGetValue(guitarFret, out fretOffset))
                    {
                        int key = fretOffset + difficultyStartRange;
                        int fret = (int)guitarFret;

                        GuitarMidiNoteNumberToProcessFnMap.Add(key, (in EventProcessParams eventProcessParams) =>
                        {
                            ProcessNoteOnEventAsNote(eventProcessParams, difficulty, fret);
                        });
                    }
                }

                // Process forced hopo or forced strum
                {
                    int flagKey = difficultyStartRange + 5;
                    GuitarMidiNoteNumberToProcessFnMap.Add(flagKey, (in EventProcessParams eventProcessParams) =>
                    {
                        ProcessNoteOnEventAsModifier(eventProcessParams, difficulty, Note.NoteType.Hopo);
                    });
                }
                {
                    int flagKey = difficultyStartRange + 6;
                    GuitarMidiNoteNumberToProcessFnMap.Add(flagKey, (in EventProcessParams eventProcessParams) =>
                    {
                        ProcessNoteOnEventAsModifier(eventProcessParams, difficulty, Note.NoteType.Strum);
                    });
                }
            };

            foreach (var keyVal in MidIOHelper.GUITAR_DIFF_RANGE_LOOKUP)
            {
                var diff = keyVal.Key;
                var rangeStart = keyVal.Value;
                BuildPerDifficulty(rangeStart, diff);
            }
        }

        static void BuildGhlGuitarMidiNoteNumberToProcessFnDict()
        {
            Dictionary<Note.GHLiveGuitarFret, int> FretToMidiKey = new Dictionary<Note.GHLiveGuitarFret, int>()
        {
            { Note.GHLiveGuitarFret.Open, 0 },
            { Note.GHLiveGuitarFret.White1, 1 },
            { Note.GHLiveGuitarFret.White2, 2 },
            { Note.GHLiveGuitarFret.White3, 3 },
            { Note.GHLiveGuitarFret.Black1, 4 },
            { Note.GHLiveGuitarFret.Black2, 5 },
            { Note.GHLiveGuitarFret.Black3, 6 },
        };

            BuildPerDifficultyFn BuildPerDifficulty = (int difficultyStartRange, Song.Difficulty difficulty) =>
            {
                foreach (var guitarFret in EnumX<Note.GHLiveGuitarFret>.Values)
                {
                    int fretOffset;
                    if (FretToMidiKey.TryGetValue(guitarFret, out fretOffset))
                    {
                        int key = fretOffset + difficultyStartRange;
                        int fret = (int)guitarFret;

                        GhlGuitarMidiNoteNumberToProcessFnMap.Add(key, (in EventProcessParams eventProcessParams) =>
                        {
                            ProcessNoteOnEventAsNote(eventProcessParams, difficulty, fret);
                        });
                    }
                }

                // Process forced hopo or forced strum
                {
                    int flagKey = difficultyStartRange + 7;
                    GhlGuitarMidiNoteNumberToProcessFnMap.Add(flagKey, (in EventProcessParams eventProcessParams) =>
                    {
                        ProcessNoteOnEventAsModifier(eventProcessParams, difficulty, Note.NoteType.Hopo);
                    });
                }
                {
                    int flagKey = difficultyStartRange + 8;
                    GhlGuitarMidiNoteNumberToProcessFnMap.Add(flagKey, (in EventProcessParams eventProcessParams) =>
                    {
                        ProcessNoteOnEventAsModifier(eventProcessParams, difficulty, Note.NoteType.Strum);
                    });
                }
            };

            foreach (var keyVal in MidIOHelper.GHL_GUITAR_DIFF_RANGE_LOOKUP)
            {
                var diff = keyVal.Key;
                var rangeStart = keyVal.Value;
                BuildPerDifficulty(rangeStart, diff);
            }
        }

        static void BuildDrumsMidiNoteNumberToProcessFnDict()
        {
            Dictionary<Note.DrumPad, int> DrumPadToMidiKey = new Dictionary<Note.DrumPad, int>()
        {
            { Note.DrumPad.Kick, 0 },
            { Note.DrumPad.Red, 1 },
            { Note.DrumPad.Yellow, 2 },
            { Note.DrumPad.Blue, 3 },
            { Note.DrumPad.Orange, 4 },
            { Note.DrumPad.Green, 5 },
        };

            Dictionary<Note.DrumPad, Note.Flags> DrumPadDefaultFlags = new Dictionary<Note.DrumPad, Note.Flags>()
        {
            { Note.DrumPad.Yellow, Note.Flags.ProDrums_Cymbal },
            { Note.DrumPad.Blue, Note.Flags.ProDrums_Cymbal },
            { Note.DrumPad.Orange, Note.Flags.ProDrums_Cymbal },
        };

            BuildPerDifficultyFn BuildPerDifficulty = (int difficultyStartRange, Song.Difficulty difficulty) =>
            {
                foreach (var pad in EnumX<Note.DrumPad>.Values)
                {
                    int padOffset;
                    if (DrumPadToMidiKey.TryGetValue(pad, out padOffset))
                    {
                        int key = padOffset + difficultyStartRange;
                        int fret = (int)pad;
                        Note.Flags defaultFlags = Note.Flags.None;
                        DrumPadDefaultFlags.TryGetValue(pad, out defaultFlags);

                        DrumsMidiNoteNumberToProcessFnMap.Add(key, (in EventProcessParams eventProcessParams) =>
                        {
                            var noteEvent = eventProcessParams.midiEvent as NoteOnEvent;
                            Debug.Assert(noteEvent != null, $"Wrong note event type passed to drums note process. Expected: {typeof(NoteOnEvent)}, Actual: {eventProcessParams.midiEvent.GetType()}");

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

                            ProcessNoteOnEventAsNote(eventProcessParams, difficulty, fret, flags);
                        });
                    }
                }
            };

            foreach (var keyVal in MidIOHelper.DRUMS_DIFF_RANGE_LOOKUP)
            {
                var diff = keyVal.Key;
                var rangeStart = keyVal.Value;
                BuildPerDifficulty(rangeStart, diff);
            }

            foreach (var keyVal in MidIOHelper.PAD_TO_CYMBAL_LOOKUP)
            {
                int pad = (int)keyVal.Key;
                int midiKey = keyVal.Value;
                DrumsMidiNoteNumberToProcessFnMap.Add(midiKey, (in EventProcessParams eventProcessParams) =>
                {
                    ProcessNoteOnEventAsFlagToggle(eventProcessParams, Note.Flags.ProDrums_Cymbal, pad);
                });
            }
        }

        static void ProcessNoteOnEventAsNote(in EventProcessParams eventProcessParams, Song.Difficulty diff, int ingameFret, Note.Flags flags = Note.Flags.None)
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

            NoteOnEvent noteEvent = eventProcessParams.midiEvent as NoteOnEvent;
            Debug.Assert(noteEvent != null, $"Wrong note event type passed to ProcessNoteOnEventAsNote. Expected: {typeof(NoteOnEvent)}, Actual: {eventProcessParams.midiEvent.GetType()}");
            var tick = (uint)noteEvent.AbsoluteTime;
            var sus = CalculateSustainLength(eventProcessParams.song, noteEvent);

            Note newNote = new Note(tick, ingameFret, sus, flags);
            chart.Add(newNote, false);
        }

        static void ProcessNoteOnEventAsStarpower(in EventProcessParams eventProcessParams, Starpower.Flags flags = Starpower.Flags.None)
        {
            var noteEvent = eventProcessParams.midiEvent as NoteOnEvent;
            Debug.Assert(noteEvent != null, $"Wrong note event type passed to ProcessNoteOnEventAsStarpower. Expected: {typeof(NoteOnEvent)}, Actual: {eventProcessParams.midiEvent.GetType()}");
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            var tick = (uint)noteEvent.AbsoluteTime;
            var sus = CalculateSustainLength(song, noteEvent);

            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                song.GetChart(instrument, diff).Add(new Starpower(tick, sus, flags), false);
            }
        }

        static void ProcessNoteOnEventAsModifier(in EventProcessParams eventProcessParams, Song.Difficulty difficulty, Note.NoteType noteType)
        {
            var flagEvent = eventProcessParams.midiEvent as NoteOnEvent;
            Debug.Assert(flagEvent != null, $"Wrong note event type passed to ProcessNoteOnEventAsModifier. Expected: {typeof(NoteOnEvent)}, Actual: {eventProcessParams.midiEvent.GetType()}");

            // Delay the actual processing once all the notes are actually in
            eventProcessParams.delayedProcessesList.Add((in EventProcessParams processParams) =>
            {
                ProcessNoteOnEventAsModifierPostDelay(processParams, flagEvent, difficulty, noteType);
            });
        }

        static void ProcessNoteOnEventAsModifier(in EventProcessParams eventProcessParams, Note.NoteType noteType)
        {
            var flagEvent = eventProcessParams.midiEvent as NoteOnEvent;
            Debug.Assert(flagEvent != null, $"Wrong note event type passed to ProcessNoteOnEventAsModifier. Expected: {typeof(NoteOnEvent)}, Actual: {eventProcessParams.midiEvent.GetType()}");

            // Delay the actual processing once all the notes are actually in
            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                eventProcessParams.delayedProcessesList.Add((in EventProcessParams processParams) =>
                {
                    ProcessNoteOnEventAsModifierPostDelay(processParams, flagEvent, diff, noteType);
                });
            }
        }

        static void ProcessNoteOnEventAsModifierPostDelay(in EventProcessParams eventProcessParams, NoteOnEvent noteEvent, Song.Difficulty difficulty, Note.NoteType noteType)
        {
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            uint tick = (uint)noteEvent.AbsoluteTime;
            uint endPos = (uint)(noteEvent.OffEvent.AbsoluteTime - tick);

            Chart chart;
            if (instrument != Song.Instrument.Unrecognised)
                chart = song.GetChart(instrument, difficulty);
            else
                chart = eventProcessParams.currentUnrecognisedChart;

            int index, length;
            SongObjectHelper.GetRange(chart.notes, tick, tick + endPos, out index, out length);

            uint lastChordTick = uint.MaxValue;
            bool firstInChord = true;
            bool perNote = false;
            Note.Flags flags = Note.Flags.None;

            for (int i = index; i < index + length; ++i)
            {
                Note note = chart.notes[i];
                if (!note.IsAllowedToBeType(noteType))
                {
                    Debug.LogWarning($"Attempted to set a note as a type it is not allowed to be.\nInstrument: {instrument}, new type: {noteType}, current type: {note.type}, tick: {note.tick}, is open note: {note.IsOpenNote()} natural HOPO: {note.isNaturalHopo}, forceable: {!note.cannotBeForced}");
                    continue;
                }

                flags = note.GetFlagsToSetType(noteType);
                firstInChord = lastChordTick != note.tick;
                perNote = ((flags & Note.PER_NOTE_FLAGS) != 0) && ((flags & ~Note.PER_NOTE_FLAGS) == 0);

                // Only set flags once in a chord, except when there are per-note flags
                if (firstInChord || perNote)
                {
                    note.flags = flags;
                    if (firstInChord && !perNote)
                    {
                        note.ApplyFlagsToChord();
                    }
                }

                lastChordTick = note.tick;

                Debug.Assert(
                    note.type == noteType,
                    $"Failed to set note as note type {noteType}.\nInstrument: {instrument}, new type: {noteType}, actual type: {note.type}, tick: {note.tick}, is open note: {note.IsOpenNote()} natural HOPO: {note.isNaturalHopo}, forceable: {!note.cannotBeForced}"
                );
            }
        }

        static uint CalculateSustainLength(Song song, NoteOnEvent noteEvent)
        {
            uint tick = (uint)noteEvent.AbsoluteTime;
            var sus = (uint)(noteEvent.OffEvent.AbsoluteTime - tick);
            int rbSustainFixLength = (int)(64 * song.resolution / SongConfig.STANDARD_BEAT_RESOLUTION);
            if (sus <= rbSustainFixLength)
                sus = 0;

            return sus;
        }

        static void ProcessNoteOnEventAsEvent(EventProcessParams eventProcessParams, string eventStartText, string eventEndText)
        {
            var noteEvent = eventProcessParams.midiEvent as NoteOnEvent;
            Debug.Assert(noteEvent != null, $"Wrong note event type passed to ProcessNoteOnEventAsEvent. Expected: {typeof(NoteOnEvent)}, Actual: {eventProcessParams.midiEvent.GetType()}");
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            uint tick = (uint)noteEvent.AbsoluteTime;
            var sus = CalculateSustainLength(song, noteEvent);

            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                Chart chart = song.GetChart(instrument, diff);
                chart.Add(new ChartEvent(tick, eventStartText));
                chart.Add(new ChartEvent(tick + sus, eventEndText));
            }
        }

        static void ProcessNoteOnEventAsFlagToggle(in EventProcessParams eventProcessParams, Note.Flags flags, int individualNoteSpecifier)
        {
            var flagEvent = eventProcessParams.midiEvent as NoteOnEvent;
            Debug.Assert(flagEvent != null, $"Wrong note event type passed to ProcessNoteOnEventAsFlagToggle. Expected: {typeof(NoteOnEvent)}, Actual: {eventProcessParams.midiEvent.GetType()}");

            // Delay the actual processing once all the notes are actually in
            eventProcessParams.delayedProcessesList.Add((in EventProcessParams processParams) =>
            {
                ProcessNoteOnEventAsFlagTogglePostDelay(processParams, flagEvent, flags, individualNoteSpecifier);
            });
        }

        static void ProcessNoteOnEventAsFlagTogglePostDelay(in EventProcessParams eventProcessParams, NoteOnEvent noteEvent, Note.Flags flags, int individualNoteSpecifier)   // individualNoteSpecifier as -1 to apply to the whole chord
        {
            var song = eventProcessParams.song;
            var instrument = eventProcessParams.instrument;

            uint tick = (uint)noteEvent.AbsoluteTime;
            uint endPos = (uint)(noteEvent.OffEvent.AbsoluteTime - tick);
            --endPos;

            var flagEvent = noteEvent;

            foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
            {
                Chart chart = song.GetChart(instrument, difficulty);

                int index, length;
                SongObjectHelper.GetRange(chart.notes, tick, tick + endPos, out index, out length);

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

        static Note.GuitarFret LoadDrumNoteToGuitarNote(Note.GuitarFret fret_type)
        {
            if (fret_type == Note.GuitarFret.Open)
                return Note.GuitarFret.Orange;
            else if (fret_type == Note.GuitarFret.Green)
                return Note.GuitarFret.Open;
            else
                return fret_type - 1;
        }
    }
}
