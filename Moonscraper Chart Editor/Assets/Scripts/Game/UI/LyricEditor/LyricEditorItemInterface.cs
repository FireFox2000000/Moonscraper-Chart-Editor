using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LyricEditorItemInterface : MonoBehaviour
{
    public const string c_lyricPrefix = "lyric ";
    Event lyricEvent = null;
    bool isSelected = false;
    Color initTimestampColour;

    [SerializeField]
    Text timestampText;
    [SerializeField]
    InputField lyricInputField;
    [SerializeField]
    Color selectedColor;
    [SerializeField]
    Color invalidColor;
    [SerializeField]
    Color setColor;

    public float? time
    {
        get
        {
            Event searchedLyricEvent = FindEvent();

            if (searchedLyricEvent != null)
            {
                return searchedLyricEvent.time;
            }

            return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        initTimestampColour = timestampText.color;

        Redraw();
    }

    void Redraw()
    {
        Event searchedLyricEvent = FindEvent();
        if (searchedLyricEvent != null)
        {
            lyricInputField.text = searchedLyricEvent.title.Substring(c_lyricPrefix.Length, searchedLyricEvent.title.Length - c_lyricPrefix.Length);
            timestampText.text = Utility.timeConvertion(searchedLyricEvent.time);
            timestampText.color = setColor;
        }
        else
        {
            timestampText.text = "-";
            timestampText.color = invalidColor;
        }

        if (isSelected)
        {
            timestampText.color = selectedColor;
        }
    }

    public void OnSelect()
    {
        isSelected = true;
        Redraw();
    }

    public void OnDeselect()
    {
        isSelected = false;
        Redraw();
    }

    public void SetLyric(uint tick)
    {
        List<SongEditCommand> commands = new List<SongEditCommand>();

        Event searchedLyricEvent = FindEvent();

        if (searchedLyricEvent != null)
        {
            commands.Add(new SongEditDelete(searchedLyricEvent));
        }

        Event newLyric = new Event(c_lyricPrefix + lyricInputField.text, tick);
        commands.Add(new SongEditAdd(newLyric));

        BatchedSongEditCommand batchedCommands = new BatchedSongEditCommand(commands);
        ChartEditor.Instance.commandStack.Push(batchedCommands);

        lyricEvent = newLyric;

        Redraw();
    }

    public void TryRemoveLyric()
    {
        Event searchedLyricEvent = FindEvent();

        if (searchedLyricEvent != null)
        {
            ChartEditor.Instance.commandStack.Push(new SongEditDelete(searchedLyricEvent));
            lyricEvent = null;
        }

        Redraw();
    }

    public void OnInputFieldValueChanged()
    {
        
    }

    public void OnInputFieldEndEdit()
    {
        if (lyricEvent != null)
        {
            SetLyric(lyricEvent.tick);
        }
    }

    public void SetLyricEvent(Event eventObject)
    {
        lyricEvent = eventObject;
        Redraw();
    }

    public Event FindEvent()
    {
        if (lyricEvent == null)
            return null;

        int index = SongObjectHelper.FindObjectPosition(lyricEvent, ChartEditor.Instance.currentSong.events);
        if (index != SongObjectHelper.NOTFOUND)
        {
            return ChartEditor.Instance.currentSong.events[index];
        }

        return null;
    }
}
