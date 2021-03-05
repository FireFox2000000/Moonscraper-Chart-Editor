using MoonscraperChartEditor.Song;
using System.Collections.Generic;
using UnityEngine.UI;

public class LyricEditor2Controller : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    LyricEditor2PhraseController phraseTemplate;

    List<LyricEditor2PhraseController> phrases = new List<LyricEditor2PhraseController>();

    void OnEnable() {
        ImportExistingLyrics();
    }

    void OnDisable() {
        ClearPhraseObjects();
    }

    // Destroy all phrase GameObjects and dereference their corresponding
    // phrase controller components
    void ClearPhraseObjects() {
        foreach (LyricEditor2PhraseController controller in phrases) {
            UnityEngine.Object.Destroy(controller.gameObject);
        }
        phrases.Clear();
    }

    // Called every time the "place lyric" button is pressed; places the next
    // lyric in the current phrase
    public void PlaceNextLyric() {
        // TODO
    }

    // Called every time the "place lyric" button is released; stops placing the
    // next lyric in the current phrase and, if necessary, moves to the next
    // phrase. phrase_start and phrase_end events should be placed here if
    // necessary
    public void StopPlaceNextLyric() {
        // TODO
    }

    // Take dash-newline formatted lyrics from the lyric input menu and parse
    // them into phrases. Called when the user hits "submit" in the input menu
    public void InputLyrics() {
        // TODO
        // Hint: use regex
    }

    // Display a large input field for the user to enter lyrics in dash-
    // newline notation; field should be populated with a string given by
    // GetTextRepresentation()
    public void EnableInputMenu() {
        // TODO
    }

    // Create a text representation of stored lyrics which can be pushed to the
    // input menu when the user wants to edit lyrics
    string GetTextRepresentation() {
        // TODO
        return "";
    }

    // Import existing lyric events from the current song. Called in Start()
    void ImportExistingLyrics() {
        // TODO
        // Use CompareLyricEvents (below) to sort events, then group events into
        // sections by looking for phrase_start events
    }

    // Compare two events for use with List.Sort(). Events should be sorted by
    // tick; if two Events have the same tick, then lyric events should be
    // sortec before phrase_end and after phrase_start events. Unrelated events
    // can be sorted alphabetically using String.Compare()
    static int CompareLyricEvents (Event event1, Event event2) {
        // TODO
        return 0;
    }
}
