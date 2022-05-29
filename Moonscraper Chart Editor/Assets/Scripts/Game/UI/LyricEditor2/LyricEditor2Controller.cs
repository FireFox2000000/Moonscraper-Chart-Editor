// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

// Thank you DarkAngel2096 for your previous contributions to get lyrics into
// more charts!

using MoonscraperChartEditor.Song;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class LyricEditor2Controller : UnityEngine.MonoBehaviour
{
    // Stores a set of all commands which have been invoked to modify chart
    // events. When the lyric editor is exited, a single ICommand will be pushed
    // to the command stack which contains all the changes which were made to
    // events
    public class SongEditCommandSet : MoonscraperEngine.ICommand {
        List<SongEditCommand> commands = new List<SongEditCommand>();
        BatchedSongEditCommand batchedCommands = null;
        public bool isEmpty {get {return commands.Count == 0;}}

        public void Add(SongEditCommand c) {
            commands.Add(c);
        }

        public bool Remove(SongEditCommand c) {
            return commands.Remove(c);
        }

        public void Invoke() {
            if (batchedCommands == null) {
                batchedCommands = new BatchedSongEditCommand(commands);
            } else {
                batchedCommands.Invoke();
            }
        }

        public void Revoke() {
            batchedCommands?.Revoke();
        }
    }

    class PickupFromCommand : MoonscraperEngine.ICommand {
        public delegate void Refresh();

        Refresh refreshAfterUpdate;
        BatchedICommand pickupCommands;

        public PickupFromCommand(BatchedICommand pickupCommands, Refresh refreshAfterUpdate) {
            this.pickupCommands = pickupCommands;
            this.refreshAfterUpdate = refreshAfterUpdate;
        }

        public void Invoke() {
            pickupCommands.Invoke();
            refreshAfterUpdate();
        }

        public void Revoke() {
            pickupCommands.Revoke();
            refreshAfterUpdate();
        }
    }

    enum InputState {
        Full,
        Phrase
    }

    static Song currentSong {get {return ChartEditor.Instance.currentSong;}}
    static float songResolution {get {return currentSong.resolution;}}
    static bool playbackActive {get {return (ChartEditor.Instance.currentState == ChartEditor.State.Playing);}}

    public SongEditCommandSet editCommands;
    LyricEditorCommandStack m_commandStack = new LyricEditorCommandStack();

    [UnityEngine.SerializeField]
    LyricEditor2AutoScroller autoScroller;
    [UnityEngine.SerializeField]
    LyricEditor2PhraseController phraseTemplate;
    [UnityEngine.SerializeField]
    LyricEditor2InputMenu lyricInputMenu;
    [UnityEngine.SerializeField]
    [UnityEngine.Range(0, 1)]
    float phrasePaddingFactor;
    [UnityEngine.SerializeField]
    [UnityEngine.Tooltip("Phrase padding as 1/n measures; i.e. a value of 16 means one sixteenth note")]
    int phrasePaddingMax; // 16 refers to one sixteenth the length of a phrase in the current song.
    // So with a resolution of 192, the phrase_start event should have at least 12 ticks of spacing

    LyricEditor2PhraseController currentPhrase;
    List<LyricEditor2PhraseController> phrases = new List<LyricEditor2PhraseController>();
    // commandStackPushes keeps a record of all command stack pushes so they can
    // be removed from the main command stack (Pop() method returns void, not
    // the revoked command; see CommandStack.cs)
    List<PickupFromCommand> commandStackPushes = new List<PickupFromCommand>();
    int numCommandStackPushes = 0;
    LyricEditor2PhraseController lastPlaybackTarget = null;
    InputState inputState = InputState.Full;
    LyricEditor2PhraseController inputPhrase;
    uint currentTickPos {get {return ChartEditor.Instance.currentTickPos;}}
    uint currentSnappedTickPos
    {
        get
        {
            return Globals.gameSettings.lyricEditorSettings.stepSnappingEnabled 
                ? Snapable.TickToSnappedTick(currentTickPos, Globals.gameSettings.step, ChartEditor.Instance.currentSong) 
                : currentTickPos;
        }
    }
    bool playbackScrolling = false;
    bool onePhrasePickedUp = false;
    uint playbackEndTick;
    int lastPlaybackTargetIndex = 0;
    string savedUnplacedSyllables = "";
    string savedPlacedSyllables = "";

    // Called every time the "place lyric" button is pressed; places the next
    // lyric in the current phrase, and sets the phrase's start tick, if it has
    // not been set
    public void PlaceNextLyric() {
        currentPhrase = GetNextUnfinishedPhrase();

        if (currentPhrase != null && IsLegalToPlaceNow()) {
            // Clear command stack commands to prevent duplication after redo
            ClearPickupCommands();

            onePhrasePickedUp = false;
            // Distance check phase end event to this new start event.
            // If these two events are too close to each other then delete the phase end event to let CH automatically handle it.
            {
                var lastFinishedPhase = GetPreviousPhrase(currentPhrase);
                if (lastFinishedPhase != null)
                {
                    uint? endTick = lastFinishedPhase.endTick;
                    if (endTick.HasValue)
                    {
                        uint endPhaseTick = endTick.Value;
                        float oldPhaseEndTime = ChartEditor.Instance.currentSong.TickToTime(endPhaseTick);
                        float newPhaseStartTime = ChartEditor.Instance.currentSong.TickToTime(currentSnappedTickPos);
                        if ((newPhaseStartTime - oldPhaseEndTime) < Globals.gameSettings.lyricEditorSettings.phaseEndThreashold)
                        {
                            // Remove phase end event
                            lastFinishedPhase.PickupPhraseEnd();
                        }
                    }
                }
            }

            // Set the next lyric's tick
            currentPhrase.StartPlaceNextLyric(currentSnappedTickPos);

            // Set phrase_start if it is not already set
            if (!currentPhrase.phraseStartPlaced) {
                AutoPlacePhraseStart(currentPhrase);
            }
        }
        // All phrases placed already, so currentPhrase was null
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
                    currentPhrase.SetPhraseEnd(currentSnappedTickPos);
                } else {
                    AutoPlacePhraseEnd(currentPhrase);
                }
                currentPhrase = GetNextUnfinishedPhrase();
                autoScroller.ScrollTo(currentPhrase?.rectTransform);
                ClearPickupCommands();
            }
        }
    }

    // Display a large input field for the user to enter lyrics in dash-
    // newline notation; field should be populated with a string given by
    // GetTextRepresentation()
    public void EnableInputMenu() {
        lyricInputMenu.SetTitle("Input Lyrics");
        inputState = InputState.Full;
        string existingLyrics = GetTextRepresentation();
        lyricInputMenu.Display(existingLyrics);
    }

    // Take dash-newline formatted lyrics from the lyric input menu and parse
    // them into phrases. Called when the user hits "submit" in the input menu.
    // Consider the input state!
    public void InputLyrics() {
        if (inputState == InputState.Full) {
            ClearPickupCommands();
            PickupAllPhrases();
            ClearPhraseObjects();
            string inputLyrics = lyricInputMenu.text ?? "";

            phrases.AddRange(CreatePhrases(inputLyrics));

            if (phrases.Count > 0) {
                currentPhrase = phrases[0];
            }

            // Update search order
            UpdateSortIds();

        } else if (inputState == InputState.Phrase) {
            ClearPickupCommands();
            string inputLyrics = lyricInputMenu.text ?? "";
            int inputIndex = phrases.BinarySearch(inputPhrase);
            if (inputIndex >= 0) {
                // Remove existing phrases
                PickupFrom(inputPhrase, false, true);
                for (int i = inputIndex; i < phrases.Count; i++) {
                    UnityEngine.Object.Destroy(phrases[i].gameObject);
                }
                phrases.RemoveRange(inputIndex, phrases.Count - inputIndex);
                // Create new phrases
                var newPhrases = CreatePhrases(inputLyrics);
                phrases.InsertRange(inputIndex, newPhrases);
                UpdateSortIds();
                UpdateDisplayOrder();
            }
        }
    }

    public void PickupFrom(LyricEditor2PhraseController start, bool pushToStack = true, bool forcePickupAll = false) {
        if (forcePickupAll || onePhrasePickedUp || start.numSyllables == 1 || HasFollowingLyrics(start)) {
            List<MoonscraperEngine.ICommand> commands = new List<MoonscraperEngine.ICommand>();
            int startIndex = phrases.BinarySearch(start);
            if (startIndex >= 0) {
                for (int i = startIndex; i < phrases.Count; i++) {
                    if (phrases[i].anySyllablesPlaced) {
                        commands.Add(phrases[i].Pickup());
                    }
                }
            }
            currentPhrase = GetNextUnfinishedPhrase();
            // Invoke commands
            if (commands.Count > 0) {
                var batchedCommands = new BatchedICommand(commands);
                var pickupFromCommand = new PickupFromCommand(batchedCommands, RefreshAfterPickupFrom);
                if (pushToStack) {
                    commandStackPushes.Add(pickupFromCommand);
                    ChartEditor.Instance.commandStack.Push(pickupFromCommand);
                } else {
                    pickupFromCommand.Invoke();
                }
            }
            onePhrasePickedUp = true;
        } else if (start.anySyllablesPlaced) {
            ClearPickupCommands();
            start.PickupLastSyllable();
            onePhrasePickedUp = true;
            currentPhrase = GetNextUnfinishedPhrase();
            if (!playbackActive) {
                AutoPlacePhraseStartEnd(start);
            }
        }
    }

    // Opens the lyric input menu to edit the given lyrics
    public void EditPhrase(LyricEditor2PhraseController phrase) {
        inputPhrase = phrase;
        inputState = InputState.Phrase;
        string rep = "";
        int startIndex = phrases.BinarySearch(phrase);
        for (int i = startIndex; i >= 0 && i < phrases.Count; i++) {
            rep += phrases[i].GetTextRepresentation();
        }
        EnableInputMenu(rep, title: (startIndex < phrases.Count - 1) ? "Edit Phrases" : "Edit Phrase");
    }

    public void OnStateChanged(in ChartEditor.State newState) {
        if (!isActiveAndEnabled) return;

        autoScroller.enabled = playbackActive;
        playbackScrolling = playbackActive;
        if (playbackActive) {
            if (!IsLegalToPlaceNow()) {
                StartPlaybackScroll();
            } else {
                autoScroller.ScrollTo(currentPhrase?.rectTransform);
            }
        } else {
            lastPlaybackTarget?.PlaybackHighlight(null);
        }
    }

    public void onCommandStackPush(in MoonscraperEngine.ICommand command) {
        UnityEngine.Debug.Assert(command is PickupFromCommand, "Trying to push a non-lyric editor command onto the lyrics editor stack! Did you miss a call to ChartEditor.Instance.SetDefaultCommandStack?");

        if (!(command is PickupFromCommand c && commandStackPushes.Contains(c))) {
            gameObject.SetActive(false);
        } else {
            numCommandStackPushes++;
        }
    }

    public void onCommandStackPop(in MoonscraperEngine.ICommand command) {
        UnityEngine.Debug.Assert(command is PickupFromCommand, "Trying to pop a non-lyric editor command from the lyrics editor stack! Did you miss a call to ChartEditor.Instance.SetDefaultCommandStack?");

        if (!(command is PickupFromCommand c && commandStackPushes.Contains(c))) {
            gameObject.SetActive(false);
        } else {
            numCommandStackPushes--;
        }
    }

    public void Reset() {
        savedPlacedSyllables = "";
        savedUnplacedSyllables = "";
        numCommandStackPushes = 0;
        ClearPhraseObjects();
        OnEnable();
    }

    void OnEnable() {
        ChartEditor.Instance.SetActiveCommandStack(m_commandStack);

        // Create a new edit command set
        editCommands = new SongEditCommandSet();
        ImportExistingLyrics();
        AddSavedSyllables();
        currentPhrase = GetNextUnfinishedPhrase();
        // Activate auto-scrolling if playback is active on lyric editor enable
        autoScroller.enabled = playbackActive;

        UnityEngine.Debug.Log("Opened lyric editor");
    }

    void OnDisable() {
        // Save lyrics
        SaveUnplacedSyllables();
        SavePlacedSyllables();
        // Place phrase_end for current phrase if it hasn't been placed
        if (currentPhrase != null && !currentPhrase.phraseEndPlaced && currentPhrase.anySyllablesPlaced) {
            // Ensure valid placement
            AutoPlacePhraseEnd(currentPhrase);
        }
        ClearPhraseObjects();
        autoScroller.enabled = false;
        // Remove command stack commands
        ClearPickupCommands();
        // Push batched edits to command stack

        ChartEditor.Instance.SetDefaultCommandStack();

        if (!editCommands.isEmpty) {
            // Push the finalised commands back onto the primary stack
            ChartEditor.Instance.commandStack.Push(editCommands);
        }

        editCommands = null; // Release memory, don't wait for the next OnEnable call
        m_commandStack.Clear();

        UnityEngine.Debug.Log("Closed lyric editor");
    }

    void Start() {
        phraseTemplate.gameObject.SetActive(false);

        ChartEditor.Instance.events.editorStateChangedEvent.Register(OnStateChanged);
        ChartEditor.Instance.events.songLoadedEvent.Register(Reset);

        m_commandStack.onPush.Register(onCommandStackPush);
        m_commandStack.onPop.Register(onCommandStackPop);
    }

    void Update() {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.LyricEditorSetTime))
        {
            PlaceNextLyric();
        }
        else if (MSChartEditorInput.GetInputUp(MSChartEditorInputActions.LyricEditorSetTime))
        {
            StopPlaceNextLyric();
        }

        if (playbackScrolling) {
            PlaybackScroll(false);
        }
    }

    static bool HasLyricEvents(List<SongEditCommand> commands) {
        foreach (SongEditCommand c in commands) {
            if (HasLyricEvents(c)) {
                return true;
            }
        }
        // No lyric events found
        return false;
    }

    static bool HasLyricEvents(SongEditCommand command) {
        // Check for batched commands
        if (command is BatchedSongEditCommand batched && HasLyricEvents(batched.GetSongEditCommands())) {
            return true;
        }
        // Not a batched command
        var songObjects = command.GetSongObjects();
        foreach (SongObject o in songObjects) {
            if (o is Event e && IsLyricEvent(e)) {
                return true;
            }
        }
        // No lyric events found
        return false;
    }

    static bool IsLyricEvent(Event selectedEvent) {
        if (selectedEvent.IsLyric() ||
                selectedEvent.title.Equals(LyricEditor2PhraseController.c_phraseStartKeyword) ||
                selectedEvent.title.Equals(LyricEditor2PhraseController.c_phraseEndKeyword)) {
            return true;
        } else {
            return false;
        }
    }

    static bool MakesValidPhrase (List<Event> potentialPhrase) {
        for (int i = 0; i < potentialPhrase.Count; i++) {
            if (potentialPhrase[i].IsLyric()) {
                return true;
            }
        }
        // No lyric events found
        return false;
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
            if (currentEvent.title.TrimEnd().Equals(LyricHelper.LYRIC_EVENT_PREFIX.TrimEnd())) {
                var deleteCommand = new SongEditDelete(currentEvent);
                deleteCommand.Invoke();
                editCommands.Add(deleteCommand);
                continue;
            }
            tempEvents.Add(currentEvent);
            if (currentEvent.title.Equals(LyricEditor2PhraseController.c_phraseEndKeyword) || i == importedEvents.Count - 1 ||
                    (importedEvents[i+1].title.Equals(LyricEditor2PhraseController.c_phraseStartKeyword))) {
                if (MakesValidPhrase(tempEvents)) {
                    LyricEditor2PhraseController newPhrase = Instantiate(phraseTemplate, phraseTemplate.transform.parent).GetComponent<LyricEditor2PhraseController>();
                    newPhrase.InitializeSyllables(tempEvents);
                    phrases.Add(newPhrase);
                    newPhrase.gameObject.SetActive(true);
                } else {
                    // phrase has no associated lyrics, delete it
                    foreach (var e in tempEvents) {
                        var deleteCommand = new SongEditDelete(e);
                        deleteCommand.Invoke();
                        editCommands.Add(deleteCommand);
                    }
                }
                // No lyrics in the current phrase, clear temp events to avoid pollution with extra phrase events
                tempEvents.Clear();
            }
        }

        // Update search order
        UpdateSortIds();

        // Check to ensure all fully-placed phrases have their phrase_start and
        // phrase_end events set, if appropriate
        for(int i = 0; i < phrases.Count; i++) {
            LyricEditor2PhraseController currentPhrase = phrases[i];
            AutoPlacePhraseStartEnd(currentPhrase);
        }
    }

    void AutoPlacePhraseStartEnd(LyricEditor2PhraseController currentPhrase) {
        if ((currentPhrase.allSyllablesPlaced && !currentPhrase.phraseStartPlaced) ||
              (currentPhrase.GetFirstEventTick() < currentPhrase.startTick)) {
            AutoPlacePhraseStart(currentPhrase);
        }
        // Check for phrase_end is a little more complex, only auto-place
        // phrase_end if the spacing between phrases is small enough
        var nextPhrase = GetNextPhrase(currentPhrase);
        if (currentPhrase.allSyllablesPlaced && !currentPhrase.phraseEndPlaced) {
            if (nextPhrase == null) {
                AutoPlacePhraseEnd(currentPhrase);
            } else {
                uint nextPhraseStart = (uint)nextPhrase.GetFirstEventTick();
                float nextPhraseStartTime = ChartEditor.Instance.currentSong.TickToTime(nextPhraseStart);
                uint thisPhraseEnd = PhraseEndAutoSpacer(currentPhrase);
                float thisPhraseEndTime = ChartEditor.Instance.currentSong.TickToTime(thisPhraseEnd);
                // UnityEngine.Debug.LogFormat("Time difference was calculated to be {0} (from nextPhraseStartTime {1} and thisPhraseEnd {2})", (nextPhraseStartTime - thisPhraseEndTime), nextPhraseStartTime, thisPhraseEndTime);
                if ((nextPhraseStartTime - thisPhraseEndTime) >= Globals.gameSettings.lyricEditorSettings.phaseEndThreashold) {
                    AutoPlacePhraseEnd(currentPhrase);
                }
            }
        }
    }

    // Add phrases from last session, if the other imported lyrics match the
    // previously-stored lyrics
    void AddSavedSyllables() {
        if (savedUnplacedSyllables.Length != 0 && GetTextRepresentation().Equals(savedPlacedSyllables)) {
            int firstNewline = savedUnplacedSyllables.IndexOf('\n');
            string firstLine = savedUnplacedSyllables.Substring(0, firstNewline);
            List<List<string>> firstLineSyllablesRaw = ParseLyrics(firstLine);
            if (firstLineSyllablesRaw.Count > 0) {
                List<string> firstLineSyllables = firstLineSyllablesRaw[0];
                phrases[phrases.Count-1].AddSyllables(firstLineSyllables);
                phrases[phrases.Count-1].PickupPhraseEnd();
            }

            string otherLines = savedUnplacedSyllables.Substring(firstNewline+1);
            List<LyricEditor2PhraseController> extraPhrases = CreatePhrases(otherLines);
            phrases.AddRange(extraPhrases);
            UpdateSortIds();
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

    // Display input field with custom prepopulated field; function is called
    // internally only, so it should not update inputState
    void EnableInputMenu(string prefilledLyrics, string title = "Input Lyrics") {
        lyricInputMenu.SetTitle(title);
        lyricInputMenu.Display(prefilledLyrics);
    }

    // Check to see if the current tick is valid to place a lyric; if the
    // current time falls before the last element of currentPhrase, the
    // placement is considered invalid
    bool IsLegalToPlaceNow() {
        uint firstPhraseTick = GetFirstSafeTick(currentPhrase);
        uint snappedTick = currentSnappedTickPos;
        if (snappedTick <= firstPhraseTick) {
            // Current position is before first safe tick
            return false;
        } else if (snappedTick <= currentPhrase?.startTick || snappedTick <= currentPhrase?.GetLastEventTick()) {
            // Current position is in the middle of currentPhrase
            return false;
        } else {
            // No illegal state found
            return true;
        }
    }

    // Find the most recent previous phrase which has been placed and return its
    // end tick. If no such phrase exists, return 0. If the passed targetPhrase
    // is null, return the last safe tick of the entire song.
    uint GetFirstSafeTick(LyricEditor2PhraseController targetPhrase) {
        int currentPhraseIndex = phrases.BinarySearch(targetPhrase);
        if (currentPhraseIndex == -1 || targetPhrase == null) {
            currentPhraseIndex = phrases.Count;
        }
        // Iterate through up to the last 50 phrases
        for (int i = currentPhraseIndex - 1; i > currentPhraseIndex - 51; i--) {
            if (i < 0) {
                break;
            }
            // Check for any placed lyrics first, as a phrase with no unplaced
            // lyrics will not have an endTick or lastEventTick
            if (phrases[i].anySyllablesPlaced) {
                uint? finalTick = phrases[i].endTick;
                if (finalTick == null) {
                    finalTick = phrases[i].GetLastEventTick();
                }
                if (finalTick != null) {
                    return (uint)finalTick + 1;
                }
            }
        }
        // No previous phrase found, return 0
        return 0;
    }

    // Gets the last safe tick a given phrase can legally occupy
    uint GetLastSafeTick(LyricEditor2PhraseController targetPhrase) {
        // Look for a next-up phrase
        int targetIndex = phrases.BinarySearch(targetPhrase);
        if (targetIndex + 1 < phrases.Count) {
            LyricEditor2PhraseController nextPhrase = phrases[targetIndex + 1];
            uint? nextPhraseStart = nextPhrase.startTick ?? nextPhrase.GetFirstEventTick();
            if (nextPhraseStart != null) {
                return (uint)nextPhraseStart - 1;
            }
        }
        // No next phrase start found
        uint songEnd = currentSong.TimeToTick(ChartEditor.Instance.currentSongLength, songResolution);
        return songEnd;
    }

    void AutoPlacePhraseStart (LyricEditor2PhraseController phrase) {
        uint firstSafeTick = GetFirstSafeTick(phrase);
        // Tick calculation by set distance before first lyric
        uint startTick1 = (uint)(phrase.GetFirstEventTick() - (int)(songResolution / phrasePaddingMax * 4));
        // Tick calculation proportional to distance to last phrase
        uint startTick2 = (uint)(firstSafeTick + (int)((phrase.GetFirstEventTick() - firstSafeTick) * (1 - phrasePaddingFactor)));
        // Actual start tick is the maximum of these two values
        uint startTick = System.Math.Max(startTick1, startTick2);

        // Set the start tick
        phrase.SetPhraseStart(startTick);
    }

    // Place the end event of the target phrase automatically
    void AutoPlacePhraseEnd (LyricEditor2PhraseController phrase) {
        phrase.SetPhraseEnd(PhraseEndAutoSpacer(phrase));
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

    // Get the latest phrase which does have all its syllables placed
    LyricEditor2PhraseController GetPreviousPhrase(LyricEditor2PhraseController phrase)
    {
        int previousPhraseIndex = phrases.IndexOf(phrase) - 1;
        return previousPhraseIndex >= 0 ? phrases[previousPhraseIndex] : null;
    }

    LyricEditor2PhraseController GetNextPhrase(LyricEditor2PhraseController phrase) {
        int nextIndex = phrases.IndexOf(phrase) + 1;
        return nextIndex < phrases.Count ? phrases[nextIndex] : null;
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

    // Checks whether a phrase has any subsequent placed lyrics
    bool HasFollowingLyrics(LyricEditor2PhraseController phrase) {
        int phraseIndex = phrases.BinarySearch(phrase);
        if (phraseIndex == phrases.Count - 1) {
            return false;
            // Phrase is last
        }
        else {
            return phrases[phraseIndex + 1].anySyllablesPlaced;
        }
    }

    // Pickup all phrases; not revokable, as references to phrases and events
    // are lost when ClearPickupCommands() is called
    void PickupAllPhrases() {
        foreach (var phrase in phrases) {
            phrase.Pickup().Invoke();
        }
        ClearPickupCommands();
    }

    // Set the search IDs of all phrase controllers based on their position in
    // phrases
    void UpdateSortIds() {
        for (int i = 0; i < phrases.Count; i++) {
            phrases[i].sortID = i;
        }
    }

    void UpdateDisplayOrder() {
        for (int i = 0; i < phrases.Count; i++) {
            phrases[i].transform.SetSiblingIndex(phrases[i].sortID);
        }
    }

    // Creates a list of LyricEditor2PhraseController objects from a string
    // input
    List<LyricEditor2PhraseController> CreatePhrases(string inputLyrics) {
        List<LyricEditor2PhraseController> createdPhrases = new List<LyricEditor2PhraseController>();
        List<List<string>> parsedLyrics = ParseLyrics(inputLyrics);
        for (int i = 0; i < parsedLyrics.Count; i++) {
            LyricEditor2PhraseController newPhrase = UnityEngine.GameObject.Instantiate(phraseTemplate, phraseTemplate.transform.parent).GetComponent<LyricEditor2PhraseController>();
            newPhrase.InitializeSyllables(parsedLyrics[i]);
            createdPhrases.Add(newPhrase);
            newPhrase.gameObject.SetActive(true);
        }
        return createdPhrases;
    }

    // Parse a string into a double string array (phrases of syllables) to be
    // given as phrase controller input. Does not have an implemented time-out
    // period in case of excessively long strings to be parsed.
    List<List<string>> ParseLyrics(string inputString) {
        // Remove short tags (5 characters or less), plus any other < or > keys
        // inputString = Regex.Replace(inputString, "</?[^<>]{0,5}>|<|>", System.String.Empty);
        // Regex was NOT the answer
        // https://blog.codinghorror.com/content/images/2014/Apr/stack-overflow-regex-zalgo.png

        // Split into phrases
        char[] newlineCharacters = {'\n', '\r'};
        string[] tempPhrases = inputString.Split(newlineCharacters, System.StringSplitOptions.RemoveEmptyEntries);

        // Prepare the regex engine to parse each phrase
        List<List<string>> parsedLyrics = new List<List<string>>();
        // [^-^=\s]+  matches one or more characters in a syllable, excluding
        //                  spaces, dashes, and equals signs
        // (-|=|\s?)  matches a dash, equals sign, or trailing whitespace
        string regexPattern = @"[^-^=\s]+(-|=|\s?)";
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

    // Create a text representation of stored lyrics which can be pushed to the
    // input menu when the user wants to edit lyrics
    string GetTextRepresentation() {
        string rep = "";
        for (int i = 0; i < phrases.Count; i++) {
            rep += phrases[i].GetTextRepresentation();
        }
        return rep.TrimEnd();
    }

    // Begin auto-scrolling for lyric playback (before the user can place more
    // phrases)
    void StartPlaybackScroll() {
        playbackScrolling = true;
        lastPlaybackTargetIndex = 0;
        LyricEditor2PhraseController firstUnplacedPhrase = GetNextUnfinishedPhrase();
        playbackEndTick = GetFirstSafeTick(firstUnplacedPhrase);
        PlaybackScroll(true);
    }

    // Auto-scroll during lyric playback
    void PlaybackScroll(bool forceScroll) {
        // Check for playback end
        if (currentTickPos >= playbackEndTick) {
            playbackScrolling = false;
            currentPhrase = GetNextUnfinishedPhrase();
            autoScroller.ScrollTo(currentPhrase?.rectTransform);
            return;
        }

        // Find target phrase
        LyricEditor2PhraseController playbackTarget = lastPlaybackTarget;
        for (int i = lastPlaybackTargetIndex; i < phrases.Count; i++) {
            if (phrases[i].anySyllablesPlaced) {
                uint endBound = phrases[i].endTick ?? PhraseEndAutoSpacer(phrases[i]);
                if (currentTickPos < endBound) {
                    playbackTarget = phrases[i];
                    // update lastPlaybackTargetIndex
                    lastPlaybackTargetIndex = i;
                    break;
                }
            }
        }
        // Scroll to phrase
        if (forceScroll || playbackTarget != lastPlaybackTarget) {
            autoScroller.ScrollTo(playbackTarget?.rectTransform);
            lastPlaybackTarget?.PlaybackHighlight(null);
        }
        // Update lastPlaybackTarget
        lastPlaybackTarget = playbackTarget;
        // Highlight syllables
        playbackTarget.PlaybackHighlight(currentTickPos);
    }

    void RefreshAfterPickupFrom() {
        currentPhrase = GetNextUnfinishedPhrase();
    }

    void ClearPickupCommands() {
        // numCommandStackPushes gets decremented automatically, need to assign
        // to another variable first
        int pushesToDelete = numCommandStackPushes;
        for (int i = 0; i < pushesToDelete; ++i) {
            ChartEditor.Instance.commandStack.Pop();
        }

        if (commandStackPushes.Count > 0) {
            ChartEditor.Instance.commandStack.ResetTail();
        }

        // Need to redo the changes made in those pushes
        for (int i = 0; i < pushesToDelete; ++i) {
            commandStackPushes[i].Invoke();
        }

        commandStackPushes.Clear();
        numCommandStackPushes = 0;
    }

    // Create a string representation of all unplaced syllables
    void SaveUnplacedSyllables() {
        savedUnplacedSyllables = "";
        var incompletePhrase = GetNextUnfinishedPhrase();
        if (incompletePhrase != null) {
            int firstSearchIndex;
            // Check for some, but not all, syllables placed
            if (incompletePhrase.anySyllablesPlaced && !incompletePhrase.allSyllablesPlaced) {
                savedUnplacedSyllables += incompletePhrase.GetTextRepresentation(onlyConsiderUnplaced: true);
                firstSearchIndex = phrases.BinarySearch(incompletePhrase) + 1;
            } else {
                firstSearchIndex = phrases.BinarySearch(incompletePhrase);
                savedUnplacedSyllables += "\n";
            }

            // Save fully-placed phrases
            for (int i = firstSearchIndex; i < phrases.Count; i++) {
                savedUnplacedSyllables += phrases[i].GetTextRepresentation();
            }
        }
    }

    // Create a string representation of all placed syllables
    void SavePlacedSyllables() {
        savedPlacedSyllables = "";
        var incompletePhrase = GetNextUnfinishedPhrase();
        if (incompletePhrase != null) {
            int unfinishedIndex = phrases.BinarySearch(incompletePhrase);
            for (int i = 0; i < unfinishedIndex; i++) {
                savedPlacedSyllables += phrases[i].GetTextRepresentation();
            }
            savedPlacedSyllables += incompletePhrase.GetTextRepresentation(onlyConsiderPlaced: true);
            savedPlacedSyllables = savedPlacedSyllables.TrimEnd();
        } else {
            savedPlacedSyllables = GetTextRepresentation();
        }
    }
}
