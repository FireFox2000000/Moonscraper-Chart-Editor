using System;
using System.Runtime.InteropServices;

public static class NativeMessageBox {
    public static volatile int m_messageBoxesRefCount = 0;
    public static bool messageBoxActive { get { return m_messageBoxesRefCount > 0; } }

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

    public static Result Show(string text, string caption, Type messageBoxType)
    {
#if !UNITY_EDITOR
        ++m_messageBoxesRefCount;
        UnityEngine.Debug.Log("Incrementing NativeMessageBox ref count, new value: " + m_messageBoxesRefCount);

        IntPtr messagePtr = ChartEditor.Instance.windowHandleManager.windowPtr;
        int result = CommDlgBindings.MessageBox(messagePtr, text.ToString(), caption.ToString(), (uint)messageBoxType);

        --m_messageBoxesRefCount;
        UnityEngine.Debug.Log("Decrementing NativeMessageBox ref count, new value: " + m_messageBoxesRefCount);

        return (Result)result;
#else
        switch (messageBoxType)
        {
            case Type.YesNo:
                return UnityEditor.EditorUtility.DisplayDialog(caption, text, "Yes", "No") ? Result.Yes : Result.No;

            case Type.YesNoCancel:
                {
                    int result = UnityEditor.EditorUtility.DisplayDialogComplex(caption, text, "Yes", "No", "Cancel");
                    switch (result)
                    {
                        case 0: return Result.Yes;
                        case 1: return Result.No;
                        case 2: return Result.Cancel;

                        default: break;
                    }
                }

                break;
            default: break;
        }

        throw new NotSupportedException();
#endif
    }
}
