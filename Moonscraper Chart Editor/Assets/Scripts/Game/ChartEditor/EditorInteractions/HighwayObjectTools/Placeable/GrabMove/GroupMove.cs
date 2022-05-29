// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoonscraperChartEditor.Song;

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
    List<MoonscraperChartEditor.Song.Event> eventsToEnable = new List<MoonscraperChartEditor.Song.Event>();

    List<SongEditCommand> fullCommands = new List<SongEditCommand>();
    SongEditDelete initialDeleteCommands;

    Vector2 initMousePos = Vector2.zero;
    uint initObjectSnappedChartPos = 0;

    public bool movementInProgress
    {
        get
        {
            return movingSongObjects.Count > 0;
        }
    }

    // Update is called once per frame  
    protected override void Update () {
        if (editor.currentState != ChartEditor.State.Editor)
            return;

        if (movingSongObjects.Count > 0)
        {
            if (Input.GetMouseButtonUp(0))
                CompleteMoveAction();
            else
            {
                UpdateSnappedPos();

                //Debug.Log("Group move: " + editor.services.mouseMonitorSystem.world2DPosition);
                if (editor.services.mouseMonitorSystem.world2DPosition != null)
                {
                    Vector2 mousePosition = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
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

    public void CompleteMoveAction()
    {
        SongEditAdd addAction = new SongEditAdd(movingSongObjects);
        fullCommands.Add(addAction);

        BatchedSongEditCommand moveAction = new BatchedSongEditCommand(fullCommands);

        editor.commandStack.Pop();
        editor.commandStack.Push(moveAction);

        editor.selectedObjectsManager.TryFindAndSelectSongObjects(movingSongObjects);

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

        initialDeleteCommands = null;
        fullCommands.Clear();
    }

    public void StartMoveAction(SongObject songObject)
    {
        StartMoveAction(new SongObject[] { songObject }, 0);
    }

    public void StartMoveAction(IList<SongObject> songObjects, int anchorArrayPos, bool delete = false)
    {
        if (editor.services.mouseMonitorSystem.world2DPosition != null)
            initMousePos = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
        else
            initMousePos = Vector2.zero;

        Reset();

        this.anchorArrayPos = anchorArrayPos;

        originalSongObjects.Clear();
        movingSongObjects.Clear();

        originalSongObjects.AddRange(songObjects);

        initObjectSnappedChartPos = objectSnappedChartPos;

        int lastNotePos = -1;
        initialDeleteCommands = new SongEditDelete(songObjects);
        fullCommands.Clear();
        fullCommands.Add(new SongEditDelete(songObjects));

        for (int i = 0; i < songObjects.Count; ++i)
        {
            movingSongObjects.Add(songObjects[i].Clone());

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

        MouseMonitor.cancel = true;

        editor.commandStack.Push(initialDeleteCommands);

        notesToEnable.AddRange(movingSongObjects.OfType<Note>());
        starpowerToEnable.AddRange(movingSongObjects.OfType<Starpower>());
        chartEventsToEnable.AddRange(movingSongObjects.OfType<ChartEvent>());
        bpmsToEnable.AddRange(movingSongObjects.OfType<BPM>());
        timeSignaturesToEnable.AddRange(movingSongObjects.OfType<TimeSignature>().ToArray());
        sectionsToEnable.AddRange(movingSongObjects.OfType<Section>().ToArray());
        eventsToEnable.AddRange(movingSongObjects.OfType<MoonscraperChartEditor.Song.Event>().ToArray());

        editor.selectedObjectsManager.currentSelectedObject = null;
        editor.events.groupMoveStart.Fire();
    }

    public override void ToolDisable()
    {
        base.ToolDisable();
        Reset();
    }
}
