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
