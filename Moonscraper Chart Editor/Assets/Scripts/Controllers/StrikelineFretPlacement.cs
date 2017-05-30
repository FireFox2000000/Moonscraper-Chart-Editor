using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikelineFretPlacement : MonoBehaviour {

    public GameObject greenStrike;
    public GameObject redStrike;
    public GameObject yellowStrike;
    public GameObject blueStrike;
    public GameObject orangeStrike;

    void Start()
    {
        SetFretPlacement();
        enabled = false;
    }
    public void SetFretPlacement()
    {
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
        }
    }
}
