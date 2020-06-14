// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteVisualsManager : MonoBehaviour {
    public NoteController nCon;
    [SerializeField]
    protected bool isTool = false;
    protected Renderer noteRenderer;

    [HideInInspector]
    public Note.NoteType noteType = Note.NoteType.Strum;
    [HideInInspector]
    public Note.SpecialType specialType = Note.SpecialType.None;

    Note prevNote;

    // Use this for initialization
    protected virtual void Awake () {
        noteRenderer = GetComponent<Renderer>();
    }

    void LateUpdate()
    {        
        Animate();
    }

    public static Note.NoteType GetVisualNoteType(Note note)
    {
        Note.NoteType noteType = note.type;

        if (ChartEditor.Instance.currentGameMode == Chart.GameMode.Drums)
        {
            if (Globals.gameSettings.drumsModeOptions == GameSettings.DrumModeOptions.Standard)
            {
                noteType = Note.NoteType.Strum;
            }

            if (note.ShouldBeCulledFromLanes(ChartEditor.Instance.laneInfo))
            {
                noteType = Note.NoteType.Tap;   // Gives the user some kind of clue that this note has come from the 5th lane
            } 
        }

        return noteType;
    }

    public virtual void UpdateVisuals() {
        Note note = nCon.note;
        if (note != null)
        {
            noteType = GetVisualNoteType(note);

            // Star power?
            specialType = IsStarpower(note);

            // Update note visuals
            if (!noteRenderer)
                noteRenderer = GetComponent<Renderer>();
            noteRenderer.sortingOrder = -(int)note.tick;

            if (Globals.drumMode && note.guitarFret == Note.GuitarFret.Open)
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
        Note.SpecialType specialType = Note.SpecialType.None;

        int index = SongObjectHelper.FindClosestPositionRoundedDown(note.tick, note.chart.starPower);
        if (index != SongObjectHelper.NOTFOUND)
        {
            Starpower sp = note.chart.starPower[index];
            if (sp.tick == note.tick || (sp.tick <= note.tick && sp.tick + sp.length > note.tick))
            {
                specialType = Note.SpecialType.StarPower;
            }
        }

        return specialType;
    }
}
