// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using MoonscraperChartEditor.Song;

public class SustainController : SelectableClick {
    public NoteController nCon;
    public SustainResources resources;

    ChartEditor editor;

    LineRenderer sustainRen;

    // This is the data we use to track command pushing and popping when adjusting the length of sustains. 
    // We keep this static to minimise memory usage as there should only be one instance of dragging at a time (chord are handled as a single instance)
    static List<Note> originalDraggedNotes = new List<Note>();
    static List<SongEditCommand> sustainDragCommands = new List<SongEditCommand>();
    static int commandPushCount = 0;
    static uint? initialDraggingSnappedPos = null;  // If this is null then we have moved the mouse cursor the minimum amount to allow sustains to be dragged. 
                                                    // This lets us right-click delete individual notes from chords without resetting the sustains of the rest of the chord. 

    public void Awake()
    {
        editor = ChartEditor.Instance;

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
                ResetSustainDragData();

                {
                    uint snappedSustainPos = GetSnappedSustainPos();
                    if (snappedSustainPos == nCon.note.tick)        // Only assigned if we're clicking on the note itself, otherwise we can modify the sustain instantly. 
                    {
                        initialDraggingSnappedPos = snappedSustainPos;
                    }
                }

                if (!Globals.gameSettings.extendedSustainsEnabled || MSChartEditorInput.GetInput(MSChartEditorInputActions.ChordSelect))
                {
                    foreach (Note chordNote in nCon.note.chord)
                    {
                        originalDraggedNotes.Add(chordNote.CloneAs<Note>());
                    }
                }
                else
                    originalDraggedNotes.Add(nCon.note.CloneAs<Note>());

                GenerateSustainDragCommands(false);
                if (sustainDragCommands.Count > 0)
                {
                    editor.commandStack.Push(new BatchedSongEditCommand(sustainDragCommands));
                    ++commandPushCount;
                }
            }
        }
    }

    public override void OnSelectableMouseDrag()
    {
        if (initialDraggingSnappedPos.HasValue && initialDraggingSnappedPos != GetSnappedSustainPos())
            initialDraggingSnappedPos = null;

        if (nCon.note != null && nCon.note.song != null)
        {
            // Update sustain
            if (editor.currentState == ChartEditor.State.Editor && Input.GetMouseButton(1))
            {
                GenerateSustainDragCommands(false);
                if (sustainDragCommands.Count > 0)
                {
                    if (commandPushCount > 0)
                    {
                        editor.commandStack.Pop();
                        --commandPushCount;
                    }

                    editor.commandStack.Push(new BatchedSongEditCommand(sustainDragCommands));
                    ++commandPushCount;
                }
            }
        }
    }

    public override void OnSelectableMouseUp()
    {      
        if (commandPushCount > 0)
        {
            GenerateSustainDragCommands(true);
            if (sustainDragCommands.Count <= 0)
            {
                // No overall change. Pop action that doesn't actually do anything.
                editor.commandStack.Pop();
                editor.commandStack.ResetTail();
                
            }

            --commandPushCount;

            Debug.Assert(commandPushCount == 0);
        }

        ResetSustainDragData();
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
            float lowerPos = ChartEditor.WorldYPosition(note);
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

    public static void ResetSustainDragData()
    {
        originalDraggedNotes.Clear();
        sustainDragCommands.Clear();
        commandPushCount = 0;
        initialDraggingSnappedPos = null;
    }

    public static bool SustainDraggingInProgress
    {
        get
        {
            return (originalDraggedNotes.Count > 0 || sustainDragCommands.Count > 0) && commandPushCount > 0;
        }
    }

    void GenerateSustainDragCommands(bool compareWithOriginal)
    {
        if (nCon.note == null || nCon.note.song == null || Input.GetMouseButton(0) || initialDraggingSnappedPos.HasValue)
            return;

        uint snappedPos = GetSnappedSustainPos();
        sustainDragCommands.Clear();

        Song song = editor.currentSong;
        bool extendedSustainsEnabled = Globals.gameSettings.extendedSustainsEnabled;
        bool commandsActuallyChangeData = false;

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

            Note lengthComparisionNote = compareWithOriginal ? note : referenceNote;
            commandsActuallyChangeData |= lengthComparisionNote.length != newNote.length;
            sustainDragCommands.Add(new SongEditModify<Note>(note, newNote));
        }

        if (!commandsActuallyChangeData)
        {
            sustainDragCommands.Clear();
        }
    }

    uint GetSnappedSustainPos()
    {
        uint snappedChartPos;
        Note note = nCon.note;

        if (editor.services.mouseMonitorSystem.world2DPosition != null && ((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.TickToSnappedTick(nCon.note.song.WorldYPositionToTick(((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y), Globals.gameSettings.step, note.song);
        }
        else
        {
            snappedChartPos = Snapable.TickToSnappedTick(note.song.WorldYPositionToTick(editor.mouseYMaxLimit.position.y), Globals.gameSettings.step, note.song);
        }

        // Cap to within the range of the song
        snappedChartPos = (uint)Mathf.Min(editor.maxPos, snappedChartPos);

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
