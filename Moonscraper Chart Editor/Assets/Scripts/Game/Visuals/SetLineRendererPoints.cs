// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class SetLineRendererPoints : MonoBehaviour {
    const float MIN = -0.5f, MAX = 0.5f;
    public int iterationsPerUnit = 1000;

    LineRenderer lineRen;
    Vector3 prevScale;

	// Use this for initialization
	void Start () {
        lineRen = GetComponent<LineRenderer>();
        prevScale = transform.localScale;
	}

    public void SetPositionsMinimum()
    {
        SetPoints(1);
    }
	
	public void UpdateLineRendererPoints () {
        if (transform.localScale != prevScale)
        {
            if (iterationsPerUnit <= 0)
                iterationsPerUnit = 1;

            int totalIterations = (int)(iterationsPerUnit * transform.localScale.y);

            SetPoints(totalIterations);
        }
        prevScale = transform.localScale;
    }

    void SetPoints (int totalIterations)
    {
        lineRen.positionCount = totalIterations + 1;

        float offset = (MAX - MIN) / (float)totalIterations;

        Vector3 pos = Vector3.zero;
        Vector3[] positions = new Vector3[lineRen.positionCount];

        for (int i = 0; i < totalIterations; ++i)
        {
            pos.y = MIN + i * offset;
            positions[i] = pos;
        }

        positions[positions.Length - 1] = new Vector3(0, MAX);
        lineRen.SetPositions(positions);
    }
}
