// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public class Eraser : ToolObject {

    static bool _dragging = false;
    public static bool dragging { get { return _dragging; } }
    Vector3 mouseDownPos;

    public static List<ActionHistory.Action> dragEraseHistory = new List<ActionHistory.Action>();

    public override void ToolEnable()
    {
        editor.currentSelectedObject = null;
    }

    public override void ToolDisable()
    {
        base.ToolDisable();
        _dragging = false;

        if (dragEraseHistory.Count > 0)
        {
            editor.actionHistory.Insert(dragEraseHistory.ToArray());
            dragEraseHistory.Clear();
        }
    }

    protected override void Update()
    {
        base.Update();

        _dragging = false;
        if (Input.GetMouseButtonDown(0))
            mouseDownPos = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            if (dragEraseHistory.Count > 0)
            {
                editor.actionHistory.Insert(dragEraseHistory.ToArray());
                dragEraseHistory.Clear();
            }
        }

        _dragging = (Input.GetMouseButton(0) && Input.mousePosition != mouseDownPos);   
    }
}
