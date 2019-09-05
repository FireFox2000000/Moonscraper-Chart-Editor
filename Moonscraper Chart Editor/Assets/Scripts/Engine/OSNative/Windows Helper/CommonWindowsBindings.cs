using System;
using System.Runtime.InteropServices;

public static class CommonWindowsBindings
{
    public const string c_comDlgDll = "comdlg32.dll";
    public const string c_user32Dll = "user32.dll";

    [DllImport(c_user32Dll)]
    public static extern IntPtr GetActiveWindow();

    [DllImport(c_comDlgDll, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern int CommDlgExtendedError();
}
