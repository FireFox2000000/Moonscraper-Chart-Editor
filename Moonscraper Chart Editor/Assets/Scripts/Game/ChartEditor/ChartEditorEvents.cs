// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

public class ChartEditorEvents
{
    public MoonscraperEngine.Event songLoadedEvent { get; private set; }              // Fires upon loading or creating a new song
    public MoonscraperEngine.Event chartReloadedEvent { get; private set; }         // Fires upon loading or creating a new song, or when the difficulty/instrument changes as well as drum mode
    public MoonscraperEngine.Event hyperspeedChangeEvent { get; private set; }
    public MoonscraperEngine.Event leftyFlipToggledEvent { get; private set; }
    public MoonscraperEngine.Event saveEvent { get; private set; }
    public MoonscraperEngine.Event toolChangedEvent { get; private set; }
    public MoonscraperEngine.Event notePlacementModeChangedEvent { get; private set; }
    public MoonscraperEngine.Event drumsModeOptionChangedEvent { get; private set; }
    public MoonscraperEngine.Event playbackStoppedEvent { get; private set; }
    public MoonscraperEngine.Event groupMoveStart { get; private set; }

    public MoonscraperEngine.Event<int> lanesChangedEvent { get; private set; }
    public MoonscraperEngine.Event<bool> keyboardModeToggledEvent { get; private set; }
    public MoonscraperEngine.Event<Globals.ViewMode> viewModeSwitchEvent { get; private set; }
    public MoonscraperEngine.Event<ChartEditor.State> editorStateChangedEvent { get; private set; }
    public MoonscraperEngine.Event<EditorInteractionManager.InteractionType> editorInteractionTypeChangedEvent { get; private set; }

    public ChartEditorEvents()
    {
        songLoadedEvent = new MoonscraperEngine.Event();
        chartReloadedEvent = new MoonscraperEngine.Event();
        hyperspeedChangeEvent = new MoonscraperEngine.Event();
        leftyFlipToggledEvent = new MoonscraperEngine.Event();
        saveEvent = new MoonscraperEngine.Event();
        toolChangedEvent = new MoonscraperEngine.Event();
        notePlacementModeChangedEvent = new MoonscraperEngine.Event();
        drumsModeOptionChangedEvent = new MoonscraperEngine.Event();
        playbackStoppedEvent = new MoonscraperEngine.Event();
        groupMoveStart = new MoonscraperEngine.Event();

        lanesChangedEvent = new MoonscraperEngine.Event<int>();
        keyboardModeToggledEvent = new MoonscraperEngine.Event<bool>();
        viewModeSwitchEvent = new MoonscraperEngine.Event<Globals.ViewMode>();
        editorStateChangedEvent = new MoonscraperEngine.Event<ChartEditor.State>();
        editorInteractionTypeChangedEvent = new MoonscraperEngine.Event<EditorInteractionManager.InteractionType>();
    }
}
