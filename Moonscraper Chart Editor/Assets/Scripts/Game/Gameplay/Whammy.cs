// Copyright (c) 2016-2020 Alexander Ong
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

    AnimationCurve lineCurve;
    ChartEditor.State previousApplicationMode;

    public float desiredWhammy = GuitarInput.kNoWhammy;

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
            UpdateWhammy(desiredWhammy);

            lineRenderer.widthCurve = lineCurve;
        }
    }

    void OnEnable()
    {
        ResetWhammy();
    }

    void OnDisable()
    {
        ResetWhammy();
    }

    void UpdateWhammy(float desiredWhammy)
    {
        pointsController.UpdateLineRendererPoints();
        ShiftAnimationKeys(lineCurve, keyShiftSpeed * Time.deltaTime * (Globals.gameSettings.hyperspeed / Globals.gameSettings.gameSpeed) / transform.localScale.y);

        float whammyVal = (GetLerpedWhammyVal(desiredWhammy) + 1) * widthMultiplier;
        lineCurve.AddKey(new Keyframe(0, whammyVal + 1));
    }

    void ResetWhammy()
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

        desiredWhammy = GuitarInput.kNoWhammy;
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
    float GetLerpedWhammyVal(float desiredWhammy)
    {
        float rawVal = GuitarInput.kNoWhammy;
        rawVal = desiredWhammy;

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
