// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using MoonscraperChartEditor.Song;

public class SongEditAdd : SongEditCommand
{
    public SongEditAdd(IList<SongObject> songObjects) : base(songObjects)
    {
    }

    public SongEditAdd(SongObject songObject) : base(songObject)
    {
    }

    public override void InvokeSongEditCommand()
    {
        if (subActions.Count <= 0)
        {
            AddAndInvokeSubActions(songObjects, subActions, Globals.gameSettings.extendedSustainsEnabled);
        }
        else
        {
            InvokeSubActions();
        }
    }

    public override void RevokeSongEditCommand()
    {
        RevokeSubActions();
    }

    public static void AddAndInvokeSubActions(IList<SongObject> songObjects, IList<BaseAction> subActions, bool extendedSustainsEnabled)
    {
        foreach (SongObject songObject in songObjects)
        {
            AddAndInvokeSubActions(songObject, subActions, extendedSustainsEnabled);
        }
    }

    static void AddAndInvokeSubActions(SongObject songObject, IList<BaseAction> subActions, bool extendedSustainsEnabled)
    {
        switch (songObject.classID)
        {
            case ((int)SongObject.ID.Note):
                AddNote((Note)songObject, subActions, extendedSustainsEnabled);
                break;

            case ((int)SongObject.ID.Starpower):
                AddStarpower((Starpower)songObject, subActions);
                break;

            case ((int)SongObject.ID.ChartEvent):
                AddChartEvent((ChartEvent)songObject, subActions);
                break;

            case ((int)SongObject.ID.BPM):
                AddBPM((BPM)songObject, subActions);
                break;

            case ((int)SongObject.ID.Section):
                AddSection((Section)songObject, subActions);
                break;

            case ((int)SongObject.ID.TimeSignature):
                AddTimeSignature((TimeSignature)songObject, subActions);
                break;

            case ((int)SongObject.ID.Event):
                AddEvent((Event)songObject, subActions);
                break;

            default:
                UnityEngine.Debug.LogError("Unhandled songobject!");
                break;
        }

    }

    #region Object specific add functions

    static void TryRecordOverwrite<T>(T songObject, IList<T> searchObjects, IList<BaseAction> subActions) where T : SongObject
    {
        if (subActions == null)
            return;

        ChartEditor editor = ChartEditor.Instance;
        int overwriteIndex = SongObjectHelper.FindObjectPosition(songObject, searchObjects);

        if (overwriteIndex != SongObjectHelper.NOTFOUND)
        {
            AddAndInvokeSubAction(new DeleteAction(searchObjects[overwriteIndex]), subActions);
        }
    }

    static void TryRecordOverwrite(Starpower songObject, IList<ChartObject> searchObjects, IList<BaseAction> subActions)
    {
        if (subActions == null)
            return;

        ChartEditor editor = ChartEditor.Instance;
        int overwriteIndex = SongObjectHelper.FindObjectPosition(songObject, searchObjects);

        if (overwriteIndex != SongObjectHelper.NOTFOUND)
        {
            AddAndInvokeSubAction(new DeleteAction(searchObjects[overwriteIndex]), subActions);
            SetNotesDirty(songObject, searchObjects);
        }
    }

    static void AddNote(Note note, IList<BaseAction> subActions, bool extendedSustainsEnabled)
    {
        ChartEditor editor = ChartEditor.Instance;
        Chart chart = editor.currentChart;
        Song song = editor.currentSong;

        NoteFunctions.PerformPreChartInsertCorrections(note, chart, subActions, extendedSustainsEnabled);
        AddAndInvokeSubAction(new AddAction(note), subActions);

        int arrayPos = SongObjectHelper.FindObjectPosition(note, chart.chartObjects);
        if (arrayPos != SongObjectHelper.NOTFOUND)
        {
            Note justAdded = chart.chartObjects[arrayPos] as Note;
            if (justAdded == null)
                UnityEngine.Debug.LogError("Object just added was not a note");
            else
                NoteFunctions.PerformPostChartInsertCorrections(justAdded, subActions, extendedSustainsEnabled);
        }
        else
        {
            UnityEngine.Debug.LogError("Unable to find note that was just added");
        }
    }

    static void AddStarpower(Starpower sp, IList<BaseAction> subActions)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(sp, editor.currentChart.chartObjects, subActions);

        CapPrevAndNextPreInsert(sp, editor.currentChart, subActions);

        AddAndInvokeSubAction(new AddAction(sp), subActions);
    }

    static void AddChartEvent(ChartEvent chartEvent, IList<BaseAction> subActions)
    {      
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(chartEvent, editor.currentChart.chartObjects, subActions);

        AddAndInvokeSubAction(new AddAction(chartEvent), subActions);
    }

    static void AddBPM(BPM bpm, IList<BaseAction> subActions)
    {
        ChartEditor editor = ChartEditor.Instance;
        Song song = editor.currentSong;
        TryRecordOverwrite(bpm, editor.currentSong.syncTrack, subActions);

        AddAndInvokeSubAction(new AddAction(bpm), subActions);
        if (bpm.anchor != null)
        {
            int arrayPos = SongObjectHelper.FindObjectPosition(bpm, song.syncTrack);
            if (arrayPos != SongObjectHelper.NOTFOUND)
            {
                BPM justAdded = song.syncTrack[arrayPos] as BPM;
                if (justAdded == null)
                    UnityEngine.Debug.LogError("Object just added was not a bpm");
                else
                {
                    float anchorValue = justAdded.song.LiveTickToTime(justAdded.tick, justAdded.song.resolution);
                    BPM newBpm = new BPM(bpm.tick, bpm.value, anchorValue);

                    AddAndInvokeSubAction(new DeleteAction(justAdded), subActions);
                    AddAndInvokeSubAction(new AddAction(newBpm), subActions);
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Unable to find bpm that was just added");
            }
        }
    }

    static void AddTimeSignature(TimeSignature timeSignature, IList<BaseAction> subActions)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(timeSignature, editor.currentSong.syncTrack, subActions);

        AddAndInvokeSubAction(new AddAction(timeSignature), subActions);
    }

    static void AddEvent(Event songEvent, IList<BaseAction> subActions)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(songEvent, editor.currentSong.eventsAndSections, subActions);

        AddAndInvokeSubAction(new AddAction(songEvent), subActions);
    }

    static void AddSection(Section section, IList<BaseAction> subActions)
    {
        ChartEditor editor = ChartEditor.Instance;
        TryRecordOverwrite(section, editor.currentSong.eventsAndSections, subActions);

        AddAndInvokeSubAction(new AddAction(section), subActions);
    }

    #endregion

    #region Starpower Helper Functions

    public static void SetNotesDirty(Starpower sp, IList<ChartObject> notes)
    {
        int start, length;
        SongObjectHelper.GetRange(notes, sp.tick, sp.tick + sp.length, out start, out length);

        for (int i = start; i < start + length; ++i)
        {
            if (notes[i].classID == (int)SongObject.ID.Note && notes[i].controller)
                notes[i].controller.SetDirty();
        }
    }

    static void CapPrevAndNextPreInsert(Starpower sp, Chart chart, IList<BaseAction> subActions)
    {
        int arrayPos = SongObjectHelper.FindClosestPosition(sp, chart.chartObjects);

        if (arrayPos != SongObjectHelper.NOTFOUND)       // Found an object that matches
        {
            Starpower previousSp = null;
            Starpower nextSp = null;

            bool currentArrayPosIsStarpower = chart.chartObjects[arrayPos] as Starpower == null;

            // Find the previous starpower
            {
                int previousSpIndex = currentArrayPosIsStarpower ? arrayPos - 1 : arrayPos;
                while (previousSpIndex >= 0 && chart.chartObjects[previousSpIndex].tick < sp.tick)
                {
                    Starpower maybeSp = chart.chartObjects[previousSpIndex] as Starpower;
                    if (maybeSp == null)
                    {
                        --previousSpIndex;
                    }
                    else
                    {
                        previousSp = maybeSp;
                        break;
                    }
                }
            }

            // Find the next starpower
            {
                int nextSpIndex = currentArrayPosIsStarpower ? arrayPos + 1 : arrayPos;
                while (nextSpIndex < chart.chartObjects.Count && chart.chartObjects[nextSpIndex].tick > sp.tick)
                {
                    Starpower maybeSp = chart.chartObjects[nextSpIndex] as Starpower;
                    if (maybeSp == null)
                    {
                        ++nextSpIndex;
                    }
                    else
                    {
                        nextSp = maybeSp;
                        break;
                    }
                }
            }

            if (previousSp != null)
            {
                // Cap previous sp
                if (previousSp.tick + previousSp.length > sp.tick)
                {
                    uint newLength = sp.tick - previousSp.tick;
                    Starpower newSp = new Starpower(previousSp.tick, newLength, previousSp.flags);

                    AddAndInvokeSubAction(new DeleteAction(previousSp), subActions);
                    AddAndInvokeSubAction(new AddAction(newSp), subActions);
                }
            }

            if (nextSp != null)
            {
                // Cap self
                if (sp.tick + sp.length > nextSp.tick)
                {
                    sp.length = nextSp.tick - sp.tick;
                }
            }
        }
    }

    #endregion
}
