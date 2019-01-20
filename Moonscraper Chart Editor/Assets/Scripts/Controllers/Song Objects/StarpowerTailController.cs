// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class StarpowerTailController : SelectableClick {
    public StarpowerController spCon;
    public ChartEditor editor;

    void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    public override void OnSelectableMouseDown()
    {
        if (Input.GetMouseButton(1))
            OnSelectableMouseDrag();
    }

    public override void OnSelectableMouseDrag()
    {
        // Update sustain
        spCon.DragCheck();
    }

    public override void OnSelectableMouseUp()
    {
        spCon.OnSelectableMouseUp();
    }
}
