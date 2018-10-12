using System;
using System.Runtime.InteropServices;

public static class CommDlgBindings {
    public static int CommDlgExtendedError()
    {
        return CommDlgBindings_Windows.CommDlgExtendedError();
    }

    public static int MessageBox(IntPtr hWnd, String text, String caption, uint type)
    {
        return CommDlgBindings_Windows.MessageBox(hWnd, text, caption, type);
    }
}

static class CommDlgBindings_Windows
{
    const string c_user32Dll = "user32.dll";
    const string c_comDlgDll = "comdlg32.dll";

    [DllImport(c_user32Dll, SetLastError = true)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    [DllImport(c_comDlgDll, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern int CommDlgExtendedError();
}