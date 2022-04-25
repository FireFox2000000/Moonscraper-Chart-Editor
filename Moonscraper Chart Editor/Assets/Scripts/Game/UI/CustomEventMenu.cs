// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class CustomEventMenu : UnityEngine.MonoBehaviour {

    SongObject currentEventToCustomise = null;
    Event currentEvent { get { return currentEventToCustomise as Event; } }
    ChartEvent currentChartEvent { get { return currentEventToCustomise as ChartEvent; } }

    [UnityEngine.SerializeField]
    InputField eventInputField;

    SongObject originalEvent;
    string eventStr = string.Empty;

    // Use this for initialization
    void Start () {
		
	}

    public void StartEdit(ChartEvent eventObject)
    {
        StartRealEdit(eventObject);
    }

    public void StartEdit(Event eventObject)
    {
        StartRealEdit(eventObject);
    }

    void StartRealEdit(SongObject eventObject)
    {
        currentEventToCustomise = eventObject;
        originalEvent = eventObject.Clone();

        eventInputField.text = currentChartEvent != null ? currentChartEvent.eventName : currentEvent.title;

        gameObject.SetActive(true);
    }

    void OnDisable()
    {
        currentEventToCustomise = null;
        originalEvent = null;
        eventStr = string.Empty;
    }

    public void EndInputEdit(string name)
    {
        if (eventStr != string.Empty)
            name = eventStr;

        UpdateEvent(name);
    }

    public void ChangedInputEdit(string name)
    {
        if (!UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
            eventStr = name;
    }

    void UpdateEvent(string name)
    {
        ChartEditor editor = ChartEditor.Instance;

        if (currentChartEvent != null)
        {
            ChartEvent newChartEvent = new ChartEvent(currentChartEvent.tick, name);
            editor.commandStack.Push(new SongEditModify<ChartEvent>(currentChartEvent, newChartEvent));
            editor.selectedObjectsManager.SelectSongObject(newChartEvent, editor.currentChart.events);

        }
        else if (currentEvent != null)
        {
            Event newEvent = new Event(name, currentEvent.tick);
            editor.commandStack.Push(new SongEditModify<Event>(currentEvent, newEvent));
            editor.selectedObjectsManager.SelectSongObject(newEvent, editor.currentSong.events);
        }
        else
        {
            UnityEngine.Debug.LogError("Trying to update event when object is not recognised as an event");
        }
    }
}
