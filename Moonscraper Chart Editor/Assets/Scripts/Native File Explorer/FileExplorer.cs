using System.Runtime.InteropServices;
using System;
using System.IO;

public static class FileExplorer  {

	public static string SaveFilePanel(string filter, string defaultFileName, string defExt)
    {        
        string filename = string.Empty;
        defaultFileName = new string(defaultFileName.ToCharArray());

        string invalidFileChars = "!@#$%^&*\"\'<>\\/:";
        foreach (char c in invalidFileChars)
        {
            defaultFileName = defaultFileName.Replace(c.ToString(), "");
        }
        /*
        string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        UnityEngine.Debug.Log(invalid);
        foreach (char c in invalid)
        {
            defaultFileName = defaultFileName.Replace(c.ToString(), "");
        }*/

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
        openSaveFileDialog.flags = 0x000002;       // Overwrite warning

        if (LibWrap.GetSaveFileName(openSaveFileDialog))
            filename = openSaveFileDialog.file;
        else
            throw new System.Exception("Could not open file");
#endif
        return filename;
    }
}
