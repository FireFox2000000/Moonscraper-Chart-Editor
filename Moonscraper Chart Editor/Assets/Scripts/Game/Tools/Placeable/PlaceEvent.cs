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
        editor.commandStack.Push(new SongEditAdd(new Event(this.songEvent)));
        editor.SelectSongObject(songEvent, editor.currentSong.events);
    }

    protected override void Controls()
    {
        if (!GameSettings.keysModeEnabled)
        {
            if (Toolpane.currentTool == Toolpane.Tools.SongEvent && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0))
            {
                int pos = SongObjectHelper.FindObjectPosition(songEvent, editor.currentSong.events);
                if (pos == SongObjectHelper.NOTFOUND)
                {
                    AddObject();
                }
                // Link to the event already in
                else
                    editor.currentSelectedObject = editor.currentSong.events[pos];
            }
        }
        else if (ShortcutInput.GetInputDown(Shortcut.AddSongObject))
        {
            var searchArray = editor.currentSong.events;
            int pos = SongObjectHelper.FindObjectPosition(songEvent, searchArray);
            if (pos == SongObjectHelper.NOTFOUND)
            {
                AddObject();
            }
            else
            {
                editor.commandStack.Push(new SongEditDelete(searchArray[pos]));
                editor.currentSelectedObject = null;
            }
        }
    }

    protected new void LateUpdate()
    {
        base.LateUpdate();

        // Re-do the controller's position setting
        var events = editor.currentSong.events;

        int offset = 0;
        int index, length;
        SongObjectHelper.GetRange(events, songEvent.tick, songEvent.tick, out index, out length);

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
