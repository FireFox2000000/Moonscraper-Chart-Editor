using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class DirectoryHelper {
    public static string GetMainDirectory()
    {
        return Directory.GetParent(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).Parent.ToString();
    }
}
