// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class ChartWriter {
    string path;

    public ChartWriter(string path)
    {
        this.path = path;
    }

    public void Write(Song song, ExportOptions exportOptions)
    {
        string musicString = string.Empty;
        string guitarString = string.Empty;
        string rhythmString = string.Empty;
        string drumString = string.Empty;

        // Check if the audio location is the same as the filepath. If so, we only have to save the name of the file, not the full path.
        if (song.songAudioLoaded && Path.GetDirectoryName(song.audioLocations[Song.MUSIC_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(path).Replace("\\", "/"))
            musicString = Path.GetFileName(song.audioLocations[Song.MUSIC_STREAM_ARRAY_POS]);
        else
            musicString = song.audioLocations[Song.MUSIC_STREAM_ARRAY_POS];

        if (song.guitarAudioLoaded && Path.GetDirectoryName(song.audioLocations[Song.GUITAR_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(path).Replace("\\", "/"))
            guitarString = Path.GetFileName(song.audioLocations[Song.GUITAR_STREAM_ARRAY_POS]);
        else
            guitarString = song.audioLocations[Song.GUITAR_STREAM_ARRAY_POS];

        if (song.rhythmAudioLoaded && Path.GetDirectoryName(song.audioLocations[Song.RHYTHM_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(path).Replace("\\", "/"))
            rhythmString = Path.GetFileName(song.audioLocations[Song.RHYTHM_STREAM_ARRAY_POS]);
        else
            rhythmString = song.audioLocations[Song.RHYTHM_STREAM_ARRAY_POS];

        if (song.drumAudioLoaded && Path.GetDirectoryName(song.audioLocations[Song.DRUM_STREAM_ARRAY_POS]).Replace("\\", "/") == Path.GetDirectoryName(path).Replace("\\", "/"))
            drumString = Path.GetFileName(song.audioLocations[Song.DRUM_STREAM_ARRAY_POS]);
        else
            drumString = song.audioLocations[Song.DRUM_STREAM_ARRAY_POS];

        string saveString = string.Empty;

        // Song properties
        saveString += "[Song]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
        saveString += GetPropertiesStringWithoutAudio(song, exportOptions);

        // Song audio
        if (song.songAudioLoaded)
            saveString += Globals.TABSPACE + "MusicStream = \"" + musicString + "\"" + Globals.LINE_ENDING;

        if (song.guitarAudioLoaded)
            saveString += Globals.TABSPACE + "GuitarStream = \"" + guitarString + "\"" + Globals.LINE_ENDING;

        if (song.rhythmAudioLoaded)
            saveString += Globals.TABSPACE + "RhythmStream = \"" + rhythmString + "\"" + Globals.LINE_ENDING;

        if (song.drumAudioLoaded)
            saveString += Globals.TABSPACE + "DrumStream = \"" + drumString + "\"" + Globals.LINE_ENDING;

        saveString += "}" + Globals.LINE_ENDING;

        // SyncTrack
        saveString += "[SyncTrack]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
        if (exportOptions.tickOffset > 0)
        {
            saveString += new BPM().GetSaveString();
            saveString += new TimeSignature().GetSaveString();
        }
        saveString += GetSaveString(song, song.syncTrack, exportOptions);
        saveString += "}" + Globals.LINE_ENDING;

        // Events
        saveString += "[Events]" + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
        saveString += GetSaveString(song, song.events, exportOptions);
        saveString += "}" + Globals.LINE_ENDING;

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
                case (Song.Instrument.Drums):
                    instrumentSaveString = "Drums";
                    break;
                case (Song.Instrument.Keys):
                    instrumentSaveString = "Keyboard";
                    break;
                default:
                    continue;
            }

            foreach (Song.Difficulty difficulty in Enum.GetValues(typeof(Song.Difficulty)))
            {
                string difficultySaveString = difficulty.ToString();
                
                string chartString = GetSaveString(song, song.GetChart(instrument, difficulty).chartObjects, exportOptions, instrument);

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

                            chartString = GetSaveString(song, song.GetChart(instrument, chartDiff).chartObjects, exportOptions, instrument);

                            if (exit)
                                break;
                        }

                        if (exit)
                            continue;
                    }
                    else
                        continue;

                }

                string seperator = "[" + difficultySaveString + instrumentSaveString + "]";
                saveString += seperator + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
                saveString += chartString;
                saveString += "}" + Globals.LINE_ENDING;
            }
        }

        // Unrecognised charts
        foreach (Chart chart in song.unrecognisedCharts)
        {
            string chartString = GetSaveString(song, chart.chartObjects, exportOptions, Song.Instrument.Unrecognised);

            string seperator = "[" + chart.name + "]";
            saveString += seperator + Globals.LINE_ENDING + "{" + Globals.LINE_ENDING;
            saveString += chartString;
            saveString += "}" + Globals.LINE_ENDING;
        }

        try
        {
            // Save to file
            File.WriteAllText(path, saveString);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    string GetPropertiesStringWithoutAudio(Song song, ExportOptions exportOptions)
    {
        string saveString = string.Empty;
        if (exportOptions.targetResolution <= 0)
            exportOptions.targetResolution = song.resolution;

        // Song properties  
        if (song.name != string.Empty)
            saveString += Globals.TABSPACE + "Name = \"" + song.name + "\"" + Globals.LINE_ENDING;
        if (song.artist != string.Empty)
            saveString += Globals.TABSPACE + "Artist = \"" + song.artist + "\"" + Globals.LINE_ENDING;
        if (song.charter != string.Empty)
            saveString += Globals.TABSPACE + "Charter = \"" + song.charter + "\"" + Globals.LINE_ENDING;
        if (song.album != string.Empty)
            saveString += Globals.TABSPACE + "Album = \"" + song.album + "\"" + Globals.LINE_ENDING;
        if (song.year != string.Empty)
            saveString += Globals.TABSPACE + "Year = \", " + song.year + "\"" + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Offset = " + song.offset + Globals.LINE_ENDING;

        saveString += Globals.TABSPACE + "Resolution = " + exportOptions.targetResolution + Globals.LINE_ENDING;
        if (song.player2 != string.Empty)
            saveString += Globals.TABSPACE + "Player2 = " + song.player2.ToLower() + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "Difficulty = " + song.difficulty + Globals.LINE_ENDING;
        if (song.manualLength)
            saveString += Globals.TABSPACE + "Length = " + song.length + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "PreviewStart = " + song.previewStart + Globals.LINE_ENDING;
        saveString += Globals.TABSPACE + "PreviewEnd = " + song.previewEnd + Globals.LINE_ENDING;
        if (song.genre != string.Empty)
            saveString += Globals.TABSPACE + "Genre = \"" + song.genre + "\"" + Globals.LINE_ENDING;
        if (song.mediatype != string.Empty)
            saveString += Globals.TABSPACE + "MediaType = \"" + song.mediatype + "\"" + Globals.LINE_ENDING;

        return saveString;
    }

    string GetSaveString<T>(Song song, T[] list, ExportOptions exportOptions, Song.Instrument instrument = Song.Instrument.Guitar) where T : SongObject
    {
        System.Text.StringBuilder saveString = new System.Text.StringBuilder();

        //string saveString = string.Empty;

        float resolutionScaleRatio = song.ResolutionScaleRatio(exportOptions.targetResolution);

        foreach (SongObject songObject in list)
        {
            uint tick = (uint)Mathf.Round(songObject.position * resolutionScaleRatio) + exportOptions.tickOffset;
            saveString.Append(Globals.TABSPACE + tick);

            switch ((SongObject.ID)songObject.classID)
            {
                case (SongObject.ID.BPM):
                    BPM bpm = songObject as BPM;
                    if (bpm.anchor != null)
                    {
                        saveString.Append(" = A " + (uint)((double)bpm.anchor * 1000000));
                        saveString.Append(Globals.LINE_ENDING);
                        saveString.Append(Globals.TABSPACE + tick);
                    }

                    saveString.Append(" = B " + bpm.value);
                    break;

                case (SongObject.ID.TimeSignature):
                    TimeSignature ts = songObject as TimeSignature;
                    saveString.Append(" = TS " + ts.numerator);

                    if (ts.denominator != 4)
                        saveString.Append(" " + (uint)Mathf.Log(ts.denominator, 2));
                    break;

                case (SongObject.ID.Section):
                    Section section = songObject as Section;
                    saveString.Append(" = E \"section " + section.title + "\"");
                    break;

                case (SongObject.ID.Event):
                    Event songEvent = songObject as Event;
                    saveString.Append(" = E \"" + songEvent.title + "\"");
                    break;

                case (SongObject.ID.ChartEvent):
                    ChartEvent chartEvent = songObject as ChartEvent;
                    saveString.Append(" = E " + chartEvent.eventName);
                    break;

                case (SongObject.ID.Starpower):
                    Starpower sp = songObject as Starpower;
                    saveString.Append(" = S 2 " + (uint)Mathf.Round(sp.length * resolutionScaleRatio));
                    break;

                case (SongObject.ID.Note):
                    Note note = songObject as Note;
                    int fretNumber;

                    if (instrument != Song.Instrument.Unrecognised)
                    {
                        Note.Fret_Type fret = note.fret_type;

                        // Drum notes are saved differently
                        if (instrument == Song.Instrument.Drums)
                            fret = Note.SaveGuitarNoteToDrumNote(fret);

                        fretNumber = (int)fret;
                        if (fret == Note.Fret_Type.OPEN && instrument != Song.Instrument.Drums)     // Last note is saved as 7 unless it's in the drum chart in which case it is 5
                            fretNumber = 7;
                    }
                    else
                        fretNumber = note.rawNote;

                    saveString.Append(" = N " + fretNumber + " " + (uint)Mathf.Round(note.sustain_length * resolutionScaleRatio));

                    saveString.Append(Globals.LINE_ENDING);

                    // Only need to get the flags of one note of a chord
                    if (exportOptions.forced && (note.next == null || (note.next != null && note.next.position != note.position)))
                    {
                        if ((note.flags & Note.Flags.FORCED) == Note.Flags.FORCED)
                            saveString.Append(Globals.TABSPACE + tick + " = N 5 0 " + Globals.LINE_ENDING);

                        // Save taps line if not an open note, as open note taps cause weird artifacts under sp
                        if (note.fret_type != Note.Fret_Type.OPEN && (note.flags & Note.Flags.TAP) == Note.Flags.TAP)
                            saveString.Append(Globals.TABSPACE + tick + " = N 6 0 " + Globals.LINE_ENDING);
                    }
                    continue;

                default:
                    continue;
            }

            saveString.Append(Globals.LINE_ENDING);
        }

        return saveString.ToString();
    }
}
