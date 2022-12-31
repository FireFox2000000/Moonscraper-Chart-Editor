// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Runtime.InteropServices;
using System;
using System.IO;

public static class FileExplorer  {

    static IFileExplorer m_platformWrapper = null;

    static FileExplorer()
    {
#if UNITY_EDITOR
        m_platformWrapper = new FileExplorerEditor();
#elif UNITY_STANDALONE_WIN
        m_platformWrapper = new FileExplorerWindows_gkngkc();
#elif UNITY_STANDALONE_LINUX
        m_platformWrapper = new FileExplorerLinux();
#elif UNITY_STANDALONE_OSX

#endif

        UnityEngine.Debug.Assert(m_platformWrapper != null, "Platform wrapper needs implementation!");
    }

    public static bool OpenFilePanel(ExtensionFilter filter, string defExt, out string resultPath)
    {
        return m_platformWrapper.OpenFilePanel(filter, defExt, out resultPath);
    }

    public static bool SaveFilePanel(ExtensionFilter filter, string defaultFileName, string defExt, out string resultPath)
    {
        return m_platformWrapper.SaveFilePanel(filter, defaultFileName, defExt, out resultPath);
    }

    public static bool OpenFolderPanel(out string resultPath)
    {
        return m_platformWrapper.OpenFolderPanel(out resultPath);
    }

    public static string StripIllegalChars(string filename)
    {
        const string invalidFileChars = "!@#$%^&*\"\'<>\\/:|?";
        foreach (char c in invalidFileChars)
        {
            filename = filename.Replace(c.ToString(), "");
        }

        return filename.Trim();
    }
}
