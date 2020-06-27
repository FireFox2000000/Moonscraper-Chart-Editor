#if UNITY_STANDALONE_OSX

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class NativeMessageBoxOSX : INativeMessageBox
{
    public NativeMessageBox.Result Show(string text, string caption, NativeMessageBox.Type messageBoxType, NativeWindow childWindow)
    {
        throw new NotImplementedException();
    }
}

#endif
