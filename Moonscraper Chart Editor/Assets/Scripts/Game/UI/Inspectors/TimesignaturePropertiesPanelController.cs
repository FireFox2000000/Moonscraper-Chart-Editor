// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class TimesignaturePropertiesPanelController : PropertiesPanelController {
    public TimeSignature currentTS { get { return (TimeSignature)currentSongObject; } set { currentSongObject = value; } }
    public InputField tsValue;
    public InputField tsDenomValue;

    void Start()
    {
        tsValue.onValidateInput = validatePositiveInteger;
        tsDenomValue.onValidateInput = validatePositiveInteger;
    }

    void OnEnable()
    {
        bool edit = ChartEditor.isDirty;

        if (currentTS != null)
        {
            positionText.text = "Position: " + currentTS.tick.ToString();
            tsValue.text = currentTS.numerator.ToString();
            tsDenomValue.text = currentTS.denominator.ToString();
        }

        ChartEditor.isDirty = edit;
    }

    protected override void Update()
    {
        base.Update();
        if (currentTS != null)
        {
            positionText.text = "Position: " + currentTS.tick.ToString();

            if (tsValue.text != string.Empty)
                tsValue.text = currentTS.numerator.ToString();

            if (tsDenomValue.text != string.Empty)
                tsDenomValue.text = currentTS.denominator.ToString();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        currentTS = null;
    }

    public void UpdateTSValue(string value)
    {
        float prevValue = currentTS.numerator;
        uint numerator = 0;
        bool isValid = uint.TryParse(value, out numerator);
        isValid &= numerator > 0;

        if (currentTS != null && isValid)
        {
            bool tentativeRecord, lockedRecord;
            ShouldRecordInputField(value, currentTS.numerator.ToString(), out tentativeRecord, out lockedRecord);

            if (!lockedRecord && tentativeRecord)
            {
                editor.commandStack.Pop();
            }

            if (tentativeRecord || lockedRecord)
            {
                TimeSignature newTs = new TimeSignature(currentTS.tick, numerator, currentTS.denominator);
                var command = new SongEditModify<TimeSignature>(currentTS, newTs);
                editor.commandStack.Push(command);
                editor.selectedObjectsManager.SelectSongObject(newTs, editor.currentSong.syncTrack);
            }
        }
    }

    public void UpdateTSDenom(string value)
    {
    }

    public void EndEdit(string value)
    {
        tsValue.text = currentTS.numerator.ToString();
    }

    public void EndEditDenom(string value)
    {
    }

    public char validatePositiveInteger(string text, int charIndex, char addedChar)
    {
        if (addedChar >= '0' && addedChar <= '9')
            return addedChar;
        else
            return '\0';
    }

    public void IncreaseDenom()
    {
        float prevValue = currentTS.denominator;

        // Get the next highest power of 2
        uint pow = GetNextHigherPowOf2(currentTS.denominator);

        if (prevValue != pow)
        {
            TimeSignature newTs = new TimeSignature(currentTS.tick, currentTS.numerator, pow);
            var command = new SongEditModify<TimeSignature>(currentTS, newTs);
            editor.commandStack.Push(command);
            var selected = editor.selectedObjectsManager.SelectSongObject(newTs, editor.currentSong.timeSignatures);
            tsDenomValue.text = selected.denominator.ToString();
        }
    }

    public void DecreaseDenom()
    {
        float prevValue = currentTS.denominator;

        // Get the next highest power of 2
        uint pow = GetNextHigherPowOf2(currentTS.denominator);

        while (pow > 1 && pow >= prevValue)
        {
            pow /= 2;
        }

        if (prevValue != pow)
        {
            TimeSignature newTs = new TimeSignature(currentTS.tick, currentTS.numerator, pow);
            var command = new SongEditModify<TimeSignature>(currentTS, newTs);
            editor.commandStack.Push(command);
            var selected = editor.selectedObjectsManager.SelectSongObject(newTs, editor.currentSong.timeSignatures);
            tsDenomValue.text = selected.denominator.ToString();
        }
    }

    uint GetNextHigherPowOf2(uint startVal)
    {
        const uint CAP = 64;

        uint pow = 1;
        while (pow <= startVal && pow < CAP)
        {
            pow *= 2;
        }

        return pow;
    }
}
