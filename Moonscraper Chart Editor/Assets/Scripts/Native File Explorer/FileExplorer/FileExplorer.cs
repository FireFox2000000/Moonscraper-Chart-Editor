// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Runtime.InteropServices;
using System;
using System.IO;

public static class FileExplorer  {
    [Flags]
    enum OFN_Flags
    {
        OverwritePrompt = 0x000002,
    }

    public class FileExplorerExitException : Exception
    {
        public FileExplorerExitException()
        {
        }

        public FileExplorerExitException(string message)
            : base(message)
        {
        }

        public FileExplorerExitException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


    public static volatile int m_filePanelsRefCount = 0;
    public static bool filePanelActive { get { return m_filePanelsRefCount > 0; } }

    public static string OpenFilePanel(string filter, string defExt)
    {
        ++m_filePanelsRefCount;
        UnityEngine.Debug.Log("Incrementing FileExplorer ref count, new value: " + m_filePanelsRefCount);

        string filename = string.Empty;

#if UNITY_EDITOR
        filename = UnityEditor.EditorUtility.OpenFilePanel("Open file", "", defExt);
        if (filename == string.Empty)
        {
            --m_filePanelsRefCount;
            UnityEngine.Debug.Log("Decrementing FileExplorer ref count, new value: " + m_filePanelsRefCount);
            throw new FileExplorerExitException("Could not open file");
        }
#else
        UnityEngine.Debug.Log("Native file explorer: Preparing to create OpenFileName dialog");
        OpenFileName openChartFileDialog = new OpenFileName();

        UnityEngine.Debug.Log("Native file explorer: Preparing to set struct size");
        openChartFileDialog.structSize = Marshal.SizeOf(openChartFileDialog);

        openChartFileDialog.dlgOwner = ChartEditor.Instance.windowHandleManager.windowPtr;

        UnityEngine.Debug.Log("Native file explorer: Preparing to set filter");
        openChartFileDialog.filter = filter;
        UnityEngine.Debug.Log("Native file explorer: Preparing to set file array");
        openChartFileDialog.file = new String(new char[256]);
        UnityEngine.Debug.Log("Native file explorer: Preparing to set max file length");
        openChartFileDialog.maxFile = openChartFileDialog.file.Length;
        UnityEngine.Debug.Log("Native file explorer: Preparing to set file title size");
        openChartFileDialog.fileTitle = new String(new char[64]);
        UnityEngine.Debug.Log("Native file explorer: Preparing to set max file title length");
        openChartFileDialog.maxFileTitle = openChartFileDialog.fileTitle.Length;

        UnityEngine.Debug.Log("Native file explorer: Preparing to set initial directory");
        openChartFileDialog.initialDir = "";
        UnityEngine.Debug.Log("Native file explorer: Preparing to set title");
        openChartFileDialog.title = "Open file";
        UnityEngine.Debug.Log("Native file explorer: Preparing to set defExt");
        openChartFileDialog.defExt = defExt;

        if (CommDlgBindings.GetOpenFileName(openChartFileDialog))
        {
            filename = openChartFileDialog.file;
        }
        else
        {
            --m_filePanelsRefCount;
            UnityEngine.Debug.Log("Decrementing FileExplorer ref count, new value: " + m_filePanelsRefCount);

            CommonDialogBox.ErrorCodes errorCode = CommonDialogBox.GetErrorCode();
            if (errorCode != CommonDialogBox.ErrorCodes.None)
                ChartEditor.Instance.errorManager.QueueErrorMessage("Error occured when bringing up the Open File file explorer. \nError Code: " + errorCode);

            throw new FileExplorerExitException("Could not open file");
        }
#endif

        --m_filePanelsRefCount;
        UnityEngine.Debug.Log("Decrementing FileExplorer ref count, new value: " + m_filePanelsRefCount);

        return new string(filename.ToCharArray());
    }

    public static string SaveFilePanel(string filter, string defaultFileName, string defExt)
    {
        ++m_filePanelsRefCount;
        UnityEngine.Debug.Log("Incrementing FileExplorer ref count, new value: " + m_filePanelsRefCount);

        string filename = string.Empty;
        defaultFileName = new string(defaultFileName.ToCharArray());

        string invalidFileChars = "!@#$%^&*\"\'<>\\/:|?";
        foreach (char c in invalidFileChars)
        {
            defaultFileName = defaultFileName.Replace(c.ToString(), "");
        }

#if UNITY_EDITOR
        filename = UnityEditor.EditorUtility.SaveFilePanel("Save as...", "", defaultFileName, defExt);
        if (filename == string.Empty)
        {
            --m_filePanelsRefCount;
            UnityEngine.Debug.Log("Decrementing FileExplorer ref count, new value: " + m_filePanelsRefCount);
            throw new FileExplorerExitException("Could not open file");
        }
#else
        UnityEngine.Debug.Log("Native file explorer: Preparing to create OpenFileName save dialog");
        OpenFileName openSaveFileDialog = new OpenFileName();

        UnityEngine.Debug.Log("Native file explorer: Preparing to set struct size");
        openSaveFileDialog.structSize = Marshal.SizeOf(openSaveFileDialog);

        openSaveFileDialog.dlgOwner = ChartEditor.Instance.windowHandleManager.windowPtr;

        UnityEngine.Debug.Log("Native file explorer: Preparing to set filter");
        openSaveFileDialog.filter = filter;
        UnityEngine.Debug.Log("Native file explorer: Preparing to set file array");
        openSaveFileDialog.file = new String(new char[256]);
        UnityEngine.Debug.Log("Native file explorer: Preparing to set max file");
        openSaveFileDialog.maxFile = openSaveFileDialog.file.Length;

        UnityEngine.Debug.Log("Native file explorer: Preparing to set file title array");
        openSaveFileDialog.fileTitle = new String(new char[64]);
        UnityEngine.Debug.Log("Native file explorer: Preparing to set max file title");
        openSaveFileDialog.maxFileTitle = openSaveFileDialog.fileTitle.Length;

        UnityEngine.Debug.Log("Native file explorer: Preparing to set file");
        openSaveFileDialog.file = defaultFileName;

        UnityEngine.Debug.Log("Native file explorer: Preparing to set initial directory");
        openSaveFileDialog.initialDir = "";
        UnityEngine.Debug.Log("Native file explorer: Preparing to set title");
        openSaveFileDialog.title = "Save as";
        UnityEngine.Debug.Log("Native file explorer: Preparing to set defExt");
        openSaveFileDialog.defExt = defExt;
        UnityEngine.Debug.Log("Native file explorer: Preparing to set flags");
        openSaveFileDialog.flags = (int)OFN_Flags.OverwritePrompt;

        if (CommDlgBindings.GetSaveFileName(openSaveFileDialog))
        {
            filename = openSaveFileDialog.file;
        }
        else
        {
            --m_filePanelsRefCount;
            UnityEngine.Debug.Log("Decrementing FileExplorer ref count, new value: " + m_filePanelsRefCount);

            CommonDialogBox.ErrorCodes errorCode = CommonDialogBox.GetErrorCode();
            if (errorCode != CommonDialogBox.ErrorCodes.None)
                ChartEditor.Instance.errorManager.QueueErrorMessage("Error occured when bringing up the Save As file explorer. \nError Code: " + errorCode);

            throw new FileExplorerExitException("Could not open file");
        }
#endif
        --m_filePanelsRefCount;
        UnityEngine.Debug.Log("Decrementing FileExplorer ref count, new value: " + m_filePanelsRefCount);

        return new string(filename.ToCharArray());
    }
}
