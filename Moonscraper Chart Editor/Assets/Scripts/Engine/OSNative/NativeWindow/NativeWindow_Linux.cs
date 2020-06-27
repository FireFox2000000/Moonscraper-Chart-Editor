#if UNITY_STANDALONE_LINUX

using System.Runtime.InteropServices;
using System;

public class NativeWindow_Linux : INativeWindow
{
    public bool IsConnectedToWindow()
    {
        throw new NotImplementedException();
    }

    public bool SetApplicationWindowPointerByName(string desiredWindowName)
    {
        // #LINUX TODO
        // Need to get a window pointer that is compatible with SDL_CreateWindowFrom
        // Potential solution- https://stackoverflow.com/questions/42449050/cant-get-a-window-handle
        throw new NotImplementedException();
    }

    public void SetWindowTitle(string title)
    {
        throw new NotImplementedException();
    }
}

#endif
