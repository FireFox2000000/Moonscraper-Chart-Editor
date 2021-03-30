using MoonscraperChartEditor.Song;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class LyricEditor2Controller : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    LyricEditor2AutoScroller autoScroller;
    [UnityEngine.SerializeField]
    LyricEditor2PhraseController phraseTemplate;
    [UnityEngine.SerializeField]
    LyricEditor2InputMenu lyricInputMenu;
    LyricEditor2PhraseController currentPhrase;
    List<LyricEditor2PhraseController> phrases = new List<LyricEditor2PhraseController>();

    uint currentTickPos {get {return ChartEditor.Instance.currentTickPos;}}

    bool playbackScrolling = false;
    uint playbackEndTick;
    int lastPlaybackTargetIndex = 0;
    LyricEditor2PhraseController lastPlaybackTarget = null;

    static float phrasePaddingFactor = 0.5f;
    static int phrasePaddingMax = 16; // 16 refers to one sixteenth the length of a phrase in the current song.
    // So with a resolution of 192, the phrase_start event should have at least 12 ticks of spacing
    static Song currentSong {get {return ChartEditor.Instance.currentSong;}}
    static float songResolution {get {return currentSong.resolution;}}
    static bool playbackActive {get {return (ChartEditor.Instance.currentState == ChartEditor.State.Playing);}}


    void OnEnable() {
        ImportExistingLyrics();
        // Activate auto-scrolling if playback is active on lyric editor enable
        autoScroller.SetActive(playbackActive);
    }

    void OnDisable() {
        // Place phrase_end for current phrase if it hasn't been placed
        if (currentPhrase != null && !currentPhrase.phraseEndPlaced) {
            // Ensure valid placement
            AutoPlacePhraseEnd(currentPhrase);
        }
        ClearPhraseObjects();
        autoScroller.gameObject.SetActive(false);
    }

    void Start() {
        phraseTemplate.gameObject.SetActive(false);

        ChartEditor.Instance.events.editorStateChangedEvent.Register(OnStateChanged);
    }

    public void OnStateChanged(in ChartEditor.State newState) {
        autoScroller.SetActive(playbackActive);
        if (playbackActive) {
            if (!IsLegalToPlaceNow()) {
                StartPlaybackScroll();
            } else {
                autoScroller.ScrollTo(currentPhrase?.rectTransform);
            }
        }
    }

    void Update() {
        if (playbackScrolling) {
            PlaybackScroll();
        }
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
    // end tick. If no such phrase exists, return 0. If the passed targetPhrase
    // is null, return the last safe tick of the entire song.
    uint GetFirstSafeTick(LyricEditor2PhraseController targetPhrase) {
        int currentPhraseIndex = phrases.IndexOf(targetPhrase);
        if (currentPhraseIndex == -1 || targetPhrase == null) {
            currentPhraseIndex = phrases.Count;
        }
        // Iterate through up to the last 50 phrases
        for (int i = currentPhraseIndex - 1; i > currentPhraseIndex - 51; i--) {
            if (i < 0) {
                break;
            }
            uint? finalTick = phrases[i].endTick;
            if (finalTick == null) {
                finalTick = phrases[i].GetLastEventTick();
            }
            if (finalTick != null) {
                return (uint)finalTick + 1;
            }
        }
        // No previous phrase found, return 0
        return 0;
    }

    void AutoPlacePhraseStart (LyricEditor2PhraseController phrase) {
        uint firstSafeTick = GetFirstSafeTick(phrase);
        // Tick calculation by set distance before first lyric
        uint startTick1 = (uint)(phrase.GetFirstEventTick() - (int)(songResolution / phrasePaddingMax * 4));
        // Tick calculation proportional to distance to last phrase
        uint startTick2 = (uint)(firstSafeTick + (int)((phrase.GetFirstEventTick() - firstSafeTick) * phrasePaddingFactor));
        // Actual start tick is the maximum of these two values
        uint startTick = System.Math.Max(startTick1, startTick2);

        // Set the start tick
        phrase.SetPhraseStart(startTick);
    }

    // Gets the last safe tick a given phrase can legally occupy
    uint GetLastSafeTick(LyricEditor2PhraseController targetPhrase) {
        // Look for a next-up phrase
        int targetIndex = phrases.IndexOf(targetPhrase);
        if (targetIndex + 1 < phrases.Count) {
            LyricEditor2PhraseController nextPhrase = phrases[targetIndex + 1];
            uint? nextPhraseStart = nextPhrase.startTick ?? nextPhrase.GetFirstEventTick();
            if (nextPhraseStart != null) {
                return (uint)nextPhraseStart;
            }
        }
        // No next phrase start found
        uint songEnd = currentSong.TimeToTick(ChartEditor.Instance.currentSongLength, songResolution);
        return songEnd;
    }

    // Returns the phrase-end auto-spacing when the phrase end event is placed
    // automatically
    uint PhraseEndAutoSpacer(LyricEditor2PhraseController targetPhrase) {
        uint lastSafeTick = GetLastSafeTick(targetPhrase);
        // Tick calculation by set distance
        uint endTick1 = (uint)(targetPhrase.GetLastEventTick() + (int)(songResolution / phrasePaddingMax * 4));
        // Tick calculation by proportional distance
        uint endTick2 = (uint)(lastSafeTick - (int)((targetPhrase.GetLastEventTick() - lastSafeTick) * phrasePaddingFactor));
        // Actual start tick is the minimum of these two values
        uint endTick = System.Math.Min(endTick1, endTick2);
        return endTick;
    }

    // Place the end event of the target phrase automatically
    void AutoPlacePhraseEnd (LyricEditor2PhraseController phrase) {
        phrase.SetPhraseEnd(PhraseEndAutoSpacer(phrase));
    }

    // Called every time the "place lyric" button is pressed; places the next
    // lyric in the current phrase, and sets the phrase's start tick, if it has
    // not been set
    public void PlaceNextLyric() {
        currentPhrase = GetNextUnfinishedPhrase();

        if (currentPhrase != null && IsLegalToPlaceNow()) {
            // Set the next lyric's tick
            currentPhrase.StartPlaceNextLyric(currentTickPos);

            // Set phrase_start if it is not already set
            if (!currentPhrase.phraseStartPlaced) {
                AutoPlacePhraseStart(currentPhrase);
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
    // phrase. phrase_end events are placed here if necessary
    public void StopPlaceNextLyric() {
        if (currentPhrase != null) {
            currentPhrase.StopPlaceNextLyric();

            // Place phrase_end event and move to next phrase if all syllables
            // were just placed
            if (currentPhrase.allSyllablesPlaced) {
                if (IsLegalToPlaceNow()) {
                    currentPhrase.SetPhraseEnd(currentTickPos);
                } else {
                    AutoPlacePhraseEnd(currentPhrase);
                }
                currentPhrase = GetNextUnfinishedPhrase();
                autoScroller.ScrollTo(currentPhrase?.rectTransform);
            }
        }
    }

    // Pickup all phrases
    void PickupAllPhrases() {
        foreach (LyricEditor2PhraseController phrase in phrases) {
            phrase.Pickup();
        }
    }

    // Take dash-newline formatted lyrics from the lyric input menu and parse
    // them into phrases. Called when the user hits "submit" in the input menu
    public void InputLyrics() {
        PickupAllPhrases();
        ClearPhraseObjects();
        string inputLyrics = lyricInputMenu.text ?? "";

        List<List<string>> parsedLyrics = ParseLyrics(inputLyrics);
        for (int i = 0; i < parsedLyrics.Count; i++) {
            LyricEditor2PhraseController newPhrase = UnityEngine.GameObject.Instantiate(phraseTemplate, phraseTemplate.transform.parent).GetComponent<LyricEditor2PhraseController>();
            newPhrase.InitializeSyllables(parsedLyrics[i]);
            phrases.Add(newPhrase);
            newPhrase.gameObject.SetActive(true);
        }

        if (phrases.Count > 0) {
            // Taken care of in OnStateChanged()
            // autoScroller.ScrollTo(phrases[0].rectTransform);
            currentPhrase = phrases[0];
        }
    }

    // Parse a string into a double string array (phrases of syllables) to be
    // given as PhrUnityEngine.Debugput. Does not have an implemented time-out
    // period in case of excessively long strings to be parsed.
    List<List<string>> ParseLyrics(string inputString) {
        // Start by splitting the string into phrases
        char[] newlineCharacters = {'\n', '\r'};
        string[] tempPhrases = inputString.Split(newlineCharacters, System.StringSplitOptions.RemoveEmptyEntries);

        // Prepare the regex engine to parse each phrase
        List<List<string>> parsedLyrics = new List<List<string>>();
        // [^-\s]+      matches one or more characters in a syllable, excluding
        //                  spaces and dashes
        // (-\s?|\s?)   matches a dash, or whitespace if no dash is found
        string regexPattern = @"[^-\s]+(-|\s?)";
        Regex rx = new Regex(regexPattern);

        foreach (string basePhrase in tempPhrases)
        {
            // Match each phrase
            MatchCollection matches = rx.Matches(basePhrase);
            // Convert the MatchCollection into a List and append that to
            // parsedLyrics
            List<string> matchesList = new List<string>();
            for (int i = 0; i < matches.Count; i++)
            {
                matchesList.Add(matches[i].ToString());
            }
            parsedLyrics.Add(matchesList);
        }

        return parsedLyrics;
    }

    // Display a large input field for the user to enter lyrics in dash-
    // newline notation; field should be populated with a string given by
    // GetTextRepresentation()
    public void EnableInputMenu() {
        string existingLyrics = GetTextRepresentation();
        lyricInputMenu.Display(existingLyrics);
    }

    // Create a text representation of stored lyrics which can be pushed to the
    // input menu when the user wants to edit lyrics
    string GetTextRepresentation() {
        string rep = "";
        for (int i = 0; i < phrases.Count; i++) {
            rep += phrases[i].GetTextRepresentation();
        }
        return rep;
    }

    static bool IsLyricEvent(Event selectedEvent) {
        if (selectedEvent.title.TrimEnd().StartsWith(LyricEditor2PhraseController.c_lyricPrefix) ||
                selectedEvent.title.Equals(LyricEditor2PhraseController.c_phraseStartKeyword) ||
                selectedEvent.title.Equals(LyricEditor2PhraseController.c_phraseEndKeyword)) {
            return true;
        } else {
            return false;
        }
    }

    static bool MakesValidPhrase (List<Event> potentialPhrase) {
        for (int i = 0; i < potentialPhrase.Count; i++) {
            if (potentialPhrase[i].title.StartsWith(LyricEditor2PhraseController.c_lyricPrefix)) {
                return true;
            }
        }
        // No lyric events found
        return false;
    }

    // Import existing lyric events from the current song. Called in Start()
    void ImportExistingLyrics() {

        // Use CompareEditorEvents (below) to sort events, then group events into
        // sections by looking for phrase_start events
        List<Event> importedEvents = new List<Event>();

        foreach (Event eventObject in ChartEditor.Instance.currentSong.events) {
            if (IsLyricEvent(eventObject)) {
                importedEvents.Add(eventObject);
            }
        }

        importedEvents.Sort(CompareEditorEvents);

        List<Event> tempEvents = new List<Event>();
        for (int i = 0; i < importedEvents.Count; i++) {
            Event currentEvent = importedEvents[i];
            tempEvents.Add(currentEvent);
            if (currentEvent.title.Equals(LyricEditor2PhraseController.c_phraseEndKeyword)) {
                if (MakesValidPhrase(tempEvents)) {
                    LyricEditor2PhraseController newPhrase = UnityEngine.GameObject.Instantiate(phraseTemplate, phraseTemplate.transform.parent).GetComponent<LyricEditor2PhraseController>();
                    newPhrase.InitializeSyllables(tempEvents);
                    phrases.Add(newPhrase);
                    newPhrase.gameObject.SetActive(true);
                }
                // No lyrics in the current phrase, clear temp events to avoid pollution with extra phrase events
                tempEvents.Clear();
            }
        }

        // Check to ensure all fully-placed phrases have their phrase_start events set
        foreach (LyricEditor2PhraseController currentPhrase in phrases) {
            if (currentPhrase.allSyllablesPlaced && !currentPhrase.phraseStartPlaced) {
                AutoPlacePhraseStart(currentPhrase);
            }
        }
    }

    // Compare two events for use with List.Sort(). Events should be sorted by
    // tick; if two Events have the same tick, then lyric events should be
    // sorted before phrase_end and after phrase_start events. Unrelated events
    // can be sorted alphabetically using String.Compare()
    static int CompareEditorEvents (Event event1, Event event2) {
        if (event1 == null && event2 == null) {
            // Both events null and are equivalent
            return 0;
        } else if (event1 == null) {
            // event1 null
            return 1;
        } else if (event2 == null) {
            // event2 null
            return -1;
        } else if (event1.tick != event2.tick) {
            // Two events at different ticks
            return event1.tick > event2.tick ? 1 : -1;
        } else if (event1.title.Equals(LyricEditor2PhraseController.c_phraseStartKeyword) || event2.title.Equals(LyricEditor2PhraseController.c_phraseEndKeyword)) {
            // Two events at the same tick, event1 is phrase_start or event2 is phrase_end
            return -1;
        } else if (event2.title.Equals(LyricEditor2PhraseController.c_phraseStartKeyword) || event1.title.Equals(LyricEditor2PhraseController.c_phraseEndKeyword)) {
            // Two events at the same tick, event1 is phrase_end or event2 is phrase_start
            return 1;
        } else {
            // Two events at the same tick, neither is phrase_start or phrase_end
            return System.String.Compare(event1.title, event2.title);
        }
    }

    // Check to see if the current tick is valid to place a lyric; if the
    // current time falls before the last element of currentPhrase, the
    // placement is considered invalid
    bool IsLegalToPlaceNow() {
        uint lastPhraseTick = GetFirstSafeTick(currentPhrase);
        if (currentTickPos < lastPhraseTick) {
            // Current position is before last safe tick
            return false;
        } else if (currentTickPos < currentPhrase?.startTick || currentTickPos < currentPhrase?.GetLastEventTick()) {
            // Current position is in the middle of currentPhrase
            return false;
        } else {
            // No illegal state found
            return true;
        }
    }

    // Begin auto-scrolling for lyric playback (before the user can place more
    // phrases)
    void StartPlaybackScroll() {
        playbackScrolling = true;
        lastPlaybackTargetIndex = 0;
        LyricEditor2PhraseController firstUnplacedPhrase = GetNextUnfinishedPhrase();
        playbackEndTick = GetFirstSafeTick(firstUnplacedPhrase);
        PlaybackScroll();
    }

    // Auto-scroll during lyric playback
    void PlaybackScroll() {
        // Check for playback end
        if (currentTickPos >= playbackEndTick) {
            playbackScrolling = false;
            currentPhrase = GetNextUnfinishedPhrase();
            autoScroller.ScrollTo(currentPhrase?.rectTransform);
            return;
        }

        // TODO unexpected behaviour when all phrases have been placed (auto-
        // scrolls to the bottom of the editor)

        // Find target phrase
        LyricEditor2PhraseController playbackTarget = lastPlaybackTarget;
        for (int i = lastPlaybackTargetIndex; i < phrases.Count; i++) {
            uint startBound = GetFirstSafeTick(phrases[i]);
            uint endbound; // TODO waiting on GetLastSafeTick() signature change
            if (currentTickPos >= startBound) {
                playbackTarget = phrases[i];
                // update lastPlaybackTargetIndex
                lastPlaybackTargetIndex = i;
                break;
            }
        }
        // Scroll to phrase
        if (playbackTarget != lastPlaybackTarget) {
            autoScroller.ScrollTo(playbackTarget?.rectTransform);
        }
        // Update lastPlaybackTarget
        lastPlaybackTarget = playbackTarget;
    }
}
