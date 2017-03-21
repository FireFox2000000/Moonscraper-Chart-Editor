using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapBPMCalculator {
    const float RESET_TIME = 2.0f;

    float bpm
    {
        get
        {
            if (numOfTaps != 0)
                return (lastTapTime - initTapTime) / numOfTaps * 60.0f;
            else
                return 0;
        }
    }

    float lastTapTime = 0;
    float initTapTime = 0;
    float numOfTaps = 0;
	
    public void Tap()
    {
        if (Time.realtimeSinceStartup - lastTapTime >= RESET_TIME)
        {
            Reset();
        }

        lastTapTime = Time.realtimeSinceStartup;
        ++numOfTaps;
    }

    public void Reset()
    {
        initTapTime = Time.realtimeSinceStartup;
        lastTapTime = Time.realtimeSinceStartup;
        numOfTaps = 0;
    }
}
