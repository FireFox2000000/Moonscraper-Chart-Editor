// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikelineFretPlacement : MonoBehaviour {

    public GameObject greenStrike;
    public GameObject redStrike;
    public GameObject yellowStrike;
    public GameObject blueStrike;
    public GameObject orangeStrike;

    public GameObject[] strikers;

    void Start()
    {
        SetFretPlacement();
        enabled = false;
    }

    public void SetFretPlacement()
    {
        int range = strikers.Length;
        for (int i = 0; i < range; ++i)
        {
            int number = i;
            if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
            {
                number = range - (number + 1);
            }

            float xPos = NoteController.CHART_CENTER_POS + number * NoteController.positionIncrementFactor + NoteController.noteObjectPositionStartOffset;
            strikers[i].transform.position = new Vector3(xPos, strikers[i].transform.position.y, strikers[i].transform.position.z);
        }
        /*
        if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
        {
            greenStrike.transform.position = new Vector3(2, greenStrike.transform.position.y, greenStrike.transform.position.z);
            redStrike.transform.position = new Vector3(1, redStrike.transform.position.y, redStrike.transform.position.z);
            yellowStrike.transform.position = new Vector3(0, yellowStrike.transform.position.y, yellowStrike.transform.position.z);
            blueStrike.transform.position = new Vector3(-1, blueStrike.transform.position.y, blueStrike.transform.position.z);
            orangeStrike.transform.position = new Vector3(-2, orangeStrike.transform.position.y, orangeStrike.transform.position.z);
        }
        else
        {
            greenStrike.transform.position = new Vector3(-2, greenStrike.transform.position.y, greenStrike.transform.position.z);
            redStrike.transform.position = new Vector3(-1, redStrike.transform.position.y, redStrike.transform.position.z);
            yellowStrike.transform.position = new Vector3(0, yellowStrike.transform.position.y, yellowStrike.transform.position.z);
            blueStrike.transform.position = new Vector3(1, blueStrike.transform.position.y, blueStrike.transform.position.z);
            orangeStrike.transform.position = new Vector3(2, orangeStrike.transform.position.y, orangeStrike.transform.position.z);
        }*/
    }
}
