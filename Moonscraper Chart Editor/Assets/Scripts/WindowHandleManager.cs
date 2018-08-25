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

#if !UNITY_EDITOR
    System.IntPtr windowPtr = IntPtr.Zero;
    string originalWindowName;
    string productName;
    bool isDirty = false;
#endif

    public WindowHandleManager(string originalWindowName, string productName)
    {
#if !UNITY_EDITOR
        this.originalWindowName = originalWindowName;
        this.productName = productName;

        SetApplicationWindowPointer();
#endif
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
            SetWindowText(windowPtr, originalWindowName);
#endif
    }

    public void UpdateDirtyNotification(bool isDirty)
    {
#if !UNITY_EDITOR
        if (windowPtr != IntPtr.Zero && this.isDirty != isDirty)
        {
            if (isDirty)
                SetWindowText(windowPtr, originalWindowName + "*");
            else
                SetWindowText(windowPtr, originalWindowName);
        }

        this.isDirty = isDirty;
#endif
    }

    public void OnApplicationFocus(bool hasFocus)
    {
#if !UNITY_EDITOR
        if (hasFocus && windowPtr == IntPtr.Zero)
            SetApplicationWindowPointer();
#endif
    }
}
