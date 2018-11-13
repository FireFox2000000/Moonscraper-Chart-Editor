#if UNITY_STANDALONE_WIN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using Ookii.Dialogs;

public class FileExplorerWindows_gkngkc : IFileExplorer
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    public class WindowWrapper : IWin32Window
    {
        private IntPtr _hwnd;
        public WindowWrapper(IntPtr handle) { _hwnd = handle; }
        public IntPtr Handle { get { return _hwnd; } }
    }

    public bool OpenFilePanel(ExtensionFilter filter, string defExt, out string resultPath)
    {
        var fd = new VistaOpenFileDialog();
        fd.Title = "Open file";
        fd.Filter = GetFilterFromFileExtensionList(filter);
        fd.FilterIndex = 1;
        fd.Multiselect = false;

        var res = fd.ShowDialog(new WindowWrapper(GetActiveWindow()));
        var filename = res == DialogResult.OK ? fd.FileNames[0] : string.Empty;
        fd.Dispose();

        resultPath = filename;
        return !string.IsNullOrEmpty(resultPath);
    }

    public bool SaveFilePanel(ExtensionFilter filter, string defaultFileName, string defExt, out string resultPath)
    {
        var fd = new VistaSaveFileDialog();
        fd.Title = "Save As";

        var finalFilename = "";

        if (!string.IsNullOrEmpty(defaultFileName))
        {
            finalFilename += defaultFileName;
        }

        fd.FileName = finalFilename;
        fd.Filter = GetFilterFromFileExtensionList(filter);
        fd.FilterIndex = 1;
        fd.DefaultExt = defExt;
        fd.AddExtension = true;

        var res = fd.ShowDialog(new WindowWrapper(GetActiveWindow()));
        var filename = res == DialogResult.OK ? fd.FileName : "";
        fd.Dispose();
        resultPath = filename;

        return !string.IsNullOrEmpty(resultPath);
    }

    // .NET Framework FileDialog Filter format
    // https://msdn.microsoft.com/en-us/library/microsoft.win32.filedialog.filter
    private static string GetFilterFromFileExtensionList(ExtensionFilter filter)
    {
        var filterString = "";

        filterString += filter.name + "(";

        foreach (var ext in filter.extensions)
        {
            filterString += "*." + ext + ",";
        }

        filterString = filterString.Remove(filterString.Length - 1);
        filterString += ") |";

        foreach (var ext in filter.extensions)
        {
            filterString += "*." + ext + "; ";
        }

        filterString += "|";

        filterString = filterString.Remove(filterString.Length - 1);
        return filterString;
    }
}

#endif
