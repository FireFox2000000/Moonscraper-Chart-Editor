// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

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
