using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TriggerManager {
    public delegate void OnChartReloadTrigger();
    public static List<OnChartReloadTrigger> onChartReloadTriggerList = new List<OnChartReloadTrigger>();
    public static void FireChartReloadTriggers()
    {
        foreach (OnChartReloadTrigger function in onChartReloadTriggerList)
            function();
    }

    public delegate void HyperspeedChangeTrigger();
    public static List<HyperspeedChangeTrigger> onHyperspeedChangeTriggerList = new List<HyperspeedChangeTrigger>();
    public static void FireHyperspeedChangeTriggers()
    {
        foreach (HyperspeedChangeTrigger function in onHyperspeedChangeTriggerList)
            function();
    }

    public delegate void ViewModeSwitchTrigger(Globals.ViewMode viewMode);
    public static List<ViewModeSwitchTrigger> onViewModeSwitchTriggerList = new List<ViewModeSwitchTrigger>();
    public static void FireViewModeSwitchTriggers()
    {
        foreach (ViewModeSwitchTrigger function in onViewModeSwitchTriggerList)
            function(Globals.viewMode);
    }

    public delegate void LeftyFlipToggledTrigger();
    public static List<LeftyFlipToggledTrigger> onLeftyFlipToggledTriggerList = new List<LeftyFlipToggledTrigger>();
    public static void FireLeftyFlipToggledTriggers()
    {
        foreach (LeftyFlipToggledTrigger function in onLeftyFlipToggledTriggerList)
            function();
    }

    public delegate void ApplicationModeChangedTrigger(Globals.ApplicationMode applicationMode);
    public static List<ApplicationModeChangedTrigger> onApplicationModeChangedTriggerList = new List<ApplicationModeChangedTrigger>();
    public static void FireApplicationModeChangedTriggers()
    {
        foreach (ApplicationModeChangedTrigger function in onApplicationModeChangedTriggerList)
            function(Globals.applicationMode);
    }
}
