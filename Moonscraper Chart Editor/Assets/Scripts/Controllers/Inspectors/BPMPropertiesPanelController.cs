using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BPMPropertiesPanelController : PropertiesPanelController {
    public BPM currentBPM { get { return (BPM)currentSongObject; } set { currentSongObject = value; } }
    public InputField bpmValue;
    public Toggle anchorToggle;
    public Button increment, decrement;
    public Selectable[] AnchorAheadDisable;

    float incrementalTimer = 0;
    float autoIncrementTimer = 0;
    const float AUTO_INCREMENT_WAIT_TIME = 0.5f;
    const float AUTO_INCREMENT_RATE = 0.08f;

    uint lastAutoVal = 0;

    void Start()
    {
        bpmValue.onValidateInput = validatePositiveDecimal;
    }

    void OnEnable()
    {
        bool edit = ChartEditor.editOccurred;
        UpdateBPMInputFieldText();

        incrementalTimer = 0;
        autoIncrementTimer = 0;

        ChartEditor.editOccurred = edit;
    }

    void UpdateBPMInputFieldText()
    {
        if (currentBPM != null)
            bpmValue.text = ((float)currentBPM.value / 1000.0f).ToString();
    }

    protected override void Update()
    {
        base.Update();
        if (currentBPM != null)
        {
            // Update inspector information
            positionText.text = "Position: " + currentBPM.position.ToString();
            if (!Globals.IsTyping)//if (bpmValue.text != string.Empty && bpmValue.text[bpmValue.text.Length - 1] != '.' && bpmValue.text[bpmValue.text.Length - 1] != '0')
                UpdateBPMInputFieldText();

            anchorToggle.isOn = currentBPM.anchor != null;

            bool interactable = !IsNextBPMAnAnchor();
            foreach (Selectable ui in AnchorAheadDisable)
                ui.interactable = interactable;

        }

        editor.currentSong.updateArrays();

        if (incrementalTimer > AUTO_INCREMENT_WAIT_TIME)
            autoIncrementTimer += Time.deltaTime;
        else
            autoIncrementTimer = 0;

        if (!(Input.GetKey(KeyCode.Equals) && Input.GetKey(KeyCode.Minus)))
        {
            if (!Globals.IsTyping && !Globals.modifierInputActive)
            {
                if (Input.GetKeyDown(KeyCode.Minus) && decrement.interactable)
                {
                    lastAutoVal = currentBPM.value;
                    decrement.onClick.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.Equals) && increment.interactable)
                {
                    lastAutoVal = currentBPM.value;
                    increment.onClick.Invoke();
                }

                // Adjust to time rather than framerate
                if (incrementalTimer > AUTO_INCREMENT_WAIT_TIME && autoIncrementTimer > AUTO_INCREMENT_RATE)
                {
                    if (Input.GetKey(KeyCode.Minus) && decrement.interactable)
                        decrement.onClick.Invoke();
                    else if (Input.GetKey(KeyCode.Equals) && increment.interactable)
                        increment.onClick.Invoke();

                    autoIncrementTimer = 0;
                }

                // 
                if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Minus))
                {
                    incrementalTimer += Time.deltaTime;
                    ChartEditor.editOccurred = true;
                }
            }
            else
                incrementalTimer = 0;

            // Handle key release, add in action history
            if (Input.GetKeyUp(KeyCode.Equals) || Input.GetKeyUp(KeyCode.Minus))
            {
                incrementalTimer = 0;
                editor.actionHistory.Insert(new ActionHistory.Modify(new BPM(currentSongObject.position, lastAutoVal), currentSongObject));
                ChartEditor.editOccurred = true;
                lastAutoVal = currentBPM.value;
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        currentBPM = null;
        editor.currentSong.updateArrays();
    }

    public void UpdateBPMValue(string value)
    {
        uint prevValue = currentBPM.value;
        if (value[value.Length - 1] == '.')
            value = value.Remove(value.Length - 1);
        
        if (value != string.Empty && value[value.Length - 1] != '.' && currentBPM != null && float.Parse(value) != 0)
        {
            // Convert the float string to an int string
            int zerosToAdd = 0;
            if (value.Contains("."))
            {
                int index = value.IndexOf('.');

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

            //AdjustForAnchors(parsedVal);
            currentBPM.value = (uint)parsedVal;
            UpdateInputFieldRecord();
        }
        else if (value == ".")
            bpmValue.text = string.Empty;

        if (prevValue != currentBPM.value)
            ChartEditor.editOccurred = true;
    }

    public void EndEdit(string value)
    {
        if (value == string.Empty || currentBPM.value <= 0)
        {
            //currentBPM.value = 120000;
            AdjustForAnchors(120000);
            UpdateInputFieldRecord();
        }

        UpdateBPMInputFieldText();
        //Debug.Log(((float)currentBPM.value / 1000.0f).ToString().Length);
    }

    public char validatePositiveDecimal(string text, int charIndex, char addedChar)
    {
        int selectionLength = Mathf.Abs(bpmValue.selectionAnchorPosition - bpmValue.selectionFocusPosition);
        int selectStart = bpmValue.selectionAnchorPosition < bpmValue.selectionFocusPosition ? bpmValue.selectionAnchorPosition : bpmValue.selectionFocusPosition;

        if (selectStart < bpmValue.text.Length)
            text = text.Remove(selectStart, selectionLength);

        if ((addedChar == '.' && !text.Contains(".") && text.Length > 0) || (addedChar >= '0' && addedChar <= '9'))
        {
            if ((text.Contains(".") && text.IndexOf('.') > 2 && charIndex <= text.IndexOf('.')) || (addedChar != '.' && !text.Contains(".") && text.Length > 2))
                return '\0';

            if (addedChar != '.')
            {
                if (bpmValue.selectionAnchorPosition == text.Length && bpmValue.selectionFocusPosition == 0)
                    return addedChar;

                if (!text.Contains(".") && text.Length < 3)         // Adding a number, no decimal point
                    return addedChar;
                else if (text.Contains(".") && text.IndexOf('.') <= 3)
                    return addedChar;
            }

             return addedChar;
        }

        return '\0';
    }

    public void IncrementBPM()
    {
        //currentBPM.value += 1000;

        AdjustForAnchors(currentBPM.value + 1000);
        UpdateBPMInputFieldText();
    }

    public void DecrementBPM()
    {
        uint newValue = currentBPM.value;

        if (newValue > 1000)
            newValue -= 1000;

        AdjustForAnchors(newValue);
        UpdateBPMInputFieldText();
    }

    bool AdjustForAnchors(uint newBpmValue)
    {
        int pos = SongObject.FindObjectPosition(currentBPM, currentBPM.song.bpms);
        if (pos != SongObject.NOTFOUND)
        {
            BPM anchor = null;
            BPM bpmToAdjust = null;

            // Get the next anchor
            for (int i = pos + 1; i < currentBPM.song.bpms.Length; ++i)
            {
                if (currentBPM.song.bpms[i].anchor != null)
                {
                    anchor = currentBPM.song.bpms[i];

                    // Get the bpm before that anchor
                    bpmToAdjust = currentBPM.song.bpms[i - 1];
                }
            }

            if (anchor == null)
                return true;

            if (bpmToAdjust == currentBPM)
                return false;

            // Adjust the bpm value before the anchor to match the anchor's set time to it's actual time
            double bpmToAdjustTime = Song.dis_to_time(currentBPM.position, bpmToAdjust.position, currentBPM.song.resolution, newBpmValue / 1000);// (float)anchor.anchor - bpmToAdjust.time;
            double deltaTime = (double)anchor.anchor - bpmToAdjustTime;
            uint newValue = (uint)(Song.dis_to_bpm(bpmToAdjust.position, anchor.position, deltaTime, currentBPM.song.resolution) * 1000);
            Debug.Log(bpmToAdjustTime + ", " + anchor.anchor);
            if (deltaTime > 0 && newValue > 0)
            {
                if (newValue != 0)
                    bpmToAdjust.value = newValue;
                currentBPM.value = newBpmValue;
            }
        }

        return true;
    }

    BPM NextBPM()
    {
        int pos = SongObject.FindObjectPosition(currentBPM, currentBPM.song.bpms);
        if (pos != SongObject.NOTFOUND && pos + 1 < currentBPM.song.bpms.Length)
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
        if (anchored)
            currentBPM.anchor = currentBPM.time;
        else
            currentBPM.anchor = null;

        Debug.Log("Anchor toggled to: " + currentBPM.anchor);
    }
}
