// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class PropertiesPanelController : UnityEngine.MonoBehaviour {
    public Text positionText;

    protected ChartEditor editor;
    protected SongObject currentSongObject;
    SongObject prevSongObject;
    SongObject prevSongObjectRef;
    string prevValue = string.Empty;

    protected bool newSongObject { get { return currentSongObject != prevSongObjectRef; } }

    ValueDirection lastKnownDirection = ValueDirection.NONE;
    bool commandRecordingInProcess = false;

    void ResetActionRecording()
    {
        lastKnownDirection = ValueDirection.NONE;
        prevValue = string.Empty;
        prevSongObject = null;
        prevSongObjectRef = null;
        commandRecordingInProcess = false;
    }

    void Awake()
    {
        editor = ChartEditor.Instance;
    }

    protected virtual void OnDisable()
    {
        ResetActionRecording();
    }

    protected virtual void Update()
    {
        if (currentSongObject == null || prevSongObject != currentSongObject)
        {
            // Fucking awful but it works. Basically a work-around due to how you can no longer use != to compare between events because they compare the strings of their titles.
            if (!(currentSongObject != null && prevSongObject != null && currentSongObject.GetType() == prevSongObjectRef.GetType() &&
                ReferenceEquals(prevSongObjectRef, currentSongObject)))
                ResetActionRecording();
        }
    }

    private void LateUpdate()
    {
        prevSongObject = currentSongObject.Clone();
        prevSongObjectRef = currentSongObject;
    }

    protected void ShouldRecordInputField(string newLabel, string oldLabel, out bool tentativeRecord, out bool lockedRecord, bool ignoreEmptyLabels = false)
    {
        tentativeRecord = false;
        lockedRecord = false;

        if (ignoreEmptyLabels && string.IsNullOrEmpty(newLabel))
            return;

        if (currentSongObject == null || prevSongObject == null || prevSongObject != currentSongObject)
        {
            bool sameType = prevSongObject != null && currentSongObject.GetType() == prevSongObject.GetType();
            if (!sameType)
            {
                return;
            }
        }

        string value = newLabel;

        if (value.Equals(oldLabel))
            return;

        // Check if there's a record already in
        if (!commandRecordingInProcess || lastKnownDirection == ValueDirection.NONE)
        {
            if (value.Length < oldLabel.Length)
                lastKnownDirection = ValueDirection.DOWN;
            else if (value.Length > oldLabel.Length)
                lastKnownDirection = ValueDirection.UP;
            else
                lastKnownDirection = ValueDirection.NONE;

            lockedRecord = !commandRecordingInProcess;
            tentativeRecord = true;
            commandRecordingInProcess = true;            
        }
        // Else check if a new record needs to overwrite this current one or if this one needs to be edited
        else if (commandRecordingInProcess)
        {
            if (value.Length < oldLabel.Length)
            {
                if (lastKnownDirection == ValueDirection.DOWN)
                {
                    tentativeRecord = true;
                }
                else
                {
                    lockedRecord = true;
                }

                lastKnownDirection = ValueDirection.DOWN;
            }
            else if (value.Length > oldLabel.Length)
            {
                if (lastKnownDirection == ValueDirection.UP)
                {
                    tentativeRecord = true;
                }
                else
                {
                    lockedRecord = true;
                }

                lastKnownDirection = ValueDirection.UP;
            }
            else
            {
                lockedRecord = true;
            }
        }
    }

    static string GetValue(SongObject songObject)
    {
        if (songObject == null)
            return string.Empty;

        switch ((SongObject.ID)songObject.classID)
        {
            case (SongObject.ID.BPM):
                return ((BPM)songObject).value.ToString();
            case (SongObject.ID.TimeSignature):
                return (((TimeSignature)songObject).numerator * ((TimeSignature)songObject).denominator).ToString();
            case (SongObject.ID.Event):
                return ((Event)songObject).title.ToString();
            case (SongObject.ID.ChartEvent):
                return ((ChartEvent)songObject).eventName.ToString();
            case (SongObject.ID.Section):
                return ((Section)songObject).title.ToString();
            default:
                UnityEngine.Debug.LogError("Song object has no value");
                return string.Empty;
        }
    }

    enum ValueDirection
    {
        NONE, DOWN, UP
    }
}
