using UnityEngine;
using System.Collections;

public class Eraser : ToolObject {

    public override void ToolEnable()
    {
        editor.currentSelectedObject = null;
    }
}
