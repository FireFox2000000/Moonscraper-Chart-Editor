// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumSustainRotation : MonoBehaviour {
    const int STANDARD_FRETS = 5;
    [SerializeField]
    SustainResources defaultSustainResources;

    Color[] sustainColors;

	// Use this for initialization
	void Start () {
        sustainColors = new Color[STANDARD_FRETS];

        for (int i = 0; i < STANDARD_FRETS; ++i)
        {
            sustainColors[i] = defaultSustainResources.sustainColours[i].GetColor("_Color");
        }

        ChartEditor.Instance.events.lanesChangedEvent.Register(OnLanesChangedEvent);
    }
	
	// Update is called once per frame
	void OnLanesChangedEvent (in int laneCount) {
        if (Globals.drumMode)
        {
            defaultSustainResources.sustainColours[STANDARD_FRETS - 1].SetColor("_Color", sustainColors[0]);

            for (int i = 1; i < STANDARD_FRETS; ++i)
            {
                defaultSustainResources.sustainColours[i - 1].SetColor("_Color", sustainColors[i >= laneCount ? 0 : i]);
            }
        }
        else
        {
            for (int i = 0; i < STANDARD_FRETS; ++i)
                defaultSustainResources.sustainColours[i].SetColor("_Color", sustainColors[i]);
        }
    }
#if UNITY_EDITOR
    void OnApplicationQuit()
    {
        for (int i = 0; i < STANDARD_FRETS; ++i)
            defaultSustainResources.sustainColours[i].SetColor("_Color", sustainColors[i]);
    }
#endif
}
