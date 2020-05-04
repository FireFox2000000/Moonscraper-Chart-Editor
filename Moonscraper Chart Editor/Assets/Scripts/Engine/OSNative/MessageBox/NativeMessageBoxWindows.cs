#if UNITY_STANDALONE_WIN

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class NativeMessageBoxWindows : INativeMessageBox
{
    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
        IntPtr messagePtr = IntPtr.Zero;

        if (childWindow != null)
        {
            NativeWindow_Windows winInterface = childWindow.GetInterface() as NativeWindow_Windows;

            UnityEngine.Debug.Assert(winInterface != null);

            messagePtr = winInterface.sdlWindowPtr;
        }

        return NativeMessageBoxSDL.Show(text, caption, messageBoxType, messagePtr);
    }
}

#endif
