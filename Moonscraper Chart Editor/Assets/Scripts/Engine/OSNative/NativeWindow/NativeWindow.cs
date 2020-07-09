using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NativeWindow
{
    INativeWindow m_platformWrapper = null;

    public NativeWindow()
    {
#if UNITY_EDITOR
        m_platformWrapper = new NativeWindow_Editor();
#elif UNITY_STANDALONE_WIN
        m_platformWrapper = new NativeWindow_Windows();
#elif UNITY_STANDALONE_LINUX
        m_platformWrapper = new NativeWindow_Linux();
#elif UNITY_STANDALONE_OSX

#endif

        Debug.Assert(m_platformWrapper != null, "Platform wrapper needs implementation!");
    }

    public bool IsConnectedToWindow()
    {
        return m_platformWrapper.IsConnectedToWindow();
    }

    public bool SetApplicationWindowPointerByName(string desiredWindowName)
    {
        return m_platformWrapper.SetApplicationWindowPointerByName(desiredWindowName);
    }

    public void SetWindowTitle(string title)
    {
        m_platformWrapper.SetWindowTitle(title);
    }

    public INativeWindow GetInterface()
    {
        return m_platformWrapper;
    }
}
