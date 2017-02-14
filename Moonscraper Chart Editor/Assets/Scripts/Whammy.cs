using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whammy : MonoBehaviour {
    public LineRenderer rightLine;
    public LineRenderer leftLine;

    public int pointsPerUnit;

    // Use this for initialization
    void Start () {
        StartCoroutine(waveWhammy());
	}
	
	// Update is called once per frame
	void Update () {

        Debug.Log(Input.GetAxisRaw("Whammy"));
        float whammyVal = (lerpedWhammyVal() + 1) / 3.0f;

        Vector3 rightWhammyOriginalPos = rightLine.GetPosition(0);
        rightLine.SetPosition(0, new Vector3(whammyVal, rightWhammyOriginalPos.y, rightWhammyOriginalPos.z));

        Vector3 leftWhammyOriginalPos = leftLine.GetPosition(0);
        leftLine.SetPosition(0, new Vector3(-whammyVal, leftWhammyOriginalPos.y, leftWhammyOriginalPos.z));
    }

    float currentWhammyVal = -1;
    float lerpedWhammyVal()
    {
        const float increment = 20;
        float rawVal = Input.GetAxisRaw("Whammy").Round(2);
        if (rawVal > currentWhammyVal)
        {
            currentWhammyVal += increment * Time.deltaTime;
            if (currentWhammyVal > rawVal)
                currentWhammyVal = rawVal;
        }
        else if (rawVal.Round(2) < currentWhammyVal)
        {
            currentWhammyVal -= increment * Time.deltaTime;
            if (currentWhammyVal < rawVal)
                currentWhammyVal = rawVal;
        }

        return currentWhammyVal;
    }

    IEnumerator waveWhammy()
    {
        while (true)
        {
            for (int i = rightLine.numPositions - 1; i > 0; --i)
            {
                Vector3 originalPos = rightLine.GetPosition(i);
                rightLine.SetPosition(i, new Vector3(rightLine.GetPosition(i - 1).x, originalPos.y, originalPos.z));
            }

            for (int i = leftLine.numPositions - 1; i > 0; --i)
            {
                Vector3 originalPos = leftLine.GetPosition(i);
                leftLine.SetPosition(i, new Vector3(leftLine.GetPosition(i - 1).x, originalPos.y, originalPos.z));
            }

            yield return new WaitForSeconds(0.012f);
        }
    }

    void SetSize(LineRenderer line)
    {
        
    }
}
