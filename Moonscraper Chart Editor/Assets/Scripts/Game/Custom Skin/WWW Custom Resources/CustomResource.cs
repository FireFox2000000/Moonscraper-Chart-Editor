// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public abstract class CustomResource
{
    public string name { get; private set; }
    public string filepath { get; private set; }
    public UnityWebRequest www { get; protected set; }
    protected readonly string[] validExtentions;

    protected CustomResource(string name, string[] validExtentions)
    {
        this.name = name;
        this.validExtentions = validExtentions;
    }

    protected bool validateFile(Dictionary<string, string> files)
    {
        string file = string.Empty;

        if (!(files.TryGetValue(name, out file) && Utility.validateExtension(file, validExtentions)))
            return false;

        if (file != string.Empty)
        {
            filepath = "file://" + file;
            return true;
        }

        return false;
    }

    public abstract void AssignResource();
    public abstract UnityEngine.Object GetObject();
    public abstract bool InitWWW(Dictionary<string, string> files);
}
