// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

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
        editor.selectedObjectsManager.SelectSongObject(songEvent, editor.currentSong.events);
    }

    protected override void Controls()
    {
        if (!Globals.gameSettings.keysModeEnabled)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                int pos = SongObjectHelper.FindObjectPosition(songEvent, editor.currentSong.events);
                if (pos == SongObjectHelper.NOTFOUND)
                {
                    AddObject();
                }
                // Link to the event already in
                else
                    editor.selectedObjectsManager.currentSelectedObject = editor.currentSong.events[pos];
            }
        }
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.AddSongObject))
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
                editor.selectedObjectsManager.currentSelectedObject = null;
            }
        }
    }

    protected new void LateUpdate()
    {
        base.LateUpdate();

        // Re-do the controller's position setting
        var events = editor.currentSong.events;

        float offset = EventController.BASE_OFFSET;
        int index, length;
        SongObjectHelper.GetRange(events, songEvent.tick, songEvent.tick, out index, out length);

        // Determine the offset for the object
        for (int i = index; i < index + length; ++i)
        {
            if (events[i].GetType() != songEvent.GetType())
                continue;

            offset += EventController.OFFSET_SPACING;
        }

        transform.position = new UnityEngine.Vector3(SongObjectController.CHART_CENTER_POS + EventController.position, ChartEditor.WorldYPosition(songEvent), offset);
    }
}
