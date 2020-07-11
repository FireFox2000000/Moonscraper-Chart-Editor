#if UNITY_STANDALONE_OSX

using System;
using System.Linq;
using System.Runtime.InteropServices;

public class FileExplorerMacOS : IFileExplorer {
    [DllImport("NativeWindow")]
    private static extern IntPtr nativewindow_show_save_file_panel(IntPtr defaultDirectory, IntPtr defaultFilename, IntPtr fileExtensions);

    [DllImport("NativeWindow")]
    private static extern IntPtr nativewindow_show_open_file_panel(IntPtr defaultDirectory, IntPtr defaultFilename, IntPtr fileExtensions);

    [DllImport("NativeWindow")]
    private static extern IntPtr nativewindow_show_open_directory_panel(IntPtr defaultDirectory);

    public bool OpenFilePanel(ExtensionFilter filter, string defExt, out string resultPath)
    {
        string defaultDirectory = "";
        string defaultFileName = "";
        string fileExtensions = String.Join(",", filter.extensions);

        string path = NativeWindow_macOS.MarshalNativeUTF8ToManagedString(
            nativewindow_show_open_file_panel(
                NativeWindow_macOS.MarshalManagedStringToNativeUTF8(defaultDirectory),
                NativeWindow_macOS.MarshalManagedStringToNativeUTF8(defaultFileName),
                NativeWindow_macOS.MarshalManagedStringToNativeUTF8(fileExtensions)
            )
        );

        resultPath = path;
        return !string.IsNullOrEmpty(resultPath);
    }

    public bool SaveFilePanel(ExtensionFilter filter, string defaultFileName, string defExt, out string resultPath)
    {
        string defaultDirectory = "";
        defaultFileName = FileExplorer.StripIllegalChars(defaultFileName);
        defaultFileName = $"{defaultFileName}.{defExt}";
        string fileExtensions = String.Join(",", filter.extensions);

        string path = NativeWindow_macOS.MarshalNativeUTF8ToManagedString(
            nativewindow_show_save_file_panel(
                NativeWindow_macOS.MarshalManagedStringToNativeUTF8(defaultDirectory),
                NativeWindow_macOS.MarshalManagedStringToNativeUTF8(defaultFileName),
                NativeWindow_macOS.MarshalManagedStringToNativeUTF8(fileExtensions)
            )
        );

        resultPath = path;
        return !string.IsNullOrEmpty(resultPath);
    }

    public bool OpenFolderPanel(out string resultPath)
    {
        resultPath = string.Empty;

        string defaultDirectory = "";

        string path = NativeWindow_macOS.MarshalNativeUTF8ToManagedString(
            nativewindow_show_open_directory_panel(
                NativeWindow_macOS.MarshalManagedStringToNativeUTF8(defaultDirectory)
            )
        );

        resultPath = path;
        return !string.IsNullOrEmpty(resultPath);
    }
}

#endif
