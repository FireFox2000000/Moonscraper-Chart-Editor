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

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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
        if (currentEventToCustomise != originalEvent)
        {
            ActionHistory.Modify actionHistory = currentChartEvent != null
              ? new ActionHistory.Modify(originalEvent as ChartEvent, currentChartEvent)
              : new ActionHistory.Modify(originalEvent as Event, currentEvent);

            ChartEditor.GetInstance().actionHistory.Insert(actionHistory);
        }

        currentEventToCustomise = null;
        originalEvent = null;
    }

    public void EndInputEdit(string name)
    {
        UpdateEvent(name);
    }

    public void ChangedInputEdit(string name)
    {
        UpdateEvent(name);
    }

    void UpdateEvent(string name)
    {
        if (currentChartEvent != null)
        {
            currentChartEvent.eventName = name;
            if (currentChartEvent.controller)
                currentChartEvent.controller.SetDirty();
        }
        else if (currentEvent != null)
        {
            currentEvent.title = name;
            if (currentEvent.controller)
                currentEvent.controller.SetDirty();
        }
        else
        {
            Debug.LogError("Trying to update event when object is not recognised as an event");
        }

        ChartEditor.isDirty = true;
    }
}
