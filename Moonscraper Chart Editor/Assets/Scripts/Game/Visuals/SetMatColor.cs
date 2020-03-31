// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SetMatColor : MonoBehaviour {
    public int materialSlot = 0;
    public Color col;
    Renderer ren;

	// Use this for initialization
	void Awake () {
        ren = GetComponent<Renderer>();

        if (materialSlot < ren.materials.Length)
            ren.materials[materialSlot].color = col;
	}	
}
