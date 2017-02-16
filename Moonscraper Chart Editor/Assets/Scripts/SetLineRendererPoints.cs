using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class SetLineRendererPoints : MonoBehaviour {
    public float min, max;
    public int iterations;

    LineRenderer lineRen;

	// Use this for initialization
	void Start () {
        lineRen = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        lineRen.numPositions = iterations + 1;
        float offset = (max - min) / (float)iterations;

		for (int i = 0; i < iterations; ++i)
        {
            lineRen.SetPosition(i, new Vector3(0, min + i * offset));
        }

        lineRen.SetPosition(lineRen.numPositions - 1, new Vector3(0, max));
	}
}
