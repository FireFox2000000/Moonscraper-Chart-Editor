using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class ChartIOHelper
{
    public const string
        c_dataBlockSong = "[Song]"
        , c_dataBlockSyncTrack = "[SyncTrack]"
        , c_dataBlockEvents = "[Events]"
        ;

    public static readonly Dictionary<int, int> c_guitarNoteNumLookup = new Dictionary<int, int>()
    {
        { 0, (int)Note.GuitarFret.Green     },
        { 1, (int)Note.GuitarFret.Red       },
        { 2, (int)Note.GuitarFret.Yellow    },
        { 3, (int)Note.GuitarFret.Blue      },
        { 4, (int)Note.GuitarFret.Orange    },
        { 7, (int)Note.GuitarFret.Open      },
    };

    public static readonly Dictionary<int, Note.Flags> c_guitarFlagNumLookup = new Dictionary<int, Note.Flags>()
    {
        { 5      , Note.Flags.Forced },
        { 6      , Note.Flags.Tap },
    };

    public static readonly Dictionary<int, int> c_drumNoteNumLookup = new Dictionary<int, int>()
    {
        { 0, (int)Note.DrumPad.Kick      },
        { 1, (int)Note.DrumPad.Red       },
        { 2, (int)Note.DrumPad.Yellow    },
        { 3, (int)Note.DrumPad.Blue      },
        { 4, (int)Note.DrumPad.Orange    },
        { 5, (int)Note.DrumPad.Green     },
    };

    // Default flags, mark as cymbal for pro drums automatically
    public static readonly Dictionary<int, Note.Flags> c_drumNoteDefaultFlagsLookup = new Dictionary<int, Note.Flags>()
    {
        { (int)Note.DrumPad.Kick      , Note.Flags.None },
        { (int)Note.DrumPad.Red       , Note.Flags.None },
        { (int)Note.DrumPad.Yellow    , Note.Flags.ProDrums_Cymbal },
        { (int)Note.DrumPad.Blue      , Note.Flags.ProDrums_Cymbal },
        { (int)Note.DrumPad.Orange    , Note.Flags.None },
        { (int)Note.DrumPad.Green     , Note.Flags.ProDrums_Cymbal },
    };

    public static readonly Dictionary<int, int> c_ghlNoteNumLookup = new Dictionary<int, int>()
    {
        { 0, (int)Note.GHLiveGuitarFret.White1     },
        { 1, (int)Note.GHLiveGuitarFret.White2       },
        { 2, (int)Note.GHLiveGuitarFret.White3    },
        { 3, (int)Note.GHLiveGuitarFret.Black1      },
        { 4, (int)Note.GHLiveGuitarFret.Black2    },
        { 8, (int)Note.GHLiveGuitarFret.Black3      },
        { 7, (int)Note.GHLiveGuitarFret.Open      },
    };

    public static readonly Dictionary<int, Note.Flags> c_ghlFlagNumLookup = c_guitarFlagNumLookup;

    public static readonly Dictionary<string, Song.Difficulty> c_trackNameToTrackDifficultyLookup = new Dictionary<string, Song.Difficulty>()
    {
        { "Easy",   Song.Difficulty.Easy    },
        { "Medium", Song.Difficulty.Medium  },
        { "Hard",   Song.Difficulty.Hard    },
        { "Expert", Song.Difficulty.Expert  },
    };

    public static readonly Dictionary<string, Song.Instrument> c_instrumentStrToEnumLookup = new Dictionary<string, Song.Instrument>()
    {
        { "Single",         Song.Instrument.Guitar },
        { "DoubleGuitar",   Song.Instrument.GuitarCoop },
        { "DoubleBass",     Song.Instrument.Bass },
        { "DoubleRhythm",   Song.Instrument.Rhythm },
        { "Drums",          Song.Instrument.Drums },
        { "Keyboard",       Song.Instrument.Keys },
        { "GHLGuitar",      Song.Instrument.GHLiveGuitar },
        { "GHLBass",        Song.Instrument.GHLiveBass },
    };

    public static readonly Dictionary<Song.Instrument, Song.Instrument> c_instrumentParsingTypeLookup = new Dictionary<Song.Instrument, Song.Instrument>()
    {
        // Other instruments default to loading as a guitar type track
        { Song.Instrument.Drums,          Song.Instrument.Drums },
        { Song.Instrument.GHLiveGuitar ,  Song.Instrument.GHLiveGuitar },
        { Song.Instrument.GHLiveBass ,  Song.Instrument.GHLiveBass },
    };

    public static class MetaData
    {
        const string QUOTEVALIDATE = @"""[^""\\]*(?:\\.[^""\\]*)*""";
        const string QUOTESEARCH = "\"([^\"]*)\"";
        const string FLOATSEARCH = @"[\-\+]?\d+(\.\d+)?";

        public readonly static Regex nameRegex = new Regex(@"Name = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex artistRegex = new Regex(@"Artist = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex charterRegex = new Regex(@"Charter = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex offsetRegex = new Regex(@"Offset = " + FLOATSEARCH, RegexOptions.Compiled);
        public readonly static Regex resolutionRegex = new Regex(@"Resolution = " + FLOATSEARCH, RegexOptions.Compiled);
        public readonly static Regex player2TypeRegex = new Regex(@"Player2 = \w+", RegexOptions.Compiled);
        public readonly static Regex difficultyRegex = new Regex(@"Difficulty = \d+", RegexOptions.Compiled);
        public readonly static Regex lengthRegex = new Regex(@"Length = " + FLOATSEARCH, RegexOptions.Compiled);
        public readonly static Regex previewStartRegex = new Regex(@"PreviewStart = " + FLOATSEARCH, RegexOptions.Compiled);
        public readonly static Regex previewEndRegex = new Regex(@"PreviewEnd = " + FLOATSEARCH, RegexOptions.Compiled);
        public readonly static Regex genreRegex = new Regex(@"Genre = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex yearRegex = new Regex(@"Year = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex albumRegex = new Regex(@"Album = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex mediaTypeRegex = new Regex(@"MediaType = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex musicStreamRegex = new Regex(@"MusicStream = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex guitarStreamRegex = new Regex(@"GuitarStream = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex bassStreamRegex = new Regex(@"BassStream = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex rhythmStreamRegex = new Regex(@"RhythmStream = " + QUOTEVALIDATE, RegexOptions.Compiled);
        public readonly static Regex drumStreamRegex = new Regex(@"DrumStream = " + QUOTEVALIDATE, RegexOptions.Compiled);

        public static string ParseAsString(string line)
        {
            return Regex.Matches(line, QUOTESEARCH)[0].ToString().Trim('"');
        }

        public static float ParseAsFloat(string line)
        {
            return float.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
        }

        public static short ParseAsShort(string line)
        {
            return short.Parse(Regex.Matches(line, FLOATSEARCH)[0].ToString());
        }
    }
}
