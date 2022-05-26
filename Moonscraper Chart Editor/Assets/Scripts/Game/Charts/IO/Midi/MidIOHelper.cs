// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MoonscraperChartEditor.Song.IO
{
    public static class MidIOHelper
    {
        public const string EVENTS_TRACK = "EVENTS";           // Sections
        public const string GUITAR_TRACK = "PART GUITAR";
        public const string GH1_GUITAR_TRACK = "T1 GEMS";
        public const string GUITAR_COOP_TRACK = "PART GUITAR COOP";
        public const string BASS_TRACK = "PART BASS";
        public const string RHYTHM_TRACK = "PART RHYTHM";
        public const string KEYS_TRACK = "PART KEYS";
        public const string DRUMS_TRACK = "PART DRUMS";
        public const string DRUMS_REAL_TRACK = "PART REAL_DRUMS_PS";
        public const string GHL_GUITAR_TRACK = "PART GUITAR GHL";
        public const string GHL_BASS_TRACK = "PART BASS GHL";
        public const string VOCALS_TRACK = "PART VOCALS";

        public static readonly Dictionary<Song.Difficulty, int> GUITAR_DIFF_RANGE_LOOKUP = new Dictionary<Song.Difficulty, int>()
    {
        { Song.Difficulty.Easy, 60 },
        { Song.Difficulty.Medium, 72 },
        { Song.Difficulty.Hard, 84 },
        { Song.Difficulty.Expert, 96 }
    };

        public static readonly Dictionary<Song.Difficulty, int> GHL_GUITAR_DIFF_RANGE_LOOKUP = new Dictionary<Song.Difficulty, int>()
    {
        { Song.Difficulty.Easy, 58 },
        { Song.Difficulty.Medium, 70 },
        { Song.Difficulty.Hard, 82 },
        { Song.Difficulty.Expert, 94 }
    };

        public static readonly Dictionary<Song.Difficulty, int> DRUMS_DIFF_RANGE_LOOKUP = new Dictionary<Song.Difficulty, int>()
    {
        { Song.Difficulty.Easy, 60 },
        { Song.Difficulty.Medium, 72 },
        { Song.Difficulty.Hard, 84 },
        { Song.Difficulty.Expert, 96 }
    };

        public const string LYRIC_EVENT_PREFIX = LyricHelper.LYRIC_EVENT_PREFIX;
        public const byte SOLO_NOTE = 0x67;                 // 103, http://docs.c3universe.com/rbndocs/index.php?title=Guitar_and_Bass_Authoring#Solo_Sections
        public const byte TAP_NOTE = 0x68;                  // 104, https://strikeline.myjetbrains.com/youtrack/issue/CH-390
        public const byte STARPOWER_NOTE = 0x74;            // 116, http://docs.c3universe.com/rbndocs/index.php?title=Overdrive_and_Big_Rock_Endings

        // 120 - 124 http://docs.c3universe.com/rbndocs/index.php?title=Drum_Authoring#Drum_Fills
        public const byte STARPOWER_DRUM_FILL_0 = 120;
        public const byte STARPOWER_DRUM_FILL_1 = 121;
        public const byte STARPOWER_DRUM_FILL_2 = 122;
        public const byte STARPOWER_DRUM_FILL_3 = 123;
        public const byte STARPOWER_DRUM_FILL_4 = 124;

        public const string SoloEventText = "solo";
        public const string SoloEndEventText = "soloend";
        public const int DOUBLE_KICK_NOTE = 95;
        public const byte FLAM_MARKER = 0x6d;                  // 109

        public const int PhraseMarker = 105; // http://docs.c3universe.com/rbndocs/index.php?title=Vocal_Authoring
        public const int PhraseMarker2 = 106; // Rock Band charts before RB3 mark phrases using this note as well
        public const string PhraseStartText = LyricHelper.PhraseStartText;
        public const string PhraseEndText = LyricHelper.PhraseEndText;

        public const string Rb2SectionPrefix = "section ";
        public const string Rb3SectionPrefix = "prc_";

        public const byte VELOCITY = 0x64;              // 100
        public const byte VELOCITY_ACCENT = 0x7f;       // 127, fof/ps
        public const byte VELOCITY_GHOST = 0x1;         // 1, fof/ps

        // http://docs.c3universe.com/rbndocs/index.php?title=Drum_Authoring
        public static readonly Dictionary<Note.DrumPad, int> PAD_TO_CYMBAL_LOOKUP = new Dictionary<Note.DrumPad, int>()
    {
        { Note.DrumPad.Yellow, 110 },
        { Note.DrumPad.Blue, 111 },
        { Note.DrumPad.Orange, 112 },
    };

        public static readonly Dictionary<int, Note.DrumPad> CYMBAL_TO_PAD_LOOKUP = PAD_TO_CYMBAL_LOOKUP.ToDictionary((i) => i.Value, (i) => i.Key);
    }
}
