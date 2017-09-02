using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventPropertiesPanelController : PropertiesPanelController
{
    public Event currentEvent { get { return currentSongObject as Event; } set { currentSongObject = value; } }
    public ChartEvent currentChartEvent { get { return currentSongObject as ChartEvent; } set { currentSongObject = value; } }
    public InputField eventName;
    [SerializeField]
    Button eventOptionTemplate;
    [SerializeField]
    RectTransform scrollViewContentBox;

    SongObject previous;

    bool updateInputField = true;

    void Start()
    {
        // Populate the scroll view
        for(int i = 0; i < Globals.commonEvents.Length; ++i)
        {
            Button button = Instantiate(eventOptionTemplate);
            button.transform.SetParent(scrollViewContentBox, false);

            Text text = button.GetComponentInChildren<Text>();
            text.text = Globals.commonEvents[i];

            RectTransform rectTransform = button.GetComponent<RectTransform>();
            Vector3 pos = rectTransform.localPosition;
            pos.x = scrollViewContentBox.rect.width / 2.0f;
            pos.y = -i * rectTransform.sizeDelta.y - rectTransform.sizeDelta.y / 2.0f;
            rectTransform.localPosition = pos;

            button.gameObject.SetActive(true);
        }

        scrollViewContentBox.sizeDelta = new Vector2(scrollViewContentBox.sizeDelta.x, Globals.commonEvents.Length * eventOptionTemplate.GetComponent<RectTransform>().sizeDelta.y);
    }

    protected override void Update()
    {
        base.Update();
        if ((currentChartEvent != null && currentChartEvent != previous) || currentEvent != previous)
            updateInputField = true;

        UpdateInfoDisplay();

        if (currentChartEvent != null)
            previous = currentChartEvent;
        else
            previous = currentEvent;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        currentEvent = null;
    }

    public void EndEdit(string name)
    {
        updateInputField = true;
        UpdateInfoDisplay();
    }

    public void UpdateEventName(string name)
    {
        // Make sure the user isn't editing to create 2 of the same events
        if (currentEvent != null)
        {
            string prevName = currentEvent.title;
            if (SongObject.FindObjectPosition(new Event(name, currentEvent.position), editor.currentSong.events) == SongObject.NOTFOUND)
            {
                currentEvent.title = name;
                UpdateInputFieldRecord();

                if (prevName != currentEvent.title)
                    ChartEditor.editOccurred = true;

                updateInputField = true;
            }
            else
                updateInputField = false;
        }
        else if (currentChartEvent != null)
        {
            string prevName = currentChartEvent.eventName;
            if (SongObject.FindObjectPosition(new ChartEvent(currentChartEvent.position, name), editor.currentChart.events) == SongObject.NOTFOUND)
            {
                currentChartEvent.eventName = name;
                UpdateInputFieldRecord();

                if (prevName != currentChartEvent.eventName)
                    ChartEditor.editOccurred = true;

                updateInputField = true;
            }
            else
                updateInputField = false;
        }

        UpdateInfoDisplay();
    }

    void UpdateInfoDisplay()
    {
        uint position = 0;
        string eventTitle;

        if (currentEvent != null)
        {
            position = currentEvent.position;
            eventTitle = currentEvent.title;
        }
        else if (currentChartEvent != null)
        {
            position = currentChartEvent.position;
            eventTitle = currentChartEvent.eventName;
        }
        else
            return;

        if (updateInputField)
        {
            positionText.text = "Position: " + position.ToString();
            eventName.text = eventTitle;
        }
    }
}
