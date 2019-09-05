#if UNITY_STANDALONE_WIN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text;

public class FileExplorerWindows : IFileExplorer
{
    [Flags]
    enum OFN_Flags
    {
        OverwritePrompt = 0x000002,
    }

    const string c_comDlgDll = "comdlg32.dll";
    const string c_user32Dll = "user32.dll";

    [DllImport(c_user32Dll)]
    private static extern IntPtr GetActiveWindow();

    [DllImport(c_comDlgDll, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [DllImport(c_comDlgDll, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

    public bool OpenFilePanel(ExtensionFilter filter, string defExt, out string resultPath)
    {
        return OpenFilePanel(ParseExtentionFilter(filter), defExt, out resultPath);
    }

    public bool SaveFilePanel(ExtensionFilter filter, string defaultFileName, string defExt, out string resultPath)
    {
        return SaveFilePanel(ParseExtentionFilter(filter), defaultFileName, defExt, out resultPath);
    }

    bool OpenFilePanel(string filter, string defExt, out string resultPath)
    {
        OpenFileName openChartFileDialog = new OpenFileName();

        openChartFileDialog.structSize = Marshal.SizeOf(openChartFileDialog);
        openChartFileDialog.dlgOwner = GetActiveWindow();

        openChartFileDialog.filter = filter;
        openChartFileDialog.file = new String(new char[256]);
        openChartFileDialog.maxFile = openChartFileDialog.file.Length;
        openChartFileDialog.fileTitle = new String(new char[64]);
        openChartFileDialog.maxFileTitle = openChartFileDialog.fileTitle.Length;

        openChartFileDialog.initialDir = "";
        openChartFileDialog.title = "Open file";
        openChartFileDialog.defExt = defExt;

        if (GetOpenFileName(openChartFileDialog))
        {
            resultPath = new string(openChartFileDialog.file.ToCharArray());
            return true;
        }
        else
        {
            CommonDialogBox.ErrorCodes errorCode = CommonDialogBox.GetErrorCode();
            if (errorCode != CommonDialogBox.ErrorCodes.None)
                ChartEditor.Instance.errorManager.QueueErrorMessage("Error occured when bringing up the Open File file explorer. \nError Code: " + errorCode);

            resultPath = string.Empty;
            return false;
        }
    }

    bool SaveFilePanel(string filter, string defaultFileName, string defExt, out string resultPath)
    {
        defaultFileName = FileExplorer.StripIllegalChars(defaultFileName);

        OpenFileName openSaveFileDialog = new OpenFileName();

        openSaveFileDialog.structSize = Marshal.SizeOf(openSaveFileDialog);

        openSaveFileDialog.dlgOwner = GetActiveWindow();

        openSaveFileDialog.filter = filter;
        openSaveFileDialog.file = new String(new char[256]);
        openSaveFileDialog.maxFile = openSaveFileDialog.file.Length;

        openSaveFileDialog.fileTitle = new String(new char[64]);
        openSaveFileDialog.maxFileTitle = openSaveFileDialog.fileTitle.Length;

        openSaveFileDialog.file = defaultFileName;

        openSaveFileDialog.initialDir = "";
        openSaveFileDialog.title = "Save as";
        openSaveFileDialog.defExt = defExt;
        openSaveFileDialog.flags = (int)OFN_Flags.OverwritePrompt;

        if (GetSaveFileName(openSaveFileDialog))
        {
            resultPath = new string(openSaveFileDialog.file.ToCharArray());
            return true;
        }
        else
        {
            CommonDialogBox.ErrorCodes errorCode = CommonDialogBox.GetErrorCode();
            if (errorCode != CommonDialogBox.ErrorCodes.None)
                ChartEditor.Instance.errorManager.QueueErrorMessage("Error occured when bringing up the Save As file explorer. \nError Code: " + errorCode);

            resultPath = string.Empty;
            return false;
        }
    }

    static string ParseExtentionFilter(ExtensionFilter exFilter)
    {
        // "Chart files (*.chart, *.mid)\0*.chart;*.mid"
        StringBuilder sb = new StringBuilder();
        sb.Append(exFilter.name);
        sb.Append(" (");

        for (int i = 0; i < exFilter.extensions.Length; ++i)
        {
            sb.Append("*.");
            sb.Append(exFilter.extensions[i]);

            if(i < exFilter.extensions.Length - 1)
                sb.Append(", ");
        }

        sb.Append(")\0");

        for (int i = 0; i < exFilter.extensions.Length; ++i)
        {
            sb.Append("*.");
            sb.Append(exFilter.extensions[i]);

            if (i < exFilter.extensions.Length - 1)
                sb.Append(";");
        }

        return sb.ToString();
    }

    public bool OpenFolderPanel(out string resultPath)
    {
        throw new NotImplementedException();
    }

    // Copyright
    // Microsoft Corporation
    // All rights reserved

    // OpenFileDlg.cs
    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms646839(v=vs.85).aspx


//    typedef struct tagOFN { 
//      DWORD         lStructSize; 
//      HWND          hwndOwner; 
//      HINSTANCE     hInstance; 
//      LPCTSTR       lpstrFilter; 
//      LPTSTR        lpstrCustomFilter; 
//      DWORD         nMaxCustFilter; 
//      DWORD         nFilterIndex; 
//      LPTSTR        lpstrFile; 
//      DWORD         nMaxFile; 
//      LPTSTR        lpstrFileTitle; 
//      DWORD         nMaxFileTitle; 
//      LPCTSTR       lpstrInitialDir; 
//      LPCTSTR       lpstrTitle; 
//      DWORD         Flags; 
//      WORD          nFileOffset; 
//      WORD          nFileExtension; 
//      LPCTSTR       lpstrDefExt; 
//      LPARAM        lCustData; 
//      LPOFNHOOKPROC lpfnHook; 
//      LPCTSTR       lpTemplateName; 
//    #if (_WIN32_WINNT >= 0x0500)
//      void *        pvReserved;
//      DWORD         dwReserved;
//      DWORD         FlagsEx;
//    #endif // (_WIN32_WINNT >= 0x0500)
//    } OPENFILENAME, *LPOPENFILENAME; 
    

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;

        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;

        public String file = null;
        public int maxFile = 0;

        public String fileTitle = null;
        public int maxFileTitle = 0;

        public String initialDir = null;

        public String title = null;

        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;

        public String defExt = null;

        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;

        public String templateName = null;

        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }
}

#endif
