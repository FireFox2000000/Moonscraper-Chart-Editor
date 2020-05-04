using System;
using SDL2;
using UnityEngine;

public class NativeWindow_SDL
{
    public IntPtr sdlWindowPtr { get; private set; }

    public NativeWindow_SDL()
    {
        sdlWindowPtr = IntPtr.Zero;
    }

    protected bool SetWindowPtrFromNative(IntPtr nativeWindowPtr)
    {
        sdlWindowPtr = SDL.SDL_CreateWindowFrom(nativeWindowPtr);

        if (sdlWindowPtr == IntPtr.Zero)
        {
            Debug.LogError("Failed to convert window handle to SDL window. Error: " + SDL.SDL_GetError());
            return false;
        }

        return true;
    }

    public bool IsConnectedToWindow()
    {
        return sdlWindowPtr != IntPtr.Zero;
    }

    public void SetWindowTitle(string title)
    {
        if (IsConnectedToWindow())
        {
            SDL.SDL_SetWindowTitle(sdlWindowPtr, title);
        }
    }
}
