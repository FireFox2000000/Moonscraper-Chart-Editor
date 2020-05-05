#if UNITY_STANDALONE_LINUX

// Based on https://github.com/gkngkc/UnityStandaloneFileBrowser/blob/master/Assets/StandaloneFileBrowser/StandaloneFileBrowserLinux.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FileExplorerLinux : IFileExplorer
{
    [DllImport("StandaloneFileBrowser")]
    private static extern void DialogInit();
    [DllImport("StandaloneFileBrowser")]
    private static extern IntPtr DialogOpenFilePanel(string title, string directory, string extension, bool multiselect);
    //[DllImport("StandaloneFileBrowser")]
    //private static extern void DialogOpenFilePanelAsync(string title, string directory, string extension, bool multiselect, AsyncCallback callback);
    [DllImport("StandaloneFileBrowser")]
    private static extern IntPtr DialogOpenFolderPanel(string title, string directory, bool multiselect);
    //[DllImport("StandaloneFileBrowser")]
    //private static extern void DialogOpenFolderPanelAsync(string title, string directory, bool multiselect, AsyncCallback callback);
    [DllImport("StandaloneFileBrowser")]
    private static extern IntPtr DialogSaveFilePanel(string title, string directory, string defaultName, string extension);
    //[DllImport("StandaloneFileBrowser")]
    //private static extern void DialogSaveFilePanelAsync(string title, string directory, string defaultName, string extension, AsyncCallback callback);

    public FileExplorerLinux()
    {
        DialogInit();
    }

    public bool OpenFilePanel(ExtensionFilter filter, string defExt, out string resultPath)
    {
        var paths = Marshal.PtrToStringAnsi(DialogOpenFilePanel(
                "Open file",
                "",
                GetFilterFromFileExtensionList(new ExtensionFilter[] { filter }),
                false));

        string[] results = paths.Split((char)28);
        resultPath = results.Length > 0 ? results[0] : string.Empty;

        return !string.IsNullOrEmpty(resultPath);
    }

    public bool OpenFolderPanel(out string resultPath)
    {
        var paths = Marshal.PtrToStringAnsi(DialogOpenFolderPanel(
                "",
                "",
                false));

        string[] results = paths.Split((char)28);
        resultPath = results.Length > 0 ? results[0] : string.Empty;

        return !string.IsNullOrEmpty(resultPath);
    }

    public bool SaveFilePanel(ExtensionFilter filter, string defaultFileName, string defExt, out string resultPath)
    {
        resultPath = Marshal.PtrToStringAnsi(DialogSaveFilePanel(
                "Save As",
                "",
                defaultFileName,
                GetFilterFromFileExtensionList(new ExtensionFilter[] { filter })
                ));

        return !string.IsNullOrEmpty(resultPath);
    }

    private static string GetFilterFromFileExtensionList(ExtensionFilter[] extensions)
    {
        if (extensions == null)
        {
            return "";
        }

        var filterString = "";
        foreach (var filter in extensions)
        {
            filterString += filter.name + ";";

            foreach (var ext in filter.extensions)
            {
                filterString += ext + ",";
            }

            filterString = filterString.Remove(filterString.Length - 1);
            filterString += "|";
        }
        filterString = filterString.Remove(filterString.Length - 1);
        return filterString;
    }
}

#endif
