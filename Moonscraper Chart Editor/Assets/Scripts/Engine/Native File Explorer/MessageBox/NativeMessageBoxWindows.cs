#if UNITY_STANDALONE_WIN

using System;
using System.Runtime.InteropServices;

public class NativeMessageBoxWindows : INativeMessageBox
{
    [DllImport(CommonWindowsBindings.c_user32Dll, SetLastError = true)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, bool useWindowHandle = true)
    {
        IntPtr messagePtr = useWindowHandle ? ChartEditor.Instance.windowHandleManager.windowPtr : IntPtr.Zero;
        int result = MessageBox(messagePtr, text.ToString(), caption.ToString(), (uint)messageBoxType);

        return (NativeMessageBox.Result)result;
    }
}

#endif
