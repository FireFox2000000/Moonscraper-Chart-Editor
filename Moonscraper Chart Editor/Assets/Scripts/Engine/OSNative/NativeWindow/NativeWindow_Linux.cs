#if UNITY_STANDALONE_LINUX

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using X11;

/// <summary>NativeWindow implementation for X11 on Linux</summary>
public class NativeWindow_Linux : INativeWindow
{
    private Window? windowHandle;

    public bool IsConnectedToWindow() => windowHandle != null;

    public bool SetApplicationWindowPointerByName(string desiredWindowName)
    {
        IntPtr display = Xlib.XOpenDisplay("");

        Window? window = FindWindowWithTitle(display, desiredWindowName);

        if (window is Window theWindow) {
            this.windowHandle = theWindow;
            return true;
        } else {
            return false;
        }
    }

    public void SetWindowTitle(string title)
    {
        if (!(windowHandle is Window window)) { return; }

        IntPtr display = Xlib.XOpenDisplay("");

        Atom nameAtom = XlibCustom.XInternAtom(display, "_NET_WM_NAME", false);
        Atom utf8Atom = XlibCustom.XInternAtom(display, "UTF8_STRING", false);

        Xlib.XStoreName(display, window, title);

        IntPtr titleptr = Marshal.StringToHGlobalAnsi(title);
        Xlib.XChangeProperty(display, window, nameAtom, utf8Atom, 8, 0, titleptr, title.Length);

        Xlib.XFlush(display);
    }

    // https://stackoverflow.com/questions/42449050/cant-get-a-window-handle
    private static Window? FindChildWindows(IntPtr display, Window window, string title)
    {
        Window rootWindow = 0;
        Window parentWindow = 0;

        string windowName = "";
        try {
            Xlib.XFetchName(display, window, ref windowName);
        } catch {
            return null;
        }

        if (windowName == title) {
            return window;
        }

        List<Window> childWindows = new List<Window>();
        Xlib.XQueryTree(display, window, ref rootWindow, ref parentWindow, out childWindows);

        foreach (Window childWindow in childWindows)
        {
            if (FindChildWindows(display, childWindow, title) is Window foundWindow) {
                return foundWindow;
            }
        }

        return null;
    }

    private static Window? FindWindowWithTitle(IntPtr display, string title)
    {
        return FindChildWindows(display, Xlib.XDefaultRootWindow(display), title);
    }
}

namespace X11 {
    public partial class XlibCustom {
        [DllImport("libX11.so.6")]
        public static extern X11.Atom XInternAtom(IntPtr display, string name, bool only_if_exists);
    }
}

#endif
