// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteVisualsManager : MonoBehaviour {
    public NoteController nCon;
    protected Renderer noteRenderer;

    [HideInInspector]
    public Note.NoteType noteType = Note.NoteType.Strum;
    [HideInInspector]
    public Note.SpecialType specialType = Note.SpecialType.NONE;

    Note prevNote;

    // Use this for initialization
    protected virtual void Awake () {
        noteRenderer = GetComponent<Renderer>();
    }

    void LateUpdate()
    {        
        Animate();
    }

    public virtual void UpdateVisuals() {
        Note note = nCon.note;
        if (note != null)
        {
            noteType = note.type;

            // Star power?
            specialType = IsStarpower(note);

            // Update note visuals
            if (!noteRenderer)
                noteRenderer = GetComponent<Renderer>();
            noteRenderer.sortingOrder = -(int)note.position;

            if (Globals.drumMode && note.guitarFret == Note.GuitarFret.OPEN)
                noteRenderer.sortingOrder -= 1;
        }
    }

    protected virtual void Animate() {}

    public static Note.NoteType GetTypeWithViewChange(Note note)
    {
        if (Globals.viewMode == Globals.ViewMode.Chart)
        {
            return note.type;
        }
        else
        {
            // Do this simply because the HOPO glow by itself looks pretty cool
            return Note.NoteType.Hopo;
        }
    }

    public static Note.SpecialType IsStarpower(Note note)
    {
        Note.SpecialType specialType = Note.SpecialType.NONE;
 
        foreach (Starpower sp in note.chart.starPower)
        {
            if (sp.position == note.position || (sp.position <= note.position && sp.position + sp.length > note.position))
            {
                specialType = Note.SpecialType.STAR_POW;
            }
            else if (sp.position > note.position)
                break;
        }

        return specialType;
    }
}
