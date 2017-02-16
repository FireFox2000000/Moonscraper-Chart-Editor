using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class SetLineRendererPoints : MonoBehaviour {
    const float MIN = -0.5f, MAX = 0.5f;
    public int iterationsPerUnit = 1000;

    LineRenderer lineRen;

	// Use this for initialization
	void Start () {
        lineRen = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        if (iterationsPerUnit <= 0)
            iterationsPerUnit = 1;

        int totalIterations = (int)(iterationsPerUnit * transform.localScale.y);

        lineRen.numPositions = totalIterations + 1;
        float offset = (MAX - MIN) / (float)totalIterations;

		for (int i = 0; i < totalIterations; ++i)
        {
            lineRen.SetPosition(i, new Vector3(0, MIN + i * offset));
        }

        lineRen.SetPosition(lineRen.numPositions - 1, new Vector3(0, MAX));
	}
}
