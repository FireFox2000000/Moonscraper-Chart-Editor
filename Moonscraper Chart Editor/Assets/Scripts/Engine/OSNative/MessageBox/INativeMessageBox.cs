using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INativeMessageBox
{
    NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow);
}
