#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileExplorerEditor : IFileExplorer {

    public bool OpenFilePanel(ExtensionFilter filter, string defExt, out string resultPath)
    {
        resultPath = string.Empty;
        resultPath = UnityEditor.EditorUtility.OpenFilePanel("Open file", "", defExt);

        return !string.IsNullOrEmpty(resultPath);
    }

    public bool SaveFilePanel(ExtensionFilter filter, string defaultFileName, string defExt, out string resultPath)
    {
        resultPath = string.Empty;

        defaultFileName = FileExplorer.StripIllegalChars(defaultFileName);
        resultPath = UnityEditor.EditorUtility.SaveFilePanel("Save as...", "", defaultFileName, defExt);

        return !string.IsNullOrEmpty(resultPath);
    }

    public bool OpenFolderPanel(out string resultPath)
    {
        resultPath = string.Empty;
        resultPath = UnityEditor.EditorUtility.OpenFolderPanel("Open folder", "", "");

        return !string.IsNullOrEmpty(resultPath);
    }
}

#endif
