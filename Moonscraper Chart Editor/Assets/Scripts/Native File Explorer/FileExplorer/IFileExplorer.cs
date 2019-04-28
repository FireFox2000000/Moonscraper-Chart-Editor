using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFileExplorer {
    bool OpenFilePanel(ExtensionFilter filter, string defExt, out string resultPath);
    bool SaveFilePanel(ExtensionFilter filter, string defaultFileName, string defExt, out string resultPath);
    bool OpenFolderPanel(out string resultPath);
}
