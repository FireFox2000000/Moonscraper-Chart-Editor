
public class ChartEditorEvents
{
    public EventHandler.Event chartReloadedEvent { get; private set; }
    public EventHandler.Event hyperspeedChangeEvent { get; private set; }
    public EventHandler.Event leftyFlipToggledEvent { get; private set; }
    public EventHandler.Event saveEvent { get; private set; }
    public EventHandler.Event toolChangedEvent { get; private set; }
    public EventHandler.Event notePlacementModeChangedEvent { get; private set; }

    public EventHandler.Event<int> lanesChangedEvent { get; private set; }
    public EventHandler.Event<bool> keyboardModeToggledEvent { get; private set; }
    public EventHandler.Event<Globals.ViewMode> viewModeSwitchEvent { get; private set; }
    public EventHandler.Event<ChartEditor.State> editorStateChangedEvent { get; private set; }

    public ChartEditorEvents()
    {
        chartReloadedEvent = new EventHandler.Event();
        hyperspeedChangeEvent = new EventHandler.Event();
        leftyFlipToggledEvent = new EventHandler.Event();
        saveEvent = new EventHandler.Event();
        toolChangedEvent = new EventHandler.Event();
        notePlacementModeChangedEvent = new EventHandler.Event();

        lanesChangedEvent = new EventHandler.Event<int>();
        keyboardModeToggledEvent = new EventHandler.Event<bool>();
        viewModeSwitchEvent = new EventHandler.Event<Globals.ViewMode>();
        editorStateChangedEvent = new EventHandler.Event<ChartEditor.State>();
    }
}
