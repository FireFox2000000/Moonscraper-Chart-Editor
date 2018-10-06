// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public abstract class CustomResource
{
    public string name { get; private set; }
    public WWW www { get; private set; }
    protected readonly string[] validExtentions;

    protected CustomResource(string name, string[] validExtentions)
    {
        this.name = name;
        this.validExtentions = validExtentions;
    }

    public bool InitWWW(Dictionary<string, string> files)
    {
        string file = string.Empty;

        if (!(files.TryGetValue(name, out file) && Utility.validateExtension(file, validExtentions)))
            return false;

        if (file != string.Empty)
        {
            www = new WWW("file://" + file);
            return true;
        }
        else
            return false;
    }

    public abstract void AssignResource();
    public abstract UnityEngine.Object GetObject();
}
