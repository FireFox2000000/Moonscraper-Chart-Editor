// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AnchoringLimiter : MonoBehaviour {

    RectTransform rectTransform;
    Vector2 initAnchorMin, initAnchorMax;

    Vector2 sizeDelta;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();
        initAnchorMin = rectTransform.anchorMin;
        initAnchorMax = rectTransform.anchorMax;
        sizeDelta = rectTransform.sizeDelta;
    }
	
	// Update is called once per frame
	void Update () {
        // if less than 16/9 start scaling it, else keep it at that specific size

		if (Screen.width / Screen.height < (16.0f / 9.0f))  
        {

            rectTransform.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y * ((Screen.width / Screen.height) / (16.0f / 9.0f) * 0.5f));
            Debug.Log((Screen.width / (float)Screen.height) / (16.0f / 9.0f));
            Debug.Log(Screen.width / (float)Screen.height);
        }
        else
        {

        }

        //
    }
}
