// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

        if (value != string.Empty && currentTS != null)
        {
            currentTS.numerator = uint.Parse(value);
            UpdateInputFieldRecord();
        }

        if (prevValue != currentTS.numerator)
            ChartEditor.isDirty = true;
    }

    public void UpdateTSDenom(string value)
    {
        float prevValue = currentTS.denominator;

        if (value != string.Empty && currentTS != null)
        {
            currentTS.denominator = uint.Parse(value);
            UpdateInputFieldRecord();
        }

        if (prevValue != currentTS.denominator)
            ChartEditor.isDirty = true;
    }

    public void EndEdit(string value)
    {
        if (value == string.Empty || currentTS.numerator < 1)
        {
            currentTS.numerator = 4;
            UpdateInputFieldRecord();
        }

        tsValue.text = currentTS.numerator.ToString();
    }

    public void EndEditDenom(string value)
    {
        if (value == string.Empty || currentTS.denominator < 1)
        {
            currentTS.denominator = 4;
            UpdateInputFieldRecord();
        }

        tsDenomValue.text = currentTS.denominator.ToString();
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
            TimeSignature prev = (TimeSignature)currentTS.Clone();
            currentTS.denominator = pow;

            editor.actionHistory.Insert(new ActionHistory.Modify(prev, currentTS));

            ChartEditor.isDirty = true;
        }

        tsDenomValue.text = currentTS.denominator.ToString();
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
            TimeSignature prev = (TimeSignature)currentTS.Clone();
            currentTS.denominator = pow;

            editor.actionHistory.Insert(new ActionHistory.Modify(prev, currentTS));

            ChartEditor.isDirty = true;
        }

        tsDenomValue.text = currentTS.denominator.ToString();
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
