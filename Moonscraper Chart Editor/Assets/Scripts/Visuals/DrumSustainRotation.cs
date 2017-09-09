using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumSustainRotation : MonoBehaviour {
    const int STANDARD_FRETS = 5;
    [SerializeField]
    SustainResources defaultSustainResources;

    Color[] sustainColors;

    bool prevDrumMode = false;
	// Use this for initialization
	void Start () {
        sustainColors = new Color[STANDARD_FRETS];

        for (int i = 0; i < STANDARD_FRETS; ++i)
        {
            sustainColors[i] = defaultSustainResources.sustainColours[i].GetColor("_Color");
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Globals.drumMode != prevDrumMode)
        {
            if (Globals.drumMode)
            {
                defaultSustainResources.sustainColours[STANDARD_FRETS - 1].SetColor("_Color", sustainColors[0]);

                for (int i = 1; i < STANDARD_FRETS; ++i)
                    defaultSustainResources.sustainColours[i - 1].SetColor("_Color", sustainColors[i]);
            }
            else
            {
                for (int i = 0; i < STANDARD_FRETS; ++i)
                    defaultSustainResources.sustainColours[i].SetColor("_Color", sustainColors[i]);
            }
        }

        prevDrumMode = Globals.drumMode;
    }
#if UNITY_EDITOR
    void OnApplicationQuit()
    {
        for (int i = 0; i < STANDARD_FRETS; ++i)
            defaultSustainResources.sustainColours[i].SetColor("_Color", sustainColors[i]);
    }
#endif
}
