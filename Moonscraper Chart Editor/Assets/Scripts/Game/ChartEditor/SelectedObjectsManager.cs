// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedObjectsManager
{
    List<SongObject> currentSelectedObjectsList = new List<SongObject>();
    ChartEditor editor;

    public SelectedObjectsManager(ChartEditor editor)
    {
        this.editor = editor;
    }

    public SongObject currentSelectedObject
    {
        get
        {
            if (currentSelectedObjects.Count == 1)
                return currentSelectedObjects[0];
            else
                return null;
        }
        set
        {
            currentSelectedObjects.Clear();
            if (value != null)
            {
                currentSelectedObjects.Add(value);
            }

            editor.timeHandler.RefreshHighlightIndicator();
        }
    }
    
    public IList<SongObject> currentSelectedObjects
    {
        get
        {
            return currentSelectedObjectsList;
        }
        set
        {
            SetCurrentSelectedObjects(value);
        }
    }

    public void SetCurrentSelectedObjects<T>(IEnumerable<T> list) where T : SongObject
    {
        currentSelectedObjectsList.Clear();

        foreach (T so in list)
        {
            currentSelectedObjectsList.Add(so);
        }

        editor.timeHandler.RefreshHighlightIndicator();
    }

    public void SetCurrentSelectedObjects<T>(IList<T> list, int index, int length) where T : SongObject
    {
        currentSelectedObjectsList.Clear();
        for (int i = index; i < index + length; ++i)
        {
            currentSelectedObjectsList.Add(list[i]);
        }
        editor.timeHandler.RefreshHighlightIndicator();
    }

    public void AddToSelectedObjects(SongObject songObjects)
    {
        AddToSelectedObjects(new SongObject[] { songObjects });
    }

    public void AddToSelectedObjects(IEnumerable<SongObject> songObjects)
    {
        var selectedObjectsList = new List<SongObject>(currentSelectedObjects);

        foreach (SongObject songObject in songObjects)
        {
            if (!selectedObjectsList.Contains(songObject))
            {
                int pos = SongObjectHelper.FindClosestPosition(songObject, selectedObjectsList);
                if (pos != SongObjectHelper.NOTFOUND)
                {
                    if (selectedObjectsList[pos] > songObject)
                        selectedObjectsList.Insert(pos, songObject);
                    else
                        selectedObjectsList.Insert(pos + 1, songObject);
                }
                else
                    selectedObjectsList.Add(songObject);
            }
        }

        currentSelectedObjects = selectedObjectsList;
    }

    public void RemoveFromSelectedObjects(SongObject songObjects)
    {
        RemoveFromSelectedObjects(new SongObject[] { songObjects });
    }

    public void RemoveFromSelectedObjects(IEnumerable<SongObject> songObjects)
    {
        var selectedObjectsList = new List<SongObject>(currentSelectedObjects);

        foreach (SongObject songObject in songObjects)
        {
            selectedObjectsList.Remove(songObject);
        }

        currentSelectedObjects = selectedObjectsList;
    }

    public void AddOrRemoveSelectedObjects(IEnumerable<SongObject> songObjects)
    {
        var selectedObjectsList = new List<SongObject>(currentSelectedObjects);

        foreach (SongObject songObject in songObjects)
        {
            if (!selectedObjectsList.Contains(songObject))
            {
                AddToSelectedObjects(songObject);
            }
            else
            {
                RemoveFromSelectedObjects(songObject);
            }
        }
    }

    public bool IsSelected(SongObject songObject)
    {
        return (SongObjectHelper.FindObjectPosition(songObject, currentSelectedObjects) != SongObjectHelper.NOTFOUND);
    }

    public T SelectSongObject<T>(T songObject, IList<T> arrToSearch) where T : SongObject
    {
        int insertionIndex = SongObjectHelper.FindObjectPosition(songObject, arrToSearch);
        Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Failed to find songObject to highlight");
        currentSelectedObject = arrToSearch[insertionIndex];
        return currentSelectedObject as T;
    }

    List<SongObject> foundSongObjects = new List<SongObject>();
    public void TryFindAndSelectSongObjects(IList<SongObject> songObjects)
    {
        Song song = editor.currentSong;
        Chart chart = editor.currentChart;
        foundSongObjects.Clear();

        foreach (SongObject so in songObjects)
        {
            ChartObject chartObject = so as ChartObject;
            SyncTrack syncTrack = so as SyncTrack;
            Event eventObject = so as Event;
            if (chartObject != null)
            {
                int insertionIndex = SongObjectHelper.FindObjectPosition(chartObject, chart.chartObjects);
                if (insertionIndex != SongObjectHelper.NOTFOUND)
                {
                    foundSongObjects.Add(chart.chartObjects[insertionIndex]);
                }
                else
                {
                    Debug.LogWarning("Failed to find chart object to highlight");
                }
            }
            else if (syncTrack != null)
            {
                int insertionIndex = SongObjectHelper.FindObjectPosition(syncTrack, song.syncTrack);
                if (insertionIndex != SongObjectHelper.NOTFOUND)
                {
                    foundSongObjects.Add(song.syncTrack[insertionIndex]);
                }
                else
                {
                    Debug.LogWarning("Failed to find synctrack to highlight");
                }
            }
            else if (eventObject != null)
            {
                int insertionIndex = SongObjectHelper.FindObjectPosition(eventObject, song.eventsAndSections);
                if (insertionIndex != SongObjectHelper.NOTFOUND)
                {
                    foundSongObjects.Add(song.eventsAndSections[insertionIndex]);
                }
                else
                {
                    Debug.LogWarning("Failed to find event to highlight");
                }
            }
            else
            {
                Debug.LogError("Unable to handle object " + so.ToString());
            }
        }

        currentSelectedObjects = foundSongObjects;
        foundSongObjects.Clear();
    }

    public void SelectAllInView(Globals.ViewMode viewMode)
    {
        currentSelectedObject = null;

        if (viewMode == Globals.ViewMode.Chart)
        {
            currentSelectedObjects = editor.currentChart.chartObjects.ToArray();
        }
        else
        {
            currentSelectedObjects = editor.currentSong.syncTrack.ToArray();
            AddToSelectedObjects(editor.currentSong.eventsAndSections.ToArray());
        }
    }

    delegate void SongObjectSelectedManipFn(IEnumerable<SongObject> songObjects);
    public void HighlightCurrentSection(Globals.ViewMode viewMode)
    {
        currentSelectedObject = null;

        AddHighlightCurrentSection(viewMode);
    }

    public void AddHighlightCurrentSection(Globals.ViewMode viewMode, int sectionOffset = 0)
    {
        HighlightCurrentSection(AddToSelectedObjects, viewMode, sectionOffset);
    }

    public void RemoveHighlightCurrentSection(Globals.ViewMode viewMode, int sectionOffset = 0)
    {
        HighlightCurrentSection(RemoveFromSelectedObjects, viewMode, sectionOffset);
    }

    public void AddOrRemoveHighlightCurrentSection(Globals.ViewMode viewMode)
    {
        HighlightCurrentSection(AddOrRemoveSelectedObjects, viewMode);
    }

    void HighlightCurrentSection(SongObjectSelectedManipFn manipFn, Globals.ViewMode viewMode, int sectionOffset = 0)
    {
        // Get the previous and next section
        uint currentPos = editor.currentTickPos;
        var sections = editor.currentSong.sections;
        int maxSectionIndex = 0;
        while (maxSectionIndex < sections.Count && !(sections[maxSectionIndex].tick > currentPos))
        {
            ++maxSectionIndex;
        }

        maxSectionIndex += sectionOffset;

        uint rangeMin = (maxSectionIndex - 1) >= 0 ? sections[maxSectionIndex - 1].tick : 0;
        uint rangeMax = maxSectionIndex < sections.Count ? sections[maxSectionIndex].tick : uint.MaxValue;
        if (rangeMax > 0)
            --rangeMax;

        if (viewMode == Globals.ViewMode.Chart)
        {
            manipFn(SongObjectHelper.GetRangeCopy(editor.currentChart.chartObjects.ToArray(), rangeMin, rangeMax));
        }
        else
        {
            manipFn(SongObjectHelper.GetRangeCopy(editor.currentSong.syncTrack.ToArray(), rangeMin, rangeMax));
            manipFn(SongObjectHelper.GetRangeCopy(editor.currentSong.eventsAndSections.ToArray(), rangeMin, rangeMax));
        }
    }
}
