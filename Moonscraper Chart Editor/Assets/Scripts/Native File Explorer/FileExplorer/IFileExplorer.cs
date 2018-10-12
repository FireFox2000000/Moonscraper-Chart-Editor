using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFileExplorer {
    bool OpenFilePanel(string filter, string defExt, out string resultPath);
    bool SaveFilePanel(string filter, string defaultFileName, string defExt, out string resultPath);
}
