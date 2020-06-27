#if UNITY_STANDALONE_OSX

using System.Runtime.InteropServices;
using System;

public class NativeWindow_OSX : INativeWindow
{
    public NativeWindow_OSX()
    {
    }

    public bool IsConnectedToWindow()
    {
        throw new NotImplementedException();
    }

    public bool SetApplicationWindowPointerByName(string desiredWindowName)
    {
        throw new NotImplementedException();
    }

    public void SetWindowTitle(string title)
    {
        throw new NotImplementedException();
    }
}

#endif
