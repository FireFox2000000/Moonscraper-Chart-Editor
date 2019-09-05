using System;
using System.Runtime.InteropServices;

public static class NativeMessageBox {

    static INativeMessageBox m_platformWrapper = null;

    public enum Type
    {
        //OK = 0,
        //OKCancel = 1,
        //AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        //RetryCancel = 5,
    }

    public enum Result
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
    }

    static NativeMessageBox()
    {
#if UNITY_EDITOR
        m_platformWrapper = new NativeMessageBoxEditor();
#elif UNITY_STANDALONE_WIN
        m_platformWrapper = new NativeMessageBoxWindows();
#endif
    }

    public static Result Show(string text, string caption, Type messageBoxType, NativeWindow childWindow)
    {
        return m_platformWrapper.Show(text, caption, messageBoxType, childWindow);
    }
}
