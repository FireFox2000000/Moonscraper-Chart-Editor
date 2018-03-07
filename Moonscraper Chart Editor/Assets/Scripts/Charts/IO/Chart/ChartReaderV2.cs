using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class HackyStringViewFunctions
{
    const char CHAR_QUOTE = '\"';

    public static uint AdvanceStringToUInt(string str, ref int charOffset)
    {
        uint val = 0;

        if (!(charOffset < str.Length))
            return val;

        if (str[charOffset] == ' ')
            ++charOffset;

        while (charOffset < str.Length && str[charOffset] != ' ')
        {
            int d;
            if ((d = str[charOffset] - '0') <= 9)
            {
                val = val * 10 + (uint)d;
            }

            ++charOffset;
        }

        ++charOffset;

        return val;
    }

    public static uint GetNextTick(string str, ref int charOffset)
    {
        return AdvanceStringToUInt(str, ref charOffset);
    }

    public static string GetNextTextUpToQuote(string str, ref int charOffset)
    {
        if (str[charOffset] == CHAR_QUOTE)
            ++charOffset;

        int substringStart = charOffset;

        while (charOffset < str.Length && str[charOffset] != CHAR_QUOTE)
        {
            ++charOffset;
        }

        return str.Substring(substringStart, charOffset - substringStart);
    }

   public static void AdvanceChartLineStringView(string str, ref int charOffset)
    {
        if (!(charOffset < str.Length))
            return;

        if (str[charOffset] == ' ')
            ++charOffset;

        while (charOffset < str.Length && str[charOffset] != ' ')
        {
            ++charOffset;
        }

        ++charOffset;
    }
}

public static class ChartReaderV2 {
    const char CHAR_QUOTE = '\"';

    struct Anchor
    {
        public uint position;
        public float anchorTime;
    }

    struct NoteFlag
    {
        public uint position;
        public Note.Flags flag;

        public NoteFlag(uint position, Note.Flags flag)
        {
            this.position = position;
            this.flag = flag;
        }
    }

    static void ProcessSongComponentLine(string line, Song song, List<Anchor> anchorData)
    {
        int stringViewIndex = 0;

        uint position = HackyStringViewFunctions.GetNextTick(line, ref stringViewIndex);
        HackyStringViewFunctions.AdvanceChartLineStringView(line, ref stringViewIndex);  // Skip over the equals sign
        char type = line[stringViewIndex];
        HackyStringViewFunctions.AdvanceChartLineStringView(line, ref stringViewIndex);

        switch (type)
        {
            case 'B':
                {
                    uint value = HackyStringViewFunctions.AdvanceStringToUInt(line, ref stringViewIndex);
                    song.Add(new BPM(position, value), false);
                }
                break;
            case 'T':
                {
                    uint numerator = HackyStringViewFunctions.AdvanceStringToUInt(line, ref stringViewIndex);

                    TimeSignature ts = new TimeSignature(position, numerator);
                    song.Add(ts, false);

                    if (stringViewIndex < line.Length)
                    {
                        uint denominator = HackyStringViewFunctions.AdvanceStringToUInt(line, ref stringViewIndex);

                        denominator = (uint)(Mathf.Pow(2, denominator));
                        ts.denominator = denominator;
                    }
                }
                break;
            case 'E':
                {
                    string text = HackyStringViewFunctions.GetNextTextUpToQuote(line, ref stringViewIndex);
                    const string SECTION_ID = "section";

                    // Check if it's a section
                    if (string.Compare(text, 0, SECTION_ID, 0, SECTION_ID.Length) == 0)
                    {
                        text = text.Remove(0, SECTION_ID.Length);
                        text = text.Trim();
                        song.Add(new Section(text, position), false);
                    }
                    else
                    {
                        song.Add(new Event(text, position), false);
                    }
                }
                break;
            case 'A':
                {
                    uint anchorValue = HackyStringViewFunctions.AdvanceStringToUInt(line, ref stringViewIndex);
                    Anchor a;
                    a.position = position;
                    a.anchorTime = (float)(anchorValue / 1000000.0d);
                    anchorData.Add(a);
                }
                break;
            default:
                return;
        }
    }

    static void ProcessTrackComponentLine(string line, Chart chart, Song.Instrument instrument, List<NoteFlag> flagsList)
    {
        int stringViewIndex = 0;

        uint position = HackyStringViewFunctions.GetNextTick(line, ref stringViewIndex);
        HackyStringViewFunctions.AdvanceChartLineStringView(line, ref stringViewIndex);  // Skip over the equals sign

        char type = line[stringViewIndex];
        HackyStringViewFunctions.AdvanceChartLineStringView(line, ref stringViewIndex);

        switch (type)
        {
            case 'N':
                {
                    int noteNumber = (int)HackyStringViewFunctions.AdvanceStringToUInt(line, ref stringViewIndex);
                    uint length = HackyStringViewFunctions.GetNextTick(line, ref stringViewIndex);

                    switch (instrument)
                    {
                        case Song.Instrument.GHLiveGuitar:
                        case Song.Instrument.GHLiveBass:
                            LoadGHLiveNote(chart, position, noteNumber, length, flagsList);
                            break;
                        case Song.Instrument.Drums:
                            LoadDrumNote(chart, position, noteNumber, length);
                            break;
                        case Song.Instrument.Guitar:
                        case Song.Instrument.GuitarCoop:
                        case Song.Instrument.Rhythm:
                        case Song.Instrument.Bass:
                        case Song.Instrument.Keys:
                            LoadStandardNote(chart, position, noteNumber, length, flagsList);
                            break;
                        default:    // Unrecognised
                            Note newNote = new Note(position, noteNumber, length);
                            chart.Add(newNote, false);
                            break;
                    }
                }
                break;

            case 'S':
                {
                    int specialNumber = (int)HackyStringViewFunctions.AdvanceStringToUInt(line, ref stringViewIndex);
                    uint length = HackyStringViewFunctions.GetNextTick(line, ref stringViewIndex);

                    if (specialNumber == 2)
                        chart.Add(new Starpower(position, length), false);
                }
                break;

            case 'E':
                {
                    string eventName = HackyStringViewFunctions.GetNextTextUpToQuote(line, ref stringViewIndex);
                    chart.Add(new ChartEvent(position, eventName), false);
                }
                break;

            default:
                break;
        }
    }

    static void LoadStandardNote(Chart chart, uint position, int noteNumber, uint length, List<NoteFlag> flagsList)
    {
        Note.Fret_Type? noteFret = null;
        switch (noteNumber)
        {
            case (0):
                noteFret = Note.Fret_Type.GREEN;
                break;
            case (1):
                noteFret = Note.Fret_Type.RED;
                break;
            case (2):
                noteFret = Note.Fret_Type.YELLOW;
                break;
            case (3):
                noteFret = Note.Fret_Type.BLUE;
                break;
            case (4):
                noteFret = Note.Fret_Type.ORANGE;
                break;
            case (5):
                NoteFlag forcedFlag = new NoteFlag(position, Note.Flags.FORCED);
                flagsList.Add(forcedFlag);
                break;
            case (6):
                NoteFlag tapFlag = new NoteFlag(position, Note.Flags.TAP);
                flagsList.Add(tapFlag);
                break;
            case (7):
                noteFret = Note.Fret_Type.OPEN;
                break;
            default:
                return;
        }

        if (noteFret != null)
        {
            Note newNote = new Note(position, (int)noteFret, length);
            chart.Add(newNote, false);
        }
    }

    static void LoadDrumNote(Chart chart, uint position, int noteNumber, uint length)
    {
        Note.Drum_Fret_Type? noteFret = null;
        switch (noteNumber)
        {
            case (0):
                noteFret = Note.Drum_Fret_Type.KICK;
                break;
            case (1):
                noteFret = Note.Drum_Fret_Type.RED;
                break;
            case (2):
                noteFret = Note.Drum_Fret_Type.YELLOW;
                break;
            case (3):
                noteFret = Note.Drum_Fret_Type.BLUE;
                break;
            case (4):
                noteFret = Note.Drum_Fret_Type.ORANGE;
                break;
            case (5):
                noteFret = Note.Drum_Fret_Type.GREEN;
                break;
            default:
                return;
        }

        if (noteFret != null)
        {
            Note newNote = new Note(position, (int)noteFret, length);
            chart.Add(newNote, false);
        }
    }

    static void LoadGHLiveNote(Chart chart, uint position, int noteNumber, uint length, List<NoteFlag> flagsList)
    {
        Note.GHLive_Fret_Type? noteFret = null;
        switch (noteNumber)
        {
            case (0):
                noteFret = Note.GHLive_Fret_Type.WHITE_1;
                break;
            case (1):
                noteFret = Note.GHLive_Fret_Type.WHITE_2;
                break;
            case (2):
                noteFret = Note.GHLive_Fret_Type.WHITE_3;
                break;
            case (3):
                noteFret = Note.GHLive_Fret_Type.BLACK_1;
                break;
            case (4):
                noteFret = Note.GHLive_Fret_Type.BLACK_2;
                break;
            case (5):
                flagsList.Add(new NoteFlag(position, Note.Flags.FORCED));
                break;
            case (6):
                flagsList.Add(new NoteFlag(position, Note.Flags.TAP));
                break;
            case (7):
                noteFret = Note.GHLive_Fret_Type.OPEN;
                break;
            case (8):
                noteFret = Note.GHLive_Fret_Type.BLACK_3;
                break;
            default:
                return;
        }

        if (noteFret != null)
        {
            Note newNote = new Note(position, (int)noteFret, length);
            chart.Add(newNote, false);
        }
    }
}