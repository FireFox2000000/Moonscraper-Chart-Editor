#if UNITY_STANDALONE_WIN

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class NativeMessageBoxWindows : INativeMessageBox
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
        IntPtr messagePtr = IntPtr.Zero;

        if (childWindow != null)
        {
            NativeWindow_Windows winInterface = childWindow.GetInterface() as NativeWindow_Windows;

            UnityEngine.Debug.Assert(winInterface != null);

            messagePtr = winInterface.windowPtr;
        }

        int result = MessageBox(messagePtr, text.ToString(), caption.ToString(), (uint)messageBoxType);

        return (NativeMessageBox.Result)result;
    }
}

#endif
