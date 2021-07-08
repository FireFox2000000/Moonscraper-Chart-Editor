#if UNITY_STANDALONE_OSX

using System;
using System.Text;
using System.Runtime.InteropServices;

/// <summary>NativeWindow implementation for macOS</summary>
public class NativeWindow_macOS : INativeWindow
{
    [DllImport("NativeWindow")]
    private static extern void nativewindow_set_window_title(IntPtr title);

    [DllImport("NativeWindow")]
    private static extern bool nativewindow_get_has_window();

    /// https://stackoverflow.com/a/58358514
    public unsafe static string MarshalNativeUTF8ToManagedString(IntPtr ptr) {
        var bytes = (byte*)ptr;
        var len = 0;
        while (bytes[len] != 0) len++;
        string str = Encoding.UTF8.GetString(bytes, len);
        Marshal.FreeHGlobal(ptr);
        return str;
    }

    /// https://stackoverflow.com/a/58358514
    public unsafe static IntPtr MarshalManagedStringToNativeUTF8(string str) {
        fixed (char* bytes = str) {
            var len = Encoding.UTF8.GetByteCount(bytes, str.Length);
            var pResult = (byte*)Marshal.AllocHGlobal(len + 1).ToPointer();
            var bytesWritten = Encoding.UTF8.GetBytes(bytes, str.Length, pResult, len);
            UnityEngine.Debug.Assert(len == bytesWritten);
            pResult[len] = 0;
            return (IntPtr)pResult;
        }
    }

    public bool IsConnectedToWindow()
        => nativewindow_get_has_window();

    public bool SetApplicationWindowPointerByName(string desiredWindowName)
        => nativewindow_get_has_window();

    public void SetWindowTitle(string title)
    {
        nativewindow_set_window_title(MarshalManagedStringToNativeUTF8(title));
    }
}

#endif
