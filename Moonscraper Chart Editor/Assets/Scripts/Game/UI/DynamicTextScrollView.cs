// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Dynamically adjust a RectTransform's height based on the text
[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class DynamicTextScrollView : MonoBehaviour {
    public Text text;
    RectTransform rectTransform;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        if (text)
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, text.preferredHeight);
	}
}
