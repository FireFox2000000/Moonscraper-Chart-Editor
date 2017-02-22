using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class ContextSizeAutoAdjust : MonoBehaviour {
    public RectTransform height;
    public float extraOffset = 0;

    RectTransform content;

    void Start()
    {
        content = GetComponent<RectTransform>();
    }

	// Update is called once per frame
	void Update () {
        content.sizeDelta = new Vector2(content.sizeDelta.x, height.sizeDelta.y + extraOffset);
	}
}
