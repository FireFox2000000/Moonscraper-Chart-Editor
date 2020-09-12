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
        public const string GUITAR_COOP_TRACK = "PART GUITAR COOP";
        public const string BASS_TRACK = "PART BASS";
        public const string RHYTHM_TRACK = "PART RHYTHM";
        public const string KEYS_TRACK = "PART KEYS";
        public const string DRUMS_TRACK = "PART DRUMS";
        public const string GHL_GUITAR_TRACK = "PART GUITAR GHL";
        public const string GHL_BASS_TRACK = "PART BASS GHL";
        public const string VOCALS_TRACK = "PART VOCALS";

        public const string LYRIC_EVENT_PREFIX = "lyric ";
        public const byte SOLO_NOTE = 0x67;    // 103, http://docs.c3universe.com/rbndocs/index.php?title=Guitar_and_Bass_Authoring#Solo_Sections
        public const byte STARPOWER_NOTE = 0x74;   // 116, http://docs.c3universe.com/rbndocs/index.php?title=Overdrive_and_Big_Rock_Endings
        public const string SoloEventText = "solo";
        public const string SoloEndEventText = "soloend";
        public const int DOUBLE_KICK_NOTE = 95;

        public const int PhraseMarker = 105; // http://docs.c3universe.com/rbndocs/index.php?title=Vocal_Authoring
        public const string PhraseStartText = "phrase_start";
        public const string PhraseEndText = "phrase_end";

        public const string Rb2SectionPrefix = "section ";
        public const string Rb3SectionPrefix = "prc_";

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
