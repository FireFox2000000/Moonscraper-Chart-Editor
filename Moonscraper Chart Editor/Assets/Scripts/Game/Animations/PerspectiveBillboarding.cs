// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveBillboarding : MonoBehaviour {
    const float ROTATION_FACTOR = 15;

    Vector3 initRotation;
    Camera cam;
    Transform tf;

	// Use this for initialization
	void Start () {
        initRotation = transform.rotation.eulerAngles;
        cam = Camera.main;
        tf = this.transform;
	}
	
	// Update is called once per frame
	void Update () {
        float screenPosY = cam.WorldToScreenPoint(tf.position).y;
        float percentageofScreenHeight = screenPosY / Screen.height;

        float xRotation;

        if (cam.orthographic)
            xRotation = initRotation.x;
        else
            xRotation = initRotation.x - (percentageofScreenHeight * 2 - 1) * ROTATION_FACTOR;

        tf.rotation = Quaternion.Euler(xRotation, 0, 0);
    }
}
