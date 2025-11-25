// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Text))]
public class TextAssetToUIText : MonoBehaviour {
    [SerializeField]
    TextAsset textAsset = null;

    Text textUI;
    // Use this for initialization
    void Start () {
        if (!textUI)
            textUI = GetComponent<Text>();

        if (textAsset)
            textUI.text = textAsset.text;
        else
            textUI.text = "";
    }

    void Update()
    {
        if (Application.isEditor)
            Start();
    }
}
