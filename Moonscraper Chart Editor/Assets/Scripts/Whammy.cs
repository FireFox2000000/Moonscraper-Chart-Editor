using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Whammy : MonoBehaviour {
    public float keyShiftSpeed = 5;
    public float widthMultiplier = 1;
    public float whammyLerpSpeed = 20;

    LineRenderer lineRenderer;
    AnimationCurve lineCurve;

    // Use this for initialization
    void Start () {
        lineRenderer = GetComponent<LineRenderer>();

        lineCurve = lineRenderer.widthCurve;
        
	}
	
	// Update is called once per frame
	void Update () {
        lineCurve = lineRenderer.widthCurve;

        ShiftAnimationKeys(lineCurve, keyShiftSpeed * Time.deltaTime);

        Debug.Log(Input.GetAxisRaw("Whammy"));
        float whammyVal = (lerpedWhammyVal() + 1) * widthMultiplier;

        lineCurve.AddKey(new Keyframe(0, whammyVal + 1));

        lineRenderer.widthCurve = lineCurve;
    }

    static void ShiftAnimationKeys(AnimationCurve lineCurve, float shiftDistance)
    {
        for (int i = lineCurve.keys.Length - 1; i >= 0; --i)
        {
            float keyTime = lineCurve.keys[i].time + shiftDistance;
            float keyValue = lineCurve.keys[i].value;

            if (keyTime <= 1)
                lineCurve.MoveKey(i, new Keyframe(keyTime, keyValue));
            else
                lineCurve.RemoveKey(i);
        }
    }

    public static Keyframe[] KeyframeSizeReduction(Keyframe[] keys, float timeCutoff)
    {
        List<Keyframe> newKeys = new List<Keyframe>();

        if (timeCutoff <= 0)
            return keys;
        else if (timeCutoff >= 1)
            return new Keyframe[0];

        foreach(Keyframe key in keys)
        {
            if (key.time < timeCutoff)
                continue;
            else
            {
                float newTime = key.time - timeCutoff / (1 - timeCutoff);
                newKeys.Add(new Keyframe(newTime, key.value));
            }
        }

        return newKeys.ToArray();
    }

    float currentWhammyVal = -1;
    float lerpedWhammyVal()
    {
        float rawVal = Input.GetAxisRaw("Whammy");
        if (rawVal > currentWhammyVal)
        {
            currentWhammyVal += whammyLerpSpeed * Time.deltaTime;
            if (currentWhammyVal > rawVal)
                currentWhammyVal = rawVal;
        }
        else if (rawVal.Round(2) < currentWhammyVal)
        {
            currentWhammyVal -= whammyLerpSpeed * Time.deltaTime;
            if (currentWhammyVal < rawVal)
                currentWhammyVal = rawVal;
        }

        return currentWhammyVal;
    }
}
