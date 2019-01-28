using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomEventMenu : MonoBehaviour {

    SongObject currentEventToCustomise = null;
    Event currentEvent { get { return currentEventToCustomise as Event; } }
    ChartEvent currentChartEvent { get { return currentEventToCustomise as ChartEvent; } }

    [SerializeField]
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
        if (!Input.GetKeyDown(KeyCode.Escape))
            eventStr = name;
    }

    void UpdateEvent(string name)
    {
        ChartEditor editor = ChartEditor.Instance;

        if (currentChartEvent != null)
        {
            ChartEvent newChartEvent = new ChartEvent(currentChartEvent.tick, name);
            editor.commandStack.Push(new SongEditModify<ChartEvent>(currentChartEvent, newChartEvent));
            int insertionIndex = SongObjectHelper.FindObjectPosition(newChartEvent, editor.currentChart.events);
            Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Chart event failed to be inserted?");
        }
        else if (currentEvent != null)
        {
            Event newEvent = new Event(name, currentEvent.tick);
            editor.commandStack.Push(new SongEditModify<Event>(currentEvent, newEvent));
            int insertionIndex = SongObjectHelper.FindObjectPosition(newEvent, editor.currentSong.events);
            Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Song event failed to be inserted?");
        }
        else
        {
            Debug.LogError("Trying to update event when object is not recognised as an event");
        }
    }
}
