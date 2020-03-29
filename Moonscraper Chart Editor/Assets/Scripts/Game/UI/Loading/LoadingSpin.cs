// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingSpin : MonoBehaviour {
    public float spinSpeed;
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(0, 0, 1 * spinSpeed * Time.deltaTime));
	}
}
