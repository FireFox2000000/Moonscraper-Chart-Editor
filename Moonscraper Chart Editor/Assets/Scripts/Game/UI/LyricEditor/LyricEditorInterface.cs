// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public class LyricEditorInterface : MonoBehaviour
{
    class LyricItem
    {
        public LyricEditorItemInterface lyricItemInterface;

        public LyricItem(LyricEditorItemInterface lyricItemInterface)
        {
            this.lyricItemInterface = lyricItemInterface;
        }
    }

    class LyricItemComparer : IComparer<LyricItem>
    {
        public int Compare(LyricItem x, LyricItem y)
        {
            float? xTime = x.lyricItemInterface.time;
            float? yTime = y.lyricItemInterface.time;
            if (xTime == null && yTime == null)
            {
                return 0;
            }
            else if (xTime == yTime)
            {
                return 0;
            }
            else if (xTime == null)
            {
                return 1;
            }
            else if (yTime == null)
            {
                return -1;
            }
            else
            {
                return (float)xTime < (float)yTime ? -1 : 1;
            }

        }
    }

    [SerializeField]
    LyricEditorItemInterface lyricItemTemplate;
    [SerializeField]
    RectTransform content;
    [SerializeField]
    float rowHeightPadding = 15;

    const int INVALID_ITEM_INDEX = -1;

    List<LyricItem> currentLyrics = new List<LyricItem>();
    int currentSelectedItemIndex = INVALID_ITEM_INDEX;
    float rowHeight;
    Vector3 templatePosition;

    // Start is called before the first frame update
    void Start()
    {
        lyricItemTemplate.gameObject.SetActive(false);
        RectTransform templateTransform = lyricItemTemplate.gameObject.GetComponent<RectTransform>();
        rowHeight = templateTransform.sizeDelta.y;
        templatePosition = templateTransform.localPosition;

        UpdateScrollContentSize();
    }

    private void OnEnable()
    {
        PopulateFromCurrentSong();
        PositionAllLyrics();
    }

    private void OnDisable()
    {
        ClearLyricObjects();
    }

    void PopulateFromCurrentSong()
    {
        Song currentSong = ChartEditor.Instance.currentSong;
        ClearLyricObjects();

        foreach(MoonscraperChartEditor.Song.Event eventObject in currentSong.events)
        {
            if (eventObject.title.StartsWith(LyricEditorItemInterface.c_lyricPrefix))
            {
                LyricEditorItemInterface newLyricInterface = GameObject.Instantiate(lyricItemTemplate, lyricItemTemplate.transform.parent).GetComponent<LyricEditorItemInterface>();
                newLyricInterface.gameObject.SetActive(true);
                var newItem = new LyricItem(newLyricInterface);
                newItem.lyricItemInterface.SetLyricEvent(eventObject);
                currentLyrics.Add(newItem);
            }
        }

        SelectLyric(INVALID_ITEM_INDEX);
    }

    void ClearLyricObjects()
    {
        foreach (LyricItem item in currentLyrics)
        {
            GameObject.Destroy(item.lyricItemInterface.gameObject);
        }

        currentLyrics.Clear();
    }

    public void Add()
    {
        LyricEditorItemInterface newLyricInterface = GameObject.Instantiate(lyricItemTemplate, lyricItemTemplate.transform.parent).GetComponent<LyricEditorItemInterface>();
        newLyricInterface.gameObject.SetActive(true);
        var newItem = new LyricItem(newLyricInterface);
        currentLyrics.Add(newItem);

        PositionAllLyrics();

        if (currentSelectedItemIndex == INVALID_ITEM_INDEX)
        {
            SelectLyric(currentLyrics[0].lyricItemInterface);
        }
    }

    public void Delete()
    {
        if (currentSelectedItemIndex == INVALID_ITEM_INDEX)
            return;

        LyricItem itemToDelete = currentLyrics[currentSelectedItemIndex];
        itemToDelete.lyricItemInterface.TryRemoveLyric();
        GameObject.Destroy(itemToDelete.lyricItemInterface.gameObject);
        currentLyrics.RemoveAt(currentSelectedItemIndex);

        // Shift position of all UI down a row
        PositionAllLyrics();

        if (currentSelectedItemIndex >= currentLyrics.Count)
        {
            SelectLyric(currentSelectedItemIndex - 1);
        }
    }

    public void Set()
    {
        if (currentSelectedItemIndex == INVALID_ITEM_INDEX)
            return;

        uint currentTick = ChartEditor.Instance.currentTickPos;
        var lyricItem = currentLyrics[currentSelectedItemIndex];
        lyricItem.lyricItemInterface.SetLyric(currentTick);

        // Sort all previous lyrics in case someone does a set at a timestamp that's before another lyric
        currentLyrics.Sort(new LyricItemComparer());

        SelectLyric(lyricItem.lyricItemInterface);

        // Invalidate all lyrics after this
        for (int i = currentSelectedItemIndex + 1; i < currentLyrics.Count; ++i)
        {
            currentLyrics[i].lyricItemInterface.TryRemoveLyric();
        }

        AdvanceSelectedLyric();

        PositionAllLyrics();
    }

    void AdvanceSelectedLyric()
    {
        if (currentSelectedItemIndex < currentLyrics.Count - 1)
        {
            SelectLyric(currentSelectedItemIndex + 1);
        }
    }

    public void SelectLyric(LyricEditorItemInterface interfaceSelected)
    {
        // Find new lyric to select
        for (int i = 0; i < currentLyrics.Count; ++i)
        {
            var lyric = currentLyrics[i];
            if (lyric.lyricItemInterface == interfaceSelected)
            {
                SelectLyric(i);
                return;
            }
        }

        currentSelectedItemIndex = INVALID_ITEM_INDEX;
    }

    public void SelectLyric(int index)
    {
        // Deselect old lyric
        if (currentSelectedItemIndex != INVALID_ITEM_INDEX && currentSelectedItemIndex < currentLyrics.Count)
        {
            currentLyrics[currentSelectedItemIndex].lyricItemInterface.OnDeselect();
        }

        currentSelectedItemIndex = index;
        if (currentSelectedItemIndex != INVALID_ITEM_INDEX)
            currentLyrics[currentSelectedItemIndex].lyricItemInterface.OnSelect();
    }

    void PositionAllLyrics()
    {
        for (int i = 0; i < currentLyrics.Count; ++i)
        {
            LyricItem item = currentLyrics[i];
            PositionLyricItem(i, item);
        }
    }

    void PositionLyricItem(int index, LyricItem item)
    {
        RectTransform transform = item.lyricItemInterface.GetComponent<RectTransform>();

        float startYPos = templatePosition.y;
        float rowOffset = (rowHeight + rowHeightPadding) * index;
        float yPos = startYPos - rowOffset;

        Vector3 position = transform.localPosition;
        position.y = yPos;
        transform.localPosition = position;

        UpdateScrollContentSize();
    }

    void UpdateScrollContentSize()
    {
        Vector2 contentSizeDelta = content.sizeDelta;
        contentSizeDelta.y = (rowHeight + rowHeightPadding) * currentLyrics.Count + (-templatePosition.y);
        content.sizeDelta = contentSizeDelta;
    }

    public void Disable()
    {
        ChartEditor.Instance.interactionMethodManager.ChangeInteraction(EditorInteractionManager.InteractionType.HighwayObjectEdit);
    }

    public void Enable()
    {
        ChartEditor.Instance.interactionMethodManager.ChangeInteraction(EditorInteractionManager.InteractionType.LyricsEditor);
    }
}
