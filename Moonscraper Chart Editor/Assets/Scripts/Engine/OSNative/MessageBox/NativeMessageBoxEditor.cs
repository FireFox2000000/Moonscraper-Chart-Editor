#if UNITY_EDITOR

using System;

public class NativeMessageBoxEditor : INativeMessageBox
{
    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
        switch (messageBoxType)
        {
            case NativeMessageBox.Type.YesNo:
                return UnityEditor.EditorUtility.DisplayDialog(caption, text, "Yes", "No") ? NativeMessageBox.Result.Yes : NativeMessageBox.Result.No;

            case NativeMessageBox.Type.YesNoCancel:
                {
                    int result = UnityEditor.EditorUtility.DisplayDialogComplex(caption, text, "Yes", "No", "Cancel");
                    switch (result)
                    {
                        case 0: return NativeMessageBox.Result.Yes;
                        case 1: return NativeMessageBox.Result.No;
                        case 2: return NativeMessageBox.Result.Cancel;

                        default: break;
                    }
                }

                break;
            default: break;
        }

        throw new NotSupportedException();
    }
}

#endif
