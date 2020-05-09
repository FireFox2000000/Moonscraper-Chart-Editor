#if UNITY_STANDALONE_OSX

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class NativeMessageBoxOSX : INativeMessageBox
{
    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
        IntPtr messagePtr = IntPtr.Zero;

        if (childWindow != null)
        {
            NativeWindow_OSX winInterface = childWindow.GetInterface() as NativeWindow_OSX;

            UnityEngine.Debug.Assert(winInterface != null);

            messagePtr = winInterface.sdlWindowPtr;
        }

        return NativeMessageBoxSDL.Show(text, caption, messageBoxType, messagePtr);
    }
}

#endif
