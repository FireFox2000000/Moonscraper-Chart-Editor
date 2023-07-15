#if UNITY_STANDALONE_WIN

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class NativeMessageBoxWindows : INativeMessageBox
{
    public enum WinType
    {
        OK = 0,
        OKCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5,
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
        IntPtr messagePtr = IntPtr.Zero;

        if (childWindow != null)
        {
            NativeWindow_Windows winInterface = childWindow.GetInterface() as NativeWindow_Windows;

            UnityEngine.Debug.Assert(winInterface != null);

            messagePtr = winInterface.windowPtr;
        }

        WinType boxType = TranslateMessageBoxType(messageBoxType);

        int result = MessageBox(messagePtr, text.ToString(), caption.ToString(), (uint)boxType);

        return (NativeMessageBox.Result)result;
    }

    WinType TranslateMessageBoxType(NativeMessageBox.Type messageBoxType)
    {
        switch (messageBoxType)
        {
            case NativeMessageBox.Type.OK: return WinType.OK;
            case NativeMessageBox.Type.YesNo: return WinType.YesNo;
            case NativeMessageBox.Type.YesNoCancel: return WinType.YesNoCancel;

            default:
                throw new NotImplementedException("NativeMessageBox.Type to WinType not implemented for message box type " + messageBoxType);
        };
    }
}

#endif
