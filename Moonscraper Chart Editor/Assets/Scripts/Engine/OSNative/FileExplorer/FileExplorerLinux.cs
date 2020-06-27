#if UNITY_STANDALONE_LINUX

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class FileExplorerLinux : IFileExplorer
{
    [DllImport("StandaloneFileBrowser")]
    private static extern IntPtr noc_file_dialog_open(int flags, [In] byte[] filters, string defaultPath, string defaultName);

    /// Create an open file dialog.
    private static int NOC_FILE_DIALOG_OPEN = 1 << 0;
    /// Create a save file dialog.
    private static int NOC_FILE_DIALOG_SAVE = 1 << 1;
    /// Open a directory.
    private static int NOC_FILE_DIALOG_DIR = 1 << 2;
    private static int NOC_FILE_DIALOG_OVERWRITE_CONFIRMATION = 1 << 3;

    public bool OpenFilePanel(ExtensionFilter filter, string defExt, out string resultPath)
    {
        resultPath = Marshal.PtrToStringAnsi(noc_file_dialog_open(
                NOC_FILE_DIALOG_OPEN,
                GetFilterFromFileExtensionList(new ExtensionFilter[] { filter, new ExtensionFilter("All Files", "*") }),
                null,
                null));

        return !string.IsNullOrEmpty(resultPath);
    }

    public bool OpenFolderPanel(out string resultPath)
    {
        resultPath = Marshal.PtrToStringAnsi(noc_file_dialog_open(
                NOC_FILE_DIALOG_DIR,
                null,
                null,
                null));

        return !string.IsNullOrEmpty(resultPath);
    }

    public bool SaveFilePanel(ExtensionFilter filter, string defaultFileName, string defExt, out string resultPath)
    {
        resultPath = Marshal.PtrToStringAnsi(noc_file_dialog_open(
                NOC_FILE_DIALOG_SAVE | NOC_FILE_DIALOG_OVERWRITE_CONFIRMATION,
                GetFilterFromFileExtensionList(new ExtensionFilter[] { filter, new ExtensionFilter("All Files", "*") }),
                null,
                $"{defaultFileName}.{defExt}"));

        return !string.IsNullOrEmpty(resultPath);
    }

    private static byte[] GetFilterFromFileExtensionList(ExtensionFilter[] extensions)
    {
        List<byte> bytes = new List<byte>();

        if (extensions == null)
        {
            return null;
        }

        foreach (var filter in extensions)
        {
            bytes.AddRange(Encoding.ASCII.GetBytes(filter.name));
            bytes.Add((byte)'\0');

            for (int i = 0; i < filter.extensions.Length; i++)
            {
                bytes.AddRange(Encoding.ASCII.GetBytes($"*.{filter.extensions[i]}"));
                if (i != (filter.extensions.Length - 1))
                    bytes.Add((byte)';');
            }

            bytes.Add((byte)'\0');
        }

        return bytes.ToArray();
    }
}

#endif
