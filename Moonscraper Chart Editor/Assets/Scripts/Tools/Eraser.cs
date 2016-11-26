using UnityEngine;
using System.Collections;

public class Eraser : ToolObject {
    protected override void AddObject()
    {

    }

    public override void ToolEnable()
    {
        editor.currentSelectedObject = null;
    }
}
