// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

#define GAMEPAD

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Whammy : MonoBehaviour {
    public float keyShiftSpeed = 5;
    public float widthMultiplier = 1;
    public float whammyLerpSpeed = 20;

    LineRenderer lineRenderer;
    SetLineRendererPoints pointsController;
    [HideInInspector]
    public bool canWhammy = false;

    AnimationCurve lineCurve;
    ChartEditor.State previousApplicationMode;

    // Use this for initialization
    void Start () {
        lineRenderer = GetComponent<LineRenderer>();
        pointsController = GetComponent<SetLineRendererPoints>();
        lineCurve = lineRenderer.widthCurve;
        previousApplicationMode = ChartEditor.Instance.currentState;
    }
    
	// Update is called once per frame
	void Update () {
        if (transform.localScale.y > 0)
        {
            if (ChartEditor.Instance.currentState == ChartEditor.State.Playing && transform.localScale.y > 0 && canWhammy)
            {
                pointsController.UpdateLineRendererPoints();

                ShiftAnimationKeys(lineCurve, keyShiftSpeed * Time.deltaTime * (GameSettings.hyperspeed / GameSettings.gameSpeed) / transform.localScale.y);

                float whammyVal = (lerpedWhammyVal() + 1) * widthMultiplier;

                lineCurve.AddKey(new Keyframe(0, whammyVal + 1));
            }
            else if (previousApplicationMode != ChartEditor.Instance.currentState)
            {
                OnEnable();
            }

            lineRenderer.widthCurve = lineCurve;
        }
    }

    void OnEnable()
    {
        // Remove all whammy animation and reset
        if (lineRenderer && lineCurve != null)
        {
            Keyframe[] defaultKeys = new Keyframe[] { new Keyframe(0, 1), new Keyframe(0.9f, 1) };

            lineCurve.keys = defaultKeys;
            lineRenderer.widthCurve = lineCurve;
            pointsController.SetPositionsMinimum();

            currentWhammyVal = GuitarInput.kNoWhammy;
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

    float currentWhammyVal = GuitarInput.kNoWhammy;
    float lerpedWhammyVal()
    {
        float rawVal = GuitarInput.kNoWhammy;
        rawVal = GuitarInput.GetWhammyInput();

        if (!canWhammy)
            currentWhammyVal = GuitarInput.kNoWhammy;
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
