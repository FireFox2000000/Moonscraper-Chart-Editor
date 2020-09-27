// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class NoteVisualsManager : MonoBehaviour {
    public NoteController nCon;
    public TMPro.TextMeshPro text;
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

    protected void UpdateTextDisplay(Note note)
    {
        if (!text)
            return;

        bool isDoubleKick = Globals.drumMode && note.IsOpenNote() && ((note.flags & Note.Flags.DoubleKick) != 0);
        bool culledFromLanes = note.ShouldBeCulledFromLanes(ChartEditor.Instance.laneInfo);

        bool active = isDoubleKick | culledFromLanes;

        Vector3 position = Vector3.zero;
        position.z = -0.3f;  // Places the text above the note due to rotation

        if (isDoubleKick)
        {
            position.x = -1.5f;
            text.text = "2x";
        }
        else if (culledFromLanes)
        {
            text.text = "Lane Merged";
        }

        text.transform.localPosition = position;
        text.gameObject.SetActive(active);
    }
}
