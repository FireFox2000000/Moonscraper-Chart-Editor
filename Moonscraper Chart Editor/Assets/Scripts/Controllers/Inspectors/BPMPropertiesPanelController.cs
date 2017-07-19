using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BPMPropertiesPanelController : PropertiesPanelController {
    public BPM currentBPM { get { return (BPM)currentSongObject; } set { currentSongObject = value; } }
    public InputField bpmValue;

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

        if (currentBPM != null)
            bpmValue.text = ((float)currentBPM.value / 1000.0f).ToString();

        incrementalTimer = 0;
        autoIncrementTimer = 0;

        ChartEditor.editOccurred = edit;
    }

    protected override void Update()
    {
        base.Update();
        if (currentBPM != null)
        {
            // Update inspector information
            positionText.text = "Position: " + currentBPM.position.ToString();
            if (bpmValue.text != string.Empty && bpmValue.text[bpmValue.text.Length - 1] != '.' && bpmValue.text[bpmValue.text.Length - 1] != '0')
                bpmValue.text = (currentBPM.value / 1000.0f).ToString();
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
                if (Input.GetKeyDown(KeyCode.Minus))
                {
                    lastAutoVal = currentBPM.value;
                    DecrementBPM();
                }
                else if (Input.GetKeyDown(KeyCode.Equals))
                {
                    lastAutoVal = currentBPM.value;
                    IncrementBPM();
                }

                // Adjust to time rather than framerate
                if (incrementalTimer > AUTO_INCREMENT_WAIT_TIME && autoIncrementTimer > AUTO_INCREMENT_RATE)
                {
                    if (Input.GetKey(KeyCode.Minus))
                        DecrementBPM();
                    else if (Input.GetKey(KeyCode.Equals))
                        IncrementBPM();

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
        float prevValue = currentBPM.value;
        if (value != string.Empty && value[value.Length - 1] != '.' && currentBPM != null && float.Parse(value) != 0)
        {
            float floatVal = float.Parse(value) * 1000;     // Store it in another variable due to weird parsing-casting bug at decimal points of 2 or so. Seems to fix it for whatever reason.

            currentBPM.value = (uint)floatVal;
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
            currentBPM.value = 120000;
            UpdateInputFieldRecord();
        }

        bpmValue.text = ((float)currentBPM.value / 1000.0f).ToString();
    }

    public char validatePositiveDecimal(string text, int charIndex, char addedChar)
    {
        if ((addedChar == '.' && !text.Contains(".") && text.Length > 0) || (addedChar >= '0' && addedChar <= '9'))
        {
            if (addedChar != '.')
            {
                if (bpmValue.selectionAnchorPosition == text.Length && bpmValue.selectionFocusPosition == 0)
                    return addedChar;

                if (!text.Contains(".") && text.Length < 3)         // Adding a number, no decimal point
                    return addedChar;
                else if (text.Contains(".") && text.IndexOf('.') <= 3)
                    return addedChar;
            }
            else
                return addedChar;
        }

        return '\0';
    }

    public void IncrementBPM()
    {
        currentBPM.value += 1000;
    }

    public void DecrementBPM()
    {
        if (currentBPM.value > 1000)
            currentBPM.value -= 1000;
    }
}
