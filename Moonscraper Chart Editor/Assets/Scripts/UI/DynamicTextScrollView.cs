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
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, text.preferredHeight);
	}
}
