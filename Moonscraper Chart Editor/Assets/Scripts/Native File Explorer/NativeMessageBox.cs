using System;
using System.Runtime.InteropServices;

public static class NativeMessageBox {
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public enum Type
    {
        OK = 0,
        OKCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5,
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

    public static Result Show(string text, string caption, Type messageBoxType)
    {
        IntPtr messagePtr = new IntPtr();
        int result = MessageBox(messagePtr, text.ToString(), caption.ToString(), (uint)messageBoxType);

        return (Result)result;
    }
}
