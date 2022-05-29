// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MoonscraperChartEditor.Song;

public class BPMPropertiesPanelController : PropertiesPanelController {
    public BPM currentBPM { get { return (BPM)currentSongObject; } set { currentSongObject = value; } }
    public InputField bpmValue;
    public Toggle anchorToggle;
    public Button increment, decrement;
    public Selectable[] AnchorAheadDisable;

    char c_decimal = LocalesManager.decimalSeperator;
    string c_decimalStr;

    float incrementInputHoldTime = 0;
    float autoIncrementTimer = 0;
    const float AUTO_INCREMENT_WAIT_TIME = 0.5f;
    const float AUTO_INCREMENT_RATE = 0.08f;

    BPM prevBPM;
    BPM prevClonedBPM = new BPM();

    const uint c_minBpmValue = 1000;
    const uint c_incrementRate = 1000;
    const uint c_decrementRate = 1000;

    void Start()
    {
        c_decimalStr = c_decimal.ToString();
        bpmValue.onValidateInput = validatePositiveDecimal;
    }

    void OnEnable()
    {
        bool edit = ChartEditor.isDirty;
        UpdateBPMInputFieldText();

        incrementInputHoldTime = 0;
        autoIncrementTimer = 0;

        ChartEditor.isDirty = edit;
    }

    void UpdateBPMInputFieldText()
    {
        if (currentBPM != null)
            bpmValue.text = (currentBPM.displayValue).ToString();
    }

    void Controls()
    {
        if (!(MSChartEditorInput.GetInput(MSChartEditorInputActions.BpmIncrease) && MSChartEditorInput.GetInput(MSChartEditorInputActions.BpmDecrease)))    // Can't hit both at the same time
        {
            if (!Services.IsTyping && !Globals.modifierInputActive)
            {
                if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.BpmDecrease) && decrement.interactable)
                {
                    uint newValue = GetValueForDecrement();
                    SetBpmValue(newValue);
                }
                else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.BpmIncrease) && increment.interactable)
                {
                    uint newValue = GetValueForIncrement();
                    SetBpmValue(newValue);
                }

                // Adjust to time rather than framerate
                if (incrementInputHoldTime > AUTO_INCREMENT_WAIT_TIME && autoIncrementTimer > AUTO_INCREMENT_RATE)
                {
                    if (MSChartEditorInput.GetInput(MSChartEditorInputActions.BpmDecrease) && decrement.interactable)
                    {
                        uint newValue = GetValueForDecrement();
                        SetBpmValue(newValue, true);
                    }
                    else if (MSChartEditorInput.GetInput(MSChartEditorInputActions.BpmIncrease) && increment.interactable)
                    {
                        uint newValue = GetValueForIncrement();
                        SetBpmValue(newValue, true);
                    }

                    autoIncrementTimer = 0;
                }

                // 
                if (MSChartEditorInput.GetInput(MSChartEditorInputActions.BpmIncrease) || MSChartEditorInput.GetInput(MSChartEditorInputActions.BpmDecrease))
                {
                    incrementInputHoldTime += Time.deltaTime;
                }
                else
                    incrementInputHoldTime = 0;
            }
            else
                incrementInputHoldTime = 0;
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleBpmAnchor) && anchorToggle.IsInteractable())
            anchorToggle.isOn = !anchorToggle.isOn;
    }

    protected override void Update()
    {
        base.Update();
        if (currentBPM != null && currentBPM.song != null)
        {
            if (currentBPM.value != prevClonedBPM.value)
            {
                editor.currentSong.UpdateCache();    
            }

            if (currentBPM.tick != prevClonedBPM.tick)
            {
                // Update inspector information
                positionText.text = "Position: " + currentBPM.tick.ToString();
            }

            if (!Services.IsTyping || currentBPM != prevBPM)
                UpdateBPMInputFieldText();

            anchorToggle.isOn = currentBPM.anchor != null;

            bool interactable = !IsNextBPMAnAnchor();
            foreach (Selectable ui in AnchorAheadDisable)
                ui.interactable = interactable;
        }

        prevClonedBPM.CopyFrom(currentBPM);

        if (incrementInputHoldTime > AUTO_INCREMENT_WAIT_TIME)
            autoIncrementTimer += Time.deltaTime;
        else
            autoIncrementTimer = 0;

        Controls();

        prevBPM = currentBPM;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        currentBPM = null;
        editor.currentSong.UpdateCache();
    }

    public void UpdateBPMValue(string value)
    {
        if (prevBPM != currentBPM)
            return;

        bool tentativeRecord, lockedRecord;
        ShouldRecordInputField(value, currentBPM.displayValue.ToString(), out tentativeRecord, out lockedRecord, true);

        uint prevValue = currentBPM.value;
        if (value.Length > 0 && value[value.Length - 1] == c_decimal)
            value = value.Remove(value.Length - 1);
        
        if (value != string.Empty && value[value.Length - 1] != c_decimal && currentBPM != null && float.Parse(value) != 0)
        {
            // Convert the float string to an int string
            int zerosToAdd = 0;
            if (value.Contains(c_decimal))
            {
                int index = value.IndexOf(c_decimal);

                zerosToAdd = 7 - (value.Length + (3 - index));      // string length can be a total of 7 characters; 6 digits and the "."
                value = value.Remove(index, 1);
            }
            else
            {
                zerosToAdd = 3;     // Number of zeros after the decimal point
            }

            for (int i = 0; i < zerosToAdd; ++i)
                value += "0";

            // Actually parse the value now
            uint parsedVal = uint.Parse(value);// * 1000;     // Store it in another variable due to weird parsing-casting bug at decimal points of 2 or so. Seems to fix it for whatever reason.

            bool pop = !lockedRecord && tentativeRecord;

            if (tentativeRecord || lockedRecord)
            {
                SetBpmValue(parsedVal, pop);
            }
        }
        else if (value == c_decimalStr)
            bpmValue.text = string.Empty;
    }

    public void EndEdit(string value)
    {
        if (value == string.Empty || currentBPM.value <= 0)
        {
            SetBpmValue(120000);
        }

        UpdateBPMInputFieldText();
    }

    public char validatePositiveDecimal(string text, int charIndex, char addedChar)
    {
        int selectionLength = Mathf.Abs(bpmValue.selectionAnchorPosition - bpmValue.selectionFocusPosition);
        int selectStart = bpmValue.selectionAnchorPosition < bpmValue.selectionFocusPosition ? bpmValue.selectionAnchorPosition : bpmValue.selectionFocusPosition;

        if (selectStart < bpmValue.text.Length)
            text = text.Remove(selectStart, selectionLength);

        if ((addedChar == c_decimal && !text.Contains(c_decimal) && text.Length > 0) || (addedChar >= '0' && addedChar <= '9'))
        {
            bool invalidDecimalPosition = text.Contains(c_decimal) && text.IndexOf(c_decimal) > 2 && charIndex <= text.IndexOf(c_decimal);
            bool exceedsDigitLimitAfterDecimalPoint = text.Contains(c_decimal) && (text.Length - text.IndexOf(c_decimal)) > 3 && charIndex > text.IndexOf(c_decimal);
            bool greaterThanMaxInts = addedChar != c_decimal && !text.Contains(c_decimal) && text.Length > 2;

            if (invalidDecimalPosition || greaterThanMaxInts || exceedsDigitLimitAfterDecimalPoint)
                return '\0';

            if (addedChar != c_decimal)
            {
                if (bpmValue.selectionAnchorPosition == text.Length && bpmValue.selectionFocusPosition == 0)
                    return addedChar;

                if (!text.Contains(c_decimal) && text.Length < 3)         // Adding a number, no decimal point
                    return addedChar;
                else if (text.Contains(c_decimal) && text.IndexOf(c_decimal) <= 3)
                    return addedChar;
            }

             return addedChar;
        }

        return '\0';
    }

    public void IncrementBPM()
    {
        uint newValue = GetValueForIncrement();
        SetBpmValue(newValue);
    }

    public void DecrementBPM()
    {
        uint newValue = GetValueForDecrement();

        SetBpmValue(newValue);
    }

    uint GetValueForIncrement()
    {
        return currentBPM.value + c_incrementRate;
    }

    uint GetValueForDecrement()
    {
        uint newValue = currentBPM.value;

        if (newValue > c_minBpmValue)
            newValue -= c_decrementRate;

        return newValue;
    }

    void SetBpmValue(uint newValue, bool popCommands = false)
    {
        if (popCommands)
            editor.commandStack.Pop();

        var command = GenerateCommandsAdjustedForAnchors(currentBPM, newValue);

        if (command != null)
        {
            editor.commandStack.Push(command);
            int newBpmPos = SongObjectHelper.FindObjectPosition(currentBPM.tick, editor.currentSong.bpms);
            editor.selectedObjectsManager.SelectSongObject(editor.currentSong.bpms[newBpmPos], editor.currentSong.bpms);
        }
        else
            editor.commandStack.Push();     // Popped at the start, need to redo push as pop wasn't replaced

        editor.songObjectPoolManager.SetAllPoolsDirty();
    }

    static MoonscraperEngine.ICommand GenerateCommandsAdjustedForAnchors(BPM currentBPM, uint desiredBpmValue)
    {
        List<SongEditCommand> commands = new List<SongEditCommand>();

        int pos = SongObjectHelper.FindObjectPosition(currentBPM, currentBPM.song.bpms);
        if (pos != SongObjectHelper.NOTFOUND)
        {
            BPM anchor = null;
            BPM bpmToAdjust = null;

            int anchorPos = 0;

            // Get the next anchor
            for (int i = pos + 1; i < currentBPM.song.bpms.Count; ++i)
            {
                if (currentBPM.song.bpms[i].anchor != null)
                {
                    anchor = currentBPM.song.bpms[i];
                    anchorPos = i;
                    // Get the bpm before that anchor
                    bpmToAdjust = currentBPM.song.bpms[i - 1];

                    break;
                }
            }

            if (anchor == null || bpmToAdjust == currentBPM)
            {
                commands.Add(new SongEditModify<BPM>(currentBPM, new BPM(currentBPM.tick, desiredBpmValue, currentBPM.anchor)));
                return new BatchedSongEditCommand(commands);
            }

            // Calculate the minimum the bpm can adjust to
            const float MIN_DT = 0.01f;

            float bpmTime = (float)anchor.anchor - MIN_DT;
            float resolution = currentBPM.song.resolution;
            // Calculate the time of the 2nd bpm pretending that the adjustable one is super close to the anchor
            for (int i = anchorPos - 1; i > pos + 1; --i)
            {
                // Calculate up until 2 bpms before the anchor
                // Re-hash of the actual time calculation equation in Song.cs
                bpmTime -= (float)TickFunctions.DisToTime(currentBPM.song.bpms[i - 1].tick, currentBPM.song.bpms[i].tick, resolution, currentBPM.song.bpms[i - 1].value / 1000.0f);
            }

            float timeBetweenFirstAndSecond = bpmTime - currentBPM.time;
            // What bpm will result in this exact time difference?
            uint minVal = (uint)(Mathf.Ceil((float)TickFunctions.DisToBpm(currentBPM.song.bpms[pos].tick, currentBPM.song.bpms[pos + 1].tick, timeBetweenFirstAndSecond, currentBPM.song.resolution)) * 1000);

            if (desiredBpmValue < minVal)
                desiredBpmValue = minVal;

            BPM anchorBPM = anchor;
            uint oldValue = currentBPM.value;

            ChartEditor editor = ChartEditor.Instance;
            currentBPM.value = desiredBpmValue; // Very much cheating, better to not do this
            double deltaTime = (double)anchorBPM.anchor - editor.currentSong.LiveTickToTime(bpmToAdjust.tick, editor.currentSong.resolution);
            uint newValue = (uint)Mathf.Round((float)(TickFunctions.DisToBpm(bpmToAdjust.tick, anchorBPM.tick, deltaTime, editor.currentSong.resolution) * 1000.0d));
            currentBPM.value = oldValue;

            uint finalValue = oldValue;
            if (deltaTime > 0 && newValue > 0)
            {
                if (newValue != 0)
                {
                    commands.Add(new SongEditModify<BPM>(bpmToAdjust, new BPM(bpmToAdjust.tick, newValue, bpmToAdjust.anchor)));
                }

                finalValue = desiredBpmValue;
            }

            desiredBpmValue = finalValue;
        }

        if (desiredBpmValue == currentBPM.value)
            return null;

        commands.Add(new SongEditModify<BPM>(currentBPM, new BPM(currentBPM.tick, desiredBpmValue, currentBPM.anchor)));
        return new BatchedSongEditCommand(commands);
    }

    BPM NextBPM()
    {
        int pos = SongObjectHelper.FindObjectPosition(currentBPM, currentBPM.song.bpms);
        if (pos != SongObjectHelper.NOTFOUND && pos + 1 < currentBPM.song.bpms.Count)
        {
            return currentBPM.song.bpms[pos + 1];
        }

        return null;
    }

    bool IsNextBPMAnAnchor()
    {
        BPM next = NextBPM();
        if (next != null && next.anchor != null)
            return true;

        return false;
    }

    public void SetAnchor(bool anchored)
    {
        if (currentBPM != prevBPM)
            return;

        BPM newBpm = new BPM(currentBPM);
        if (anchored)
            newBpm.anchor = currentBPM.song.LiveTickToTime(currentBPM.tick, currentBPM.song.resolution);
        else
            newBpm.anchor = null;

        editor.commandStack.Push(new SongEditModify<BPM>(currentBPM, newBpm));
        editor.selectedObjectsManager.SelectSongObject(newBpm, editor.currentSong.syncTrack);

        Debug.Log("Anchor toggled to: " + newBpm.anchor);
    }
}
