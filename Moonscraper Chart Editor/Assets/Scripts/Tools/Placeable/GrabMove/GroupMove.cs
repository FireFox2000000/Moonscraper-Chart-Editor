// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupMove : ToolObject
{
    int anchorArrayPos = SongObjectHelper.NOTFOUND;
    SongObject[] originalSongObjects = new SongObject[0];
    SongObject[] movingSongObjects = new SongObject[0];

    List<ActionHistory.Action> bpmAnchorRecord;
    
    Vector2 initMousePos = Vector2.zero;
    uint initObjectSnappedChartPos = 0;

    // Update is called once per frame  
    protected override void Update () {
        if (Globals.applicationMode != Globals.ApplicationMode.Editor)
            return;

        if (movingSongObjects.Length > 0)
        {
            if (Input.GetMouseButtonUp(0))
                AddSongObjects();
            else
            {
                UpdateSnappedPos();

                if (Mouse.world2DPosition != null)
                {
                    Vector2 mousePosition = (Vector2)Mouse.world2DPosition;
                    int chartPosOffset = (int)(objectSnappedChartPos - initObjectSnappedChartPos);
                    if (anchorArrayPos >= 0)
                        chartPosOffset = (int)(objectSnappedChartPos - originalSongObjects[anchorArrayPos].tick);
                    //Debug.Log(anchorArrayPos);
                    bool hitStartOfChart = false;

                    // Guard for chart limit, if the offset was negative, yet the position becomes greater
                    if (movingSongObjects.Length > 0 && chartPosOffset < 0 && (uint)((int)originalSongObjects[0].tick + chartPosOffset) > originalSongObjects[0].tick)
                    {
                        hitStartOfChart = true;
                    }

                    // Update the new positions of all the notes that have been moved
                    for (int i = 0; i < movingSongObjects.Length; ++i)
                    {
                        // Alter X position
                        if ((SongObject.ID)movingSongObjects[i].classID == SongObject.ID.Note)
                        {
                            Note note = movingSongObjects[i] as Note;
                            if (!note.IsOpenNote())
                            {
                                float position = NoteController.GetXPos(0, originalSongObjects[i] as Note) + (mousePosition.x - initMousePos.x);      // Offset
                                note.rawNote = PlaceNote.XPosToNoteNumber(position);
                            }
                        }

                        // Alter chart position
                        if (!hitStartOfChart)
                            movingSongObjects[i].tick = (uint)((int)originalSongObjects[i].tick + chartPosOffset);
                        else
                        {
                            movingSongObjects[i].tick = originalSongObjects[i].tick - originalSongObjects[0].tick;
                        }

                        if (movingSongObjects[i].controller)
                            movingSongObjects[i].controller.SetDirty();
                    }
                }

                // Enable objects into the pool
                editor.songObjectPoolManager.EnableNotes(movingSongObjects.OfType<Note>().ToArray());
                editor.songObjectPoolManager.EnableSP(movingSongObjects.OfType<Starpower>().ToArray());
                editor.songObjectPoolManager.EnableChartEvents(movingSongObjects.OfType<ChartEvent>().ToArray());
                editor.songObjectPoolManager.EnableBPM(movingSongObjects.OfType<BPM>().ToArray());
                editor.songObjectPoolManager.EnableTS(movingSongObjects.OfType<TimeSignature>().ToArray());
                editor.songObjectPoolManager.EnableSections(movingSongObjects.OfType<Section>().ToArray());
                editor.songObjectPoolManager.EnableSongEvents(movingSongObjects.OfType<Event>().ToArray());              
            }
        }
	}  

    public void AddSongObjects()
    {
        List<ActionHistory.Action> record = new List<ActionHistory.Action>();
        List<ActionHistory.Action> deleteRecord = new List<ActionHistory.Action>();

        // Need to remember to undo/redo. This current will only work once object pools are implemented.
        // Check to see what the current offset is to decide how to record
        // Will also need to check for overwrites
        // All relative to the original notes

        bool moved = false;

        for (int i = 0; i < movingSongObjects.Length; ++i)
        {
            ActionHistory.Action overwriteRecord;

            if (movingSongObjects[i] != originalSongObjects[i])
            {
                moved = true;
                deleteRecord.Add(new ActionHistory.Delete(originalSongObjects[i]));
            }
            
            switch ((SongObject.ID)movingSongObjects[i].classID)
            {
                case (SongObject.ID.Note):
                    record.AddRange(PlaceNote.AddObjectToCurrentChart((Note)movingSongObjects[i], editor, false, false));     // Capping
                    break;
                case (SongObject.ID.Starpower):
                    record.AddRange(PlaceStarpower.AddObjectToCurrentChart((Starpower)movingSongObjects[i], editor, false, false));       // Capping
                    break;
                case (SongObject.ID.ChartEvent):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory(movingSongObjects[i], editor.currentChart.events);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentChart.Add((ChartEvent)movingSongObjects[i], false);
                    break;
                case (SongObject.ID.BPM):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory(movingSongObjects[i], editor.currentSong.bpms);
                    if (record != null)
                        record.Add(overwriteRecord);
                    BPM bpm = (BPM)movingSongObjects[i];
                    editor.currentSong.Add(bpm, false);
                    if (bpm.anchor != null)
                        bpm.anchor = bpm.song.LiveTickToTime(bpm.tick, bpm.song.resolution);

                    ChartEditor.GetInstance().songObjectPoolManager.SetAllPoolsDirty();
                    break;
                case (SongObject.ID.TimeSignature):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory(movingSongObjects[i], editor.currentSong.timeSignatures);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((TimeSignature)movingSongObjects[i], false);
                    break;
                case (SongObject.ID.Section):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory(movingSongObjects[i], editor.currentSong.sections);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((Section)movingSongObjects[i], false);
                    break;
                case (SongObject.ID.Event):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory(movingSongObjects[i], editor.currentSong.events);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((Event)movingSongObjects[i], false);
                    break;
                default:
                    break;
            }     
        }

        editor.currentSelectedObjects = movingSongObjects;

        if (moved)
        {
            editor.actionHistory.Insert(deleteRecord.ToArray());                // In case user removes a bpm from an anchor area
            editor.actionHistory.Insert(bpmAnchorRecord.ToArray());

            editor.currentSong.UpdateCache();
            editor.currentChart.UpdateCache();

            editor.actionHistory.Insert(record.ToArray());
            editor.actionHistory.Insert(editor.FixUpBPMAnchors().ToArray());    // In case user moves a bpm into an anchor area
        }

        editor.currentSong.UpdateCache();
        editor.currentChart.UpdateCache();

        Reset();
    }

    void Reset()
    {
        originalSongObjects = new ChartObject[0];

        foreach (SongObject songObject in movingSongObjects)
        {
            if (songObject.controller)
                songObject.controller.gameObject.SetActive(false);
        }
        movingSongObjects = new ChartObject[0];
        anchorArrayPos = SongObjectHelper.NOTFOUND;
    }

    public void SetSongObjects(SongObject songObject)
    {
        SetSongObjects(new SongObject[] { songObject }, 0);
    }

    public void SetSongObjects(SongObject[] songObjects, int anchorArrayPos, bool delete = false)
    {
        if (Mouse.world2DPosition != null)
            initMousePos = (Vector2)Mouse.world2DPosition;
        else
            initMousePos = Vector2.zero;

        editor.currentSelectedObject = null;
        Reset();

        this.anchorArrayPos = anchorArrayPos;

        originalSongObjects = songObjects;
        movingSongObjects = new SongObject[songObjects.Length];

        initObjectSnappedChartPos = objectSnappedChartPos;

        int lastNotePos = -1;
        for (int i = 0; i < songObjects.Length; ++i)
        {
            //originalSongObjects[i] = songObjects[i];
            movingSongObjects[i] = songObjects[i].Clone();

            //if (delete)
                songObjects[i].Delete(false);

            // Rebuild linked list          
            if ((SongObject.ID)songObjects[i].classID == SongObject.ID.Note)
            {
                if (lastNotePos >= 0)
                {
                    ((Note)movingSongObjects[i]).previous = ((Note)movingSongObjects[lastNotePos]);
                    ((Note)movingSongObjects[lastNotePos]).next = ((Note)movingSongObjects[i]);
                }

                lastNotePos = i;
            }

            originalSongObjects[i].song = editor.currentSong;
            movingSongObjects[i].song = editor.currentSong;

            if (originalSongObjects[i].GetType().IsSubclassOf(typeof(ChartObject)))
            {
                ((ChartObject)originalSongObjects[i]).chart = editor.currentChart;
                ((ChartObject)movingSongObjects[i]).chart = editor.currentChart;
            }
        }

        Mouse.cancel = true;
        editor.currentSong.UpdateCache();
        editor.currentChart.UpdateCache();

        bpmAnchorRecord = editor.FixUpBPMAnchors();
    }

    public override void ToolDisable()
    {
        base.ToolDisable();
        Reset();
    }
}
