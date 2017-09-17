// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public abstract class ToolObject : Snapable {
    [SerializeField]
    Toolpane.Tools tool;

    public Toolpane.Tools GetTool()
    {
        return tool;
    }

    public virtual void ToolDisable()
    { }

    public virtual void ToolEnable()
    { }
}
