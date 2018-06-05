using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventsManager {
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
}
