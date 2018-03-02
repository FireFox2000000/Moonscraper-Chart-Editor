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

    public static string OpenFilePanel(string filter, string defExt)
    {
        string filename = string.Empty;

#if UNITY_EDITOR
        filename = UnityEditor.EditorUtility.OpenFilePanel("Open file", "", defExt);
        if (filename == string.Empty)
            throw new Exception("Could not open file");
#else
        OpenFileName openChartFileDialog = new OpenFileName();

        openChartFileDialog.structSize = Marshal.SizeOf(openChartFileDialog);
        openChartFileDialog.filter = filter;
        openChartFileDialog.file = new String(new char[256]);
        openChartFileDialog.maxFile = openChartFileDialog.file.Length;

        openChartFileDialog.fileTitle = new String(new char[64]);
        openChartFileDialog.maxFileTitle = openChartFileDialog.fileTitle.Length;

        openChartFileDialog.initialDir = "";
        openChartFileDialog.title = "Open file";
        openChartFileDialog.defExt = defExt;

        if (LibWrap.GetOpenFileName(openChartFileDialog))
        {
            filename = openChartFileDialog.file;
        }
        else
        {
            throw new System.Exception("Could not open file");
        }
#endif

        return filename;
    }

    public static string SaveFilePanel(string filter, string defaultFileName, string defExt)
    {        
        string filename = string.Empty;
        defaultFileName = new string(defaultFileName.ToCharArray());

        string invalidFileChars = "!@#$%^&*\"\'<>\\/:";
        foreach (char c in invalidFileChars)
        {
            defaultFileName = defaultFileName.Replace(c.ToString(), "");
        }

#if UNITY_EDITOR
        filename = UnityEditor.EditorUtility.SaveFilePanel("Save as...", "", defaultFileName, defExt);
        if (filename == string.Empty)
            throw new Exception("Could not open file");
#else
        OpenFileName openSaveFileDialog = new OpenFileName();

        openSaveFileDialog.structSize = Marshal.SizeOf(openSaveFileDialog);
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

        if (LibWrap.GetSaveFileName(openSaveFileDialog))
            filename = openSaveFileDialog.file;
        else
            throw new System.Exception("Could not open file");
#endif
        return filename;
    }
}
