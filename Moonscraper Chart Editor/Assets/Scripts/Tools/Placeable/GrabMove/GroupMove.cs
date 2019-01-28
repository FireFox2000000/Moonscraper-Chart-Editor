// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupMove : ToolObject
{
    int anchorArrayPos = SongObjectHelper.NOTFOUND;
    List<SongObject> originalSongObjects = new List<SongObject>();
    List<SongObject> movingSongObjects = new List<SongObject>();

    List<Note> notesToEnable = new List<Note>();
    List<Starpower> starpowerToEnable = new List<Starpower>();
    List<ChartEvent> chartEventsToEnable = new List<ChartEvent>();
    List<BPM> bpmsToEnable = new List<BPM>();
    List<TimeSignature> timeSignaturesToEnable = new List<TimeSignature>();
    List<Section> sectionsToEnable = new List<Section>();
    List<Event> eventsToEnable = new List<Event>();

    List<ActionHistory.Action> bpmAnchorRecord;
    
    Vector2 initMousePos = Vector2.zero;
    uint initObjectSnappedChartPos = 0;

    // Update is called once per frame  
    protected override void Update () {
        if (Globals.applicationMode != Globals.ApplicationMode.Editor)
            return;

        if (movingSongObjects.Count > 0)
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
                    if (movingSongObjects.Count > 0 && chartPosOffset < 0 && (uint)((int)originalSongObjects[0].tick + chartPosOffset) > originalSongObjects[0].tick)
                    {
                        hitStartOfChart = true;
                    }

                    // Update the new positions of all the notes that have been moved
                    for (int i = 0; i < movingSongObjects.Count; ++i)
                    {
                        // Alter X position
                        if ((SongObject.ID)movingSongObjects[i].classID == SongObject.ID.Note)
                        {
                            Note note = movingSongObjects[i] as Note;
                            if (!note.IsOpenNote())
                            {
                                float position = NoteController.GetXPos(0, originalSongObjects[i] as Note) + (mousePosition.x - initMousePos.x);      // Offset
                                note.rawNote = PlaceNote.XPosToNoteNumber(position, editor.laneInfo);
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
                editor.songObjectPoolManager.EnableNotes(notesToEnable);
                editor.songObjectPoolManager.EnableSP(starpowerToEnable);
                editor.songObjectPoolManager.EnableChartEvents(chartEventsToEnable);
                editor.songObjectPoolManager.EnableBPM(bpmsToEnable);
                editor.songObjectPoolManager.EnableTS(timeSignaturesToEnable);
                editor.songObjectPoolManager.EnableSections(sectionsToEnable);
                editor.songObjectPoolManager.EnableSongEvents(eventsToEnable);              
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

        for (int i = 0; i < movingSongObjects.Count; ++i)
        {
            ActionHistory.Action overwriteRecord;

            if (movingSongObjects[i] != originalSongObjects[i])
            {
                moved = true;
                deleteRecord.Add(new ActionHistory.Delete(originalSongObjects[i]));
            }
            
            switch ((SongObject.ID)movingSongObjects[i].classID)
            {
                /*
                case (SongObject.ID.Note):
                    // #TODO
                    //record.AddRange(PlaceNote.AddObjectToCurrentChart((Note)movingSongObjects[i], editor, false, false));     // Capping
                    break;
                case (SongObject.ID.Starpower):
                    //record.AddRange(PlaceStarpower.AddObjectToCurrentChart((Starpower)movingSongObjects[i], editor, false, false));       // Capping
                    break;
                case (SongObject.ID.ChartEvent):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory((ChartEvent)movingSongObjects[i], editor.currentChart.events);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentChart.Add((ChartEvent)movingSongObjects[i], false);
                    break;
                case (SongObject.ID.BPM):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory((BPM)movingSongObjects[i], editor.currentSong.bpms);
                    if (record != null)
                        record.Add(overwriteRecord);
                    BPM bpm = (BPM)movingSongObjects[i];
                    editor.currentSong.Add(bpm, false);
                    if (bpm.anchor != null)
                        bpm.anchor = bpm.song.LiveTickToTime(bpm.tick, bpm.song.resolution);

                    ChartEditor.Instance.songObjectPoolManager.SetAllPoolsDirty();
                    break;
                case (SongObject.ID.TimeSignature):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory((TimeSignature)movingSongObjects[i], editor.currentSong.timeSignatures);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((TimeSignature)movingSongObjects[i], false);
                    break;
                case (SongObject.ID.Section):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory((Section)movingSongObjects[i], editor.currentSong.sections);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((Section)movingSongObjects[i], false);
                    break;
                case (SongObject.ID.Event):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory((Event)movingSongObjects[i], editor.currentSong.events);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((Event)movingSongObjects[i], false);
                    break;*/
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
        originalSongObjects.Clear();

        foreach (SongObject songObject in movingSongObjects)
        {
            if (songObject.controller)
                songObject.controller.gameObject.SetActive(false);
        }
        movingSongObjects.Clear();
        anchorArrayPos = SongObjectHelper.NOTFOUND;

        notesToEnable.Clear();
        starpowerToEnable.Clear();
        chartEventsToEnable.Clear();
        bpmsToEnable.Clear();
        timeSignaturesToEnable.Clear();
        sectionsToEnable.Clear();
        eventsToEnable.Clear();
    }

    public void SetSongObjects(SongObject songObject)
    {
        SetSongObjects(new SongObject[] { songObject }, 0);
    }

    public void SetSongObjects(IList<SongObject> songObjects, int anchorArrayPos, bool delete = false)
    {
        if (Mouse.world2DPosition != null)
            initMousePos = (Vector2)Mouse.world2DPosition;
        else
            initMousePos = Vector2.zero;

        Reset();

        this.anchorArrayPos = anchorArrayPos;

        originalSongObjects.Clear();
        movingSongObjects.Clear();

        originalSongObjects.AddRange(songObjects);

        initObjectSnappedChartPos = objectSnappedChartPos;

        int lastNotePos = -1;
        for (int i = 0; i < songObjects.Count; ++i)
        {
            //originalSongObjects[i] = songObjects[i];
            movingSongObjects.Add(songObjects[i].Clone());

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

        notesToEnable.AddRange(movingSongObjects.OfType<Note>());
        starpowerToEnable.AddRange(movingSongObjects.OfType<Starpower>());
        chartEventsToEnable.AddRange(movingSongObjects.OfType<ChartEvent>());
        bpmsToEnable.AddRange(movingSongObjects.OfType<BPM>());
        timeSignaturesToEnable.AddRange(movingSongObjects.OfType<TimeSignature>().ToArray());
        sectionsToEnable.AddRange(movingSongObjects.OfType<Section>().ToArray());
        eventsToEnable.AddRange(movingSongObjects.OfType<Event>().ToArray());

        editor.currentSelectedObject = null;
    }

    public override void ToolDisable()
    {
        base.ToolDisable();
        Reset();
    }
}
