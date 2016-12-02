using UnityEngine;
using System.Collections;

// Resticts so that only one fret tye can be placed down
public class PlaceFret : PlaceNote {

    public Note.Fret_Type fret;

    protected override void Awake()
    {
        base.Awake();

        note.fret_type = fret;
    }

    protected override void UpdateFretType()
    {
        // Don't update
    }

    public override void ToolDisable()
    {
        // Don't set the current songobject, let the controller do that
    }

    void OnEnable()
    {
        // Don't set the current songobject, let the controller do that
        Update();
    }
}
