// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

// Resticts so that only one fret tye can be placed down
public class PlaceFret : PlaceNote {
    [SerializeField]
    Note.Fret_Type standardFret;

    [SerializeField]
    Note.GHLive_Fret_Type ghliveFret;

    protected override void Awake()
    {
        base.Awake();

        note.fret_type = standardFret;
    }

    protected override void UpdateFretType()
    {
        note.rawNote = Globals.ghLiveMode ? (int)ghliveFret : (int)standardFret;
    }

    public override void ToolDisable()
    {
        // Don't set the current songobject, let the controller do that
    }
}
