using System;
using NAudio.Midi;
using System.Linq;
using System.Collections.Generic;

namespace mid2chart {
    internal static class MidReader {
        private static Song s;
        private static MidiFile midi;
        private static double scaler;
        internal static Song ReadMidi(string path, bool unforceNAudioStrictMode) {
            s = new Song();
            midi = new MidiFile(path, !unforceNAudioStrictMode);
            var trackCount = midi.Events.Count();
            scaler = 192.0D / midi.DeltaTicksPerQuarterNote;
            WriteSync(midi.Events[0]);
            for (var i = 1; i < trackCount; i++) {               
                var trackName = midi.Events[i][0] as TextEvent;
                if (trackName == null)
                    continue;

                switch (trackName.Text.ToLower()) {
                    case ("events"): WriteSongSections(midi.Events[i]); break;
                    case ("part guitar"):
                        WriteNoteSection(midi.Events[i], 0);
                        WriteTapSection(midi.Events[i], 0);
                        if (Program.readOpenNotes) {
                            WriteOpenNotes(midi.Events[i], 0, 0);
                            WriteOpenNotes(midi.Events[i], 0, 1);
                            WriteOpenNotes(midi.Events[i], 0, 2);
                            WriteOpenNotes(midi.Events[i], 0, 3);
                        }
                        break;
                    case ("t1 gems"):
                        WriteNoteSection(midi.Events[i], 0);
                        WriteTapSection(midi.Events[i], 0);
                        if (Program.readOpenNotes) {
                            WriteOpenNotes(midi.Events[i], 0, 0);
                            WriteOpenNotes(midi.Events[i], 0, 1);
                            WriteOpenNotes(midi.Events[i], 0, 2);
                            WriteOpenNotes(midi.Events[i], 0, 3);
                        }
                        break;
                    case ("part bass"):
                        WriteNoteSection(midi.Events[i], 1);
                        WriteTapSection(midi.Events[i], 1);
                        if (Program.readOpenNotes) {
                            WriteOpenNotes(midi.Events[i], 1, 0);
                            WriteOpenNotes(midi.Events[i], 1, 1);
                            WriteOpenNotes(midi.Events[i], 1, 2);
                            WriteOpenNotes(midi.Events[i], 1, 3);
                        }
                        break;
                    case ("part keys"):
                        WriteNoteSection(midi.Events[i], 2);
                        break;
                }
            }
            return s;
        }

        private static void WriteOpenNotes(IList<MidiEvent> track, int sec, int diff) {
            for (int i = 0; i < track.Count; i++) {
                var se = track[i] as SysexEvent;
                if (se != null) {
                    var b = se.GetData();
                    if (b.Length == 8 && b[5] == diff && b[7] == 1) {
                        long tick = RoundToValidValue((long)Math.Floor(se.AbsoluteTime*scaler));
                        long sus = 0;
                        for (int j = i; j < track.Count; j++) {
                            var se2 = track[j] as SysexEvent;
                            if (se2 != null) {
                                var b2 = se2.GetData();
                                if (b2.Length == 8 && b2[5] == diff && b2[7] == 0) {
                                    sus = RoundToValidValue((long)Math.Floor(se2.AbsoluteTime*scaler))-tick;
                                    break;
                                }
                            }
                        }
                        if (sus == 0) sus = 1;
                        if (sec == 0) {
                            switch (diff) {
                                case 0: s.eGuitarOpenNotes.Add(new NoteSection(tick, sus)); break;
                                case 1: s.mGuitarOpenNotes.Add(new NoteSection(tick, sus)); break;
                                case 2: s.hGuitarOpenNotes.Add(new NoteSection(tick, sus)); break;
                                case 3: s.xGuitarOpenNotes.Add(new NoteSection(tick, sus)); break;
                                default: throw new Exception("Invalid diff value (min. 0 max. 3");
                            }
                        } else if (sec == 1) {
                            switch (diff) {
                                case 0: s.eBassOpenNotes.Add(new NoteSection(tick, sus)); break;
                                case 1: s.mBassOpenNotes.Add(new NoteSection(tick, sus)); break;
                                case 2: s.hBassOpenNotes.Add(new NoteSection(tick, sus)); break;
                                case 3: s.xBassOpenNotes.Add(new NoteSection(tick, sus)); break;
                                default: throw new Exception("Invalid diff value (min. 0 max. 3");
                            }
                        } else
                            throw new Exception("Invalid sec(instr.) value (must be 0 for guitar or 1 for bass).");
                    }
                }
            }
        }

        private static void WriteTapSection(IList<MidiEvent> track, int sec) {
            for (int i = 0; i < track.Count; i++) {
                var se = track[i] as SysexEvent;
                if (se != null) {
                    var b = se.GetData();
                    if (b.Length == 8 && b[5] == 255 && b[7] == 1) {
                        long tick = RoundToValidValue((long)Math.Floor(se.AbsoluteTime*scaler));
                        long sus = 0;
                        for (int j = i; j < track.Count; j++) {
                            var se2 = track[j] as SysexEvent;
                            if (se2 != null) {
                                var b2 = se2.GetData();
                                if (b2.Length == 8 && b2[5] == 255 && b2[7] == 0) {
                                    sus = RoundToValidValue((long)Math.Floor(se2.AbsoluteTime*scaler))-tick;
                                    break;
                                }
                            }
                        }
                        if (sus == 0) sus = 1;
                        if (sec == 0) { 
                            s.tapGuitar.Add(new NoteSection(tick, sus));
                        } else if (sec == 1)
                            s.tapBass.Add(new NoteSection(tick, sus));
                        else
                            throw new Exception("Invalid sec(instr.) value (must be 0 for guitar or 1 for bass).");
                    }
                }
            }
        }

        private static void WriteNoteSection(IList<MidiEvent> track, int sec) {
            for (int i = 0; i < track.Count; i++) {
                var note = track[i] as NoteOnEvent;
                if (note != null && note.OffEvent != null) {
                    var tick = RoundToValidValue((long)Math.Floor(note.AbsoluteTime*scaler));
                    var sus = RoundToValidValue((long)Math.Floor(note.OffEvent.AbsoluteTime*scaler))-tick;
                    int n = note.NoteNumber;
                    if (sec == 0) {
                        switch (n) {
                            case 60: if (sus <= 64L) sus = 0L; s.eGuitar.Add(new Note(Note.G, tick, sus)); break;
                            case 61: if (sus <= 64L) sus = 0L; s.eGuitar.Add(new Note(Note.R, tick, sus)); break;
                            case 62: if (sus <= 64L) sus = 0L; s.eGuitar.Add(new Note(Note.Y, tick, sus)); break;
                            case 63: if (sus <= 64L) sus = 0L; s.eGuitar.Add(new Note(Note.B, tick, sus)); break;
                            case 64: if (sus <= 64L) sus = 0L; s.eGuitar.Add(new Note(Note.O, tick, sus)); break;
                            case 65: if (sus == 0L) sus = 1L; s.eGuitarForceHOPO.Add(new NoteSection(tick, sus)); break;
                            case 66: if (sus == 0L) sus = 1L; s.eGuitarForceStrum.Add(new NoteSection(tick, sus)); break;
                            case 72: if (sus <= 64L) sus = 0L; s.mGuitar.Add(new Note(Note.G, tick, sus)); break;
                            case 73: if (sus <= 64L) sus = 0L; s.mGuitar.Add(new Note(Note.R, tick, sus)); break;
                            case 74: if (sus <= 64L) sus = 0L; s.mGuitar.Add(new Note(Note.Y, tick, sus)); break;
                            case 75: if (sus <= 64L) sus = 0L; s.mGuitar.Add(new Note(Note.B, tick, sus)); break;
                            case 76: if (sus <= 64L) sus = 0L; s.mGuitar.Add(new Note(Note.O, tick, sus)); break;
                            case 77: if (sus == 0L) sus = 1L; s.mGuitarForceHOPO.Add(new NoteSection(tick, sus)); break;
                            case 78: if (sus == 0L) sus = 1L; s.mGuitarForceStrum.Add(new NoteSection(tick, sus)); break;
                            case 84: if (sus <= 64L) sus = 0L; s.hGuitar.Add(new Note(Note.G, tick, sus)); break;
                            case 85: if (sus <= 64L) sus = 0L; s.hGuitar.Add(new Note(Note.R, tick, sus)); break;
                            case 86: if (sus <= 64L) sus = 0L; s.hGuitar.Add(new Note(Note.Y, tick, sus)); break;
                            case 87: if (sus <= 64L) sus = 0L; s.hGuitar.Add(new Note(Note.B, tick, sus)); break;
                            case 88: if (sus <= 64L) sus = 0L; s.hGuitar.Add(new Note(Note.O, tick, sus)); break;
                            case 89: if (sus == 0L) sus = 1L; s.hGuitarForceHOPO.Add(new NoteSection(tick, sus)); break;
                            case 90: if (sus == 0L) sus = 1L; s.hGuitarForceStrum.Add(new NoteSection(tick, sus)); break;
                            case 96: if (sus <= 64L) sus = 0L; s.xGuitar.Add(new Note(Note.G, tick, sus)); break;
                            case 97: if (sus <= 64L) sus = 0L; s.xGuitar.Add(new Note(Note.R, tick, sus)); break;
                            case 98: if (sus <= 64L) sus = 0L; s.xGuitar.Add(new Note(Note.Y, tick, sus)); break;
                            case 99: if (sus <= 64L) sus = 0L; s.xGuitar.Add(new Note(Note.B, tick, sus)); break;
                            case 100: if (sus <= 64L) sus = 0L; s.xGuitar.Add(new Note(Note.O, tick, sus)); break;
                            case 101: if (sus == 0L) sus = 1L; s.xGuitarForceHOPO.Add(new NoteSection(tick, sus)); break;
                            case 102: if (sus == 0L) sus = 1L; s.xGuitarForceStrum.Add(new NoteSection(tick, sus)); break;
                            case 103:
                                if (Program.gh1) {
                                    s.xGuitar.Add(new NoteSection(tick, sus));
                                    s.hGuitar.Add(new NoteSection(tick, sus));
                                    s.mGuitar.Add(new NoteSection(tick, sus));
                                    s.eGuitar.Add(new NoteSection(tick, sus));
                                }
                                break;
                            case 116:
                                if (!Program.gh1) { 
                                    s.xGuitar.Add(new NoteSection(tick, sus));
                                    s.hGuitar.Add(new NoteSection(tick, sus));
                                    s.mGuitar.Add(new NoteSection(tick, sus));
                                    s.eGuitar.Add(new NoteSection(tick, sus));
                                }
                                break;
                        }
                    } else if (sec == 1) {
                        switch (n) {
                            case 60: if (sus <= 64L) sus = 0L; s.eBass.Add(new Note(Note.G, tick, sus)); break;
                            case 61: if (sus <= 64L) sus = 0L; s.eBass.Add(new Note(Note.R, tick, sus)); break;
                            case 62: if (sus <= 64L) sus = 0L; s.eBass.Add(new Note(Note.Y, tick, sus)); break;
                            case 63: if (sus <= 64L) sus = 0L; s.eBass.Add(new Note(Note.B, tick, sus)); break;
                            case 64: if (sus <= 64L) sus = 0L; s.eBass.Add(new Note(Note.O, tick, sus)); break;
                            case 65: s.eBassForceHOPO.Add(new NoteSection(tick, sus)); break;
                            case 66: s.eBassForceStrum.Add(new NoteSection(tick, sus)); break;
                            case 72: if (sus <= 64L) sus = 0L; s.mBass.Add(new Note(Note.G, tick, sus)); break;
                            case 73: if (sus <= 64L) sus = 0L; s.mBass.Add(new Note(Note.R, tick, sus)); break;
                            case 74: if (sus <= 64L) sus = 0L; s.mBass.Add(new Note(Note.Y, tick, sus)); break;
                            case 75: if (sus <= 64L) sus = 0L; s.mBass.Add(new Note(Note.B, tick, sus)); break;
                            case 76: if (sus <= 64L) sus = 0L; s.mBass.Add(new Note(Note.O, tick, sus)); break;
                            case 77: s.mBassForceHOPO.Add(new NoteSection(tick, sus)); break;
                            case 78: s.mBassForceStrum.Add(new NoteSection(tick, sus)); break;
                            case 84: if (sus <= 64L) sus = 0L; s.hBass.Add(new Note(Note.G, tick, sus)); break;
                            case 85: if (sus <= 64L) sus = 0L; s.hBass.Add(new Note(Note.R, tick, sus)); break;
                            case 86: if (sus <= 64L) sus = 0L; s.hBass.Add(new Note(Note.Y, tick, sus)); break;
                            case 87: if (sus <= 64L) sus = 0L; s.hBass.Add(new Note(Note.B, tick, sus)); break;
                            case 88: if (sus <= 64L) sus = 0L; s.hBass.Add(new Note(Note.O, tick, sus)); break;
                            case 89: s.hBassForceHOPO.Add(new NoteSection(tick, sus)); break;
                            case 90: s.hBassForceStrum.Add(new NoteSection(tick, sus)); break;
                            case 96: if (sus <= 64L) sus = 0L; s.xBass.Add(new Note(Note.G, tick, sus)); break;
                            case 97: if (sus <= 64L) sus = 0L; s.xBass.Add(new Note(Note.R, tick, sus)); break;
                            case 98: if (sus <= 64L) sus = 0L; s.xBass.Add(new Note(Note.Y, tick, sus)); break;
                            case 99: if (sus <= 64L) sus = 0L; s.xBass.Add(new Note(Note.B, tick, sus)); break;
                            case 100: if (sus <= 64L) sus = 0L; s.xBass.Add(new Note(Note.O, tick, sus)); break;
                            case 101: s.xBassForceHOPO.Add(new NoteSection(tick, sus)); break;
                            case 102: s.xBassForceStrum.Add(new NoteSection(tick, sus)); break;
                            case 103:
                                if (Program.gh1) {
                                    s.xBass.Add(new NoteSection(tick, sus));
                                    s.hBass.Add(new NoteSection(tick, sus));
                                    s.mBass.Add(new NoteSection(tick, sus));
                                    s.eBass.Add(new NoteSection(tick, sus));
                                }
                                break;
                            case 116:
                                if (!Program.gh1) {
                                    s.xBass.Add(new NoteSection(tick, sus));
                                    s.hBass.Add(new NoteSection(tick, sus));
                                    s.mBass.Add(new NoteSection(tick, sus));
                                    s.eBass.Add(new NoteSection(tick, sus));
                                }
                                break;
                        }
                    } else if (sec == 2) {
                        switch (n) {
                            case 60: if (sus <= 64L) sus = 0L; s.eKeys.Add(new Note(Note.G, tick, sus)); break;
                            case 61: if (sus <= 64L) sus = 0L; s.eKeys.Add(new Note(Note.R, tick, sus)); break;
                            case 62: if (sus <= 64L) sus = 0L; s.eKeys.Add(new Note(Note.Y, tick, sus)); break;
                            case 63: if (sus <= 64L) sus = 0L; s.eKeys.Add(new Note(Note.B, tick, sus)); break;
                            case 64: if (sus <= 64L) sus = 0L; s.eKeys.Add(new Note(Note.O, tick, sus)); break;
                            case 72: if (sus <= 64L) sus = 0L; s.mKeys.Add(new Note(Note.G, tick, sus)); break;
                            case 73: if (sus <= 64L) sus = 0L; s.mKeys.Add(new Note(Note.R, tick, sus)); break;
                            case 74: if (sus <= 64L) sus = 0L; s.mKeys.Add(new Note(Note.Y, tick, sus)); break;
                            case 75: if (sus <= 64L) sus = 0L; s.mKeys.Add(new Note(Note.B, tick, sus)); break;
                            case 76: if (sus <= 64L) sus = 0L; s.mKeys.Add(new Note(Note.O, tick, sus)); break;
                            case 84: if (sus <= 64L) sus = 0L; s.hKeys.Add(new Note(Note.G, tick, sus)); break;
                            case 85: if (sus <= 64L) sus = 0L; s.hKeys.Add(new Note(Note.R, tick, sus)); break;
                            case 86: if (sus <= 64L) sus = 0L; s.hKeys.Add(new Note(Note.Y, tick, sus)); break;
                            case 87: if (sus <= 64L) sus = 0L; s.hKeys.Add(new Note(Note.B, tick, sus)); break;
                            case 88: if (sus <= 64L) sus = 0L; s.hKeys.Add(new Note(Note.O, tick, sus)); break;
                            case 96: if (sus <= 64L) sus = 0L; s.xKeys.Add(new Note(Note.G, tick, sus)); break;
                            case 97: if (sus <= 64L) sus = 0L; s.xKeys.Add(new Note(Note.R, tick, sus)); break;
                            case 98: if (sus <= 64L) sus = 0L; s.xKeys.Add(new Note(Note.Y, tick, sus)); break;
                            case 99: if (sus <= 64L) sus = 0L; s.xKeys.Add(new Note(Note.B, tick, sus)); break;
                            case 100: if (sus <= 64L) sus = 0L; s.xKeys.Add(new Note(Note.O, tick, sus)); break;
                            case 103:
                                if (Program.gh1) {
                                    s.xKeys.Add(new NoteSection(tick, sus));
                                    s.hKeys.Add(new NoteSection(tick, sus));
                                    s.mKeys.Add(new NoteSection(tick, sus));
                                    s.eKeys.Add(new NoteSection(tick, sus));
                                }
                                break;
                            case 116:
                                if (!Program.gh1) {
                                    s.xKeys.Add(new NoteSection(tick, sus));
                                    s.hKeys.Add(new NoteSection(tick, sus));
                                    s.mKeys.Add(new NoteSection(tick, sus));
                                    s.eKeys.Add(new NoteSection(tick, sus));
                                }
                                break;
                        }
                    } else {
                        throw new Exception("Invalid sec(instr.) value (must be 0 for guitar, 1 for bass or 2 for keys)");
                    }
                }
            }
        }

        private static void WriteSongSections(IList<MidiEvent> track) {
            for (int i = 0; i < track.Count; i++) {
                var text = track[i] as TextEvent;
                if (text != null && text.Text.Contains("[section ")) 
                    s.sections.Add(new Section((long)Math.Floor(text.AbsoluteTime*scaler), text.Text.Substring(9, text.Text.Length-10)));
                else if (text != null && text.Text.Contains("[prc_"))
                    s.sections.Add(new Section((long)Math.Floor(text.AbsoluteTime*scaler), text.Text.Substring(5, text.Text.Length-6)));
            }
        }

        private static void WriteSync(IList<MidiEvent> track) {
            foreach(var me in track) {
                var ts = me as TimeSignatureEvent;
                if (ts != null) {
                    var tick = RoundToValidValue((long)Math.Floor(ts.AbsoluteTime*scaler));
                    s.sync.Add(new Sync(tick, ts.Numerator, false));
                    continue;
                }
                var tempo = me as TempoEvent;
                if (tempo != null) {
                    var tick = RoundToValidValue((long)Math.Floor(tempo.AbsoluteTime*scaler));
                    s.sync.Add(new Sync(tick, (int)Math.Floor(tempo.Tempo*1000), true));
                    continue;
                }
                var text = me as TextEvent;
                if (text != null) {
                    s.songname = text.Text;
                }
            }
        }

        private static long RoundToValidValue(long tick) {
            long a = tick+(16-(tick%16));
            long b = tick-(tick%16);
            long c = tick+(12-(tick%12));
            long d = tick-(tick%12);
            long ab; long cd;
            if (a-tick < -(b-tick))
                ab = a;
            else
                ab = b;
            if (c-tick < -(d-tick))
                cd = c;
            else
                cd = d;
            long abd; long cdd;
            if (tick-ab < 0)
                abd = -(tick-ab);
            else
                abd = tick-ab;
            if (tick-cd < 0)
                cdd = -(tick-cd);
            else
                cdd = tick-cd;
            long tickd;
            if (abd < cdd)
                tickd = tick-(tick-ab);
            else
                tickd = tick-(tick-cd);
            return tickd;
        }
    }
}