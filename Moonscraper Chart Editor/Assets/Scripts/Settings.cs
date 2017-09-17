// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Settings : MonoBehaviour {
    [HideInInspector]
    public string productName;

	// Update is called once per frame
	void Update () {
        // Used in build
#if UNITY_EDITOR
        productName = UnityEditor.PlayerSettings.productName;
#endif
    }
}
