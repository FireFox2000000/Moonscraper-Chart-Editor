using UnityEngine;
using System.Collections;

public class StarpowerTailController : SelectableClick {
    public StarpowerController spCon;

    public override void OnSelectableMouseDown()
    {
        if (Input.GetMouseButton(1))
            OnSelectableMouseDrag();
    }

    public override void OnSelectableMouseDrag()
    {
        // Update sustain
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            spCon.TailDrag();
        }
    }
}
