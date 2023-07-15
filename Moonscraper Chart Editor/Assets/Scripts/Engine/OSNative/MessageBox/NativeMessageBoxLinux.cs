#if UNITY_STANDALONE_LINUX

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class NativeMessageBoxLinux : INativeMessageBox
{
    [DllImport("StandaloneFileBrowser")]
    private static extern int sfb_message_box_open(int flags, string title, string caption);

    private const int SFB_MESSAGE_BOX_OK = 1 << 0;
    private const int SFB_MESSAGE_BOX_YESNO = 1 << 1;
    private const int SFB_MESSAGE_BOX_YESNOCANCEL = 1 << 2;

    private const int SFB_MESSAGE_BOX_RESULT_NONE = 1 << 0;
    private const int SFB_MESSAGE_BOX_RESULT_OK = 1 << 1;
    private const int SFB_MESSAGE_BOX_RESULT_CANCEL = 1 << 2;
    private const int SFB_MESSAGE_BOX_RESULT_YES = 1 << 3;
    private const int SFB_MESSAGE_BOX_RESULT_NO = 1 << 4;

    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
        int flags = 0;
        switch (messageBoxType) {
        case NativeMessageBox.Type.OK:
            flags |= SFB_MESSAGE_BOX_OK;
            break;
        case NativeMessageBox.Type.YesNo:
            flags |= SFB_MESSAGE_BOX_YESNO;
            break;
        case NativeMessageBox.Type.YesNoCancel:
            flags |= SFB_MESSAGE_BOX_YESNOCANCEL;
            break;
        }

        int result = sfb_message_box_open(flags, caption, text);

        switch (result) {
        case SFB_MESSAGE_BOX_RESULT_OK:
            return NativeMessageBox.Result.OK;
        case SFB_MESSAGE_BOX_RESULT_CANCEL:
            return NativeMessageBox.Result.Cancel;
        case SFB_MESSAGE_BOX_RESULT_YES:
            return NativeMessageBox.Result.Yes;
        case SFB_MESSAGE_BOX_RESULT_NO:
            return NativeMessageBox.Result.No;
        case SFB_MESSAGE_BOX_RESULT_NONE:
        default:
            return NativeMessageBox.Result.None;
        }
    }
}

#endif
