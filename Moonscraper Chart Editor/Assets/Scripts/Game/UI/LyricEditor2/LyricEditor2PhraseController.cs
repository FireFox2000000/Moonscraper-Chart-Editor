using MoonscraperChartEditor.Song;
using System.Collections.Generic;
using UnityEngine.UI;

public class LyricEditor2PhraseController : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    Text phraseText;
    [UnityEngine.SerializeField]
    UnityEngine.Color defaultColor;
    [UnityEngine.SerializeField]
    UnityEngine.Color unfocusedColor;
    [UnityEngine.SerializeField]
    UnityEngine.Color selectionColor;

    static string c_phraseStartKeyword = "phrase_start";
    static string c_phraseEndKeyword = "phrase_end";
    static string c_lyricPrefix = "lyric ";
    private bool isCurrentlyPlacingLyric = false;
    public bool allSyllablesPlaced {get; private set;} = false;
    public bool phraseStartPlaced {get {return phraseStartEvent.hasBeenPlaced;}}
    public bool phraseEndPlaced {get {return phraseEndEvent.hasBeenPlaced;}}
    public uint? startTick {get {if (phraseStartPlaced) return phraseStartEvent.tick; else return null;}}
    public uint? endTick {get {if (phraseEndPlaced) return phraseEndEvent.tick; else return null;}}

    List<LyricEditor2Event> lyricEvents = new List<LyricEditor2Event>();
    LyricEditor2Event phraseStartEvent = new LyricEditor2Event(c_phraseStartKeyword);
    LyricEditor2Event phraseEndEvent = new LyricEditor2Event(c_phraseEndKeyword);


    void CheckForUnplacedSyllables() {
        if (GetNextUnplacedSyllable() == null) {
            allSyllablesPlaced = true;
        }
    }

    // Place the next lyric in lyricEvents
    public void StartPlaceNextLyric(uint tick) {
        LyricEditor2Event currentLyric = GetNextUnplacedSyllable();
        currentLyric.SetTick(tick);
        isCurrentlyPlacingLyric = true;

        // Check for any remaining syllables
        CheckForUnplacedSyllables();
    }

    // Stop placing the next lyric (useful for formatting in DisplayText())
    public void StopPlaceNextLyric() {
        isCurrentlyPlacingLyric = false;
    }

    // OLD
    /*
    // Returns a value that is positive if event1 occurs after event2 and vice-
    // versa; returns 0 if the two events have the same tick
    int CompareLyricEvents (LyricEditor2Event event1, LyricEditor2Event event2) {
        if (event1 == null && event2 == null) {
            return 0;
        } else if (event2 == null) {
            return 1;
        } else if (event1 == null) {
            return -1;
        } else {
            return (int)event1.tick - (int)event2.tick;
        }
    }

    // Get the tick of the first event of this phrase
    public uint? GetFirstTick() {
        LyricEditor2Event firstEvent = null;
        if (CompareLyricEvents(phraseStartEvent, firstEvent) <= 0) {
            firstEvent = phraseStartEvent;
        }
        if (CompareLyricEvents(phraseEndEvent, firstEvent) <= 0) {
            firstEvent = phraseEndEvent;
        }
        foreach (LyricEditor2Event currentEvent in lyricEvents) {
            if (CompareLyricEvents(currentEvent, firstEvent) <= 0) {
                firstEvent = currentEvent;
            }
        }
        if (firstEvent == null) {
            return null;
        } else {
            return firstEvent.tick;
        }
    }

    // Get the tick of the first event of this phrase
    public uint? GetLastTick() {
        LyricEditor2Event lastEvent = null;
        if (CompareLyricEvents(phraseStartEvent, lastEvent) >= 0) {
            lastEvent = phraseStartEvent;
        }
        if (CompareLyricEvents(phraseEndEvent, lastEvent) >= 0) {
            lastEvent = phraseEndEvent;
        }
        foreach (LyricEditor2Event currentEvent in lyricEvents) {
            if (CompareLyricEvents(currentEvent, lastEvent) >= 0) {
                lastEvent = currentEvent;
            }
        }
        if (lastEvent == null) {
            return null;
        } else {
            return lastEvent.tick;
        }
    }
    */

    // Set the phrase_start event's tick
    public void SetPhraseStart(uint tick) {
        phraseStartEvent.SetTick(tick);
    }

    // Set the phrase_end event's tick
    public void SetPhraseEnd(uint tick) {
        phraseEndEvent.SetTick(tick);
    }

    void FormatAndAddSyllable(string syllable, LyricEditor2Event targetEvent) {
        if (syllable.EndsWith("-")) {
            targetEvent.formattedText = syllable;
        } else {
            targetEvent.formattedText = syllable + " ";
        }
    }

    LyricEditor2Event GetNextUnplacedSyllable() {
        for (int i = 0; i < lyricEvents.Count; i++) {
            LyricEditor2Event currentEvent = lyricEvents[i];
            if (!currentEvent.hasBeenPlaced) {
                return currentEvent;
            }
        }
        return null;
    }

    // Initialize lyricEvents using a list of string syllables. Syllables which
    // do not end with a dash will be displayed with a trailing space
    public void InitializeSyllables(List<string> syllables) {
        phraseStartEvent = new LyricEditor2Event(c_phraseStartKeyword);
        phraseEndEvent = new LyricEditor2Event(c_phraseEndKeyword);

        for (int i = 0; i < syllables.Count; i++) {
            string currentSyllable = syllables[i];
            string formattedSyllable = currentSyllable.TrimEnd();

            LyricEditor2Event newEvent = new LyricEditor2Event(c_lyricPrefix + formattedSyllable);
            // Add syllables to lyricEvents
            lyricEvents.Add(newEvent);
            // Add formatted name to event
            FormatAndAddSyllable(formattedSyllable, newEvent);
        }
        CheckForUnplacedSyllables();
    }

    // Initialize lyricEvents using a list of events which already exist in the
    // chart editor. Method also looks for phrase_start and phrase_end events
    // and, if they exist, assign them to the appropriate variables. Events
    // which do not start with c_lyricPrefix are ignored
    public void InitializeSyllables(List<Event> existingEvents) {
        for (int i = 0; i < existingEvents.Count; i++) {
            Event currentEvent = existingEvents[i];

            if (currentEvent.title.Equals(c_phraseStartKeyword)) {
                phraseStartEvent = new LyricEditor2Event(currentEvent);
            } else if (currentEvent.title.Equals(c_phraseEndKeyword)) {
                phraseEndEvent = new LyricEditor2Event(currentEvent);
            } else if (currentEvent.title.StartsWith(c_lyricPrefix)) {
                LyricEditor2Event newEvent = new LyricEditor2Event(currentEvent);
                lyricEvents.Add(newEvent);

                string formattedSyllable = currentEvent.title.TrimEnd();
                // Remove lyric prefix
                formattedSyllable = formattedSyllable.Substring(c_lyricPrefix.Length);
                // Add formatted name to event
                FormatAndAddSyllable(formattedSyllable, newEvent);
            }
        }
        // Make sure phrase_start and phrase_end events exist
        if (phraseStartEvent == null) {
            phraseStartEvent = new LyricEditor2Event(c_phraseStartKeyword);
        }
        if (phraseEndEvent == null) {
            phraseEndEvent = new LyricEditor2Event(c_phraseEndKeyword);
        }
        CheckForUnplacedSyllables();
    }

    // Update the text content of phraseText to reflect the current phrase state
    void DisplayText() {
        string defaultColorString = UnityEngine.ColorUtility.ToHtmlStringRGBA(defaultColor);
        string unfocusedColorString = UnityEngine.ColorUtility.ToHtmlStringRGBA(unfocusedColor);
        string selectionColorString = UnityEngine.ColorUtility.ToHtmlStringRGBA(selectionColor);
        string previousColor = "";
        string textToDisplay = "";

        for (int i = 0; i < lyricEvents.Count; i++) {
            LyricEditor2Event currentEvent = lyricEvents[i];
            string currentColor;

            // Set currentColor
            if (currentEvent.hasBeenPlaced) {
                currentColor = unfocusedColorString;
            } else if (isCurrentlyPlacingLyric) {
                currentColor = selectionColorString;
            } else {
                currentColor = defaultColorString;
            }

            // Add color tags
            if (currentColor != previousColor) {
                if (previousColor != "") {
                    textToDisplay += "</color>";
                }
                textToDisplay += "<color=" + currentColor + ">";
            }
            textToDisplay += currentEvent.formattedText;
        }
        // Add terminating color tag
        textToDisplay += "</color>";
        // Update UI text
        phraseText.text = textToDisplay;
    }

    // Return a text representation of the current phrase state, using hyphen-
    // newline notation
    public string GetTextRepresentation() {
        string tempString = "";
        for (int i = 0; i < lyricEvents.Count; i++) {
            tempString += lyricEvents[i].formattedText;
        }
        tempString += "\n";
        return tempString;
    }
}
