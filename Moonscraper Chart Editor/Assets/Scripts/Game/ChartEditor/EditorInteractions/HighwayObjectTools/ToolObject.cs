// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public abstract class ToolObject : Snapable {
    [SerializeField]
    EditorObjectToolManager.ToolID toolId = default;

    public EditorObjectToolManager.ToolID GetTool()
    {
        return toolId;
    }

    public virtual void ToolDisable()
    { }

    public virtual void ToolEnable()
    { }
}
