using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

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

        public NoteFlag(uint tick, Note.Flags flag)
        {
            this.tick = tick;
            this.flag = flag;
        }
    }

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

        LoadAllAudioClips();
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
                if (ChartIOHelper.MetaData.nameRegex.IsMatch(line))
                {
                    metaData.name = ChartIOHelper.MetaData.ParseAsString(line);
                }

                // Artist = "TheEruptionOffer"
                else if (ChartIOHelper.MetaData.artistRegex.IsMatch(line))
                {
                    metaData.artist = ChartIOHelper.MetaData.ParseAsString(line);
                }

                // Charter = "TheEruptionOffer"
                else if (ChartIOHelper.MetaData.charterRegex.IsMatch(line))
                {
                    metaData.charter = ChartIOHelper.MetaData.ParseAsString(line);
                }

                // Album = "Rockman Holic"
                else if (ChartIOHelper.MetaData.albumRegex.IsMatch(line))
                {
                    metaData.album = ChartIOHelper.MetaData.ParseAsString(line);
                }

                // Offset = 0
                else if (ChartIOHelper.MetaData.offsetRegex.IsMatch(line))
                {
                    song.offset = ChartIOHelper.MetaData.ParseAsFloat(line);
                }

                // Resolution = 192
                else if (ChartIOHelper.MetaData.resolutionRegex.IsMatch(line))
                {
                    song.resolution = ChartIOHelper.MetaData.ParseAsShort(line);
                }

                // Player2 = bass
                else if (ChartIOHelper.MetaData.player2TypeRegex.IsMatch(line))
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
                else if (ChartIOHelper.MetaData.difficultyRegex.IsMatch(line))
                {
                    metaData.difficulty = int.Parse(Regex.Matches(line, @"\d+")[0].ToString());
                }

                // Length = 300
                else if (ChartIOHelper.MetaData.lengthRegex.IsMatch(line))
                {
                    song.manualLength = true;
                    song.length = ChartIOHelper.MetaData.ParseAsFloat(line);
                }

                // PreviewStart = 0.00
                else if (ChartIOHelper.MetaData.previewStartRegex.IsMatch(line))
                {
                    metaData.previewStart = ChartIOHelper.MetaData.ParseAsFloat(line);
                }

                // PreviewEnd = 0.00
                else if (ChartIOHelper.MetaData.previewEndRegex.IsMatch(line))
                {
                    metaData.previewEnd = ChartIOHelper.MetaData.ParseAsFloat(line);
                }

                // Genre = "rock"
                else if (ChartIOHelper.MetaData.genreRegex.IsMatch(line))
                {
                    metaData.genre = ChartIOHelper.MetaData.ParseAsString(line);
                }

                // MediaType = "cd"
                else if (ChartIOHelper.MetaData.mediaTypeRegex.IsMatch(line))
                {
                    metaData.mediatype = ChartIOHelper.MetaData.ParseAsString(line);
                }

                else if (ChartIOHelper.MetaData.yearRegex.IsMatch(line))
                    metaData.year = Regex.Replace(ChartIOHelper.MetaData.ParseAsString(line), @"\D", "");

                // MusicStream = "ENDLESS REBIRTH.ogg"
                else if (ChartIOHelper.MetaData.musicStreamRegex.IsMatch(line))
                {
                    AudioLoadFromChart(song, Song.AudioInstrument.Song, line, audioDirectory);
                }
                else if (ChartIOHelper.MetaData.guitarStreamRegex.IsMatch(line))
                {
                    AudioLoadFromChart(song, Song.AudioInstrument.Guitar, line, audioDirectory);
                }
                else if (ChartIOHelper.MetaData.bassStreamRegex.IsMatch(line))
                {
                    AudioLoadFromChart(song, Song.AudioInstrument.Bass, line, audioDirectory);
                }
                else if (ChartIOHelper.MetaData.rhythmStreamRegex.IsMatch(line))
                {
                    AudioLoadFromChart(song, Song.AudioInstrument.Rhythm, line, audioDirectory);
                }
                else if (ChartIOHelper.MetaData.drumStreamRegex.IsMatch(line))
                {
                    AudioLoadFromChart(song, Song.AudioInstrument.Drum, line, audioDirectory);
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
            audioFilepath = audioDirectory + "\\" + audioFilepath;

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


    public static void LoadChart(Chart chart, List<string> data, Song.Instrument instrument = Song.Instrument.Guitar)
    {
        LoadChart(chart, data.ToArray(), instrument);
    }

    public static void LoadChart(Chart chart, string[] data, Song.Instrument instrument = Song.Instrument.Guitar)
    {
#if TIMING_DEBUG
        float time = Time.realtimeSinceStartup;
#endif
        List<NoteFlag> flags = new List<NoteFlag>();

        chart.SetCapacity(data.Length);

        const int SPLIT_POSITION = 0;
        const int SPLIT_EQUALITY = 1;
        const int SPLIT_TYPE = 2;
        const int SPLIT_VALUE = 3;
        const int SPLIT_LENGTH = 4;

        try
        {
            // Load notes, collect flags
            foreach (string line in data)
            {
                try
                {
                    string[] splitString = line.Split(' ');
                    uint tick = uint.Parse(splitString[SPLIT_POSITION]);
                    string type = splitString[SPLIT_TYPE].ToLower();

                    switch (type)
                    {
                        case ("n"):
                            // Split string to get note information
                            string[] digits = splitString;

                            int fret_type = int.Parse(digits[SPLIT_VALUE]);
                            uint length = uint.Parse(digits[SPLIT_LENGTH]);

                            if (instrument == Song.Instrument.Unrecognised)
                            {
                                Note newNote = new Note(tick, fret_type, length);
                                chart.Add(newNote, false);
                            }
                            else if (instrument == Song.Instrument.Drums)
                                LoadDrumNote(chart, tick, fret_type, length, flags);
                            else if (instrument == Song.Instrument.GHLiveGuitar || instrument == Song.Instrument.GHLiveBass)
                                LoadGHLiveNote(chart, tick, fret_type, length, flags);
                            else
                                LoadStandardNote(chart, tick, fret_type, length, flags);
                            break;

                        case ("s"):
                            fret_type = int.Parse(splitString[SPLIT_VALUE]);

                            if (fret_type != 2)
                                continue;

                            length = uint.Parse(splitString[SPLIT_LENGTH]);

                            chart.Add(new Starpower(tick, length), false);
                            break;

                        case ("e"):
                            string[] strings = splitString;
                            string eventName = strings[SPLIT_VALUE];
                            chart.Add(new ChartEvent(tick, eventName), false);
                            break;
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

            // Load flags
            foreach (NoteFlag flag in flags)
            {
                int index, length;
                SongObjectHelper.FindObjectsAtPosition(flag.tick, chart.notes, out index, out length);
                if (length > 0)
                    NoteFunctions.GroupAddFlags(chart.notes, flag.flag, index, length);
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

    static void LoadNote(Chart chart, uint tick, int noteNumber, uint length, List<NoteFlag> flagsList
        , Dictionary<int, int> chartFileNoteToRawNoteLookup
        , Dictionary<int, Note.Flags> chartFileNoteToFlagLookup
        , Dictionary<int, Note.Flags> rawNoteDefaultFlagsLookup
    )
    {
        Debug.Assert(chartFileNoteToRawNoteLookup != null, "Must provide a note lookup dictionary");
        // Load chart file note to a raw note
        {
            int noteFret;
            if (chartFileNoteToRawNoteLookup.TryGetValue(noteNumber, out noteFret))
            {
                // Optional. Load any default flags that come with notes. Useful for automatically attaching cymbal flags for pro drums
                Note.Flags flags;
                if (rawNoteDefaultFlagsLookup == null || !rawNoteDefaultFlagsLookup.TryGetValue(noteFret, out flags))
                {
                    flags = Note.Flags.None;
                }

                Note newNote = new Note(tick, noteFret, length);
                chart.Add(newNote, false);
            }
        }

        // Optional. Load any flags that are parsed on a seperate tick
        if (chartFileNoteToFlagLookup != null)
        {
            Note.Flags flags;
            if (chartFileNoteToFlagLookup.TryGetValue(noteNumber, out flags))
            {
                NoteFlag parsedFlag = new NoteFlag(tick, flags);
                flagsList.Add(parsedFlag);
            }
        }
    }

    static void LoadStandardNote(Chart chart, uint tick, int noteNumber, uint length, List<NoteFlag> flagsList)
    {
        LoadNote(chart, tick, noteNumber, length, flagsList, ChartIOHelper.c_guitarNoteNumLookup, ChartIOHelper.c_guitarFlagNumLookup, null);
    }

    static void LoadDrumNote(Chart chart, uint tick, int noteNumber, uint length, List<NoteFlag> flagsList)
    {
        LoadNote(chart, tick, noteNumber, length, flagsList, ChartIOHelper.c_drumNoteNumLookup, null, ChartIOHelper.c_drumNoteDefaultFlagsLookup);
    }

    static void LoadGHLiveNote(Chart chart, uint tick, int noteNumber, uint length, List<NoteFlag> flagsList)
    {
        LoadNote(chart, tick, noteNumber, length, flagsList, ChartIOHelper.c_ghlNoteNumLookup, ChartIOHelper.c_ghlFlagNumLookup, null);
    }
}