#if UNITY_STANDALONE_WIN

using System.Runtime.InteropServices;
using System;

public class NativeWindow_Windows : INativeWindow
{
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
    [DllImport("user32.dll")]
    public static extern System.IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    public IntPtr windowPtr { get; private set; }

    public NativeWindow_Windows()
    {
        windowPtr = IntPtr.Zero;
    }

    public bool IsConnectedToWindow()
    {
        return windowPtr != IntPtr.Zero;
    }

    public bool SetApplicationWindowPointerByName(string desiredWindowName)
    {
        const int nChars = 256;
        System.Text.StringBuilder buffer = new System.Text.StringBuilder(nChars);
        windowPtr = GetForegroundWindow();
        GetWindowText(windowPtr, buffer, nChars);
        if (buffer.ToString() != desiredWindowName)
        {
            windowPtr = IntPtr.Zero;
            buffer.Length = 0;
            UnityEngine.Debug.LogError("Couldn't find window handle");

            return false;
        }
        else if (windowPtr != IntPtr.Zero)
        {
            return true;
        }

        return false;
    }

    public void SetWindowTitle(string title)
    {
        if (IsConnectedToWindow())
        {
            SetWindowText(windowPtr, title);
        }
    }
}

#endif
