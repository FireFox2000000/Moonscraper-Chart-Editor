using UnityEngine;
using System.Collections;

public class Step 
{
    const uint FULL_STEP = 768;
    const uint MIN_STEP = 1;

    int step;
    int lsbOffset;

    public int value { get { return step; } }

    public Step(uint value = 4)
    {
        step = 4;
        lsbOffset = 3;

        SetStep(value);
    }
    public void Increment()
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

    public void Decrement()
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

    public void SetStep(uint step)
    {
        if (step < MIN_STEP)
            step = MIN_STEP;
        else if (step > FULL_STEP)
            step = FULL_STEP;

        if (value < step)
        {
            while (value < step)
            {
                Increment();
            }
        }
        else
        {
            while (value > step)
            {
                Decrement();
            }
        }
    }
}
