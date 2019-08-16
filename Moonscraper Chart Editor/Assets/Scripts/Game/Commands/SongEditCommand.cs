using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SongEditCommand : ICommand {

    protected List<SongObject> songObjects = new List<SongObject>();
    protected bool extendedSustainsEnabled;
    public bool preExecuteEnabled = true;
    public bool postExecuteEnabled = true;
    public List<BaseAction> subActions = new List<BaseAction>();

    private List<SongEditModify<BPM>> bpmAnchorFixup = new List<SongEditModify<BPM>>();
    bool bpmAnchorFixupCommandsGenerated = false;
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
        PreExecuteUpdate();
        InvokeSongEditCommand();
        PostExecuteUpdate(true);
    }

    public void Revoke()
    {
        PreExecuteUpdate();
        RevokeSongEditCommand();
        PostExecuteUpdate(false);
    }

    public abstract void InvokeSongEditCommand();

    public abstract void RevokeSongEditCommand();

    void PreExecuteUpdate()
    {
        if (!preExecuteEnabled)
            return;

        selectedSongObjects.Clear();
        foreach (SongObject so in ChartEditor.Instance.currentSelectedObjects)
        {
            selectedSongObjects.Add(so.Clone());
        }
    }

    void PostExecuteUpdate(bool isInvoke)
    {
        if (!postExecuteEnabled)
            return;

        ChartEditor editor = ChartEditor.Instance;

        if (!bpmAnchorFixupCommandsGenerated)
        {
            GenerateFixUpBPMAnchorCommands();
        }

        if (isInvoke)
        {
            foreach (ICommand command in bpmAnchorFixup)
            {
                command.Invoke();
            }
        }
        else
        {
            foreach (ICommand command in bpmAnchorFixup)
            {
                command.Revoke();
            }
        }

        editor.currentChart.UpdateCache();
        editor.currentSong.UpdateCache();

        if (Toolpane.currentTool != Toolpane.Tools.Note)
            editor.currentSelectedObject = null;

        ChartEditor.isDirty = true;

        UndoRedoJumpInfo jumpInfo = GetUndoRedoJumpInfo();

        if (jumpInfo.IsValid)
        {
            editor.FillUndoRedoSnapInfo(jumpInfo.jumpToPos.Value, jumpInfo.viewMode);
        }

        editor.TryFindAndSelectSongObjects(selectedSongObjects);
        selectedSongObjects.Clear();
    }

    protected struct UndoRedoJumpInfo
    {
        public bool IsValid { get { return jumpToPos.HasValue; } }
        public uint? jumpToPos;
        public Globals.ViewMode viewMode;
    }

    protected virtual UndoRedoJumpInfo GetUndoRedoJumpInfo()
    {
        SongObject lowestTickSo = null;
        UndoRedoJumpInfo info = new UndoRedoJumpInfo();

        foreach (SongObject songObject in songObjects)
        {
            if (lowestTickSo == null || songObject.tick < lowestTickSo.tick)
                lowestTickSo = songObject;
        }

        if (lowestTickSo != null)
        {
            info.jumpToPos = lowestTickSo.tick;
            info.viewMode = lowestTickSo.GetType().IsSubclassOf(typeof(ChartObject)) ? Globals.ViewMode.Chart : Globals.ViewMode.Song;
        }
        else
        {
            info.jumpToPos = null;
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

    protected void SnapshotGameSettings()
    {
        extendedSustainsEnabled = GameSettings.extendedSustainsEnabled;
    }
}
