// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboarder : MonoBehaviour {
    public bool oneShot = true;
    public Camera billboardCamera;

	// Use this for initialization
	void Start () {
        ApplyBillboardingRotation();
        DisableIfOneShot();
    }
	
	// Update is called once per frame
	void Update () {
        ApplyBillboardingRotation();
        DisableIfOneShot();
    }

    void DisableIfOneShot()
    {
        if (oneShot)
            enabled = false;
    }

    void ApplyBillboardingRotation()
    {
        if (!billboardCamera)
            billboardCamera = Camera.main;

        transform.rotation = billboardCamera.transform.rotation;
    }
}
