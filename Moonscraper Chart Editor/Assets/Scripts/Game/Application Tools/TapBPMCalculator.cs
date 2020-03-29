// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapBPMCalculator {
    float resetTime;

    public float bpm
    {
        get
        {
            if (numOfTaps > 1)
                return (float)(numOfTaps - 1) / ((lastTapTime - initTapTime) / 60.0f);
            else
                return 0;
        }
    }

    public int taps { get { return numOfTaps; } }

    float lastTapTime = 0;
    float initTapTime = 0;
    int numOfTaps = 0;

    public TapBPMCalculator(float resetTime = 2.0f)
    {
        this.resetTime = resetTime;
    }
	
    public void Tap()
    {
        if (numOfTaps == 0 || Time.realtimeSinceStartup - lastTapTime >= resetTime)
        {
            Reset();
        }
        else
            lastTapTime = Time.realtimeSinceStartup;

        ++numOfTaps;
    }

    public void Reset()
    {
        initTapTime = Time.realtimeSinceStartup;
        lastTapTime = initTapTime;
        numOfTaps = 0;
    }
}
