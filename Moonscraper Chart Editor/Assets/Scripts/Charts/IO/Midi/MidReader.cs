using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;

public static class MidReader {

    public static Song ReadMidi(string path)
    {
        Song song = new Song();
        string directory = System.IO.Path.GetDirectoryName(path);
        song.musicSongName = directory + "\\song.ogg";
        song.guitarSongName = directory + "\\guitar.ogg";
        song.rhythmSongName = directory + "\\rhythm.ogg";
        song.drumSongName = directory + "\\drums.ogg";

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

        // Read all bpm data in first. This will also allow song.TimeToChartPosition to function properly.
        ReadSync(midi.Events[0], song);

        for (int i = 1; i < midi.Tracks; ++i)
        {
            var trackName = midi.Events[i][0] as TextEvent;
            if (trackName == null)
                continue;
            Debug.Log(trackName.Text);
            switch (trackName.Text.ToLower())
            {
                case ("events"):
                    ReadSongSections(midi.Events[i], song);
                    break;
                case ("part guitar"):
                    ReadNotes(midi.Events[i], song, Song.Instrument.Guitar);
                    break;
                case ("t1 gems"):   // GH1 midi file related
                    break;
                case ("part bass"):
                    ReadNotes(midi.Events[i], song, Song.Instrument.Bass);
                    break;
                case ("part keys"):
                    ReadNotes(midi.Events[i], song, Song.Instrument.Keys);
                    break;
                case ("part drums"):
                    ReadNotes(midi.Events[i], song, Song.Instrument.Drums);
                    break;
                case ("beat"):
                    //ReadTrack(midi.Events[i]);
                    break;
                default:
                    break;
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

        song.updateArrays();
    }

    private static void ReadSongSections(IList<MidiEvent> track, Song song)
    {
        for (int i = 0; i < track.Count; ++i)
        {
            var text = track[i] as TextEvent;

            if (text != null)
            {            
                if (text.Text.Contains("[section "))
                    song.Add(new Section(text.Text.Substring(9, text.Text.Length - 10), (uint)text.AbsoluteTime), false);
                else if (text.Text.Contains("[prc_"))       // No idea what this actually is
                    song.Add(new Section(text.Text.Substring(5, text.Text.Length - 6), (uint)text.AbsoluteTime), false);
            }
        }

        song.updateArrays();
    }

    private static void ReadNotes(IList<MidiEvent> track, Song song, Song.Instrument instrument)
    {
        List<NoteOnEvent> forceNotesList = new List<NoteOnEvent>();
        List<SysexEvent> tapAndOpenEvents = new List<SysexEvent>();

        int rbSustainFixLength = (int)(64 * song.resolution / Globals.STANDARD_BEAT_RESOLUTION);

        // Load all the notes
        for (int i = 0; i < track.Count; i++)
        {
            var text = track[i] as TextEvent;
            if (text != null)
            {
                var tick = (uint)text.AbsoluteTime;
                var eventName = text.Text.Trim(new char[] { '[', ']' });

                song.GetChart(instrument, Song.Difficulty.Expert).Add(new ChartEvent(tick, eventName));
                //Debug.Log(text.Text + " " + text.AbsoluteTime);
            }

            var note = track[i] as NoteOnEvent;
            if (note != null && note.OffEvent != null)
            {
                var tick = (uint)note.AbsoluteTime;
                var sus = (uint)(note.OffEvent.AbsoluteTime - tick);

                Song.Difficulty difficulty;

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
                    difficulty = SelectNoteDifficulty(note.NoteNumber);
                }
                catch
                {
                    continue;
                }

                // Check if we're reading a forcing event instead of a regular note
                if (instrument != Song.Instrument.Drums)
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
                
                Note.Fret_Type fret;

                if (sus <= rbSustainFixLength)
                    sus = 0;

                // Determine the fret type of the note
                switch (note.NoteNumber)
                {
                    case 60:
                    case 72:
                    case 84:
                    case 96: fret = Note.Fret_Type.GREEN; break;

                    case 61:
                    case 73:
                    case 85:
                    case 97: fret = Note.Fret_Type.RED; break;

                    case 62:
                    case 74:
                    case 86:
                    case 98: fret = Note.Fret_Type.YELLOW; break;

                    case 63:
                    case 75:
                    case 87:
                    case 99: fret = Note.Fret_Type.BLUE; break;

                    case 64:
                    case 76:
                    case 88:
                    case 100: fret = Note.Fret_Type.ORANGE; break;

                    case 65:
                    case 77:
                    case 89:
                    case 101:
                        if (instrument == Song.Instrument.Drums)
                        {
                            fret = Note.Fret_Type.OPEN;     // Gets converted to an orange note later on
                            break;
                        }
                        else
                            continue;
                    default:
                        Debug.Log(note.NoteNumber);
                        continue;
                }

                if (instrument == Song.Instrument.Drums)
                    fret = Note.DrumNoteToGuitarNote(fret);

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
        foreach (Song.Difficulty diff in System.Enum.GetValues(typeof(Song.Difficulty)))
            song.GetChart(instrument, diff).updateArrays();
        
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

            Chart chart = song.GetChart(instrument, difficulty);
            int index, length;
            SongObject.GetRange(chart.notes, tick, tick + endPos, out index, out length);

            for (int i = index; i < index + length; ++i)
            { 
                // if NoteNumber is odd force hopo, if even force strum
                if (flagEvent.NoteNumber % 2 != 0)
                    chart.notes[i].SetType(Note.Note_Type.Hopo);
                else
                    chart.notes[i].SetType(Note.Note_Type.Strum);
            }
        }

        // Apply tap and open note events
        System.Array difficultyValues = System.Enum.GetValues(typeof(Song.Difficulty));
        Chart[] chartsOfInstrument = new Chart[difficultyValues.Length];

        int difficultyCount = 0;
        foreach (Song.Difficulty difficulty in difficultyValues)
            chartsOfInstrument[difficultyCount++] = song.GetChart(instrument, difficulty);
    
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
                    SongObject.GetRange(chart.notes, tick, tick + endPos, out index, out length);
                    for (int k = index; k < index + length; ++k)
                    {
                        chart.notes[k].SetType(Note.Note_Type.Tap);
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
                Note[] notes = song.GetChart(instrument, difficulty).notes;
                SongObject.GetRange(notes, tick, tick + endPos, out index, out length);
                for (int k = index; k < index + length; ++k)
                {
                    notes[k].fret_type = Note.Fret_Type.OPEN;

                    if (instrument == Song.Instrument.Drums)
                        notes[k].fret_type = Note.DrumNoteToGuitarNote(notes[k].fret_type);
                }
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
}
