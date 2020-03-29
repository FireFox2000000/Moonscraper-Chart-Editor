using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class MidIOHelper {
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

    public static readonly Dictionary<Note.DrumPad, int> PAD_TO_CYMBAL_LOOKUP = new Dictionary<Note.DrumPad, int>()
    {
        { Note.DrumPad.Yellow, 110 },
        { Note.DrumPad.Blue, 111 },
        { Note.DrumPad.Orange, 112 },
    };

    public static readonly Dictionary<int, Note.DrumPad> CYMBAL_TO_PAD_LOOKUP = PAD_TO_CYMBAL_LOOKUP.ToDictionary((i) => i.Value, (i) => i.Key);
}
