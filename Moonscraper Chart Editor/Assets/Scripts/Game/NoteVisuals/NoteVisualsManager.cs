// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class NoteVisualsManager : MonoBehaviour
{
    public NoteController nCon;
    public TMPro.TextMeshPro text;
    [SerializeField]
    protected bool isTool = false;
    protected Renderer noteRenderer;

    [HideInInspector]
    public VisualNoteType noteType = VisualNoteType.Strum;
    [HideInInspector]
    public Note.SpecialType specialType = Note.SpecialType.None;

    Note prevNote;

    // Use this for initialization
    protected virtual void Awake()
    {
        noteRenderer = GetComponent<Renderer>();
        ChartEditor.Instance.events.kickNoteRecolorToggledEvent.Register(UpdateVisualsCallback);
    }

    void LateUpdate()
    {
        Animate();
    }

    private void UpdateVisualsCallback()
    {
        if (nCon.note != null)
            UpdateVisuals();
    }

    public static VisualNoteType GetVisualNoteType(Note note)
    {
        VisualNoteType noteType = VisualNoteType.Strum;
        switch (note.type)
        {
            case Note.NoteType.Strum:
                if (Globals.drumMode)
                {
                    if (note.drumPad == Note.DrumPad.Kick)
                        noteType = VisualNoteType.Kick;
                    if (note.guitarFret != Note.GuitarFret.Open)
                        noteType = VisualNoteType.Tom;
                }
                else
                {
                    noteType = VisualNoteType.Strum;
                }
                break;
            case Note.NoteType.Hopo:
                noteType = VisualNoteType.Hopo;
                break;
            case Note.NoteType.Tap:
                noteType = VisualNoteType.Tap;
                break;
            case Note.NoteType.Cymbal:
                noteType = VisualNoteType.Cymbal;
                break;
            default:
                break;
        }

        if (ChartEditor.Instance.currentGameMode == Chart.GameMode.Drums)
        {
            if (noteType == VisualNoteType.Strum)
            {
                noteType = VisualNoteType.Tom;
            }
            if (Globals.gameSettings.drumsModeOptions == GameSettings.DrumModeOptions.Standard)
            {
                if (Globals.gameSettings.drumsLaneCount == 5)
                {
                    switch (note.drumPad)
                    {
                        case Note.DrumPad.Red:
                        case Note.DrumPad.Blue:
                        case Note.DrumPad.Green:
                            noteType = VisualNoteType.Tom;
                            break;
                        case Note.DrumPad.Yellow:
                        case Note.DrumPad.Orange:
                            noteType = VisualNoteType.Cymbal;
                            break;
                        default:
                            break;
                    }
                }
            }
            if (note.flags == Note.Flags.DoubleKick && Globals.gameSettings.recolorDoubleKick)
            {
                noteType = VisualNoteType.DoubleBass;
            }
        }

        return noteType;
    }

    public virtual void UpdateVisuals()
    {
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

    protected virtual void Animate() { }

    public static VisualNoteType GetTypeWithViewChange(Note note)
    {
        if (Globals.viewMode == Globals.ViewMode.Chart)
        {
            return GetVisualNoteType(note);
        }
        else
        {
            // Do this simply because the HOPO glow by itself looks pretty cool
            return VisualNoteType.Hopo;
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

    public enum VisualNoteType
    {
        Strum,
        Hopo,
        Tap,
        Cymbal,
        Tom,
        Kick,
        DoubleBass
    }
}
