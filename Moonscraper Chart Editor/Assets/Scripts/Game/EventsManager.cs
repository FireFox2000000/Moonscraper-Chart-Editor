using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventsManager {
    public static void ClearAll()
    {
        onChartReloadEventList.Clear();
        onHyperspeedChangeEventList.Clear();
        onViewModeSwitchEventList.Clear();
        onLeftyFlipToggledEventList.Clear();
        onApplicationModeChangedEventList.Clear();
        onLanesChangedEventList.Clear();
        onSaveEventList.Clear();
        onToolChangedEventList.Clear();
        onKeyboardModeToggledEvent.Clear();
        onNotePlacementModeChangedEvent.Clear();
    }

    public delegate void ChartReloadedEvent();
    public static List<ChartReloadedEvent> onChartReloadEventList = new List<ChartReloadedEvent>();
    public static void FireChartReloadedEvent()
    {
        foreach (ChartReloadedEvent function in onChartReloadEventList)
            function();
    }

    public delegate void HyperspeedChangeEvent();
    public static List<HyperspeedChangeEvent> onHyperspeedChangeEventList = new List<HyperspeedChangeEvent>();
    public static void FireHyperspeedChangeEvent()
    {
        foreach (HyperspeedChangeEvent function in onHyperspeedChangeEventList)
            function();
    }

    public delegate void ViewModeSwitchEvent(Globals.ViewMode viewMode);
    public static List<ViewModeSwitchEvent> onViewModeSwitchEventList = new List<ViewModeSwitchEvent>();
    public static void FireViewModeSwitchEvent()
    {
        foreach (ViewModeSwitchEvent function in onViewModeSwitchEventList)
            function(Globals.viewMode);
    }

    public delegate void LeftyFlipToggledEvent();
    public static List<LeftyFlipToggledEvent> onLeftyFlipToggledEventList = new List<LeftyFlipToggledEvent>();
    public static void FireLeftyFlipToggledEvent()
    {
        foreach (LeftyFlipToggledEvent function in onLeftyFlipToggledEventList)
            function();
    }

    public delegate void ApplicationModeChangedEvent(Globals.ApplicationMode applicationMode);
    public static List<ApplicationModeChangedEvent> onApplicationModeChangedEventList = new List<ApplicationModeChangedEvent>();
    public static void FireApplicationModeChangedEvent()
    {
        foreach (ApplicationModeChangedEvent function in onApplicationModeChangedEventList)
            function(Globals.applicationMode);
    }

    public delegate void LanesChangedEvent(int laneCount);
    public static List<LanesChangedEvent> onLanesChangedEventList = new List<LanesChangedEvent>();
    public static void FireLanesChangedEvent(int laneCount)
    {
        foreach (LanesChangedEvent function in onLanesChangedEventList)
            function(laneCount);
    }

    public delegate void SaveEvent();
    public static List<SaveEvent> onSaveEventList = new List<SaveEvent>();
    public static void FireSaveEvent()
    {
        foreach (SaveEvent function in onSaveEventList)
            function();
    }

    public delegate void ToolChangedEvent();
    public static List<ToolChangedEvent> onToolChangedEventList = new List<ToolChangedEvent>();
    public static void FireToolChangedEvent()
    {
        foreach (ToolChangedEvent function in onToolChangedEventList)
            function();
    }

    public delegate void KeyboardModeToggledEvent(bool keyboardModeEnabled);
    public static List<KeyboardModeToggledEvent> onKeyboardModeToggledEvent = new List<KeyboardModeToggledEvent>();
    public static void FireKeyboardModeToggledEvent(bool keyboardModeEnabled)
    {
        foreach (KeyboardModeToggledEvent function in onKeyboardModeToggledEvent)
            function(keyboardModeEnabled);
    }

    public delegate void NotePlacementModeChangedEvent();
    public static List<NotePlacementModeChangedEvent> onNotePlacementModeChangedEvent = new List<NotePlacementModeChangedEvent>();
    public static void FireNotePlacementModeChangedEvent()
    {
        foreach (NotePlacementModeChangedEvent function in onNotePlacementModeChangedEvent)
            function();
    }
}
