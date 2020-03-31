// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpin : MonoBehaviour {
    const float SPIN_SPEED = -90;
    static Vector3 initEular;

	// Use this for initialization
	void Start ()
    {
        initEular = transform.rotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.Euler(new Vector3(SPIN_SPEED * Time.realtimeSinceStartup, initEular.y, initEular.z));
	}
}
