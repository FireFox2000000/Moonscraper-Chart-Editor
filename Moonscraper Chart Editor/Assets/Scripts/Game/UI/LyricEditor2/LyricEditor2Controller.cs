using MoonscraperChartEditor.Song;
using System.Collections.Generic;
using UnityEngine.UI;

public class LyricEditor2Controller : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    LyricEditor2PhraseController phraseTemplate;
    LyricEditor2PhraseController currentPhrase;
    List<LyricEditor2PhraseController> phrases = new List<LyricEditor2PhraseController>();

    uint currentTickPos {get {return ChartEditor.Instance.currentTickPos;}}

    static float phraseStartFactor = 0.5f;
    static int phraseStartMax = 16; // 16 refers to one sixteenth the length of a phrase in the current song.
    // So with a resolution of 192, the phrase_start event should have at least 12 ticks of spacing
    static Song currentSong {get {return ChartEditor.Instance.currentSong;}}
    static float songResolution {get {return currentSong.resolution;}}


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

    // Find the most recent previous phrase which has been placed and return its
    // end tick. If no such phrase exists, return 0.
    uint GetLastSafeTick() {
        int currentPhraseIndex = phrases.IndexOf(currentPhrase);
        // Iterate through up to the last 50 phrases
        for (int i = currentPhraseIndex - 1; i > currentPhraseIndex - 51; i--) {
            if (i < 0) {
                break;
            }
            uint? finalTick = phrases[i].endTick;
            if (finalTick != null) {
                return (uint)finalTick;
            }
        }
        // No previous phrase found, return 0
        return 0;
    }

    // Called every time the "place lyric" button is pressed; places the next
    // lyric in the current phrase, and sets the phrase's start tick, if it has
    // not been set
    public void PlaceNextLyric() {
        currentPhrase = GetNextUnfinishedPhrase();

        if (currentPhrase != null) {
            // Set the next lyric's tick
            currentPhrase.StartPlaceNextLyric(currentTickPos);

            // Set phrase_start if it is not already set
            if (currentPhrase.startTick == null) {
                uint lastSafeTick = GetLastSafeTick();
                // Tick calculation by set distance before first lyric
                uint startTick1 = (uint)(currentPhrase.startTick - (int)(songResolution / phraseStartMax));
                // Tick calculation proportional to distance to last phrase
                uint startTick2 = (uint)(lastSafeTick + (int)((currentPhrase.startTick - lastSafeTick) * phraseStartFactor));
                // Actual start tick is the maximum of these two values
                uint startTick = System.Math.Max(startTick1, startTick2);

                // Set the start tick
                currentPhrase.SetPhraseStart(startTick);
            }
        }
        // All phrases placed already, so currentPhrase was null
    }

    // Get the next phrase which does not yet have all its syllables placed
    LyricEditor2PhraseController GetNextUnfinishedPhrase() {
        for (int i = 0; i < phrases.Count; i++) {
            LyricEditor2PhraseController currentPhrase = phrases[i];
            if (!currentPhrase.allSyllablesPlaced) {
                return currentPhrase;
            }
        }
        // No incomplete phrase found
        return null;
    }

    // Called every time the "place lyric" button is released; stops placing the
    // next lyric in the current phrase and, if necessary, moves to the next
    // phrase. phrase_start and phrase_end events should be placed here if
    // necessary
    public void StopPlaceNextLyric() {
        currentPhrase.StopPlaceNextLyric();
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
        // Use CompareEditorEvents (below) to sort events, then group events into
        // sections by looking for phrase_start events
    }

    // Compare two events for use with List.Sort(). Events should be sorted by
    // tick; if two Events have the same tick, then lyric events should be
    // sortec before phrase_end and after phrase_start events. Unrelated events
    // can be sorted alphabetically using String.Compare()
    static int CompareEditorEvents (Event event1, Event event2) {
        // TODO
        return 0;
    }
}
5.  Update currentTickPos to respect the song audio delay
*/
