using UnityEngine;
using System.Collections;

public class Indicators : MonoBehaviour {
    public GameObject[] indicators = new GameObject[5];

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("FretGreen"))
            indicators[0].SetActive(true);
        else
            indicators[0].SetActive(false);

        if (Input.GetButton("FretRed"))
            indicators[1].SetActive(true);
        else
            indicators[1].SetActive(false);

        if (Input.GetButton("FretYellow"))
            indicators[2].SetActive(true);
        else
            indicators[2].SetActive(false);

        if (Input.GetButton("FretBlue"))
            indicators[3].SetActive(true);
        else
            indicators[3].SetActive(false);

        if (Input.GetButton("FretOrange"))
            indicators[4].SetActive(true);
        else
            indicators[4].SetActive(false);
    }
}
