// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SustainController : SelectableClick {
    public NoteController nCon;
    public SustainResources resources;

    ChartEditor editor;

    LineRenderer sustainRen;

    static List<Note> originalDraggedNotes = new List<Note>();
    static List<SongEditCommand> sustainDragCommands = new List<SongEditCommand>();

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
                originalDraggedNotes.Clear();
                sustainDragCommands.Clear();

                if (!GameSettings.extendedSustainsEnabled || ShortcutInput.GetInput(Shortcut.ChordSelect))
                {
                    foreach (Note chordNote in nCon.note.chord)
                    {
                        originalDraggedNotes.Add(chordNote.CloneAs<Note>());
                    }
                }
                else
                    originalDraggedNotes.Add(nCon.note.CloneAs<Note>());

                GenerateSustainDragCommands();
                if (sustainDragCommands.Count > 0)
                    editor.commandStack.Push(new BatchedSongEditCommand(sustainDragCommands));
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
                GenerateSustainDragCommands();
                if (sustainDragCommands.Count > 0)
                {
                    editor.commandStack.Pop();
                    editor.commandStack.Push(new BatchedSongEditCommand(sustainDragCommands));
                }
            }
        }
    }

    public override void OnSelectableMouseUp()
    {
    }

    public void UpdateSustain()
    {
        if (sustainRen)
        {          
            UpdateSustainLength();
            Skin customSkin = SkinManager.Instance.currentSkin;

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

    void GenerateSustainDragCommands()
    {
        if (nCon.note.song == null || Input.GetMouseButton(0))
            return;

        uint snappedPos = GetSnappedSustainPos();
        sustainDragCommands.Clear();

        Song song = editor.currentSong;
        bool extendedSustainsEnabled = GameSettings.extendedSustainsEnabled;

        foreach (Note note in originalDraggedNotes)
        {
            int pos = SongObjectHelper.FindObjectPosition(note, editor.currentChart.notes);

            Debug.Assert(pos != SongObjectHelper.NOTFOUND, "Was not able to find note reference in chart");
            
            Note newNote = new Note(note);

            Note referenceNote = editor.currentChart.notes[pos];
            Note capNote = referenceNote.FindNextSameFretWithinSustainExtendedCheck(extendedSustainsEnabled);
            newNote.SetSustainByPos(snappedPos, song, extendedSustainsEnabled);
            if (capNote != null)
                newNote.CapSustain(capNote, song);

            sustainDragCommands.Add(new SongEditModify<Note>(note, newNote));
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
