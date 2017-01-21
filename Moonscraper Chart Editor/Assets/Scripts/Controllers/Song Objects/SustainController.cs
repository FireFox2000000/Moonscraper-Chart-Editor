using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SustainController : SelectableClick {

    public NoteController nCon;
    ChartEditor editor;
    SpriteRenderer sustainRen;

    List<Note[]> unmodifiedNotes = new List<Note[]>();

    public void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        sustainRen = GetComponent<SpriteRenderer>();
    }

    public override void OnSelectableMouseDown()
    {
        if (nCon.note.song != null)
        {
            if (Input.GetMouseButton(1))
                OnSelectableMouseDrag();
        }
    }

    public override void OnSelectableMouseDrag()
    {
        if (nCon.note.song != null)
        {
            // Update sustain
            if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
            {
                if (!Globals.extendedSustainsEnabled || Input.GetButton("ChordSelect"))
                {                 
                    if (unmodifiedNotes.Count == 0)
                    {
                        foreach (Note chordNote in nCon.note.GetChord())
                        {
                            unmodifiedNotes.Add(new Note[] { (Note)chordNote.Clone(), chordNote });
                        }
                    }
                    ChordSustainDrag();
                }
                else
                {
                    unmodifiedNotes.Add(new Note[] { (Note)nCon.note.Clone(), nCon.note });
                    SustainDrag();
                }
            }
        }
    }

    public override void OnSelectableMouseUp()
    {
        if (unmodifiedNotes.Count > 0 && unmodifiedNotes[0][0].sustain_length != unmodifiedNotes[0][1].sustain_length)
        {
            List<ActionHistory.Modify> actions = new List<ActionHistory.Modify>();

            foreach(Note[] notes in unmodifiedNotes)
            {
                actions.Add(new ActionHistory.Modify(notes[0], notes[1]));
            }

            editor.actionHistory.Insert(actions.ToArray());
        }

        unmodifiedNotes.Clear();
    }

    public void UpdateSustain()
    {
        ForwardCap();

        UpdateSustainLength();

        sustainRen.sharedMaterial = Globals.sustainColours[(int)nCon.note.fret_type];
    }

    public void ForwardCap()
    {
        Note note = nCon.note;
        Note nextFret;
        if (note.fret_type == Note.Fret_Type.OPEN)
            nextFret = note.next;
        else
        {
            if (Globals.extendedSustainsEnabled)
                nextFret = FindNextSameFretWithinSustain();
            else
                nextFret = note.nextSeperateNote;
        }

        if (nextFret != null)
        {
            if (nextFret.position < note.position + note.sustain_length)
                note.sustain_length = nextFret.position - note.position;
                    /*
            if (Globals.extendedSustainsEnabled)
                CapSustain(nextFret);
            else
                foreach (Note chordNote in note.GetChord())
                {
                    if (chordNote.controller != null && chordNote.controller.sustain != null)
                        chordNote.controller.sustain.CapSustain(nextFret);
                }*/
        }
    }

    public void UpdateSustainLength()
    {
        Note note = nCon.note;
        float length = note.song.ChartPositionToWorldYPosition(note.position + note.sustain_length) - note.worldYPosition;

        Vector3 scale = transform.localScale;
        scale.y = length;
        transform.localScale = scale;

        Vector3 position = nCon.transform.position;
        position.y += length / 2.0f;
        transform.position = position;

        // Cap the sustain
        Note nextFret;
        if (note.fret_type == Note.Fret_Type.OPEN)
            nextFret = note.next;
        else
            nextFret = FindNextSameFretWithinSustainExtendedCheck();
    }

    public void SustainDrag()
    {
        if (nCon.note.song == null || Input.GetMouseButton(0))
            return;

        uint snappedChartPos;
        ChartEditor.editOccurred = true;

        Note note = nCon.note;

        if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(nCon.note.song.WorldYPositionToChartPosition(((Vector2)Mouse.world2DPosition).y), Globals.step, note.song.resolution);
        }
        else
        {
            snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(note.song.WorldYPositionToChartPosition(editor.mouseYMaxLimit.position.y), Globals.step, note.song.resolution);
        }

        if (snappedChartPos > note.position)
            note.sustain_length = snappedChartPos - note.position;
        else
            note.sustain_length = 0;

        // Cap the sustain
        Note nextFret;
        if (note.fret_type == Note.Fret_Type.OPEN)
            nextFret = note.next;
        else
            nextFret = FindNextSameFretWithinSustainExtendedCheck();

        if (nextFret != null)
        {
            CapSustain(nextFret);
        }
    }

    public void CapSustain(Note cap)
    {
        Note note = nCon.note;

        // Cap sustain length
        if (cap.position <= note.position)
            note.sustain_length = 0;
        else if (note.position + note.sustain_length > cap.position)        // Sustain extends beyond cap note 
        {
            note.sustain_length = cap.position - note.position;
        }

        uint gapDis = (uint)(editor.currentSong.resolution * 4.0f / Globals.sustainGap);

        if (Globals.sustainGapEnabled && note.sustain_length > 0 && (note.position + note.sustain_length > cap.position - gapDis))
        {
            if ((int)(cap.position - gapDis - note.position) > 0)
                note.sustain_length = cap.position - gapDis - note.position;
            else
                note.sustain_length = 0;
        }
    }

    public void ChordSustainDrag()
    {
        Note[] chordNotes = nCon.note.GetChord();

        foreach (Note chordNote in chordNotes)
        {
            if (chordNote.controller != null)
            {
                chordNote.controller.sustain.SustainDrag();
            }
        }
    }

    Note FindNextSameFretWithinSustain()
    {
        Note note = nCon.note;
        Note next = nCon.note.next;

        while (next != null)
        {
            if (next.fret_type == Note.Fret_Type.OPEN || (next.fret_type == note.fret_type && note.position + note.sustain_length > next.position))
                return next;
            else if (next.position >= note.position + note.sustain_length)      // Stop searching early
                return null;

            next = next.next;
        }

        return null;
    }

    Note FindNextSameFretWithinSustainExtendedCheck()
    {
        Note note = nCon.note;
        Note next = note.next;

        while (next != null)
        {
            if (!Globals.extendedSustainsEnabled)
            {
                if (next.fret_type == Note.Fret_Type.OPEN || (note.position < next.position))
                    return next;
                //else if (next.position >= note.position + note.sustain_length)      // Stop searching early
                    //return null;
            }
            else
            {
                if (next.fret_type == Note.Fret_Type.OPEN || (next.fret_type == note.fret_type))
                    return next;
                //else if (next.position >= note.position + note.sustain_length)      // Stop searching early
                    //return null;
            }

            next = next.next;
        }

        return null;
    }
}
