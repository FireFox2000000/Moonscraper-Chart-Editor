// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceEvent : PlaceSongObject
{
    public Event songEvent { get { return (Event)songObject; } set { songObject = value; } }
    new public EventController controller { get { return (EventController)base.controller; } set { base.controller = value; } }

    protected override void SetSongObjectAndController()
    {
        songEvent = new Event("Default", 0);

        controller = GetComponent<EventController>();
        controller.songEvent = songEvent;
    }

    protected override void AddObject()
    {
        ActionHistory.Add action;
        string debugMessage = string.Empty;

        // Add song event
        Event globalEvent = new Event(songEvent);
        editor.currentSong.Add(globalEvent);

        action = new ActionHistory.Add(globalEvent);

        debugMessage = "Added Song Event \"";

        debugMessage += globalEvent.title + "\"";
        Debug.Log(debugMessage);

        editor.actionHistory.Insert(action);
        editor.currentSelectedObject = globalEvent;
    }

    public static void AddObjectToCurrentSong(Event songEvent, ChartEditor editor, bool update = true)
    {
        Event eventToAdd = new Event(songEvent);
        editor.currentSong.Add(eventToAdd, update);
   
        editor.currentSelectedObject = eventToAdd;
    }

    protected override void Controls()
    {
        if (!Globals.lockToStrikeline)
        {
            if (Toolpane.currentTool == Toolpane.Tools.SongEvent && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
            {
                int pos = SongObject.FindObjectPosition(songEvent, editor.currentSong.events);
                if (pos == SongObject.NOTFOUND)
                {
                    //RecordAddActionHistory(chartEvent, editor.currentChart.events);

                    AddObject();
                }
                // Link to the event already in
                else
                    editor.currentSelectedObject = editor.currentSong.events[pos];
            }
        }
        else if (Input.GetButtonDown("Add Object"))
        {
            SongObject[] searchArray = editor.currentSong.events;
            int pos = SongObject.FindObjectPosition(songEvent, searchArray);
            if (pos == SongObject.NOTFOUND)
            {
                editor.actionHistory.Insert(new ActionHistory.Add(songEvent));
                AddObject();
            }
            else
            {
                editor.actionHistory.Insert(new ActionHistory.Delete(searchArray[pos]));
                searchArray[pos].Delete();
                editor.currentSelectedObject = null;
            }
        }
    }

    protected new void LateUpdate()
    {
        base.LateUpdate();

        // Re-do the controller's position setting
        Event[] events = editor.currentSong.events;

        int offset = 0;
        int index, length;
        SongObject.GetRange(events, songEvent.position, songEvent.position, out index, out length);

        // Determine the offset for the object
        for (int i = index; i < index + length; ++i)
        {
            if (events[i].GetType() != songEvent.GetType())
                continue;

            offset += EventController.OFFSET_SPACING;
        }

        transform.position = new Vector3(SongObjectController.CHART_CENTER_POS + EventController.position + offset, songEvent.worldYPosition, 0);
    }
}
