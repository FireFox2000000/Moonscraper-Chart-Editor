// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Settings : MonoBehaviour {
    [HideInInspector]
    public string productName;

#if UNITY_EDITOR
    // Update is called once per frame
    void Update () {
        // Used in build
        if (!Application.isPlaying)
            productName = UnityEditor.PlayerSettings.productName;
    }
#endif
}
