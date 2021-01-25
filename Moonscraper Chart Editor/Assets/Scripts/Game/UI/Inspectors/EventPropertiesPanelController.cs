// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class EventPropertiesPanelController : PropertiesPanelController
{
    public MoonscraperChartEditor.Song.Event currentEvent { get { return currentSongObject as MoonscraperChartEditor.Song.Event; } set { currentSongObject = value; } }
    public ChartEvent currentChartEvent { get { return currentSongObject as ChartEvent; } set { currentSongObject = value; } }
    public Text eventName;
    [SerializeField]
    Text inspectorTitle;
    [SerializeField]
    Button eventOptionTemplate;
    [SerializeField]
    RectTransform scrollViewContentBox;
    [SerializeField]
    CustomEventMenu customEventMenu;
    [SerializeField]
    InputField eventNameInputField;

    SongObject previous;

    GameObject localEventButtonParent;
    GameObject globalEventButtonParent;

    bool updateInputField = true;

    void Start()
    {
        localEventButtonParent = CreateEventButtonContainer("Local event buttons");
        globalEventButtonParent = CreateEventButtonContainer("Global event buttons");

        // Populate the scroll view
        for (int i = 0; i < Globals.localEvents.Length; ++i)
            CreateEventButton(Globals.localEvents[i], i).transform.SetParent(localEventButtonParent.transform);

        for (int i = 0; i < Globals.globalEvents.Length; ++i)
            CreateEventButton(Globals.globalEvents[i], i).transform.SetParent(globalEventButtonParent.transform);

        editor.events.viewModeSwitchEvent.Register(OnViewModeSwitch);
        OnViewModeSwitch(Globals.viewMode);
    }

    GameObject CreateEventButtonContainer(string name)
    {
        GameObject container = new GameObject();
        container.name = name;

        Destroy(container.transform);

        RectTransform rectTrans = container.AddComponent<RectTransform>();       

        container.transform.SetParent(scrollViewContentBox);
        rectTrans.anchorMin = new Vector2(0, 1);
        rectTrans.anchorMax = new Vector2(0, 1);
        container.transform.localPosition = Vector3.zero;

        return container;
    }

    Button CreateEventButton(string eventTitle, int indexPosition)
    {
        Button button = Instantiate(eventOptionTemplate);
        button.transform.SetParent(scrollViewContentBox, false);

        Text text = button.GetComponentInChildren<Text>();
        text.text = eventTitle;

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        Vector3 pos = rectTransform.localPosition;
        pos.x = scrollViewContentBox.rect.width / 2.0f;
        pos.y = -indexPosition * rectTransform.sizeDelta.y - rectTransform.sizeDelta.y / 2.0f;
        rectTransform.localPosition = pos;

        button.gameObject.SetActive(true);

        return button;
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

        localEventButtonParent.SetActive(Globals.viewMode == Globals.ViewMode.Chart);
        globalEventButtonParent.SetActive(!localEventButtonParent.activeSelf);

        int eventCount = (Globals.viewMode == Globals.ViewMode.Chart) ? Globals.localEvents.Length : Globals.globalEvents.Length;
        scrollViewContentBox.sizeDelta = new Vector2(scrollViewContentBox.sizeDelta.x, eventCount * eventOptionTemplate.GetComponent<RectTransform>().sizeDelta.y);
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
            var charsToRemove = new string[] { "\"" };
            foreach (var c in charsToRemove)
            {
                name = name.Replace(c, string.Empty);
            }

            string prevName = currentEvent.title;
            if (SongObjectHelper.FindObjectPosition(new MoonscraperChartEditor.Song.Event(name, currentEvent.tick), editor.currentSong.events) == SongObjectHelper.NOTFOUND)
            {
                bool tentativeRecord, lockedRecord;
                ShouldRecordInputField(name, currentEvent.title, out tentativeRecord, out lockedRecord);

                if (!lockedRecord)
                {
                    editor.commandStack.Pop();
                }

                if (tentativeRecord || lockedRecord)
                {
                    MoonscraperChartEditor.Song.Event newEvent = new MoonscraperChartEditor.Song.Event(name, currentEvent.tick);
                    editor.commandStack.Push(new SongEditModify<MoonscraperChartEditor.Song.Event>(currentEvent, newEvent));
                    editor.selectedObjectsManager.SelectSongObject(newEvent, editor.currentSong.events);
                }
            }
        }
        else if (currentChartEvent != null)
        {
            string prevName = currentChartEvent.eventName;
            if (SongObjectHelper.FindObjectPosition(new ChartEvent(currentChartEvent.tick, name), editor.currentChart.events) == SongObjectHelper.NOTFOUND)
            {
                bool tentativeRecord, lockedRecord;
                ShouldRecordInputField(name, currentChartEvent.eventName, out tentativeRecord, out lockedRecord);

                if (!lockedRecord)
                {
                    editor.commandStack.Pop();
                }

                if (tentativeRecord || lockedRecord)
                {
                    ChartEvent newChartEvent = new ChartEvent(currentChartEvent.tick, name);
                    editor.commandStack.Push(new SongEditModify<ChartEvent>(currentChartEvent, newChartEvent));
                    editor.selectedObjectsManager.SelectSongObject(newChartEvent, editor.currentChart.events);
                }
            }
        }
    }

    void UpdateInfoDisplay()
    {
        uint position = 0;
        string eventTitle;

        if (currentEvent != null)
        {
            position = currentEvent.tick;
            eventTitle = currentEvent.title;
        }
        else if (currentChartEvent != null)
        {
            position = currentChartEvent.tick;
            eventTitle = currentChartEvent.eventName;
        }
        else
            return;

        //if (updateInputField)
        {
            positionText.text = "Position: " + position.ToString();
            eventName.text = eventTitle;
            eventNameInputField.text = eventTitle;
        }
    }

    void OnViewModeSwitch(in Globals.ViewMode viewMode)
    {
        if (viewMode == Globals.ViewMode.Chart)
            inspectorTitle.text = "Local Event";
        else if (viewMode == Globals.ViewMode.Song)
            inspectorTitle.text = "Global Event";
    }

    public void ActivateCustomEventMenu()
    {
        if (currentChartEvent != null)
            customEventMenu.StartEdit(currentChartEvent);
        else
            customEventMenu.StartEdit(currentEvent);
    }
}
