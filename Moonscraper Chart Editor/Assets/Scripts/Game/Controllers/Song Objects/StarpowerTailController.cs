// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class StarpowerTailController : SelectableClick {
    public StarpowerController spCon;
    public ChartEditor editor;

    void Awake()
    {
        editor = ChartEditor.Instance;
    }

    public override void OnSelectableMouseDown()
    {
        if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(1))
        {
            spCon.Reset();
            OnSelectableMouseDrag();
        }
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
