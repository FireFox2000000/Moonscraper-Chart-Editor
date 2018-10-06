using System;
using System.Runtime.InteropServices;

public static class CommDlgBindings {
    public static bool GetOpenFileName(OpenFileName ofn)
    {
        return CommDlgBindings_Windows.GetOpenFileName(ofn);
    }

    public static bool GetSaveFileName(OpenFileName ofn)
    {
        return CommDlgBindings_Windows.GetSaveFileName(ofn);
    }

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
    const string c_comDlgDll = "comdlg32.dll";
    const string c_user32Dll = "user32.dll";

    [DllImport(c_comDlgDll, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [DllImport(c_comDlgDll, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

    [DllImport(c_comDlgDll, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern int CommDlgExtendedError();

    [DllImport(c_user32Dll, SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
}