// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

// Chart file format specifications- https://docs.google.com/document/d/1v2v0U-9HQ5qHeccpExDOLJ5CMPZZ3QytPmAG5WF0Kzs/edit?usp=sharing

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MoonscraperChartEditor.Song.IO
{
    public static class ChartReader
    {
        struct Anchor
        {
            public uint tick;
            public float anchorTime;
        }

        struct NoteFlag
        {
            public uint tick;
            public Note.Flags flag;
            public int noteNumber;

            public NoteFlag(uint tick, Note.Flags flag, int noteNumber)
            {
                this.tick = tick;
                this.flag = flag;
                this.noteNumber = noteNumber;
            }
        }

        struct NoteEvent
        {
            public uint tick;
            public int noteNumber;          
            public uint length;
        }

        struct NoteProcessParams
        {
            public Chart chart;
            public NoteEvent noteEvent;
            public List<NoteEventProcessFn> postNotesAddedProcessList;
        }

        delegate void NoteEventProcessFn(in NoteProcessParams noteProcessParams);

        // These dictionaries map the NoteNumber of each midi note event to a specific function of how to process them
        static readonly Dictionary<int, NoteEventProcessFn> GuitarChartNoteNumberToProcessFnMap = new Dictionary<int, NoteEventProcessFn>()
        {
            { 0, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GuitarFret.Green); }},
            { 1, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GuitarFret.Red); }},
            { 2, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GuitarFret.Yellow); }},
            { 3, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GuitarFret.Blue); }},
            { 4, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GuitarFret.Orange); }},
            { 7, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GuitarFret.Open); }},

            { 5, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsChordFlag(noteProcessParams, Note.Flags.Forced); }},
            { 6, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsChordFlag(noteProcessParams, Note.Flags.Tap); }},
        };

        static readonly Dictionary<int, NoteEventProcessFn> DrumsChartNoteNumberToProcessFnMap = new Dictionary<int, NoteEventProcessFn>()
        {
            { 0, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.DrumPad.Kick); }},
            { 1, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.DrumPad.Red); }},
            { 2, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.DrumPad.Yellow); }},
            { 3, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.DrumPad.Blue); }},
            { 4, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.DrumPad.Orange); }},
            { 5, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.DrumPad.Green); }},

            { ChartIOHelper.c_instrumentPlusOffset, (in NoteProcessParams noteProcessParams) => {
                ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.DrumPad.Kick, Note.Flags.DoubleKick);
            } },

            { ChartIOHelper.c_proDrumsOffset + 2, (in NoteProcessParams noteProcessParams) => {
                ProcessNoteOnEventAsNoteFlagToggle(noteProcessParams, (int)Note.DrumPad.Yellow, Note.Flags.ProDrums_Cymbal);
            } },
            { ChartIOHelper.c_proDrumsOffset + 3, (in NoteProcessParams noteProcessParams) => {
                ProcessNoteOnEventAsNoteFlagToggle(noteProcessParams, (int)Note.DrumPad.Blue, Note.Flags.ProDrums_Cymbal);
            } },
            { ChartIOHelper.c_proDrumsOffset + 4, (in NoteProcessParams noteProcessParams) => {
                ProcessNoteOnEventAsNoteFlagToggle(noteProcessParams, (int)Note.DrumPad.Orange, Note.Flags.ProDrums_Cymbal);
            } },
        };

        static readonly Dictionary<int, NoteEventProcessFn> GhlChartNoteNumberToProcessFnMap = new Dictionary<int, NoteEventProcessFn>()
        {
            { 0, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GHLiveGuitarFret.White1); }},
            { 1, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GHLiveGuitarFret.White2); }},
            { 2, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GHLiveGuitarFret.White3); }},
            { 3, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GHLiveGuitarFret.Black1); }},
            { 4, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GHLiveGuitarFret.Black2); }},
            { 8, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GHLiveGuitarFret.Black3); }},
            { 7, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsNote(noteProcessParams, (int)Note.GHLiveGuitarFret.Open); }},

            { 5, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsChordFlag(noteProcessParams, Note.Flags.Forced); }},
            { 6, (in NoteProcessParams noteProcessParams) => { ProcessNoteOnEventAsChordFlag(noteProcessParams, Note.Flags.Tap); }},
        };

        public static Song ReadChart(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                    throw new Exception("File does not exist");

                if (Path.GetExtension(filepath) == ".chart")
                {
                    Song song = new Song();

                    LoadChart(song, filepath);

                    return song;
                }
                else
                {
                    throw new Exception("Bad file type");
                }

            }
            catch (System.Exception e)
            {
                throw new Exception("Could not open file: " + e.Message);
            }
        }

        static void LoadChart(Song song, string filepath)
        {
            bool open = false;
            string dataName = string.Empty;

            List<string> dataStrings = new List<string>();
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
            StreamReader sr = File.OpenText(filepath);

            // Gather lines between {} brackets and submit data
            while (!sr.EndOfStream)
            {
                string trimmedLine = sr.ReadLine().Trim();
                if (trimmedLine.Length <= 0)
                    continue;

                if (trimmedLine[0] == '[' && trimmedLine[trimmedLine.Length - 1] == ']')
                {
                    dataName = trimmedLine;
                }
                else if (trimmedLine == "{")
                {
                    open = true;
                }
                else if (trimmedLine == "}")
                {
                    open = false;

                    // Submit data
                    SubmitChartData(song, dataName, dataStrings, filepath);

                    dataName = string.Empty;
                    dataStrings.Clear();
                }
                else
                {
                    if (open)
                    {
                        // Add data into the array
                        dataStrings.Add(trimmedLine);
                    }
                    else if (dataStrings.Count > 0 && dataName != string.Empty)
                    {
                        // Submit data
                        SubmitChartData(song, dataName, dataStrings, filepath);

                        dataName = string.Empty;
                        dataStrings.Clear();
                    }
                }
            }

            sr.Close();

#if TIMING_DEBUG
        Debug.Log("Chart file load time: " + (Time.realtimeSinceStartup - time));
        time = Time.realtimeSinceStartup;
#endif

            song.UpdateCache();
        }

        static void SubmitChartData(Song song, string dataName, List<string> stringData, string filePath = "")
        {
            switch (dataName)
            {
                case ChartIOHelper.c_dataBlockSong:
#if SONG_DEBUG
                Debug.Log("Loading chart properties");
#endif
                    SubmitDataSong(song, stringData, new FileInfo(filePath).Directory.FullName);
                    break;
                case ChartIOHelper.c_dataBlockSyncTrack:
#if SONG_DEBUG
                Debug.Log("Loading sync data");
#endif
                case ChartIOHelper.c_dataBlockEvents:
#if SONG_DEBUG
                Debug.Log("Loading events data");
#endif
                    SubmitDataGlobals(song, stringData);
                    break;
                default:
                    // Determine what difficulty
                    foreach (var kvPair in ChartIOHelper.c_trackNameToTrackDifficultyLookup)
                    {
                        if (Regex.IsMatch(dataName, string.Format(@"\[{0}.", kvPair.Key)))
                        {
                            Song.Difficulty chartDiff = kvPair.Value;
                            int instumentStringOffset = 1 + kvPair.Key.Length;

                            string instrumentKey = dataName.Substring(instumentStringOffset, dataName.Length - instumentStringOffset - 1);
                            Song.Instrument instrument;
                            if (ChartIOHelper.c_instrumentStrToEnumLookup.TryGetValue(instrumentKey, out instrument))
                            {
                                Song.Instrument instrumentParsingType;
                                if (!ChartIOHelper.c_instrumentParsingTypeLookup.TryGetValue(instrument, out instrumentParsingType))
                                {
                                    instrumentParsingType = Song.Instrument.Guitar;
                                }

                                LoadChart(song.GetChart(instrument, chartDiff), stringData, instrumentParsingType);
                            }
                            else
                            {
                                LoadUnrecognisedChart(song, dataName, stringData);
                            }

                            goto OnChartLoaded;
                        }
                    }

                    {
                        // Add to the unused chart list
                        LoadUnrecognisedChart(song, dataName, stringData);
                        goto OnChartLoaded;
                    }

                // Easy break out of loop
                OnChartLoaded:
                    return;
            }
        }

        static void LoadUnrecognisedChart(Song song, string dataName, List<string> stringData)
        {
            dataName = dataName.TrimStart('[');
            dataName = dataName.TrimEnd(']');
            Chart unrecognisedChart = new Chart(song, Song.Instrument.Unrecognised, dataName);
            LoadChart(unrecognisedChart, stringData, Song.Instrument.Unrecognised);
            song.unrecognisedCharts.Add(unrecognisedChart);
        }

        static void SubmitDataSong(Song song, List<string> stringData, string audioDirectory = "")
        {
#if SONG_DEBUG
        Debug.Log("Loading song properties");
#endif
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif

            Metadata metaData = song.metaData;

            try
            {
                foreach (string line in stringData)
                {
                    // Name = "5000 Robots"
                    if (ChartIOHelper.MetaData.name.regex.IsMatch(line))
                    {
                        metaData.name = ChartIOHelper.MetaData.ParseAsString(line);
                    }

                    // Artist = "TheEruptionOffer"
                    else if (ChartIOHelper.MetaData.artist.regex.IsMatch(line))
                    {
                        metaData.artist = ChartIOHelper.MetaData.ParseAsString(line);
                    }

                    // Charter = "TheEruptionOffer"
                    else if (ChartIOHelper.MetaData.charter.regex.IsMatch(line))
                    {
                        metaData.charter = ChartIOHelper.MetaData.ParseAsString(line);
                    }

                    // Album = "Rockman Holic"
                    else if (ChartIOHelper.MetaData.album.regex.IsMatch(line))
                    {
                        metaData.album = ChartIOHelper.MetaData.ParseAsString(line);
                    }

                    // Offset = 0
                    else if (ChartIOHelper.MetaData.offset.regex.IsMatch(line))
                    {
                        song.offset = ChartIOHelper.MetaData.ParseAsFloat(line);
                    }

                    // Resolution = 192
                    else if (ChartIOHelper.MetaData.resolution.regex.IsMatch(line))
                    {
                        song.resolution = ChartIOHelper.MetaData.ParseAsShort(line);
                    }

                    // Player2 = bass
                    else if (ChartIOHelper.MetaData.player2.regex.IsMatch(line))
                    {
                        string[] instrumentTypes = { "Bass", "Rhythm" };
                        string split = line.Split('=')[1].Trim();

                        foreach (string instrument in instrumentTypes)
                        {
                            if (split.Equals(instrument, System.StringComparison.InvariantCultureIgnoreCase))
                            {
                                metaData.player2 = instrument;
                                break;
                            }
                        }
                    }

                    // Difficulty = 0
                    else if (ChartIOHelper.MetaData.difficulty.regex.IsMatch(line))
                    {
                        metaData.difficulty = int.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                    }

                    // Length = 300
                    else if (ChartIOHelper.MetaData.length.regex.IsMatch(line))
                    {
                        song.manualLength = ChartIOHelper.MetaData.ParseAsFloat(line);
                    }

                    // PreviewStart = 0.00
                    else if (ChartIOHelper.MetaData.previewStart.regex.IsMatch(line))
                    {
                        metaData.previewStart = ChartIOHelper.MetaData.ParseAsFloat(line);
                    }

                    // PreviewEnd = 0.00
                    else if (ChartIOHelper.MetaData.previewEnd.regex.IsMatch(line))
                    {
                        metaData.previewEnd = ChartIOHelper.MetaData.ParseAsFloat(line);
                    }

                    // Genre = "rock"
                    else if (ChartIOHelper.MetaData.genre.regex.IsMatch(line))
                    {
                        metaData.genre = ChartIOHelper.MetaData.ParseAsString(line);
                    }

                    // MediaType = "cd"
                    else if (ChartIOHelper.MetaData.mediaType.regex.IsMatch(line))
                    {
                        metaData.mediatype = ChartIOHelper.MetaData.ParseAsString(line);
                    }

                    else if (ChartIOHelper.MetaData.year.regex.IsMatch(line))
                        metaData.year = Regex.Replace(ChartIOHelper.MetaData.ParseAsString(line), @"\D", "");

                    // MusicStream = "ENDLESS REBIRTH.ogg"
                    else if (ChartIOHelper.MetaData.musicStream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Song, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.guitarStream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Guitar, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.bassStream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Bass, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.rhythmStream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Rhythm, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.drumStream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Drum, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.drum2Stream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Drums_2, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.drum3Stream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Drums_3, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.drum4Stream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Drums_4, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.vocalStream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Vocals, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.keysStream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Keys, line, audioDirectory);
                    }
                    else if (ChartIOHelper.MetaData.crowdStream.regex.IsMatch(line))
                    {
                        AudioLoadFromChart(song, Song.AudioInstrument.Crowd, line, audioDirectory);
                    }
                }

#if TIMING_DEBUG
            Debug.Log("Song properties load time: " + (Time.realtimeSinceStartup - time));
#endif
            }
            catch (System.Exception e)
            {
                Logger.LogException(e, "Error when reading chart metadata");
            }
        }

        static void AudioLoadFromChart(Song song, Song.AudioInstrument streamAudio, string line, string audioDirectory)
        {
            string audioFilepath = ChartIOHelper.MetaData.ParseAsString(line);

            // Check if it's already the full path. If not, make it relative to the chart file.
            if (!File.Exists(audioFilepath))
                audioFilepath = Path.Combine(audioDirectory, audioFilepath);

            if (File.Exists(audioFilepath) && Utility.validateExtension(audioFilepath, Globals.validAudioExtensions))
                song.SetAudioLocation(streamAudio, Path.GetFullPath(audioFilepath));
        }

        static void SubmitDataGlobals(Song song, List<string> stringData)
        {
            const int TEXT_POS_TICK = 0;
            const int TEXT_POS_EVENT_TYPE = 2;
            const int TEXT_POS_DATA_1 = 3;

#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif

            List<Anchor> anchorData = new List<Anchor>();

            foreach (string line in stringData)
            {
                string[] stringSplit = line.Split(' ');
                uint tick;
                string eventType;
                if (stringSplit.Length > TEXT_POS_DATA_1 && uint.TryParse(stringSplit[TEXT_POS_TICK], out tick))
                {
                    eventType = stringSplit[TEXT_POS_EVENT_TYPE];
                    eventType = eventType.ToLower();
                }
                else
                    continue;

                switch (eventType)
                {
                    case ("ts"):
                        uint numerator;
                        uint denominator = 2;

                        if (!uint.TryParse(stringSplit[TEXT_POS_DATA_1], out numerator))
                            continue;

                        if (stringSplit.Length > TEXT_POS_DATA_1 + 1 && !uint.TryParse(stringSplit[TEXT_POS_DATA_1 + 1], out denominator))
                            continue;

                        song.Add(new TimeSignature(tick, numerator, (uint)(Mathf.Pow(2, denominator))), false);
                        break;
                    case ("b"):
                        uint value;
                        if (!uint.TryParse(stringSplit[TEXT_POS_DATA_1], out value))
                            continue;

                        song.Add(new BPM(tick, value), false);
                        break;
                    case ("e"):
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        int startIndex = TEXT_POS_DATA_1;
                        bool isSection = false;

                        if (stringSplit.Length > TEXT_POS_DATA_1 + 1 && stringSplit[TEXT_POS_DATA_1] == "\"section")
                        {
                            startIndex = TEXT_POS_DATA_1 + 1;
                            isSection = true;
                        }

                        for (int i = startIndex; i < stringSplit.Length; ++i)
                        {
                            sb.Append(stringSplit[i].Trim('"'));
                            if (i < stringSplit.Length - 1)
                                sb.Append(" ");
                        }

                        if (isSection)
                            song.Add(new Section(sb.ToString(), tick), false);
                        else
                            song.Add(new Event(sb.ToString(), tick), false);

                        break;
                    case ("a"):
                        ulong anchorValue;
                        if (ulong.TryParse(stringSplit[TEXT_POS_DATA_1], out anchorValue))
                        {
                            Anchor a;
                            a.tick = tick;
                            a.anchorTime = (float)(anchorValue / 1000000.0d);
                            anchorData.Add(a);
                        }
                        break;
                    default:
                        break;
                }
            }

            BPM[] bpms = song.syncTrack.OfType<BPM>().ToArray();        // BPMs are currently uncached
            foreach (Anchor anchor in anchorData)
            {
                int arrayPos = SongObjectHelper.FindClosestPosition(anchor.tick, bpms);
                if (bpms[arrayPos].tick == anchor.tick)
                {
                    bpms[arrayPos].anchor = anchor.anchorTime;
                }
                else
                {
                    // Create a new anchored bpm
                    uint value;
                    if (bpms[arrayPos].tick > anchor.tick)
                        value = bpms[arrayPos - 1].value;
                    else
                        value = bpms[arrayPos].value;

                    BPM anchoredBPM = new BPM(anchor.tick, value);
                    anchoredBPM.anchor = anchor.anchorTime;
                }
            }
#if TIMING_DEBUG
        Debug.Log("Synctrack load time: " + (Time.realtimeSinceStartup - time));
#endif
        }

        /*************************************************************************************
            Chart Loading
        **************************************************************************************/

        static int FastStringToIntParse(string str, int index, int length)
        {
            // https://cc.davelozinski.com/c-sharp/fastest-way-to-convert-a-string-to-an-int
            int y = 0;
            for (int i = index; i < index + length; i++)
                y = y * 10 + (str[i] - '0');

            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AdvanceNextWord(string line, ref int startIndex, ref int length)
        {
            length = 0;
            while (startIndex < line.Length && line[startIndex] == ' ') { ++startIndex; };
            while ((startIndex + ++length) < line.Length && line[startIndex + length] != ' ') ;
        }

        public static void LoadChart(Chart chart, IList<string> data, Song.Instrument instrument = Song.Instrument.Guitar)
        {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
            List<NoteFlag> flags = new List<NoteFlag>();
            List<NoteEventProcessFn> postNotesAddedProcessList = new List<NoteEventProcessFn>();

            NoteProcessParams processParams = new NoteProcessParams()
            {
                chart = chart,
                postNotesAddedProcessList = postNotesAddedProcessList
            };

            chart.SetCapacity(data.Count);

            Dictionary<int, NoteEventProcessFn> noteProcessDict = GetNoteProcessDict(chart.gameMode);

            try
            {
                // Load notes, collect flags
                foreach (string line in data)
                {
                    try
                    {
                        int stringStartIndex = 0;
                        int stringLength = 0;

                        // Advance to tick
                        AdvanceNextWord(line, ref stringStartIndex, ref stringLength);
                        uint tick = (uint)FastStringToIntParse(line, stringStartIndex, stringLength);

                        // Advance to equality
                        {
                            stringStartIndex += stringLength;
                            AdvanceNextWord(line, ref stringStartIndex, ref stringLength);
                        }

                        // Advance to type
                        {
                            stringStartIndex += stringLength;
                            AdvanceNextWord(line, ref stringStartIndex, ref stringLength);
                        }

                        switch (line[stringStartIndex])    // Note this will need to be changed if keys are ever greater than 1 character long
                        {
                            case ('N'):
                            case ('n'):
                                {
                                    // Advance to note number
                                    {
                                        stringStartIndex += stringLength;
                                        AdvanceNextWord(line, ref stringStartIndex, ref stringLength);
                                    }
                                    int fret_type = FastStringToIntParse(line, stringStartIndex, stringLength);

                                    // Advance to note length
                                    {
                                        stringStartIndex += stringLength;
                                        AdvanceNextWord(line, ref stringStartIndex, ref stringLength);
                                    }
                                    uint length = (uint)FastStringToIntParse(line, stringStartIndex, stringLength);

                                    if (instrument == Song.Instrument.Unrecognised)
                                    {
                                        Note newNote = new Note(tick, fret_type, length);
                                        chart.Add(newNote, false);
                                    }
                                    else
                                    {
                                        NoteEventProcessFn processFn;
                                        if (noteProcessDict.TryGetValue(fret_type, out processFn))
                                        {
                                            NoteEvent noteEvent = new NoteEvent() { tick = tick, noteNumber = fret_type, length = length };
                                            processParams.noteEvent = noteEvent;
                                            processFn(processParams);
                                        }
                                    }

                                    break;
                                }

                            case ('S'):
                            case ('s'):
                                {
                                    // Advance to note number
                                    {
                                        stringStartIndex += stringLength;
                                        AdvanceNextWord(line, ref stringStartIndex, ref stringLength);
                                    }
                                    int fret_type = FastStringToIntParse(line, stringStartIndex, stringLength);

                                    if (fret_type != 2)
                                        continue;

                                    // Advance to note length
                                    {
                                        stringStartIndex += stringLength;
                                        AdvanceNextWord(line, ref stringStartIndex, ref stringLength);
                                    }
                                    uint length = (uint)FastStringToIntParse(line, stringStartIndex, stringLength);

                                    chart.Add(new Starpower(tick, length), false);
                                    break;
                                }
                            case ('E'):
                            case ('e'):
                                {
                                    // Advance to event
                                    {
                                        stringStartIndex += stringLength;
                                        AdvanceNextWord(line, ref stringStartIndex, ref stringLength);
                                    }
                                    string eventName = line.Substring(stringStartIndex, stringLength);
                                    chart.Add(new ChartEvent(tick, eventName), false);
                                    break;
                                }
                            default:
                                break;
                        }

                    }
                    catch (System.Exception e)
                    {
                        Logger.LogException(e, "Error parsing chart reader line \"" + line);
                    }
                }
                chart.UpdateCache();

                foreach (var fn in postNotesAddedProcessList)
                {
                    fn(processParams);
                }

#if TIMING_DEBUG
            Debug.Log("Chart load time: " + (Time.realtimeSinceStartup - time));
#endif
            }
            catch (System.Exception e)
            {
                // Bad load, most likely a parsing error
                Logger.LogException(e, "Error parsing chart reader chart data");
                chart.Clear();
            }
        }

        static Dictionary<int, NoteEventProcessFn> GetNoteProcessDict(Chart.GameMode gameMode)
        {
            switch (gameMode)
            {
                case Chart.GameMode.GHLGuitar:
                    {
                        return GhlChartNoteNumberToProcessFnMap;
                    }
                case Chart.GameMode.Drums:
                    {
                        return DrumsChartNoteNumberToProcessFnMap;
                    }

                default: break;
            }

            return GuitarChartNoteNumberToProcessFnMap;
        }

        static void ProcessNoteOnEventAsNote(in NoteProcessParams noteProcessParams, int ingameFret, Note.Flags defaultFlags = Note.Flags.None)
        {
            Chart chart = noteProcessParams.chart;

            NoteEvent noteEvent = noteProcessParams.noteEvent;
            var tick = noteEvent.tick;
            var sus = noteEvent.length;

            Note newNote = new Note(tick, ingameFret, sus, defaultFlags);
            chart.Add(newNote, false);
        }

        static void ProcessNoteOnEventAsChordFlag(in NoteProcessParams noteProcessParams, Note.Flags flag)
        {
            var flagEvent = noteProcessParams.noteEvent;

            // Delay the actual processing once all the notes are actually in
            noteProcessParams.postNotesAddedProcessList.Add((in NoteProcessParams processParams) =>
            {
                ProcessNoteOnEventAsChordFlagPostDelay(processParams, flagEvent, flag);
            });
        }

        static void ProcessNoteOnEventAsChordFlagPostDelay(in NoteProcessParams noteProcessParams, NoteEvent noteEvent, Note.Flags flag)
        {
            Chart chart = noteProcessParams.chart;

            int index, length;
            SongObjectHelper.FindObjectsAtPosition(noteEvent.tick, chart.notes, out index, out length);
            if (length > 0)
            {
                GroupAddFlags(chart.notes, flag, index, length);
            }
        }

        static void ProcessNoteOnEventAsNoteFlagToggle(in NoteProcessParams noteProcessParams, int rawNote, Note.Flags flag)
        {
            var flagEvent = noteProcessParams.noteEvent;

            // Delay the actual processing once all the notes are actually in
            noteProcessParams.postNotesAddedProcessList.Add((in NoteProcessParams processParams) =>
            {
                ProcessNoteOnEventAsNoteFlagTogglePostDelay(processParams, rawNote, flagEvent, flag);
            });
        }

        static void ProcessNoteOnEventAsNoteFlagTogglePostDelay(in NoteProcessParams noteProcessParams, int rawNote, NoteEvent noteEvent, Note.Flags flag)
        {
            Chart chart = noteProcessParams.chart;

            int index, length;
            SongObjectHelper.FindObjectsAtPosition(noteEvent.tick, chart.notes, out index, out length);
            if (length > 0)
            {
                for (int i = index; i < index + length; ++i)
                {
                    Note note = chart.notes[i];
                    if (note.rawNote == rawNote)
                    {
                        note.flags ^= flag;
                    }
                }
            }
        }

        static void GroupAddFlags(IList<Note> notes, Note.Flags flag, int index, int length)
        {
            for (int i = index; i < index + length; ++i)
            {
                notes[i].flags = notes[i].flags | flag;
            }
        }
    }
}
