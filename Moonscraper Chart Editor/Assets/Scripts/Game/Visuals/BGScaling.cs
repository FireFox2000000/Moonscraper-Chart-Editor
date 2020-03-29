// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BGScaling : MonoBehaviour {
    public Camera cam;
	
	// Update is called once per frame
	void Update () {
        float quadHeight = cam.orthographicSize * 2.0f;
        float quadWidth = quadHeight * Screen.width / Screen.height;
        transform.localScale = new Vector3(quadWidth, quadHeight, transform.localScale.z);
    }
}
