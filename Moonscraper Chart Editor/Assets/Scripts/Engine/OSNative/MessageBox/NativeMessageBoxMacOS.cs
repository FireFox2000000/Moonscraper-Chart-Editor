#if UNITY_STANDALONE_OSX

using System;
using System.Runtime.InteropServices;

public class NativeMessageBoxMacOS : INativeMessageBox
{
    [DllImport("NativeWindow")]
    private static extern int nativewindow_show_message_box(IntPtr title, IntPtr subtitle, int type);

    private const int NATIVEWINDOW_MESSAGE_BOX_TYPE_OK            = 0;
    private const int NATIVEWINDOW_MESSAGE_BOX_TYPE_YES_NO        = 1;
    private const int NATIVEWINDOW_MESSAGE_BOX_TYPE_YES_CANCEL_NO = 2;

    private const int NATIVEWINDOW_MESSAGE_BOX_RESPONSE_OK     = 0;
    private const int NATIVEWINDOW_MESSAGE_BOX_RESPONSE_YES    = 1;
    private const int NATIVEWINDOW_MESSAGE_BOX_RESPONSE_NO     = 2;
    private const int NATIVEWINDOW_MESSAGE_BOX_RESPONSE_CANCEL = 3;

    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
        int nativeMessageBoxType = NATIVEWINDOW_MESSAGE_BOX_TYPE_OK;
        switch (messageBoxType) {
        case NativeMessageBox.Type.YesNo:
            nativeMessageBoxType = NATIVEWINDOW_MESSAGE_BOX_TYPE_YES_NO;
            break;
        case NativeMessageBox.Type.YesNoCancel:
            nativeMessageBoxType = NATIVEWINDOW_MESSAGE_BOX_TYPE_YES_CANCEL_NO;
            break;
        }

        int nativeMessageBoxResult = nativewindow_show_message_box(
            NativeWindow_macOS.MarshalManagedStringToNativeUTF8(caption),
            NativeWindow_macOS.MarshalManagedStringToNativeUTF8(text),
            nativeMessageBoxType
        );

        switch (nativeMessageBoxResult) {
        case NATIVEWINDOW_MESSAGE_BOX_RESPONSE_OK:
            return NativeMessageBox.Result.OK;
        case NATIVEWINDOW_MESSAGE_BOX_RESPONSE_YES:
            return NativeMessageBox.Result.Yes;
        case NATIVEWINDOW_MESSAGE_BOX_RESPONSE_NO:
            return NativeMessageBox.Result.No;
        case NATIVEWINDOW_MESSAGE_BOX_RESPONSE_CANCEL:
            return NativeMessageBox.Result.Cancel;
        default:
            return NativeMessageBox.Result.None;
        }
    }
}

#endif
