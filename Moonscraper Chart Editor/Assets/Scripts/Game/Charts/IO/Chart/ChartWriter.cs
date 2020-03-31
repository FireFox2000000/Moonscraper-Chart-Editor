﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
using System.Text;

public class ChartWriter
{
    string path;

    public ChartWriter(string path)
    {
        this.path = path;
    }

    static readonly string s_chartSectionHeaderFormat = "[{0}]" + Globals.LINE_ENDING + "{{" + Globals.LINE_ENDING;
    static readonly string s_chartSectionFooter = "}" + Globals.LINE_ENDING;
    static readonly string s_chartHeaderSong = string.Format(s_chartSectionHeaderFormat, "Song");
    static readonly string s_chartHeaderSyncTrack = string.Format(s_chartSectionHeaderFormat, "SyncTrack");
    static readonly string s_chartHeaderEvents = string.Format(s_chartSectionHeaderFormat, "Events");
    static readonly string s_chartHeaderTrackFormat = "[{0}{1}]" + Globals.LINE_ENDING + "{{" + Globals.LINE_ENDING;

    // Bi-directional dict lookups
    static readonly Dictionary<Song.Instrument, string> c_instrumentToStrLookup = ChartIOHelper.c_instrumentStrToEnumLookup.ToDictionary((i) => i.Value, (i) => i.Key);
    static readonly Dictionary<Song.Difficulty, string> c_difficultyToTrackNameLookup = ChartIOHelper.c_trackNameToTrackDifficultyLookup.ToDictionary((i) => i.Value, (i) => i.Key);

    static readonly Dictionary<int, int> c_guitarNoteToSaveNumberLookup = ChartIOHelper.c_guitarNoteNumLookup.ToDictionary((i) => i.Value, (i) => i.Key);
    static readonly Dictionary<int, int> c_drumNoteToSaveNumberLookup = ChartIOHelper.c_drumNoteNumLookup.ToDictionary((i) => i.Value, (i) => i.Key);
    static readonly Dictionary<int, int> c_ghlNoteToSaveNumberLookup = ChartIOHelper.c_ghlNoteNumLookup.ToDictionary((i) => i.Value, (i) => i.Key);
    static readonly Dictionary<Note.Flags, int> c_guitarFlagToNumLookup = ChartIOHelper.c_guitarFlagNumLookup.ToDictionary((i) => i.Value, (i) => i.Key);

    delegate void WriteAudioStreamSaveString(Song.AudioInstrument audio, string saveFormat);

    // Link the method with which we should write our objects out with
    struct SongObjectWriteParameters
    {
        public uint scaledTick;
        public float resolutionScaleRatio;
        public Song.Instrument instrument;
        public ExportOptions exportOptions;
    }
    delegate void AppendSongObjectData(SongObject so, in SongObjectWriteParameters writeParameters, StringBuilder output);
    static readonly Dictionary<SongObject.ID, AppendSongObjectData> c_songObjectWriteFnLookup = new Dictionary<SongObject.ID, AppendSongObjectData>()
    {
        { SongObject.ID.BPM, AppendBpmData },
        { SongObject.ID.TimeSignature, AppendTsData },
        { SongObject.ID.Event, AppendEventData },
        { SongObject.ID.Section, AppendSectionData },
        { SongObject.ID.Starpower, AppendStarpowerData },
        { SongObject.ID.ChartEvent, AppendChartEventData },
        { SongObject.ID.Note, AppendNoteData },
    };

    public void Write(Song song, ExportOptions exportOptions, out string errorList)
    {
        song.UpdateCache();
        errorList = string.Empty;
        string saveString = string.Empty;

        try
        {
            // Song properties
            saveString += s_chartHeaderSong;
            saveString += GetPropertiesStringWithoutAudio(song, exportOptions);

            WriteAudioStreamSaveString WriteSaveAudioString = (audio, saveFormat) => {
                string audioString;
                string audioLocation = song.GetAudioLocation(audio);

                if (audioLocation != string.Empty && Path.GetDirectoryName(audioLocation).Replace("\\", "/") == Path.GetDirectoryName(path).Replace("\\", "/"))
                    audioString = Path.GetFileName(audioLocation);
                else
                    audioString = audioLocation;

                if (!string.IsNullOrEmpty(audioString))
                    saveString += string.Format(saveFormat, audioString);
            };

            // Song audio
            WriteSaveAudioString(Song.AudioInstrument.Song, ChartIOHelper.MetaData.musicStream.saveFormat);
            WriteSaveAudioString(Song.AudioInstrument.Guitar, ChartIOHelper.MetaData.guitarStream.saveFormat);
            WriteSaveAudioString(Song.AudioInstrument.Bass, ChartIOHelper.MetaData.bassStream.saveFormat);
            WriteSaveAudioString(Song.AudioInstrument.Rhythm, ChartIOHelper.MetaData.rhythmStream.saveFormat);
			WriteSaveAudioString(Song.AudioInstrument.Keys, ChartIOHelper.MetaData.keysStream.saveFormat);
            WriteSaveAudioString(Song.AudioInstrument.Drum, ChartIOHelper.MetaData.drumStream.saveFormat);
			WriteSaveAudioString(Song.AudioInstrument.Drums_2, ChartIOHelper.MetaData.drum2Stream.saveFormat);
			WriteSaveAudioString(Song.AudioInstrument.Drums_3, ChartIOHelper.MetaData.drum3Stream.saveFormat);
			WriteSaveAudioString(Song.AudioInstrument.Drums_4, ChartIOHelper.MetaData.drum4Stream.saveFormat);
			WriteSaveAudioString(Song.AudioInstrument.Vocals, ChartIOHelper.MetaData.vocalStream.saveFormat);
			WriteSaveAudioString(Song.AudioInstrument.Crowd, ChartIOHelper.MetaData.crowdStream.saveFormat);
       
            saveString += s_chartSectionFooter;
        }
        catch (System.Exception e)
        {
            string error = Logger.LogException(e, "Error with saving song properties");
            errorList += error + Globals.LINE_ENDING;

            saveString = string.Empty;  // Clear all the song properties because we don't want braces left open, which will screw up the loading of the chart
        }

        // SyncTrack
        {
            saveString += s_chartHeaderSyncTrack;
            if (exportOptions.tickOffset > 0)
            {
                List<SongObject> defaultsList = new List<SongObject>()
                {
                    new BPM(), new TimeSignature()
                };

                saveString += GetSaveString(song, defaultsList, exportOptions, ref errorList);
            }

            saveString += GetSaveString(song, song.syncTrack, exportOptions, ref errorList);
            saveString += s_chartSectionFooter;
        }

        // Events
        {
            saveString += s_chartHeaderEvents;
            saveString += GetSaveString(song, song.eventsAndSections, exportOptions, ref errorList);
            saveString += s_chartSectionFooter;
        }

        // Charts      
        foreach (Song.Instrument instrument in EnumX<Song.Instrument>.Values)
        {
            string instrumentSaveString = string.Empty;
            if (!c_instrumentToStrLookup.TryGetValue(instrument, out instrumentSaveString))
            {
                continue;
            }

            foreach (Song.Difficulty difficulty in EnumX<Song.Difficulty>.Values)
            {
                string difficultySaveString = difficulty.ToString();
                if (!c_difficultyToTrackNameLookup.TryGetValue(difficulty, out difficultySaveString))
                {
                    Debug.Assert(false, "Unable to find string for difficulty " + difficulty.ToString());
                    continue;
                }

                string chartString = GetSaveString(song, song.GetChart(instrument, difficulty).chartObjects, exportOptions, ref errorList, instrument);

                if (chartString == string.Empty)
                {
                    if (exportOptions.copyDownEmptyDifficulty)
                    {
                        Song.Difficulty chartDiff = difficulty;
                        bool exit = false;
                        while (chartString == string.Empty)
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
                                    exit = true;
                                    break;
                            }

                            chartString = GetSaveString(song, song.GetChart(instrument, chartDiff).chartObjects, exportOptions, ref errorList, instrument);

                            if (exit)
                                break;
                        }

                        if (exit)
                            continue;
                    }
                    else
                        continue;

                }

                saveString += string.Format(s_chartHeaderTrackFormat, difficultySaveString, instrumentSaveString);
                saveString += chartString;
                saveString += s_chartSectionFooter;
            }
        }

        // Unrecognised charts
        foreach (Chart chart in song.unrecognisedCharts)
        {
            string chartString = GetSaveString(song, chart.chartObjects, exportOptions, ref errorList, Song.Instrument.Unrecognised);

            saveString += string.Format(s_chartSectionHeaderFormat, chart.name);
            saveString += chartString;
            saveString += s_chartSectionFooter;
        }

        try
        {
            // Save to file
            File.WriteAllText(path, saveString, System.Text.Encoding.UTF8);
        }
        catch (Exception e)
        {
            Logger.LogException(e, "Error when writing text to file");
        }
    }

    string GetPropertiesStringWithoutAudio(Song song, ExportOptions exportOptions)
    {
        string saveString = string.Empty;
        if (exportOptions.targetResolution <= 0)
            exportOptions.targetResolution = song.resolution;

        Metadata metaData = song.metaData;

        // Song properties  
        if (metaData.name != string.Empty)
            saveString += string.Format(ChartIOHelper.MetaData.name.saveFormat, metaData.name);
        if (metaData.artist != string.Empty)
            saveString += string.Format(ChartIOHelper.MetaData.artist.saveFormat, metaData.artist);
        if (metaData.charter != string.Empty)
            saveString += string.Format(ChartIOHelper.MetaData.charter.saveFormat, metaData.charter);
        if (metaData.album != string.Empty)
            saveString += string.Format(ChartIOHelper.MetaData.album.saveFormat, metaData.album);
        if (metaData.year != string.Empty)
            saveString += string.Format(ChartIOHelper.MetaData.year.saveFormat, metaData.year);
        saveString += string.Format(ChartIOHelper.MetaData.offset.saveFormat, song.offset);

        saveString += string.Format(ChartIOHelper.MetaData.resolution.saveFormat, exportOptions.targetResolution);
        if (metaData.player2 != string.Empty)
            saveString += string.Format(ChartIOHelper.MetaData.player2.saveFormat, metaData.player2.ToLower());
        saveString += string.Format(ChartIOHelper.MetaData.difficulty.saveFormat, metaData.difficulty);
        if (song.manualLength.HasValue)
            saveString += string.Format(ChartIOHelper.MetaData.length.saveFormat, song.manualLength.Value);
        saveString += string.Format(ChartIOHelper.MetaData.previewStart.saveFormat, metaData.previewStart);
        saveString += string.Format(ChartIOHelper.MetaData.previewEnd.saveFormat, metaData.previewEnd);
        if (metaData.genre != string.Empty)
            saveString += string.Format(ChartIOHelper.MetaData.genre.saveFormat, metaData.genre);
        if (metaData.mediatype != string.Empty)
            saveString += string.Format(ChartIOHelper.MetaData.mediaType.saveFormat, metaData.mediatype);

        return saveString;
    }

    string GetSaveString<T>(Song song, IList<T> list, ExportOptions exportOptions, ref string out_errorList, Song.Instrument instrument = Song.Instrument.Guitar) where T : SongObject
    {
        System.Text.StringBuilder saveString = new System.Text.StringBuilder();

        float resolutionScaleRatio = song.ResolutionScaleRatio(exportOptions.targetResolution);

        SongObjectWriteParameters writeParameters = new SongObjectWriteParameters();
        writeParameters.resolutionScaleRatio = resolutionScaleRatio;
        writeParameters.instrument = instrument;
        writeParameters.exportOptions = exportOptions;

        for (int i = 0; i < list.Count; ++i)
        {
            SongObject songObject = list[i];
            try
            {
                uint tick = (uint)Mathf.Round(songObject.tick * resolutionScaleRatio) + exportOptions.tickOffset;
                saveString.Append(Globals.TABSPACE + tick);

                writeParameters.scaledTick = tick;

                AppendSongObjectData writeMethod;
                if (c_songObjectWriteFnLookup.TryGetValue((SongObject.ID)songObject.classID, out writeMethod))
                {
                    writeMethod(songObject, writeParameters, saveString);
                }
                else
                {
                    throw new Exception("Method not defined. Unable to write data for object " + ((SongObject.ID)songObject.classID).ToString());
                }

                saveString.Append(Globals.LINE_ENDING);
            }
            catch (System.Exception e)
            {
                string error = Logger.LogException(e, "Error with saving object #" + i + " as " + songObject);
                out_errorList += error + Globals.LINE_ENDING;
            }
        }

        return saveString.ToString();
    }

    static int GetSaveNoteNumber(int note, Dictionary<int, int> lookupDict)
    {
        int noteNumber;
        if (!lookupDict.TryGetValue(note, out noteNumber))
        {
            noteNumber = 0;
        }

        return noteNumber;
    }

    static int GetStandardSaveNoteNumber(Note note)
    {
        return GetSaveNoteNumber((int)note.guitarFret, c_guitarNoteToSaveNumberLookup);
    }

    static int GetDrumsSaveNoteNumber(Note note)
    {
        return GetSaveNoteNumber((int)note.drumPad, c_drumNoteToSaveNumberLookup);
    }

    static int GetGHLSaveNoteNumber(Note note)
    {
        return GetSaveNoteNumber((int)note.ghliveGuitarFret, c_ghlNoteToSaveNumberLookup);
    }

    #region Per-object write methods

    static readonly string s_anchorFormat = string.Format(" = A {{0}}{0}{1}{{1}}", Globals.LINE_ENDING, Globals.TABSPACE);
    static readonly string s_bpmFormat = " = B {0}";
    static readonly string s_tsFormat = " = TS {0}";
    static readonly string s_tsDenomFormat = " = TS {0} {1}";
    static readonly string s_sectionFormat = " = E \"section {0}\"";
    static readonly string s_eventFormat = " = E \"{0}\"";
    static readonly string s_chartEventFormat = " = E {0}";
    static readonly string s_starpowerFormat = " = S 2 {0}";
    static readonly string s_noteFormat = " = N {0} {1}";

    // Initial tick is automatically written
    static void AppendBpmData(SongObject songObject, in SongObjectWriteParameters writeParameters, StringBuilder output)
    {
        BPM bpm = songObject as BPM;
        if (bpm.anchor != null)
        {
            uint anchorValue = (uint)((double)bpm.anchor * 1000000);
            output.AppendFormat(s_anchorFormat, anchorValue, writeParameters.scaledTick);
        }

        output.AppendFormat(s_bpmFormat, bpm.value);
    }

    static void AppendTsData(SongObject songObject, in SongObjectWriteParameters writeParameters, StringBuilder output)
    {
        TimeSignature ts = songObject as TimeSignature;

        if (ts.denominator == 4)
        {
            output.AppendFormat(s_tsFormat, ts.numerator);
        }
        else
        {
            uint denominatorSaveVal = (uint)Mathf.Log(ts.denominator, 2);
            output.AppendFormat(s_tsDenomFormat, ts.numerator, denominatorSaveVal);
        }
    }

    static void AppendSectionData(SongObject songObject, in SongObjectWriteParameters writeParameters, StringBuilder output)
    {
        Section section = songObject as Section;
        output.AppendFormat(s_sectionFormat, section.title);
    }

    static void AppendEventData(SongObject songObject, in SongObjectWriteParameters writeParameters, StringBuilder output)
    {
        Event songEvent = songObject as Event;
        output.AppendFormat(s_eventFormat, songEvent.title);
    }

    static void AppendChartEventData(SongObject songObject, in SongObjectWriteParameters writeParameters, StringBuilder output)
    {
        ChartEvent chartEvent = songObject as ChartEvent;
        output.AppendFormat(s_chartEventFormat, chartEvent.eventName);
    }

    static void AppendStarpowerData(SongObject songObject, in SongObjectWriteParameters writeParameters, StringBuilder output)
    {
        Starpower sp = songObject as Starpower;
        output.AppendFormat(s_starpowerFormat, (uint)Mathf.Round(sp.length * writeParameters.resolutionScaleRatio));
    }

    static void AppendNoteData(SongObject songObject, in SongObjectWriteParameters writeParameters, StringBuilder output)
    {
        Note note = songObject as Note;
        int fretNumber;

        Song.Instrument instrument = writeParameters.instrument;

        if (writeParameters.instrument != Song.Instrument.Unrecognised)
        {
            if (instrument == Song.Instrument.Drums)
                fretNumber = GetDrumsSaveNoteNumber(note);

            else if (instrument == Song.Instrument.GHLiveGuitar || instrument == Song.Instrument.GHLiveBass)
                fretNumber = GetGHLSaveNoteNumber(note);

            else
                fretNumber = GetStandardSaveNoteNumber(note);
        }
        else
            fretNumber = note.rawNote;

        output.AppendFormat(s_noteFormat, fretNumber, (uint)Mathf.Round(note.length * writeParameters.resolutionScaleRatio));

        if (note.flags != Note.Flags.None && writeParameters.exportOptions.forced)
        {
            // Only need to get the flags of one note of a chord
            if (note.next == null || (note.next != null && note.next.tick != note.tick))
            { 
                Note.Flags flagsToIgnore;
                if (!ChartIOHelper.c_drumNoteDefaultFlagsLookup.TryGetValue(note.rawNote, out flagsToIgnore))
                {
                    flagsToIgnore = Note.Flags.None;
                }

                // Write out forced flag
                {
                    Note.Flags flagToTest = Note.Flags.Forced;
                    if ((note.flags & flagToTest) != 0)
                    {
                        int value;
                        if (c_guitarFlagToNumLookup.TryGetValue(flagToTest, out value))  // Todo, if different flags have different values for the same flags, we'll need to use different lookups
                        {
                            output.Append(Globals.LINE_ENDING);
                            output.Append(Globals.TABSPACE + writeParameters.scaledTick);
                            output.AppendFormat(s_noteFormat, value, 0);
                        }
                    }
                }

                // Write out tap flag
                {
                    Note.Flags flagToTest = Note.Flags.Tap;
                    if (!note.IsOpenNote() && (note.flags & flagToTest) != 0)
                    {
                        int value;
                        if (c_guitarFlagToNumLookup.TryGetValue(flagToTest, out value))  // Todo, if different flags have different values for the same flags, we'll need to use different lookups
                        {
                            output.Append(Globals.LINE_ENDING);
                            output.Append(Globals.TABSPACE + writeParameters.scaledTick);
                            output.AppendFormat(s_noteFormat, value, 0);
                        }
                    }
                }
            }

            // Write out cymbal flag for each note
            if (false)      // TODO
            {
                // Need to write this out if there ISN'T a cymbal flag on yellow, blue or green.
                // Write this out if flag is set on red or orange
                Note.Flags flagToTest = Note.Flags.ProDrums_Cymbal;
                if (((note.flags & flagToTest) != 0) && !note.IsOpenNote())
                {
                    int value = ChartIOHelper.c_proDrumsOffset;

                    int noteOffset;
                    if (!c_drumNoteToSaveNumberLookup.TryGetValue(note.rawNote, out noteOffset))
                    {
                        throw new Exception("Cannot find pro drum note offset for note " + note.drumPad.ToString());
                    }

                    value += noteOffset;

                    output.Append(Globals.LINE_ENDING);
                    output.Append(Globals.TABSPACE + writeParameters.scaledTick);
                    output.AppendFormat(s_noteFormat, value, 0);
                }
            }
        }
    }

    #endregion
}
