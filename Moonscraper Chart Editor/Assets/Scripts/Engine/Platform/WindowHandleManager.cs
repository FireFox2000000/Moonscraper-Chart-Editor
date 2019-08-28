//#undef UNITY_EDITOR

using System.Runtime.InteropServices;
using System;

public class WindowHandleManager {
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
    [DllImport("user32.dll")]
    public static extern System.IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    public System.IntPtr windowPtr { get; private set; }
    string productName; // Cross checking the window handle

    string originalWindowName;
    string extraApplicationStateInfo;

    bool isDirty = false;

    public WindowHandleManager(string originalWindowName, string productName)
    {
#if !UNITY_EDITOR
        windowPtr = IntPtr.Zero;
#endif
        this.originalWindowName = originalWindowName;
        this.productName = productName;

        SetApplicationWindowPointer();
    }

    void SetApplicationWindowPointer()
    {
#if !UNITY_EDITOR
        const int nChars = 256;
        System.Text.StringBuilder buffer = new System.Text.StringBuilder(nChars);
        windowPtr = GetForegroundWindow();
        GetWindowText(windowPtr, buffer, nChars);
        if (buffer.ToString() != productName)
        {
            windowPtr = IntPtr.Zero;
            buffer.Length = 0;
            UnityEngine.Debug.LogError("Couldn't find window handle");
        }
        else if (windowPtr != IntPtr.Zero)
        {
            RepaintWindowText(false);
        }
#endif
    }

    public void UpdateDirtyNotification(bool isDirty)
    {
        if (this.isDirty != isDirty)
        {
            RepaintWindowText(isDirty);
        }

        this.isDirty = isDirty;
    }

    public void SetExtraApplicationStateInfoStr(string info)
    {
        extraApplicationStateInfo = info;
        RepaintWindowText(this.isDirty);
    }

    public void OnApplicationFocus(bool hasFocus)
    {
#if !UNITY_EDITOR
        if (hasFocus && windowPtr == IntPtr.Zero)
            SetApplicationWindowPointer();
#endif
    }

    void RepaintWindowText(bool isDirty)
    {
        string dirtySymbol = isDirty ? "*" : "";
        string windowName = originalWindowName + " ~ " + extraApplicationStateInfo + dirtySymbol;

        UnityEngine.Debug.Log("RepaintWindowText - " + windowName);

#if !UNITY_EDITOR

        if (windowPtr != IntPtr.Zero)
        {
            SetWindowText(windowPtr, windowName);
        }
#endif
    }
}
