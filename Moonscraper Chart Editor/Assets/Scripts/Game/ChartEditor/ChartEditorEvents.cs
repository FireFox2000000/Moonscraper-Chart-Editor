// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

public class ChartEditorEvents
{
    public MSE.Event chartReloadedEvent { get; private set; }
    public MSE.Event hyperspeedChangeEvent { get; private set; }
    public MSE.Event leftyFlipToggledEvent { get; private set; }
    public MSE.Event saveEvent { get; private set; }
    public MSE.Event toolChangedEvent { get; private set; }
    public MSE.Event notePlacementModeChangedEvent { get; private set; }
    public MSE.Event drumsModeOptionChangedEvent { get; private set; }

    public MSE.Event<int> lanesChangedEvent { get; private set; }
    public MSE.Event<bool> keyboardModeToggledEvent { get; private set; }
    public MSE.Event<Globals.ViewMode> viewModeSwitchEvent { get; private set; }
    public MSE.Event<ChartEditor.State> editorStateChangedEvent { get; private set; }
    public MSE.Event<EditorInteractionManager.InteractionType> editorInteractionTypeChangedEvent { get; private set; }

    public ChartEditorEvents()
    {
        chartReloadedEvent = new MSE.Event();
        hyperspeedChangeEvent = new MSE.Event();
        leftyFlipToggledEvent = new MSE.Event();
        saveEvent = new MSE.Event();
        toolChangedEvent = new MSE.Event();
        notePlacementModeChangedEvent = new MSE.Event();
        drumsModeOptionChangedEvent = new MSE.Event();

        lanesChangedEvent = new MSE.Event<int>();
        keyboardModeToggledEvent = new MSE.Event<bool>();
        viewModeSwitchEvent = new MSE.Event<Globals.ViewMode>();
        editorStateChangedEvent = new MSE.Event<ChartEditor.State>();
        editorInteractionTypeChangedEvent = new MSE.Event<EditorInteractionManager.InteractionType>();
    }
}
