#if UNITY_STANDALONE_OSX

using System.Runtime.InteropServices;
using System;

public class NativeWindow_OSX : NativeWindow_SDL, INativeWindow
{
    public NativeWindow_OSX() : base()
    {
    }

    public bool SetApplicationWindowPointerByName(string desiredWindowName)
    {
        throw new NotImplementedException();

        // #OSX TODO
        // Need to get a window pointer of type NSWindow and pass that to SDL_CreateWindowFrom
        IntPtr nsWindowHandle = IntPtr.Zero;

        if (nsWindowHandle != IntPtr.Zero)
        {
            SetWindowPtrFromNative(nsWindowHandle);
            return true;
        }

        return false;
    }
}

#endif
