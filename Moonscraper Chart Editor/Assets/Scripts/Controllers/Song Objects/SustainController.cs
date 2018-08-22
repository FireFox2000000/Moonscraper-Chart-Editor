// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SustainController : SelectableClick {
    public NoteController nCon;
    public SustainResources resources;
    public Skin customSkin;

    ChartEditor editor;

    LineRenderer sustainRen;

    List<Note[]> unmodifiedNotes = new List<Note[]>();

    public void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        sustainRen = GetComponent<LineRenderer>();

        if (sustainRen)
            sustainRen.sortingLayerName = "Sustains";
    }

    public override void OnSelectableMouseDown()
    {
        if (nCon.note.song != null)
        {
            if (Input.GetMouseButton(1))
            {
                OnSelectableMouseDrag();
            }
        }
    }

    public override void OnSelectableMouseDrag()
    {
        if (nCon.note != null && nCon.note.song != null)
        {
            // Update sustain
            if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
            {
                if (!GameSettings.extendedSustainsEnabled || ShortcutInput.GetInput(Shortcut.ChordSelect))
                {                 
                    if (unmodifiedNotes.Count == 0)
                    {
                        foreach (Note chordNote in nCon.note.chord)
                        {
                            unmodifiedNotes.Add(new Note[] { (Note)chordNote.Clone(), chordNote });
                        }
                    }
                    ChordSustainDrag();
                }
                else
                {
                    if (unmodifiedNotes.Count == 0)
                    {
                        unmodifiedNotes.Add(new Note[] { (Note)nCon.note.Clone(), nCon.note });
                    }
                    //unmodifiedNotes.Add(new Note[] { (Note)nCon.note.Clone(), nCon.note });
                    SustainDrag();
                }
            }
        }
    }

    public override void OnSelectableMouseUp()
    {
        if (unmodifiedNotes.Count > 0 && unmodifiedNotes[0][0].length != unmodifiedNotes[0][1].length)
        {
            List<ActionHistory.Modify> actions = new List<ActionHistory.Modify>();
            for (int i = 0; i < unmodifiedNotes.Count; ++i)
            {
                actions.Add(new ActionHistory.Modify(unmodifiedNotes[i][0], unmodifiedNotes[i][1]));
            }

            editor.actionHistory.Insert(actions.ToArray());
        }

        unmodifiedNotes.Clear();
    }

    public void UpdateSustain()
    {
        if (sustainRen)
        {          
            UpdateSustainLength();

            if (Globals.ghLiveMode)
            {
                sustainRen.sharedMaterial = resources.ghlSustainColours[nCon.note.rawNote];
            }

            else if (nCon.note.rawNote < customSkin.sustain_mats.Length)
            {
                if (customSkin.sustain_mats[(int)nCon.note.guitarFret])
                {
                    sustainRen.sharedMaterial = customSkin.sustain_mats[(int)nCon.note.guitarFret];
                }
                else
                    sustainRen.sharedMaterial = resources.sustainColours[(int)nCon.note.guitarFret];
            }
        }
    }

    public void ForwardCap()
    {
        Note note = nCon.note;
        Note nextFret;
        if (note.IsOpenNote())
            nextFret = note.next;
        else
        {
            if (GameSettings.extendedSustainsEnabled)
                nextFret = FindNextSameFretWithinSustain();
            else
                nextFret = note.nextSeperateNote;
        }

        if (nextFret != null)
        {
            if (nextFret.tick < note.tick + note.length)
                note.length = nextFret.tick - note.tick;
        }
    }

    public void UpdateSustainLength()
    {
        Note note = nCon.note;

        if (note.length != 0)
        {
            float lowerPos = note.worldYPosition;
            float higherPos = note.song.TickToWorldYPosition(note.tick + note.length);

            if (higherPos > editor.camYMax.position.y)
                higherPos = editor.camYMax.position.y;

            if (lowerPos < editor.camYMin.position.y)
                lowerPos = editor.camYMin.position.y;

            float length = higherPos - lowerPos;
            if (length < 0)
                length = 0;
            float centerYPos = (higherPos + lowerPos) / 2.0f;

            Vector3 scale = transform.localScale;
            scale.y = length;
            transform.localScale = scale;

            Vector3 position = nCon.transform.position;
            //position.y += length / 2.0f;
            position.y = centerYPos;
            transform.position = position;
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        }
    }

    void SustainDrag()
    {
        if (nCon.note.song == null || Input.GetMouseButton(0))
            return;

        ChartEditor.isDirty = true;
        nCon.note.SetSustainByPos(GetSnappedSustainPos());
    }

    void ChordSustainDrag()
    {
        if (nCon.note.song == null || Input.GetMouseButton(0))
            return;
        ChartEditor.isDirty = true;

        uint snappedPos = GetSnappedSustainPos();

        foreach (Note chordNote in nCon.note.chord)
        {
            chordNote.SetSustainByPos(snappedPos);
        }
    }

    uint GetSnappedSustainPos()
    {
        uint snappedChartPos;
        Note note = nCon.note;

        if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.TickToSnappedTick(nCon.note.song.WorldYPositionToTick(((Vector2)Mouse.world2DPosition).y), GameSettings.step, note.song.resolution);
        }
        else
        {
            snappedChartPos = Snapable.TickToSnappedTick(note.song.WorldYPositionToTick(editor.mouseYMaxLimit.position.y), GameSettings.step, note.song.resolution);
        }

        return snappedChartPos;
    }

    Note FindNextSameFretWithinSustain()
    {
        Note note = nCon.note;
        Note next = nCon.note.next;

        while (next != null)
        {
            if (next.IsOpenNote() || (next.rawNote == note.rawNote && note.tick + note.length > next.tick))
                return next;
            else if (next.tick >= note.tick + note.length)      // Stop searching early
                return null;

            next = next.next;
        }

        return null;
    }
}
