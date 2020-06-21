// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
[ExecuteInEditMode]
public class RendererLayerExpose : MonoBehaviour {
    public string sortingLayer;
    public int orderInLayer;

    Renderer ren;

	// Use this for initialization
	void Start () {
        ren = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (Application.isEditor)
        {
            ren.sortingLayerName = sortingLayer;
            ren.sortingOrder = orderInLayer;
        }
	}
}
