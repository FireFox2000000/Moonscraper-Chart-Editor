using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;
using Event = MoonscraperChartEditor.Song.Event;

// Song view's panel for a selection of several global events (lyrics, phrase markers and the rest)
public class EventGroupSelectPanelController : MonoBehaviour
{
    public static bool HasSeveralEvents(IList<SongObject> songObjects)
    {
        int events = 0;
        foreach (SongObject songObject in songObjects)
        {
            if (songObject.classID == (int)SongObject.ID.Event && ++events > 1)
                return true;
        }

        return false;
    }

    // The new ticks of objects at the given ascending ticks, spaced evenly between the first and the last, which stay. Objects
    // sharing a tick keep sharing one and count as one position. Nothing is snapped to the grid. The arithmetic is
    // GroupSelectPanelController.EqualiseNoteSpacing's, except that the last position is the last tick: its rounding down can
    // put the last note a tick early (8 positions over 61 ticks end at 60).
    public static uint[] EqualSpacingTicks(IList<uint> ticks)
    {
        uint[] newTicks = new uint[ticks.Count];
        int positions = 0;
        for (int i = 0; i < ticks.Count; ++i)
        {
            if (i == 0 || ticks[i] > ticks[i - 1])
                ++positions;
        }

        if (positions < 2)
        {
            ticks.CopyTo(newTicks, 0);
            return newTicks;
        }

        uint startTick = ticks[0];
        uint endTick = ticks[ticks.Count - 1];
        double offsetPerPosition = (endTick - startTick) / (double)(positions - 1);

        int index = -1;
        for (int i = 0; i < ticks.Count; ++i)
        {
            if (i == 0 || ticks[i] != ticks[i - 1])
                ++index;

            newTicks[i] = index == positions - 1 ? endTick : startTick + (uint)(offsetPerPosition * index);
        }

        return newTicks;
    }

    // Spaces the selected events as GroupSelectPanelController.EqualiseNoteSpacing spaces notes. Sections, BPMs and time
    // signatures in the selection stay where they are. An event that lands on an unselected event with the same title
    // replaces it, which loses nothing, and undo restores both.
    public void EqualiseEventSpacing()
    {
        ChartEditor editor = ChartEditor.Instance;

        List<SongObject> eventsToDelete = new List<SongObject>();
        List<uint> ticks = new List<uint>();
        foreach (SongObject songObject in editor.selectedObjectsManager.currentSelectedObjects)
        {
            if (songObject.classID == (int)SongObject.ID.Event)
            {
                eventsToDelete.Add(songObject);
                ticks.Add(songObject.tick);
            }
        }

        if (eventsToDelete.Count < 2)
            return;

        uint[] newTicks = EqualSpacingTicks(ticks);
        List<SongObject> eventsToAdd = new List<SongObject>();
        for (int i = 0; i < eventsToDelete.Count; ++i)
        {
            Event newEvent = eventsToDelete[i].CloneAs<Event>();
            newEvent.tick = newTicks[i];
            eventsToAdd.Add(newEvent);
        }

        List<SongEditCommand> songEditCommands = new List<SongEditCommand>();
        songEditCommands.Add(new SongEditDelete(eventsToDelete));
        songEditCommands.Add(new SongEditAdd(eventsToAdd));
        editor.commandStack.Push(new BatchedSongEditCommand(songEditCommands));

        // Select the moved events and the other objects that were selected
        List<SongObject> selected = new List<SongObject>();
        foreach (SongObject songObject in editor.selectedObjectsManager.currentSelectedObjects)
        {
            if (songObject.classID != (int)SongObject.ID.Event)
                selected.Add(songObject);
        }
        selected.AddRange(eventsToAdd);
        selected.Sort((a, b) => a < b ? -1 : (b < a ? 1 : 0));
        editor.selectedObjectsManager.TryFindAndSelectSongObjects(selected);
    }
}
