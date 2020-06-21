// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public abstract class CustomResource
{
    public string name { get; private set; }
    public string filepath { get; private set; }
    public WWW www { get; private set; }
    protected readonly string[] validExtentions;

    protected CustomResource(string name, string[] validExtentions)
    {
        this.name = name;
        this.validExtentions = validExtentions;
    }

    public virtual bool InitWWW(Dictionary<string, string> files)
    {
        string file = string.Empty;

        if (!(files.TryGetValue(name, out file) && Utility.validateExtension(file, validExtentions)))
            return false;

        if (file != string.Empty)
        {
            filepath = file;
            www = new WWW("file://" + file);
            return true;
        }
        else
            return false;
    }

    public abstract void AssignResource();
    public abstract UnityEngine.Object GetObject();
}
