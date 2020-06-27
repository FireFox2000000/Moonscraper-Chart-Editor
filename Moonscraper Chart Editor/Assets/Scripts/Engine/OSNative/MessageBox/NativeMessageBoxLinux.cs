#if UNITY_STANDALONE_LINUX

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class NativeMessageBoxLinux : INativeMessageBox
{
    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
<<<<<<< HEAD
        throw new NotImplementedException();
=======
        IntPtr messagePtr = IntPtr.Zero;

        if (childWindow != null)
        {
            NativeWindow_Linux winInterface = childWindow.GetInterface() as NativeWindow_Linux;

            UnityEngine.Debug.Assert(winInterface != null);

            messagePtr = winInterface.sdlWindowPtr;
        }

        return NativeMessageBoxSDL.Show(text, caption, messageBoxType, messagePtr);
>>>>>>> 1c48aff03f6a4267d0f0235df6040af3a83722cc
    }
}

#endif
