// Copyright (c) 2016-2020 Alexander Ong
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

        struct NoteProcessParams
        {
            public Song song;
            public Song.Instrument instrument;
            public Chart currentUnrecognisedChart;
            public NoteOnEvent noteEvent;
            public List<NoteEventProcessFn> forceNotesProcessesList;
        }

        delegate void NoteEventProcessFn(in NoteProcessParams noteProcessParams);

        // These dictionaries map the NoteNumber of each midi note event to a specific function of how to process them
        static readonly Dictionary<int, NoteEventProcessFn> GuitarMidiNoteNumberToProcessFnMap = new Dictionary<int, NoteEventProcessFn>()
    {
        { MidIOHelper.STARPOWER_NOTE, ProcessNoteOnEventAsStarpower },
        { MidIOHelper.SOLO_NOTE, (in NoteProcessParams noteProcessParams) => {
            ProcessNoteOnEventAsEvent(noteProcessParams, MidIOHelper.SoloEventText, MidIOHelper.SoloEndEventText);
        }},
    };

        static readonly Dictionary<int, NoteEventProcessFn> GhlGuitarMidiNoteNumberToProcessFnMap = new Dictionary<int, NoteEventProcessFn>()
    {
        { MidIOHelper.STARPOWER_NOTE, ProcessNoteOnEventAsStarpower },
        { MidIOHelper.SOLO_NOTE, (in NoteProcessParams noteProcessParams) => {
            ProcessNoteOnEventAsEvent(noteProcessParams, MidIOHelper.SoloEventText, MidIOHelper.SoloEndEventText);
        }},
    };

        static readonly Dictionary<int, NoteEventProcessFn> DrumsMidiNoteNumberToProcessFnMap = new Dictionary<int, NoteEventProcessFn>()
    {
        { MidIOHelper.STARPOWER_NOTE, ProcessNoteOnEventAsStarpower },
        { MidIOHelper.SOLO_NOTE, (in NoteProcessParams noteProcessParams) => {
            ProcessNoteOnEventAsEvent(noteProcessParams, MidIOHelper.SoloEventText, MidIOHelper.SoloEndEventText);
        }},
        { MidIOHelper.DOUBLE_KICK_NOTE, (in NoteProcessParams noteProcessParams) => {
            ProcessNoteOnEventAsNote(noteProcessParams, Song.Difficulty.Expert, (int)Note.DrumPad.Kick, Note.Flags.InstrumentPlus);
        }},
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
                        song.Add(new Section(text.Text.Substring(9, text.Text.Length - 10), (uint)text.AbsoluteTime), false);
                    else if (text.Text.Contains(rb3SectionPrefix))
                        song.Add(new Section(text.Text.Substring(5, text.Text.Length - 6), (uint)text.AbsoluteTime), false);
                    else
                        song.Add(new Event(text.Text.Trim(new char[] { '[', ']' }), (uint)text.AbsoluteTime), false);
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
                if (phrase != null && phrase.OffEvent != null && phrase.NoteNumber == MidIOHelper.PhraseMarker)
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

            NoteProcessParams processParams = new NoteProcessParams()
            {
                song = song,
                currentUnrecognisedChart = unrecognised,
                instrument = instrument,
                forceNotesProcessesList = new List<NoteEventProcessFn>(),
            };

            var noteProcessDict = GetNoteProcessDict(gameMode);

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
                    var eventName = text.Text.Trim(new char[] { '[', ']' });
                    ChartEvent chartEvent = new ChartEvent(tick, eventName);

                    if (instrument == Song.Instrument.Unrecognised)
                        unrecognised.Add(chartEvent);
                    else
                        song.GetChart(instrument, Song.Difficulty.Expert).Add(chartEvent);
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

                    processParams.noteEvent = note;

                    NoteEventProcessFn processFn;
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
                        notes[k].guitarFret = Note.GuitarFret.Open;

                        if (gameMode == Chart.GameMode.Drums)
                            notes[k].guitarFret = LoadDrumNoteToGuitarNote(notes[k].guitarFret);
                    }
                }
            }

            // Apply forcing events
            foreach (var process in processParams.forceNotesProcessesList)
            {
                process(processParams);
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

        static Dictionary<int, NoteEventProcessFn> GetNoteProcessDict(Chart.GameMode gameMode)
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

        delegate void BuildPerDifficultyFn(int difficultyStartRange, Song.Difficulty difficulty);
        static void BuildGuitarMidiNoteNumberToProcessFnDict()
        {
            const int EasyStartRange = 60;
            const int MediumStartRange = 72;
            const int HardStartRange = 84;
            const int ExpertStartRange = 96;

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

                        GuitarMidiNoteNumberToProcessFnMap.Add(key, (in NoteProcessParams noteProcessParams) =>
                        {
                            ProcessNoteOnEventAsNote(noteProcessParams, difficulty, fret);
                        });
                    }
                }

            // Process forced hopo or forced strum
            {
                    int flagKey = difficultyStartRange + 5;
                    GuitarMidiNoteNumberToProcessFnMap.Add(flagKey, (in NoteProcessParams noteProcessParams) =>
                    {
                        ProcessNoteOnEventAsForcedType(noteProcessParams, difficulty, Note.NoteType.Hopo);
                    });
                }
                {
                    int flagKey = difficultyStartRange + 6;
                    GuitarMidiNoteNumberToProcessFnMap.Add(flagKey, (in NoteProcessParams noteProcessParams) =>
                    {
                        ProcessNoteOnEventAsForcedType(noteProcessParams, difficulty, Note.NoteType.Strum);
                    });
                }
            };

            BuildPerDifficulty(EasyStartRange, Song.Difficulty.Easy);
            BuildPerDifficulty(MediumStartRange, Song.Difficulty.Medium);
            BuildPerDifficulty(HardStartRange, Song.Difficulty.Hard);
            BuildPerDifficulty(ExpertStartRange, Song.Difficulty.Expert);
        }

        static void BuildGhlGuitarMidiNoteNumberToProcessFnDict()
        {
            const int EasyStartRange = 58;
            const int MediumStartRange = 70;
            const int HardStartRange = 82;
            const int ExpertStartRange = 94;

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

                        GhlGuitarMidiNoteNumberToProcessFnMap.Add(key, (in NoteProcessParams noteProcessParams) =>
                        {
                            ProcessNoteOnEventAsNote(noteProcessParams, difficulty, fret);
                        });
                    }
                }

            // Process forced hopo or forced strum
            {
                    int flagKey = difficultyStartRange + 7;
                    GhlGuitarMidiNoteNumberToProcessFnMap.Add(flagKey, (in NoteProcessParams noteProcessParams) =>
                    {
                        ProcessNoteOnEventAsForcedType(noteProcessParams, difficulty, Note.NoteType.Hopo);
                    });
                }
                {
                    int flagKey = difficultyStartRange + 8;
                    GhlGuitarMidiNoteNumberToProcessFnMap.Add(flagKey, (in NoteProcessParams noteProcessParams) =>
                    {
                        ProcessNoteOnEventAsForcedType(noteProcessParams, difficulty, Note.NoteType.Strum);
                    });
                }
            };

            BuildPerDifficulty(EasyStartRange, Song.Difficulty.Easy);
            BuildPerDifficulty(MediumStartRange, Song.Difficulty.Medium);
            BuildPerDifficulty(HardStartRange, Song.Difficulty.Hard);
            BuildPerDifficulty(ExpertStartRange, Song.Difficulty.Expert);
        }

        static void BuildDrumsMidiNoteNumberToProcessFnDict()
        {
            const int EasyStartRange = 60;
            const int MediumStartRange = 72;
            const int HardStartRange = 84;
            const int ExpertStartRange = 96;

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

                        DrumsMidiNoteNumberToProcessFnMap.Add(key, (in NoteProcessParams noteProcessParams) =>
                        {
                            ProcessNoteOnEventAsNote(noteProcessParams, difficulty, fret, defaultFlags);
                        });
                    }
                }
            };

            BuildPerDifficulty(EasyStartRange, Song.Difficulty.Easy);
            BuildPerDifficulty(MediumStartRange, Song.Difficulty.Medium);
            BuildPerDifficulty(HardStartRange, Song.Difficulty.Hard);
            BuildPerDifficulty(ExpertStartRange, Song.Difficulty.Expert);

            foreach (var keyVal in MidIOHelper.PAD_TO_CYMBAL_LOOKUP)
            {
                int pad = (int)keyVal.Key;
                int midiKey = keyVal.Value;
                DrumsMidiNoteNumberToProcessFnMap.Add(midiKey, (in NoteProcessParams noteProcessParams) =>
                {
                    ProcessNoteOnEventAsFlagToggle(noteProcessParams, Note.Flags.ProDrums_Cymbal, pad);
                });
            }
        }

        static void ProcessNoteOnEventAsNote(in NoteProcessParams noteProcessParams, Song.Difficulty diff, int ingameFret, Note.Flags defaultFlags = Note.Flags.None)
        {
            Chart chart;
            if (noteProcessParams.instrument == Song.Instrument.Unrecognised)
            {
                chart = noteProcessParams.currentUnrecognisedChart;
            }
            else
            {
                chart = noteProcessParams.song.GetChart(noteProcessParams.instrument, diff);
            }

            NoteOnEvent noteEvent = noteProcessParams.noteEvent;
            var tick = (uint)noteEvent.AbsoluteTime;
            var sus = CalculateSustainLength(noteProcessParams.song, noteEvent);

            Note newNote = new Note(tick, ingameFret, sus, defaultFlags);
            chart.Add(newNote, false);
        }

        static void ProcessNoteOnEventAsStarpower(in NoteProcessParams noteProcessParams)
        {
            var noteEvent = noteProcessParams.noteEvent;
            var song = noteProcessParams.song;
            var instrument = noteProcessParams.instrument;

            var tick = (uint)noteEvent.AbsoluteTime;
            var sus = CalculateSustainLength(song, noteEvent);

            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                song.GetChart(instrument, diff).Add(new Starpower(tick, sus), false);
            }
        }

        static void ProcessNoteOnEventAsForcedType(in NoteProcessParams noteProcessParams, Song.Difficulty difficulty, Note.NoteType noteType)
        {
            var flagEvent = noteProcessParams.noteEvent;

            // Delay the actual processing once all the notes are actually in
            noteProcessParams.forceNotesProcessesList.Add((in NoteProcessParams processParams) =>
            {
                ProcessNoteOnEventAsForcedTypePostDelay(processParams, flagEvent, difficulty, noteType);
            });
        }

        static void ProcessNoteOnEventAsForcedTypePostDelay(in NoteProcessParams noteProcessParams, NoteOnEvent noteEvent, Song.Difficulty difficulty, Note.NoteType noteType)
        {
            var song = noteProcessParams.song;
            var instrument = noteProcessParams.instrument;

            uint tick = (uint)noteEvent.AbsoluteTime;
            uint endPos = (uint)(noteEvent.OffEvent.AbsoluteTime - tick);

            Chart chart;
            if (instrument != Song.Instrument.Unrecognised)
                chart = song.GetChart(instrument, difficulty);
            else
                chart = noteProcessParams.currentUnrecognisedChart;

            int index, length;
            SongObjectHelper.GetRange(chart.notes, tick, tick + endPos, out index, out length);

            uint lastChordTick = uint.MaxValue;
            bool shouldBeForced = false;

            for (int i = index; i < index + length; ++i)
            {
                if ((chart.notes[i].flags & Note.Flags.Tap) != 0)
                    continue;

                Note note = chart.notes[i];

                if (lastChordTick != note.tick)
                {
                    shouldBeForced = false;

                    if (noteType == Note.NoteType.Strum)
                    {
                        if (!note.isChord && note.isNaturalHopo)
                        {
                            shouldBeForced = true;
                        }
                    }
                    else if (noteType == Note.NoteType.Hopo)
                    {
                        if (!note.cannotBeForced && (note.isChord || !note.isNaturalHopo))
                        {
                            shouldBeForced = true;
                        }
                    }
                    else
                    {
                        continue;   // Unhandled
                    }
                }
                // else we set the same forced property as before since we're on the same chord

                if (shouldBeForced)
                {
                    note.flags |= Note.Flags.Forced;
                }
                else
                {
                    note.flags &= ~Note.Flags.Forced;
                }

                lastChordTick = note.tick;

                Debug.Assert(note.type == noteType);
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

        static void ProcessNoteOnEventAsEvent(NoteProcessParams noteProcessParams, string eventStartText, string eventEndText)
        {
            var noteEvent = noteProcessParams.noteEvent;
            var song = noteProcessParams.song;
            var instrument = noteProcessParams.instrument;

            uint tick = (uint)noteEvent.AbsoluteTime;
            var sus = CalculateSustainLength(song, noteEvent);

            foreach (Song.Difficulty diff in EnumX<Song.Difficulty>.Values)
            {
                Chart chart = song.GetChart(instrument, diff);
                chart.Add(new ChartEvent(tick, eventStartText));
                chart.Add(new ChartEvent(tick + sus, eventEndText));
            }
        }

        static void ProcessNoteOnEventAsFlagToggle(in NoteProcessParams noteProcessParams, Note.Flags flags, int individualNoteSpecifier)
        {
            var flagEvent = noteProcessParams.noteEvent;

            // Delay the actual processing once all the notes are actually in
            noteProcessParams.forceNotesProcessesList.Add((in NoteProcessParams processParams) =>
            {
                ProcessNoteOnEventAsFlagTogglePostDelay(processParams, flagEvent, flags, individualNoteSpecifier);
            });
        }

        static void ProcessNoteOnEventAsFlagTogglePostDelay(in NoteProcessParams noteProcessParams, NoteOnEvent noteEvent, Note.Flags flags, int individualNoteSpecifier)   // individualNoteSpecifier as -1 to apply to the whole chord
        {
            var song = noteProcessParams.song;
            var instrument = noteProcessParams.instrument;

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
