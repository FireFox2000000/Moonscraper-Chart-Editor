using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Whammy : MonoBehaviour {
    public float keyShiftSpeed = 5;
    LineRenderer lineRenderer;
    AnimationCurve lineCurve;

    // Use this for initialization
    void Start () {
        //StartCoroutine(waveWhammy());
        lineRenderer = GetComponent<LineRenderer>();

        lineCurve = lineRenderer.widthCurve;
        
	}
	
	// Update is called once per frame
	void Update () {
        AnimationCurve lineCurve = lineRenderer.widthCurve;

        Debug.Log(Input.GetAxisRaw("Whammy"));
        float whammyVal = (lerpedWhammyVal() + 1) / 1.0f;

        for (int i = lineCurve.keys.Length - 1; i >= 0; --i)
        {
            float keyTime = lineCurve.keys[i].time + keyShiftSpeed * Time.deltaTime;
            float keyValue = lineCurve.keys[i].value;

            if (keyTime <= 1)
                lineCurve.MoveKey(i, new Keyframe(keyTime, keyValue));
            else
                lineCurve.RemoveKey(i);
        }

        lineCurve.AddKey(new Keyframe(0, whammyVal + 1));

        lineRenderer.widthCurve = lineCurve;
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
    /*
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
    }*/
}
