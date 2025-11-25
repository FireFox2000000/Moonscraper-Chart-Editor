// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropshadowSizing : MonoBehaviour {
    [SerializeField]
    CustomUnityDropdown dropdown = null;
    [SerializeField]
    Dropdown standardDropdown = null;
    [SerializeField]
    RectTransform item = null;
    [SerializeField]
    Vector2 offset = new Vector2(5, -7);
    RectTransform rectTransform;

    readonly Vector2 TARGET_ASPECT_RATIO = new Vector2(16, 9);
    readonly Vector2 CONFIG_SCALE = new Vector2(1.0f / 32.0f * 2, 1.0f / 32.0f * 2);

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 size = rectTransform.sizeDelta;
        size.x = item.rect.width;
        if (dropdown)
            size.y = item.rect.height * dropdown.options.Count;
        else if (standardDropdown)
            size.y = item.rect.height * standardDropdown.options.Count;
        else
            size.y = 1;
        rectTransform.sizeDelta = size;

        Vector2 position = item.position;
        Vector2 offset = this.offset;
        offset.x *= CONFIG_SCALE.x;
        offset.y *= CONFIG_SCALE.y;
        offset *= ((float)Screen.width / Screen.height) / (TARGET_ASPECT_RATIO.x / TARGET_ASPECT_RATIO.y);
        position += offset;
        rectTransform.position = position;   
    }
}
