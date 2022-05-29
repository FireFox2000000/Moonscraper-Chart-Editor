#if UNITY_STANDALONE_WIN

/*
 * MIT License
 * 
 * Copyright (c) 2017 Gökhan Gökçe
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using Ookii.Dialogs;
using System.IO;

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
        var filename = res == DialogResult.OK ? fd.FileName : string.Empty;
        fd.Dispose();
        resultPath = filename;

        return !string.IsNullOrEmpty(resultPath);
    }

    public bool OpenFolderPanel(out string resultPath)
    {
        var fd = new VistaFolderBrowserDialog();
        var res = fd.ShowDialog(new WindowWrapper(GetActiveWindow()));
        var filename = res == DialogResult.OK ? fd.SelectedPath : string.Empty;
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

    private static string GetDirectoryPath(string directory)
    {
        var directoryPath = Path.GetFullPath(directory);
        if (!directoryPath.EndsWith("\\"))
        {
            directoryPath += "\\";
        }
        if (Path.GetPathRoot(directoryPath) == directoryPath)
        {
            return directory;
        }
        return Path.GetDirectoryName(directoryPath) + Path.DirectorySeparatorChar;
    }
}

#endif
