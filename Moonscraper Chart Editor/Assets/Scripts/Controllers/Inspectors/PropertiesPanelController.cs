using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PropertiesPanelController : MonoBehaviour {
    public Text positionText;

    protected ChartEditor editor;
    protected SongObject currentSongObject;
    SongObject prevSongObject;
    string prevValue = string.Empty;

    ActionHistory.Modify inputFieldModify = null;
    ValueDirection lastKnownDirection = ValueDirection.NONE;

    void resetActionRecording()
    {
        inputFieldModify = null;
        lastKnownDirection = ValueDirection.NONE;
        prevValue = string.Empty;
        prevSongObject = null;
    }

    void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    protected virtual void OnDisable()
    {
        resetActionRecording();
    }

    protected virtual void Update()
    {
        if (currentSongObject == null || prevSongObject != currentSongObject)
            resetActionRecording();

        prevSongObject = currentSongObject.Clone();
    }

    protected void UpdateInputFieldRecord()
    {
        if (currentSongObject == null || prevSongObject == null || prevSongObject != currentSongObject)
            return;

        string value = GetValue(currentSongObject);

        if (value == prevValue || value == GetValue(prevSongObject))
            return;

        // Check if there's a record already in
        if (inputFieldModify == null || lastKnownDirection == ValueDirection.NONE)
        {
            // Add a new record
            inputFieldModify = new ActionHistory.Modify(prevSongObject, currentSongObject);
            // Add to action history
            editor.actionHistory.Insert(inputFieldModify);

            if (GetValue(currentSongObject).Length < GetValue(prevSongObject).Length)
                lastKnownDirection = ValueDirection.DOWN;
            else if (GetValue(currentSongObject).Length > GetValue(prevSongObject).Length)
                lastKnownDirection = ValueDirection.UP;
            else
                lastKnownDirection = ValueDirection.NONE;
        }
        // Else check if a new record needs to overwrite this current one or if this one needs to be edited
        else if (inputFieldModify != null)
        {
            if (value.Length < prevValue.Length)
            {
                if (lastKnownDirection == ValueDirection.DOWN)
                {
                    // Edit action
                    inputFieldModify.after = currentSongObject.Clone();
                }
                else
                {
                    // New action
                    inputFieldModify = new ActionHistory.Modify(prevSongObject, currentSongObject);
                    // Add to action history
                    editor.actionHistory.Insert(inputFieldModify);
                }

                lastKnownDirection = ValueDirection.DOWN;
            }
            else if (value.Length > prevValue.Length)
            {
                if (lastKnownDirection == ValueDirection.UP)
                {
                    // Edit action
                    inputFieldModify.after = currentSongObject.Clone();
                }
                else
                {
                    // New action
                    inputFieldModify = new ActionHistory.Modify(prevSongObject, currentSongObject);
                    // Add to action history
                    editor.actionHistory.Insert(inputFieldModify);
                }

                lastKnownDirection = ValueDirection.UP;
            }
            else
            {
                // Add a new record
                inputFieldModify = new ActionHistory.Modify(prevSongObject, currentSongObject);
                // Add to action history
                editor.actionHistory.Insert(inputFieldModify);
            }
        }

        prevValue = value;
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
                return ((TimeSignature)songObject).numerator.ToString();
            case (SongObject.ID.Event):
                return ((Event)songObject).title.ToString();
            case (SongObject.ID.Section):
                return ((Section)songObject).title.ToString();
            default:
                return string.Empty;
        }
    }

    enum ValueDirection
    {
        NONE, DOWN, UP
    }
}
