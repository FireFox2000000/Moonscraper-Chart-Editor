#if UNITY_STANDALONE_LINUX

using System.Runtime.InteropServices;
using System;

<<<<<<< HEAD
public class NativeWindow_Linux : INativeWindow
{
    public bool IsConnectedToWindow()
    {
        throw new NotImplementedException();
    }

=======
public class NativeWindow_Linux : NativeWindow_SDL, INativeWindow
{
>>>>>>> 1c48aff03f6a4267d0f0235df6040af3a83722cc
    public bool SetApplicationWindowPointerByName(string desiredWindowName)
    {
        // #LINUX TODO
        // Need to get a window pointer that is compatible with SDL_CreateWindowFrom
        // Potential solution- https://stackoverflow.com/questions/42449050/cant-get-a-window-handle
        throw new NotImplementedException();
<<<<<<< HEAD
    }

    public void SetWindowTitle(string title)
    {
        throw new NotImplementedException();
    }
=======
    }
>>>>>>> 1c48aff03f6a4267d0f0235df6040af3a83722cc
}

#endif
