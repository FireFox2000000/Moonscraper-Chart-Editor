// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public abstract class SongEditCommand : ICommand {

    protected List<SongObject> songObjects = new List<SongObject>();
    protected bool extendedSustainsEnabled;
    public bool preExecuteEnabled = true;
    public bool postExecuteEnabled = true;
    public List<BaseAction> subActions = new List<BaseAction>();

    private List<SongEditModify<BPM>> bpmAnchorFixup = new List<SongEditModify<BPM>>();
    bool bpmAnchorFixupCommandsGenerated = false;

    private List<SongEditModify<Note>> forcedFlagFixup = new List<SongEditModify<Note>>();
    bool cannotBeForcedFixupCommandsGenerated = false;

    private List<SongObject> selectedSongObjects = new List<SongObject>();

    void AddClone(SongObject songObject)
    {
        songObjects.Add(songObject.Clone());
    }

    protected SongEditCommand()
    {
    }

    protected SongEditCommand(IList<SongObject> songObjects)
    {
        this.songObjects.Capacity = songObjects.Count;
        for (int i = 0; i < songObjects.Count; ++i)
        {
            AddClone(songObjects[i]);
        }
    }

    protected SongEditCommand(SongObject songObject) 
    {
        AddClone(songObject);
    }

    public List<SongObject> GetSongObjects()
    {
        return songObjects;
    }

    public void Invoke()
    {
        PreExecuteUpdate(true);
        InvokeSongEditCommand();
        PostExecuteUpdate(true);
    }

    public void Revoke()
    {
        PreExecuteUpdate(false);
        RevokeSongEditCommand();
        PostExecuteUpdate(false);
    }

    public abstract void InvokeSongEditCommand();

    public abstract void RevokeSongEditCommand();

    void PreExecuteUpdate(bool isInvoke)
    {
        if (!preExecuteEnabled)
            return;

        selectedSongObjects.Clear();
        foreach (SongObject so in ChartEditor.Instance.selectedObjectsManager.currentSelectedObjects)
        {
            selectedSongObjects.Add(so.Clone());
        }

        if (!isInvoke)
        {
            foreach (ICommand command in bpmAnchorFixup)
            {
                command.Revoke();
            }

            foreach (ICommand command in forcedFlagFixup)
            {
                command.Revoke();
            }
        }
    }

    void PostExecuteUpdate(bool isInvoke)
    {
        if (!postExecuteEnabled)
            return;

        ChartEditor editor = ChartEditor.Instance;
        UndoRedoJumpInfo jumpInfo = GetUndoRedoJumpInfo();

        if (!bpmAnchorFixupCommandsGenerated)
        {
            GenerateFixUpBPMAnchorCommands();
        }

        if (!cannotBeForcedFixupCommandsGenerated)
        {
            GenerateForcedFlagFixupCommands(jumpInfo);
        }

        if (isInvoke)
        {
            foreach (ICommand command in bpmAnchorFixup)
            {
                command.Invoke();
            }

            foreach (ICommand command in forcedFlagFixup)
            {
                command.Invoke();
            }
        }

        editor.currentChart.UpdateCache();
        editor.currentSong.UpdateCache();

        if (editor.toolManager.currentToolId != EditorObjectToolManager.ToolID.Note)
            editor.selectedObjectsManager.currentSelectedObject = null;

        ChartEditor.isDirty = true;

        if (jumpInfo.IsValid)
        {
            editor.FillUndoRedoSnapInfo(jumpInfo.jumpToPos.Value, jumpInfo.viewMode);
        }

        editor.selectedObjectsManager.TryFindAndSelectSongObjects(selectedSongObjects);
        selectedSongObjects.Clear();
    }

    protected struct UndoRedoJumpInfo
    {
        public bool IsValid { get { return jumpToPos.HasValue; } }
        public uint? jumpToPos;
        public uint? min;
        public uint? max;
        public Globals.ViewMode viewMode;
    }

    protected virtual UndoRedoJumpInfo GetUndoRedoJumpInfo()
    {
        SongObject lowestTickSo = null;
        SongObject highestTickSo = null;
        UndoRedoJumpInfo info = new UndoRedoJumpInfo();

        foreach (SongObject songObject in songObjects)
        {
            if (lowestTickSo == null || songObject.tick < lowestTickSo.tick)
                lowestTickSo = songObject;

            if (highestTickSo == null || songObject.tick > highestTickSo.tick)
                highestTickSo = songObject;
        }

        if (lowestTickSo != null)
        {
            info.jumpToPos = lowestTickSo.tick;
            info.viewMode = lowestTickSo.GetType().IsSubclassOf(typeof(ChartObject)) ? Globals.ViewMode.Chart : Globals.ViewMode.Song;
            info.min = lowestTickSo.tick;
        }
        else
        {
            info.jumpToPos = null;
        }

        if (highestTickSo != null)
        {
            info.max = highestTickSo.tick;
        }

        return info;
    }

    protected void InvokeSubActions()
    {
        foreach (BaseAction action in subActions)
        {
            action.Invoke();
        }
    }

    protected void RevokeSubActions()
    {
        for (int i = subActions.Count - 1; i >= 0; --i)
        {
            BaseAction action = subActions[i];
            action.Revoke();
        }
    }

    public static void AddAndInvokeSubAction(BaseAction action, IList<BaseAction> subActions)
    {
        action.Invoke();
        subActions.Add(action);
    }

    static List<BPM> tempAnchorFixupBPMs = new List<BPM>();
    static List<SyncTrack> tempAnchorFixupSynctrack = new List<SyncTrack>();
    void GenerateFixUpBPMAnchorCommands()
    {
        if (bpmAnchorFixup.Count > 0)
            return;

        Song song = ChartEditor.Instance.currentSong;
        var bpms = song.bpms;

        tempAnchorFixupBPMs.Clear();
        tempAnchorFixupSynctrack.Clear();
        foreach (BPM bpm in bpms)
        {
            BPM clone = bpm.CloneAs<BPM>();
            tempAnchorFixupBPMs.Add(clone);
            tempAnchorFixupSynctrack.Add(clone);
        }
        
        // Fix up any anchors
        for (int i = 0; i < tempAnchorFixupBPMs.Count; ++i)
        {
            if (tempAnchorFixupBPMs[i].anchor != null && i > 0)
            {
                BPM anchorBPM = tempAnchorFixupBPMs[i];
                BPM bpmToAdjust = tempAnchorFixupBPMs[i - 1];

                double deltaTime = (double)anchorBPM.anchor - Song.LiveTickToTime(bpmToAdjust.tick, song.resolution, tempAnchorFixupBPMs[0], tempAnchorFixupSynctrack);
                uint newValue = (uint)Mathf.Round((float)(TickFunctions.DisToBpm(bpmToAdjust.tick, anchorBPM.tick, deltaTime, song.resolution) * 1000.0d));

                if (deltaTime > 0 && newValue > 0)
                {
                    if (bpmToAdjust.value != newValue)
                    {
                        BPM original = bpmToAdjust.CloneAs<BPM>();
                        bpmToAdjust.value = newValue;

                        SongEditModify<BPM> command = new SongEditModify<BPM>(original, bpmToAdjust);
                        command.postExecuteEnabled = false;
                        bpmAnchorFixup.Add(command);
                    }
                }
            }
        }

        bpmAnchorFixupCommandsGenerated = true;
        tempAnchorFixupBPMs.Clear();
        tempAnchorFixupSynctrack.Clear();
    }

    void GenerateForcedFlagFixupCommands(UndoRedoJumpInfo jumpInfo)
    {
        Chart chart = ChartEditor.Instance.currentChart;
        if (chart.chartObjects.Count <= 0)
        {
            return;
        }

        int index, length;
        SongObjectHelper.GetRange(chart.chartObjects, jumpInfo.min.GetValueOrDefault(0), jumpInfo.max.GetValueOrDefault(0), out index, out length);

        Note lastCheckedNote = null;
        for (int i = index; i < index + length; ++i)
        {
            if (chart.chartObjects[i].classID == (int)SongObject.ID.Note)
            {
                Note note = chart.chartObjects[i] as Note;

                if ((note.flags & Note.Flags.Forced) != 0 && note.cannotBeForced)
                {
                    foreach (Note chordNote in note.chord)
                    {
                        Note modifiedNote = new Note(chordNote);
                        modifiedNote.flags &= ~Note.Flags.Forced;

                        SongEditModify<Note> command = new SongEditModify<Note>(chordNote, modifiedNote);
                        command.postExecuteEnabled = false;
                        forcedFlagFixup.Add(command);
                    }
                }

                lastCheckedNote = note;
            }
        }

        // Do last final check for next note that may not have been included in the range
        if (lastCheckedNote != null)
        {
            Note note = lastCheckedNote.nextSeperateNote;

            if (note != null && (note.flags & Note.Flags.Forced) != 0 && note.cannotBeForced)
            {
                foreach (Note chordNote in note.chord)
                {
                    Note modifiedNote = new Note(chordNote);
                    modifiedNote.flags &= ~Note.Flags.Forced;

                    SongEditModify<Note> command = new SongEditModify<Note>(chordNote, modifiedNote);
                    command.postExecuteEnabled = false;
                    forcedFlagFixup.Add(command);
                }
            }
        }

        // Get the note to start on, then we will traverse the linked list
        /*
        Note currentNote = null;
        for (int i = 0; i < chart.chartObjects.Count; ++i)
        {
            Note note = chart.chartObjects[i] as Note;
            if (note != null)
            {
                Note prev = note.previousSeperateNote;
                if (prev != null)
                {
                    currentNote = prev;
                }
                else
                {
                    currentNote = note;
                }
            }
        }

        while (currentNote != null)
        {
            if ((currentNote.flags & Note.Flags.Forced) != 0 && currentNote.cannotBeForced)
            {
                foreach (Note chordNote in currentNote.chord)
                {
                    Note modifiedNote = new Note(chordNote);
                    modifiedNote.flags &= ~Note.Flags.Forced;

                    SongEditModify<Note> command = new SongEditModify<Note>(chordNote, modifiedNote);
                    command.postExecuteEnabled = false;
                    forcedFlagFixup.Add(command);
                }
            }

            currentNote = currentNote.nextSeperateNote;
        }*/

        cannotBeForcedFixupCommandsGenerated = true;
    }

    protected void SnapshotGameSettings()
    {
        extendedSustainsEnabled = Globals.gameSettings.extendedSustainsEnabled;
    }
}
