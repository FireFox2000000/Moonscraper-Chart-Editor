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

    List<LyricEditor2Event> lyricEvents = new List<LyricEditor2Event>();
    LyricEditor2Event phraseStartEvent = new LyricEditor2Event(c_phraseStartKeyword);
    LyricEditor2Event phraseEndEvent = new LyricEditor2Event(c_phraseEndKeyword);


    // Place the next lyric in lyricEvents
    public void StartPlaceNextLyric(uint tick) {
        // TODO
    }

    // Stop placing the next lyric (useful for formatting in DisplayText())
    public void StopPlaceNextLyric() {
        // TODO
    }

    // Set the phrase_start event's tick
    public void SetPhraseStart(uint tick) {
        // TODO
    }

    // Set the phrase_end event's tick
    public void SetPhraseEnd(uint tick) {
        // TODO
    }

    // Initialize lyricEvents using a list of string syllables

    LyricEditor2Event GetNextUnplacedSyllable() {
        for (int i = 0; i < lyricEvents.Count; i++) {
            LyricEditor2Event currentEvent = lyricEvents[i];
            if (!currentEvent.hasBeenPlaced) {
                return currentEvent;
            }
        }
        return null;
    }

    public void InitializeSyllables(List<string> syllables) {
        // TODO
    }

    // Initialize lyricEvents using a list of events which already exist in the
    // chart editor. Method should also look for phrase_start and phrase_end
    // events and, if they exist, assign them to the appropriate variables
    public void InitializeSyllables(List<Event> existingEvents) {
        // TODO
    }

    // Update the text content of phraseText to reflect the current phrase state
    void DisplayText() {
        // TODO
    }

    // Return a text representation of the current phrase state, using hyphen-
    // newline notation
    public string GetTextRepresentation() {
        // TODO
        return "";
    }
}
