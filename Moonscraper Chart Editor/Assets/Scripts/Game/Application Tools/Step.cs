// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class Step 
{
    public const int FULL_STEP = 768;
    public const int MIN_STEP = 1;

    int step;
    int lsbOffset;

    public int value;// { get { return step; } }

    public Step(int value = 4)
    {
        step = 4;
        lsbOffset = 3;

        this.value = value;

        //SetStep(value);
    }

    void _Increment()
    {
        if (step < FULL_STEP)
        {
            if (lsbOffset % 2 == 0)
            {
                step &= 1 << (lsbOffset / 2);
                step <<= 1;
            }
            else
            {
                step |= 1 << (lsbOffset / 2);
            }
            ++lsbOffset;
        }
    }

    public void AdjustBy(int customIncrement)
    {
        value = value + customIncrement;
        value = Mathf.Clamp(value, MIN_STEP, FULL_STEP);
        SetStep(value);
    }

    public void Increment()
    {
        SetStep(value);

        if (step <= value)
        { 
            _Increment();
        }

        value = step;
    }

    void _Decrement()
    {
        if (step > MIN_STEP)
        {
            if (lsbOffset % 2 == 0)
            {
                step &= ~(1 << ((lsbOffset - 1) / 2));
            }
            else
            {
                step |= 1 << (lsbOffset / 2);
                step >>= 1;
            }

            --lsbOffset;
        }
    }

    public void Decrement()
    {
        SetStep(value);

        if (step >= value)
        {
            _Decrement();
        }

        value = step;
    }

    public void SetStep(int step)
    {
        // Cap
        if (step < MIN_STEP)
            step = MIN_STEP;
        else if (step > FULL_STEP)
            step = FULL_STEP;

        if (this.step < step)
        {
            while (this.step < step)
            {
                _Increment();
            }
        }
        else
        {
            while (this.step > step)
            {
                _Decrement();
            }
        }
    }

    public static char validateStepVal(string text, int charIndex, char addedChar)
    {
        if (addedChar >= '0' && addedChar <= '9')
            return addedChar;
        else
            return '\0';
    }
}
