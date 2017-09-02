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
}
