// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;

public static class MidReader {

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

    public static Song ReadMidi(string path, ref CallbackState callBackState)
    {
        Song song = new Song();
        string directory = System.IO.Path.GetDirectoryName(path);

        foreach(Song.AudioInstrument audio in Enum.GetValues(typeof(Song.AudioInstrument)))
        {
            string audioFilepath = directory + "\\" + audio.ToString().ToLower() + ".ogg";
            Debug.Log(audioFilepath);
            song.SetAudioLocation(audio, audioFilepath);
        }

        //song.SetAudioLocation(Song.AudioInstrument.Song, directory + "\\song.ogg");

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
            Debug.Log(trackName.Text);

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
                    NativeMessageBox.Result result = NativeMessageBox.Show("A vocals track was found in the file. Would you like to import the text events as global lyrics events?", "Vocals Track Found", NativeMessageBox.Type.YesNo, false);
                    callBackState = CallbackState.None;
                    importTrackAsVocalsEvents = result == NativeMessageBox.Result.Yes;
                }
#endif
                if (importTrackAsVocalsEvents)
                {
                    ReadTextEventsIntoGlobalEventsAsLyrics(midi.Events[i], song);
                }
                else
                {
                    Song.Instrument instrument;
                    if (!c_trackNameToInstrumentMap.TryGetValue(trackNameKey, out instrument))
                    {
                        instrument = Song.Instrument.Unrecognised;
                    }

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
        for (int i = 1; i < track.Count; ++i)
        {
            var text = track[i] as TextEvent;

            if (text != null)
            {            
                if (text.Text.Contains("[section "))
                    song.Add(new Section(text.Text.Substring(9, text.Text.Length - 10), (uint)text.AbsoluteTime), false);
                else if (text.Text.Contains("[prc_"))       // No idea what this actually is
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

            if (text != null && text.Text.Length > 0 && text.Text[0] != '[')
            {           
                string lyricEvent = "lyric " + text.Text;
                song.Add(new Event(lyricEvent, (uint)text.AbsoluteTime), false);
            }
        }

        song.UpdateCache();
    }

    private static void ReadNotes(IList<MidiEvent> track, Song song, Song.Instrument instrument)
    {
        List<NoteOnEvent> forceNotesList = new List<NoteOnEvent>();
        List<SysexEvent> tapAndOpenEvents = new List<SysexEvent>();

        Chart unrecognised = new Chart(song, Song.Instrument.Unrecognised);
        Chart.GameMode gameMode = Song.InstumentToChartGameMode(instrument);

        if (instrument == Song.Instrument.Unrecognised)
            song.unrecognisedCharts.Add(unrecognised);

        int rbSustainFixLength = (int)(64 * song.resolution / Song.STANDARD_BEAT_RESOLUTION);

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
                Song.Difficulty difficulty;

                var tick = (uint)note.AbsoluteTime;
                var sus = (uint)(note.OffEvent.AbsoluteTime - tick);

                if (instrument == Song.Instrument.Unrecognised)
                {
                    int rawNote = SelectRawNoteValue(note.NoteNumber);
                    Note newNote = new Note(tick, rawNote, sus);
                    //difficulty = SelectRawNoteDifficulty(note.NoteNumber);
                    unrecognised.Add(newNote);
                    continue;
                }

                // Check if starpower event
                if (note.NoteNumber == 116)
                {                
                    foreach (Song.Difficulty diff in System.Enum.GetValues(typeof(Song.Difficulty)))
                        song.GetChart(instrument, diff).Add(new Starpower(tick, sus), false);

                    continue;
                }

                // Determine which difficulty we are manipulating
                try
                {
                    if (gameMode == Chart.GameMode.GHLGuitar)
                        difficulty = SelectGHLNoteDifficulty(note.NoteNumber);
                    else
                        difficulty = SelectNoteDifficulty(note.NoteNumber);
                }
                catch
                {
                    continue;
                }

                // Check if we're reading a forcing event instead of a regular note
                if (gameMode != Chart.GameMode.Drums)
                {
                    switch (note.NoteNumber)
                    {
                        case 65:
                        case 66:
                        case 77:
                        case 78:
                        case 89:
                        case 90:
                        case 101:
                        case 102:
                            forceNotesList.Add(note);       // Store the event for later processing and continue
                            continue;
                        default:
                            break;
                    }
                }
                
                int fret;

                if (sus <= rbSustainFixLength)
                    sus = 0;

                if (gameMode == Chart.GameMode.Drums)
                    fret = (int)GetDrumFretType(note.NoteNumber);
                else if (gameMode == Chart.GameMode.GHLGuitar)
                    fret = (int)GetGHLFretType(note.NoteNumber);
                else
                    fret = (int)GetStandardFretType(note.NoteNumber);

                // Add the note to the correct chart
                song.GetChart(instrument, difficulty).Add(new Note(tick, fret, sus), false);             
            }

            var sysexEvent = track[i] as SysexEvent;
            if (sysexEvent != null)
            {
                //Debug.Log(BitConverter.ToString(sysexEvent.GetData()));
                tapAndOpenEvents.Add(sysexEvent);
            }
        }

        // Update all chart arrays
        if (instrument != Song.Instrument.Unrecognised)
        {
            foreach (Song.Difficulty diff in System.Enum.GetValues(typeof(Song.Difficulty)))
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
            System.Array difficultyValues = System.Enum.GetValues(typeof(Song.Difficulty));
            chartsOfInstrument = new Chart[difficultyValues.Length];

            int difficultyCount = 0;
            foreach (Song.Difficulty difficulty in difficultyValues)
                chartsOfInstrument[difficultyCount++] = song.GetChart(instrument, difficulty);
        }
    
        for(int i = 0; i < tapAndOpenEvents.Count; ++i)
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
                        chart.notes[k].SetType(Note.NoteType.Tap);
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
                        notes[k].guitarFret = NoteFunctions.LoadDrumNoteToGuitarNote(notes[k].guitarFret);
                }
            }
        }

        // Apply forcing events
        foreach (NoteOnEvent flagEvent in forceNotesList)
        {
            uint tick = (uint)flagEvent.AbsoluteTime;
            uint endPos = (uint)(flagEvent.OffEvent.AbsoluteTime - tick);

            Song.Difficulty difficulty;

            // Determine which difficulty we are manipulating
            try
            {
                difficulty = SelectNoteDifficulty(flagEvent.NoteNumber);
            }
            catch
            {
                continue;
            }

            Chart chart;
            if (instrument != Song.Instrument.Unrecognised)
                chart = song.GetChart(instrument, difficulty);
            else
                chart = unrecognised;

            int index, length;
            SongObjectHelper.GetRange(chart.notes, tick, tick + endPos, out index, out length);

            for (int i = index; i < index + length; ++i)
            {
                if ((chart.notes[i].flags & Note.Flags.Tap) != 0)
                    continue;

                // if NoteNumber is odd force hopo, if even force strum
                if (flagEvent.NoteNumber % 2 != 0)
                    chart.notes[i].SetType(Note.NoteType.Hopo);
                else
                    chart.notes[i].SetType(Note.NoteType.Strum);
            }
        }
    }

    static Song.Difficulty SelectNoteDifficulty(int noteNumber)
    {
        if (noteNumber >= 60 && noteNumber <= 66)
            return Song.Difficulty.Easy;
        else if (noteNumber >= 72 && noteNumber <= 78)
            return Song.Difficulty.Medium;
        else if (noteNumber >= 84 && noteNumber <= 90)
            return Song.Difficulty.Hard;
        else if (noteNumber >= 96 && noteNumber <= 102)
            return Song.Difficulty.Expert;
        else
            throw new System.ArgumentOutOfRangeException("Note number outside of note range");
    }

    static Song.Difficulty SelectGHLNoteDifficulty(int noteNumber)
    {
        if (noteNumber >= 94)
            return Song.Difficulty.Expert;
        else if (noteNumber >= 82)
            return Song.Difficulty.Hard;
        else if (noteNumber >= 70)
            return Song.Difficulty.Medium;
        else
            return Song.Difficulty.Easy;
    }


    static Song.Difficulty SelectRawNoteDifficulty(int noteNumber)
    {
        if (noteNumber >= 96)
            return Song.Difficulty.Expert;
        else if (noteNumber >= 84)
            return Song.Difficulty.Hard;
        else if (noteNumber >= 72)
            return Song.Difficulty.Medium;
        else
            return Song.Difficulty.Easy;
    }

    static int SelectRawNoteValue(int noteNumber)
    {
        // Generally starts at 60, every 12 notes is a change is difficulty
        //return noteNumber % 12;
        return noteNumber;
    }

    static Note.GuitarFret GetStandardFretType(int noteNumber)
    {
        Note.GuitarFret fret;
        int difficultyLessNote = noteNumber % 12;

        // Determine the fret type of the note
        switch (difficultyLessNote)
        {
            case 0: fret = Note.GuitarFret.Green; break;

            case 1: fret = Note.GuitarFret.Red; break;

            case 2: fret = Note.GuitarFret.Yellow; break;

            case 3: fret = Note.GuitarFret.Blue; break;

            case 4: fret = Note.GuitarFret.Orange; break;

            // 5 is forced
            default:
                fret = Note.GuitarFret.Green; break;
        }

        return fret;
    }

    static Note.DrumPad GetDrumFretType(int noteNumber)
    {
        Note.DrumPad fret;
        int difficultyLessNote = noteNumber % 12;

        // Determine the fret type of the note
        switch (difficultyLessNote)
        {
            case 0: fret = Note.DrumPad.Kick; break;

            case 1: fret = Note.DrumPad.Red; break;

            case 2: fret = Note.DrumPad.Yellow; break;

            case 3: fret = Note.DrumPad.Blue; break;

            case 4: fret = Note.DrumPad.Orange; break;

            case 5: fret = Note.DrumPad.Green; break;

            default:
                fret = Note.DrumPad.Red; break;
        }

        return fret;
    }

    static Note.GHLiveGuitarFret GetGHLFretType(int noteNumber)
    {
        Note.GHLiveGuitarFret fret;
        int difficultyLessNote = (noteNumber + 2) % 12;

        // Determine the fret type of the note
        switch (difficultyLessNote)
        {
            case 0: fret = Note.GHLiveGuitarFret.Open; break;

            case 1: fret = Note.GHLiveGuitarFret.White1; break;

            case 2: fret = Note.GHLiveGuitarFret.White2; break;

            case 3: fret = Note.GHLiveGuitarFret.White3; break;

            case 4: fret = Note.GHLiveGuitarFret.Black1; break;

            case 5: fret = Note.GHLiveGuitarFret.Black2; break;

            case 6: fret = Note.GHLiveGuitarFret.Black3; break;

            default:
                fret = Note.GHLiveGuitarFret.Black1; break;
        }

        return fret;
    }
}
