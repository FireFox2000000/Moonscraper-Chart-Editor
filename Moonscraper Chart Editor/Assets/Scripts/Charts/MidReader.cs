using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;
using System.Text.RegularExpressions;

// *Footnote: Alternatively convert absolute time to tick pos instead of cumlating the delta time 

public static class MidReader {

    public static Song ReadMidi(string path, bool unforceNAudioStrictMode)
    {
        Song song = new Song();
        MidiFile midi = new MidiFile(path);
        song.resolution = (short)midi.DeltaTicksPerQuarterNote;

        // Read all bpm data in first. This will also allow song.TimeToChartPosition to function properly.
        ReadSync(midi.Events[0], song);

        for (int i = 1; i < midi.Tracks; ++i)
        {
            var trackName = midi.Events[i][0] as TextEvent;
            if (trackName == null)
                continue;

            switch (trackName.Text.ToLower())
            {
                case ("events"):
                    ReadSongSections(midi.Events[i], song);
                    break;
                case ("part guitar"):
                    break;
                case ("t1 gems"):
                    break;
                case ("part bass"):
                    break;
                case ("part keys"):
                    break;
                default:
                    break;
            }
        }

        return song;
    }

    private static void ReadSync(IList<MidiEvent> track, Song song)
    {
        // Might be able to use song.TimeToChartPosition and the absolute time instead of cumulativeDT, assuming everything is in order, which it should. Both methods make sense.

        int cumulativeDT = 0;

        foreach (var me in track)
        {
            cumulativeDT += me.DeltaTime;

            var ts = me as TimeSignatureEvent;
            if (ts != null)
            {
                var tick = cumulativeDT;
                song.Add(new TimeSignature((uint)tick, (uint)ts.Numerator));
                continue;
            }
            var tempo = me as TempoEvent;
            if (tempo != null)
            {
                var tick = cumulativeDT;
                song.Add(new BPM((uint)tick, (uint)(tempo.Tempo * 1000))); 
                continue;
            }
            var text = me as TextEvent;
            if (text != null)
            {
                song.name = text.Text;
            }
        }
    }

    private static void ReadSongSections(IList<MidiEvent> track, Song song)
    {
        for (int i = 0; i < track.Count; i++)
        {
            var text = track[i] as TextEvent;

            if (text != null)
            {
                var tick = song.TimeToChartPosition(track[i].AbsoluteTime, song.resolution);
                if (text.Text.Contains("[section "))
                    song.Add(new Section(text.Text.Substring(9, text.Text.Length - 10), tick), false);
                else if (text.Text.Contains("[prc_"))
                    song.Add(new Section(text.Text.Substring(5, text.Text.Length - 6), tick), false);
            }
        }

        song.updateArrays();
    }

    private static void ReadNotes(IList<MidiEvent> track, Song song, Song.Instrument instrument)
    {
        List<NoteOnEvent> forceNotesList = new List<NoteOnEvent>();

        // Load all the notes
        for (int i = 0; i < track.Count; i++)
        {
            var note = track[i] as NoteOnEvent;
            if (note != null && note.OffEvent != null)
            {
                var tick = song.TimeToChartPosition(note.AbsoluteTime, song.resolution);
                Song.Difficulty difficulty;

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

                Note.Fret_Type fret;
                var sus = song.TimeToChartPosition(note.OffEvent.AbsoluteTime, song.resolution) - tick;

                // Determine the fret type of the note
                switch (note.NoteNumber)
                {
                    case 60:
                    case 72:
                    case 84:
                    case 96: if (sus <= 64L) sus = 0; fret = Note.Fret_Type.GREEN; break;

                    case 61:
                    case 73:
                    case 85:
                    case 97: if (sus <= 64L) sus = 0; fret = Note.Fret_Type.RED; break;

                    case 62:
                    case 74:
                    case 86:
                    case 98: if (sus <= 64L) sus = 0; fret = Note.Fret_Type.YELLOW; break;

                    case 63:
                    case 75:
                    case 87:
                    case 99: if (sus <= 64L) sus = 0; fret = Note.Fret_Type.BLUE; break;

                    case 64:
                    case 76:
                    case 88:
                    case 100: if (sus <= 64L) sus = 0; fret = Note.Fret_Type.ORANGE; break;

                    default:
                        continue;
                }

                // Add the note to the correct chart
                song.GetChart(instrument, difficulty).Add(new Note(tick, fret, sus), false);             
            }  
        }

        song.updateAllChartArrays();

        // Apply forcing events
        foreach (NoteOnEvent flagEvent in forceNotesList)
        {
            var tick = song.TimeToChartPosition(flagEvent.AbsoluteTime, song.resolution);
            var susEndPos = song.TimeToChartPosition(flagEvent.OffEvent.AbsoluteTime, song.resolution);
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
            Note[] notesToFlag = SongObject.GetRange(chart.notes, tick, tick + susEndPos);
            foreach (Note note in notesToFlag)
            { 
                // if NoteNumber is odd force hopo, if even force strum
                if (flagEvent.NoteNumber % 2 != 0)
                    note.SetType(Note.Note_Type.Hopo);
                else
                    note.SetType(Note.Note_Type.Strum);
            }
        }
    }

    private static void ReadTapEvents(IList<MidiEvent> track, Song song, Song.Instrument instrument)
    {
        for (int i = 0; i < track.Count; i++)
        {
            // Open notes or tap notes are stored as SysexEvents (whoever thought open notes shouldn't be considered regular notes was an idiot)
            var sysexEvent = track[i] as SysexEvent;
            if (sysexEvent != null)
            {
                string sysexDescription = sysexEvent.ToString();
                string hex;     // 8 total bytes, 5th byte is FF, 7th is 1 to start, 0 to end
                //const Regex tapRegex = new Regex();
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
