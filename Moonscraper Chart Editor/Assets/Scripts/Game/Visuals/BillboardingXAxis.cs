// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BillboardingXAxis : MonoBehaviour {

	void Start () { 
        Vector3 eular = transform.eulerAngles;
        transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, eular.y, eular.z);
        enabled = false;
	}
}
