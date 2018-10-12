#if UNITY_STANDALONE_WIN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class FileExplorerWindows : IFileExplorer
{
    [Flags]
    enum OFN_Flags
    {
        OverwritePrompt = 0x000002,
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    public bool OpenFilePanel(string filter, string defExt, out string resultPath)
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

        if (CommDlgBindings.GetOpenFileName(openChartFileDialog))
        {
            resultPath = openChartFileDialog.file;
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

    public bool SaveFilePanel(string filter, string defaultFileName, string defExt, out string resultPath)
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

        if (CommDlgBindings.GetSaveFileName(openSaveFileDialog))
        {
            resultPath = openSaveFileDialog.file;
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
}

#endif
