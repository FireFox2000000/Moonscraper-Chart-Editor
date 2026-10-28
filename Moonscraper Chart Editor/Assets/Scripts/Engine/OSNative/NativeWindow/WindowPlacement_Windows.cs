#if UNITY_STANDALONE_WIN

using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

/// <summary>
/// Reopens the window as it was when Moonscraper last closed: maximised or not, and its size and position when it isn't maximised.
/// Unity itself only remembers the size.
/// </summary>
public static class WindowPlacement_Windows
{
    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;
    }

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);
    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);
    [DllImport("user32.dll")]
    static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT placement);
    [DllImport("user32.dll")]
    static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT placement);
    [DllImport("kernel32.dll")]
    static extern uint GetCurrentProcessId();

    const int SW_SHOWNORMAL = 1;
    const int SW_SHOWMINIMIZED = 2;
    const int SW_SHOWMAXIMIZED = 3;
    const int WPF_RESTORETOMAXIMIZED = 2;

    // Saved next to Unity's own window size, because config.ini isn't loaded this early
    const string MaximizedKey = "Window Maximized";
    const string LeftKey = "Window Left";
    const string TopKey = "Window Top";
    const string RightKey = "Window Right";
    const string BottomKey = "Window Bottom";

    // The earliest point Unity 2018 runs scripts, before the first scene shows.
    // Unity finds this method in the editor's build of the scripts, so the editor compiles it too.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Restore()
    {
        if (Application.isEditor)
            return;

        Application.quitting += Save;

        IntPtr window = FindUnityWindow();
        if (window == IntPtr.Zero || !PlayerPrefs.HasKey(MaximizedKey))
            return;

        WINDOWPLACEMENT placement = NewPlacement();
        bool maximized = PlayerPrefs.GetInt(MaximizedKey) != 0;
        placement.showCmd = maximized ? SW_SHOWMAXIMIZED : SW_SHOWNORMAL;
        placement.rcNormalPosition.left = PlayerPrefs.GetInt(LeftKey);
        placement.rcNormalPosition.top = PlayerPrefs.GetInt(TopKey);
        placement.rcNormalPosition.right = PlayerPrefs.GetInt(RightKey);
        placement.rcNormalPosition.bottom = PlayerPrefs.GetInt(BottomKey);

        // Windows moves a window back on screen when its saved position is off every monitor
        SetWindowPlacement(window, ref placement);
        Debug.Log("Restored window placement, maximized: " + maximized);
    }

    static void Save()
    {
        IntPtr window = FindUnityWindow();
        WINDOWPLACEMENT placement = NewPlacement();
        if (window == IntPtr.Zero || !GetWindowPlacement(window, ref placement))
            return;

        bool maximized = placement.showCmd == SW_SHOWMAXIMIZED
            || (placement.showCmd == SW_SHOWMINIMIZED && (placement.flags & WPF_RESTORETOMAXIMIZED) != 0);

        PlayerPrefs.SetInt(MaximizedKey, maximized ? 1 : 0);
        PlayerPrefs.SetInt(LeftKey, placement.rcNormalPosition.left);
        PlayerPrefs.SetInt(TopKey, placement.rcNormalPosition.top);
        PlayerPrefs.SetInt(RightKey, placement.rcNormalPosition.right);
        PlayerPrefs.SetInt(BottomKey, placement.rcNormalPosition.bottom);
        PlayerPrefs.Save();
    }

    static WINDOWPLACEMENT NewPlacement()
    {
        WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
        placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
        return placement;
    }

    // Finds the window by process rather than by focus, so it works while another window is in front
    static IntPtr FindUnityWindow()
    {
        uint processId = GetCurrentProcessId();
        StringBuilder className = new StringBuilder(64);
        IntPtr found = IntPtr.Zero;

        EnumWindows((hWnd, lParam) =>
        {
            uint windowProcessId;
            GetWindowThreadProcessId(hWnd, out windowProcessId);
            if (windowProcessId != processId)
                return true;

            className.Length = 0;
            GetClassName(hWnd, className, className.Capacity);
            if (className.ToString() != "UnityWndClass")
                return true;

            found = hWnd;
            return false;
        }, IntPtr.Zero);

        return found;
    }
}

#endif
