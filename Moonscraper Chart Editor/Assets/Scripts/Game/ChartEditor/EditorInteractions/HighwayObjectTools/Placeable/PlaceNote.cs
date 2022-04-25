// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

[RequireComponent(typeof(NoteController))]
public class PlaceNote : PlaceSongObject {
    public Note note { get { return (Note)songObject; } set { songObject = value; } }
    new public NoteController controller { get { return (NoteController)base.controller; } set { base.controller = value; } }

    [HideInInspector]
    public float horizontalMouseOffset = 0;

    public static bool addNoteCheck
    {
        get
        {
            return Input.GetMouseButton(0);
        }
    }

    protected override void SetSongObjectAndController()
    {
        note = new Note(0, Note.GuitarFret.Green);

        controller = GetComponent<NoteController>();
        controller.note = note;
        note.controller = controller;
    }

    protected override void Controls()
    {
        if (addNoteCheck)   // Now handled by the PlaceNoteController
        {
            //AddObject();
        }
    }

    protected override void OnEnable()
    {
        Update();
    }

    void OnDisable()
    {
        note.previous = null;
        note.next = null;
    }

    public void ExplicitUpdate()
    {
        Update();
    }

    // Update is called once per frame
    protected override void Update () {
        note.chart = editor.currentChart;
        base.Update();

        UpdateFretType();
    }

    public void UpdatePrevAndNext()
    {
        // Get previous and next note
        int pos = SongObjectHelper.FindClosestPosition(note, editor.currentChart.notes);
        //Debug.Log(pos);
        if (pos == SongObjectHelper.NOTFOUND)
        {
            note.previous = null;
            note.next = null;
        }
        else
        {
            if (note.IsOpenNote())
                UpdateOpenPrevAndNext(pos);
            else
                UpdateStandardPrevAndNext(pos);
        }
    }

    void UpdateStandardPrevAndNext(int closestNoteArrayPos)
    {
        if (editor.currentChart.notes[closestNoteArrayPos] < note)
        {
            note.previous = editor.currentChart.notes[closestNoteArrayPos];
            note.next = editor.currentChart.notes[closestNoteArrayPos].next;
        }
        else if (editor.currentChart.notes[closestNoteArrayPos] > note)
        {
            note.next = editor.currentChart.notes[closestNoteArrayPos];
            note.previous = editor.currentChart.notes[closestNoteArrayPos].previous;
        }
        else
        {
            // Found own note
            note.previous = editor.currentChart.notes[closestNoteArrayPos].previous;
            note.next = editor.currentChart.notes[closestNoteArrayPos].next;
        }
    }

    void UpdateOpenPrevAndNext(int closestNoteArrayPos)
    {
        if (editor.currentChart.notes[closestNoteArrayPos] < note)
        {
            Note previous = GetPreviousOfOpen(note.tick, editor.currentChart.notes[closestNoteArrayPos]);

            note.previous = previous;
            if (previous != null)
                note.next = GetNextOfOpen(note.tick, previous.next);
            else
                note.next = GetNextOfOpen(note.tick, editor.currentChart.notes[closestNoteArrayPos]);
        }
        else if (editor.currentChart.notes[closestNoteArrayPos] > note)
        {
            Note next = GetNextOfOpen(note.tick, editor.currentChart.notes[closestNoteArrayPos]);

            note.next = next;
            note.previous = GetPreviousOfOpen(note.tick, next.previous);
        }
        else
        {
            // Found own note
            note.previous = editor.currentChart.notes[closestNoteArrayPos].previous;
            note.next = editor.currentChart.notes[closestNoteArrayPos].next;
        }
    }

    Note GetPreviousOfOpen(uint openNotePos, Note previousNote)
    {
        if (previousNote == null || previousNote.tick != openNotePos || (!previousNote.isChord && previousNote.tick != openNotePos))
            return previousNote;
        else
            return GetPreviousOfOpen(openNotePos, previousNote.previous);
    }

    Note GetNextOfOpen(uint openNotePos, Note nextNote)
    {
        if (nextNote == null || nextNote.tick != openNotePos || (!nextNote.isChord && nextNote.tick != openNotePos))
            return nextNote;
        else
            return GetNextOfOpen(openNotePos, nextNote.next);
    }

    protected virtual void UpdateFretType()
    {
        if (!note.IsOpenNote() && editor.services.mouseMonitorSystem.world2DPosition != null)
        {
            Vector2 mousePosition = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
            mousePosition.x += horizontalMouseOffset;
            note.rawNote = XPosToNoteNumber(mousePosition.x, editor.laneInfo);
        }
    }

    public static int XPosToNoteNumber(float xPos, LaneInfo laneInfo)
    {
        if (Globals.gameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
            xPos *= -1;

        float startPos = LaneInfo.positionRangeMin;
        float endPos = LaneInfo.positionRangeMax;

        int max = laneInfo.laneCount - 1;
        float factor = (endPos - startPos) / (max);

        for (int i = 0; i < max; ++i)
        {
            float currentPosCheck = startPos + i * factor + factor / 2.0f;
            if (xPos < currentPosCheck)
                return i;
        }

        return max;
    }

    protected override void AddObject()
    {
        editor.commandStack.Push(new SongEditAdd(this.note));
        editor.selectedObjectsManager.SelectSongObject(note, editor.currentChart.notes);
    }
}
