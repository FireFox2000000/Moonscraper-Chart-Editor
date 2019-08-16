// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

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
    static readonly string s_audioStreamFormat = Globals.TABSPACE + "{0} = \"{1}\"" + Globals.LINE_ENDING;

    delegate string GetAudioStreamSaveString(Song.AudioInstrument audio);
    public void Write(Song song, ExportOptions exportOptions, out string errorList)
    {
        song.UpdateCache();
        errorList = string.Empty;
        string saveString = string.Empty;

        try
        {
            string musicString = string.Empty;
            string guitarString = string.Empty;
            string bassString = string.Empty;
            string rhythmString = string.Empty;
            string drumString = string.Empty;

            GetAudioStreamSaveString GetSaveAudioString = audio => {
                string audioString;
                string audioLocation = song.GetAudioLocation(audio);

                if (audioLocation != string.Empty && Path.GetDirectoryName(audioLocation).Replace("\\", "/") == Path.GetDirectoryName(path).Replace("\\", "/"))
                    audioString = Path.GetFileName(audioLocation);
                else
                    audioString = audioLocation;

                return audioString;
            };

            musicString = GetSaveAudioString(Song.AudioInstrument.Song);
            guitarString = GetSaveAudioString(Song.AudioInstrument.Guitar);
            bassString = GetSaveAudioString(Song.AudioInstrument.Bass);
            rhythmString = GetSaveAudioString(Song.AudioInstrument.Rhythm);
            drumString = GetSaveAudioString(Song.AudioInstrument.Drum);

            // Song properties
            Debug.Log("Writing song properties");
            saveString += s_chartHeaderSong;
            saveString += GetPropertiesStringWithoutAudio(song, exportOptions);

            // Song audio
            if (song.GetAudioIsLoaded(Song.AudioInstrument.Song) || (musicString != null && musicString != string.Empty))
                saveString += string.Format(s_audioStreamFormat, "MusicStream", musicString);

            if (song.GetAudioIsLoaded(Song.AudioInstrument.Guitar) || (guitarString != null && guitarString != string.Empty))
                saveString += string.Format(s_audioStreamFormat, "GuitarStream", guitarString);

            if (song.GetAudioIsLoaded(Song.AudioInstrument.Bass) || (bassString != null && bassString != string.Empty))
                saveString += string.Format(s_audioStreamFormat, "BassStream", bassString);

            if (song.GetAudioIsLoaded(Song.AudioInstrument.Rhythm) || (rhythmString != null && rhythmString != string.Empty))
                saveString += string.Format(s_audioStreamFormat, "RhythmStream", rhythmString);

            if (song.GetAudioIsLoaded(Song.AudioInstrument.Drum) || (drumString != null && drumString != string.Empty))
                saveString += string.Format(s_audioStreamFormat, "DrumStream", drumString);

            saveString += s_chartSectionFooter;
        }
        catch (System.Exception e)
        {
            string error = Logger.LogException(e, "Error with saving song properties");
            errorList += error + Globals.LINE_ENDING;

            saveString = string.Empty;  // Clear all the song properties because we don't want braces left open, which will screw up the loading of the chart

#if UNITY_EDITOR
            System.Diagnostics.Debugger.Break();
#endif
        }

        // SyncTrack
        Debug.Log("Writing synctrack");
        saveString += s_chartHeaderSyncTrack;
        if (exportOptions.tickOffset > 0)
        {
            saveString += new BPM().GetSaveString();
            saveString += new TimeSignature().GetSaveString();
        }

        saveString += GetSaveString(song, song.syncTrack, exportOptions, ref errorList);
        saveString += s_chartSectionFooter;

        // Events
        Debug.Log("Writing events");
        saveString += s_chartHeaderEvents;
        saveString += GetSaveString(song, song.eventsAndSections, exportOptions, ref errorList);
        saveString += s_chartSectionFooter;

        // Charts      
        foreach (Song.Instrument instrument in Enum.GetValues(typeof(Song.Instrument)))
        {
            string instrumentSaveString = string.Empty;
            switch (instrument)
            {
                case (Song.Instrument.Guitar):
                    instrumentSaveString = "Single";
                    break;
                case (Song.Instrument.GuitarCoop):
                    instrumentSaveString = "DoubleGuitar";
                    break;
                case (Song.Instrument.Bass):
                    instrumentSaveString = "DoubleBass";
                    break;
                case (Song.Instrument.Rhythm):
                    instrumentSaveString = "DoubleRhythm";
                    break;
                case (Song.Instrument.Drums):
                    instrumentSaveString = "Drums";
                    break;
                case (Song.Instrument.Keys):
                    instrumentSaveString = "Keyboard";
                    break;
                case (Song.Instrument.GHLiveGuitar):
                    instrumentSaveString = "GHLGuitar";
                    break;
                case (Song.Instrument.GHLiveBass):
                    instrumentSaveString = "GHLBass";
                    break;
                default:
                    continue;
            }

            foreach (Song.Difficulty difficulty in Enum.GetValues(typeof(Song.Difficulty)))
            {
                string difficultySaveString = difficulty.ToString();

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

    static readonly string c_metaDataSaveFormat = string.Format("{0}{{0}} = \"{{{{0}}}}\"{1}", Globals.TABSPACE, Globals.LINE_ENDING);
    static readonly string c_metaDataSaveFormatNoQuote = string.Format("{0}{{0}} = {{{{0}}}}{1}", Globals.TABSPACE, Globals.LINE_ENDING);
    static readonly string c_nameFormat = string.Format(c_metaDataSaveFormat, "Name");
    static readonly string c_artistFormat = string.Format(c_metaDataSaveFormat, "Artist");
    static readonly string c_charterFormat = string.Format(c_metaDataSaveFormat, "Charter");
    static readonly string c_albumFormat = string.Format(c_metaDataSaveFormat, "Album");
    static readonly string c_yearFormat = string.Format("{0}{1} = \", {{0}}\"{2}", Globals.TABSPACE, "Year", Globals.LINE_ENDING);
    static readonly string c_offsetFormat = string.Format(c_metaDataSaveFormatNoQuote, "Offset");
    static readonly string c_resolutionFormat = string.Format(c_metaDataSaveFormatNoQuote, "Resolution");
    static readonly string c_player2Format = string.Format(c_metaDataSaveFormatNoQuote, "Player2");
    static readonly string c_difficultyFormat = string.Format(c_metaDataSaveFormatNoQuote, "Difficulty");
    static readonly string c_lengthFormat = string.Format(c_metaDataSaveFormatNoQuote, "Length");
    static readonly string c_previewStartFormat = string.Format(c_metaDataSaveFormatNoQuote, "PreviewStart");
    static readonly string c_previewEndFormat = string.Format(c_metaDataSaveFormatNoQuote, "PreviewEnd");
    static readonly string c_genreFormat = string.Format(c_metaDataSaveFormat, "Genre");
    static readonly string c_mediaTypeFormat = string.Format(c_metaDataSaveFormat, "MediaType");

    string GetPropertiesStringWithoutAudio(Song song, ExportOptions exportOptions)
    {
        string saveString = string.Empty;
        if (exportOptions.targetResolution <= 0)
            exportOptions.targetResolution = song.resolution;

        Metadata metaData = song.metaData;

        // Song properties  
        if (metaData.name != string.Empty)
            saveString += string.Format(c_nameFormat, metaData.name);
        if (metaData.artist != string.Empty)
            saveString += string.Format(c_artistFormat, metaData.artist);
        if (metaData.charter != string.Empty)
            saveString += string.Format(c_charterFormat, metaData.charter);
        if (metaData.album != string.Empty)
            saveString += string.Format(c_albumFormat, metaData.album);
        if (metaData.year != string.Empty)
            saveString += string.Format(c_yearFormat, metaData.year);
        saveString += string.Format(c_offsetFormat, song.offset);

        saveString += string.Format(c_resolutionFormat, exportOptions.targetResolution);
        if (metaData.player2 != string.Empty)
            saveString += string.Format(c_player2Format, metaData.player2.ToLower());
        saveString += string.Format(c_difficultyFormat, metaData.difficulty);
        if (song.manualLength)
            saveString += string.Format(c_lengthFormat, song.length);
        saveString += string.Format(c_previewStartFormat, metaData.previewStart);
        saveString += string.Format(c_previewEndFormat, metaData.previewEnd);
        if (metaData.genre != string.Empty)
            saveString += string.Format(c_genreFormat, metaData.genre);
        if (metaData.mediatype != string.Empty)
            saveString += string.Format(c_mediaTypeFormat, metaData.mediatype);

        return saveString;
    }

    static readonly string s_anchorFormat = string.Format(" = A {{0}}{0}{1}{{1}}", Globals.LINE_ENDING, Globals.TABSPACE);
    static readonly string s_bpmFormat = " = B {0}";
    static readonly string s_tsFormat = " = TS {0}";
    static readonly string s_tsDenomFormat = " = TS {0} {1}";
    static readonly string s_sectionFormat = " = E \"section {0}\"";
    static readonly string s_eventFormat = " = E \"{0}\"";
    static readonly string s_chartEventFormat = " = E {0}";
    static readonly string s_starpowerFormat = " = S 2 {0}";
    static readonly string s_noteFormat = " = N {0} {1}" + Globals.LINE_ENDING;
    static readonly string s_forcedNoteFormat = Globals.TABSPACE + "{0}" + " = N 5 0 " + Globals.LINE_ENDING;
    static readonly string s_tapNoteFormat = Globals.TABSPACE + "{0}" + " = N 6 0 " + Globals.LINE_ENDING;
    string GetSaveString<T>(Song song, IList<T> list, ExportOptions exportOptions, ref string out_errorList, Song.Instrument instrument = Song.Instrument.Guitar) where T : SongObject
    {
        System.Text.StringBuilder saveString = new System.Text.StringBuilder();

        float resolutionScaleRatio = song.ResolutionScaleRatio(exportOptions.targetResolution);

        for (int i = 0; i < list.Count; ++i)
        {
            SongObject songObject = list[i];
            try
            {
                uint tick = (uint)Mathf.Round(songObject.tick * resolutionScaleRatio) + exportOptions.tickOffset;
                saveString.Append(Globals.TABSPACE + tick);

                switch ((SongObject.ID)songObject.classID)
                {
                    case (SongObject.ID.BPM):
                        BPM bpm = songObject as BPM;
                        if (bpm.anchor != null)
                        {
                            uint anchorValue = (uint)((double)bpm.anchor * 1000000);
                            saveString.AppendFormat(s_anchorFormat, anchorValue, tick);
                        }

                        saveString.AppendFormat(s_bpmFormat, bpm.value);
                        break;

                    case (SongObject.ID.TimeSignature):
                        TimeSignature ts = songObject as TimeSignature;

                        if (ts.denominator == 4)
                            saveString.AppendFormat(s_tsFormat, ts.numerator);
                        else
                        {
                            uint denominatorSaveVal = (uint)Mathf.Log(ts.denominator, 2);
                            saveString.AppendFormat(s_tsDenomFormat, ts.numerator, denominatorSaveVal);
                        }
                        break;

                    case (SongObject.ID.Section):
                        Section section = songObject as Section;
                        saveString.AppendFormat(s_sectionFormat, section.title);
                        break;

                    case (SongObject.ID.Event):
                        Event songEvent = songObject as Event;
                        saveString.AppendFormat(s_eventFormat, songEvent.title);
                        break;

                    case (SongObject.ID.ChartEvent):
                        ChartEvent chartEvent = songObject as ChartEvent;
                        saveString.AppendFormat(s_chartEventFormat, chartEvent.eventName);
                        break;

                    case (SongObject.ID.Starpower):
                        Starpower sp = songObject as Starpower;
                        saveString.AppendFormat(s_starpowerFormat, (uint)Mathf.Round(sp.length * resolutionScaleRatio));
                        break;

                    case (SongObject.ID.Note):
                        Note note = songObject as Note;
                        int fretNumber;

                        if (instrument != Song.Instrument.Unrecognised)
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

                        saveString.AppendFormat(s_noteFormat, fretNumber, (uint)Mathf.Round(note.length * resolutionScaleRatio));

                        // Only need to get the flags of one note of a chord
                        if (exportOptions.forced && (note.next == null || (note.next != null && note.next.tick != note.tick)))
                        {
                            if ((note.flags & Note.Flags.Forced) == Note.Flags.Forced)
                            {
                                saveString.AppendFormat(s_forcedNoteFormat, tick);
                            }

                            // Save taps line if not an open note, as open note taps cause weird artifacts under sp
                            if (!note.IsOpenNote() && (note.flags & Note.Flags.Tap) == Note.Flags.Tap)
                            {
                                saveString.AppendFormat(s_tapNoteFormat, tick);
                            }
                        }
                        continue;

                    default:
                        continue;
                }
                saveString.Append(Globals.LINE_ENDING);

                //throw new System.Exception("Test error count: " + i);
            }
            catch (System.Exception e)
            {
                string error = Logger.LogException(e, "Error with saving object #" + i + " as " + songObject);
                out_errorList += error + Globals.LINE_ENDING;
            }
        }

        return saveString.ToString();
    }

    int GetStandardSaveNoteNumber(Note note)
    {
        switch (note.guitarFret)
        {
            case (Note.GuitarFret.Green):
                return 0;
            case (Note.GuitarFret.Red):
                return 1;
            case (Note.GuitarFret.Yellow):
                return 2;
            case (Note.GuitarFret.Blue):
                return 3;
            case (Note.GuitarFret.Orange):
                return 4;
            case (Note.GuitarFret.Open):
                return 7;                               // 5 and 6 are reserved for forced and taps properties
            default: break;
        }

        return 0;
    }

    int GetDrumsSaveNoteNumber(Note note)
    {
        switch (note.drumPad)
        {
            case (Note.DrumPad.Kick):
                return 0;
            case (Note.DrumPad.Red):
                return 1;
            case (Note.DrumPad.Yellow):
                return 2;
            case (Note.DrumPad.Blue):
                return 3;
            case (Note.DrumPad.Orange):
                return 4;
            case (Note.DrumPad.Green):
                return 5;

            default: break;
        }

        return 0;
    }

    int GetGHLSaveNoteNumber(Note note)
    {
        switch (note.ghliveGuitarFret)
        {
            case (Note.GHLiveGuitarFret.White1):
                return 0;
            case (Note.GHLiveGuitarFret.White2):
                return 1;
            case (Note.GHLiveGuitarFret.White3):
                return 2;
            case (Note.GHLiveGuitarFret.Black1):
                return 3;
            case (Note.GHLiveGuitarFret.Black2):
                return 4;

            case (Note.GHLiveGuitarFret.Open):
                return 7;
            case (Note.GHLiveGuitarFret.Black3):
                return 8;

            default: break;
        }

        return 0;
    }
}
