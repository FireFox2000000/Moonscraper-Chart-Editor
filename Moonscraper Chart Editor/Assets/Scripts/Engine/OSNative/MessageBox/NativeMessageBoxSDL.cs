using System;
using SDL2;
using System.Collections;
using System.Collections.Generic;

public static class NativeMessageBoxSDL
{
    static readonly SDL.SDL_MessageBoxButtonData YES_BUTTON = new SDL.SDL_MessageBoxButtonData() { text = "Yes", buttonid = (int)NativeMessageBox.Result.Yes, };
    static readonly SDL.SDL_MessageBoxButtonData NO_BUTTON = new SDL.SDL_MessageBoxButtonData() { text = "No", buttonid = (int)NativeMessageBox.Result.No, };
    static readonly SDL.SDL_MessageBoxButtonData CANCEL_BUTTON = new SDL.SDL_MessageBoxButtonData() { text = "Cancel", buttonid = (int)NativeMessageBox.Result.Cancel, };

    public static NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, IntPtr sdlWindowHandle)
    {
        List<SDL.SDL_MessageBoxButtonData> buttons = new List<SDL.SDL_MessageBoxButtonData>();

        switch (messageBoxType)
        {
            case NativeMessageBox.Type.YesNo:
                {
                    buttons.Add(NO_BUTTON);
                    buttons.Add(YES_BUTTON);
                    break;
                }
            case NativeMessageBox.Type.YesNoCancel:
                {
                    buttons.Add(CANCEL_BUTTON);
                    buttons.Add(NO_BUTTON);
                    buttons.Add(YES_BUTTON);
                    break;
                }
            default:
                {
                    UnityEngine.Debug.LogError("Unabled message box type");
                    break;
                }
        }

        SDL.SDL_MessageBoxData messageBoxData = new SDL.SDL_MessageBoxData();
        messageBoxData.window = sdlWindowHandle;
        messageBoxData.title = caption;
        messageBoxData.message = text;
        messageBoxData.buttons = buttons.ToArray();
        messageBoxData.numbuttons = buttons.Count;

        int buttonId;

        if (SDL.SDL_ShowMessageBox(ref messageBoxData, out buttonId) < 0)
        {
            UnityEngine.Debug.LogError("SDL Message box error- " + SDL2.SDL.SDL_GetError());
        }

        UnityEngine.Debug.Log("SDL message box button id = " + buttonId);

        return (NativeMessageBox.Result)buttonId;
    }
}
