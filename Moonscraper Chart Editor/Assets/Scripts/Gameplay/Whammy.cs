#define GAMEPAD

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

[RequireComponent(typeof(LineRenderer))]
public class Whammy : MonoBehaviour {
    public float keyShiftSpeed = 5;
    public float widthMultiplier = 1;
    public float whammyLerpSpeed = 20;

    LineRenderer lineRenderer;
    SetLineRendererPoints pointsController;
    [HideInInspector]
    public bool canWhammy = false;

    // Use this for initialization
    void Start () {
        lineRenderer = GetComponent<LineRenderer>();
        pointsController = GetComponent<SetLineRendererPoints>();
    }
    
	// Update is called once per frame
	void Update () {
        if (transform.localScale.y > 0)
        {
            AnimationCurve lineCurve = lineRenderer.widthCurve;

            if (Globals.applicationMode == Globals.ApplicationMode.Playing && transform.localScale.y > 0 && canWhammy)
            {
                pointsController.UpdateLineRendererPoints();

                ShiftAnimationKeys(lineCurve, keyShiftSpeed * Time.deltaTime * (Globals.hyperspeed / Globals.gameSpeed) / transform.localScale.y);

                float whammyVal = (lerpedWhammyVal() + 1) * widthMultiplier;

                lineCurve.AddKey(new Keyframe(0, whammyVal + 1));
            }
            else
            {
                lineCurve.keys = new Keyframe[] { new Keyframe(0, 1) };
                pointsController.SetPositionsMinimum();
            }

            lineRenderer.widthCurve = lineCurve;
        }
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

    public void SetWidth(float lineWidth)
    {
        if (!lineRenderer)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.widthMultiplier = lineWidth;
    }

    float currentWhammyVal = -1;
    float lerpedWhammyVal()
    {
        float rawVal = -1;
#if GAMEPAD
        if (GameplayManager.gamepad != null)
            rawVal = ((GamePadState)GameplayManager.gamepad).ThumbSticks.Right.X;
#else
        rawVal = Input.GetAxisRaw("Whammy");
#endif
        if (!canWhammy)
            currentWhammyVal = -1;
        else
        {
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
        }

        return currentWhammyVal;
    }
}
