using UnityEngine;
using System.Collections;

public class SustainController : SelectableClick {

    public NoteController nCon;

    public override void OnSelectableMouseDrag()
    {
        // Update sustain
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            if (Input.GetButton("ChordSelect"))
                nCon.ChordSustainDrag();
            else
                nCon.SustainDrag();
        }
    }
}
