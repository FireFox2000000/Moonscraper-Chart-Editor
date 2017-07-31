using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropshadowSizing : MonoBehaviour {
    [SerializeField]
    CustomUnityDropdown dropdown;
    [SerializeField]
    RectTransform item;
    [SerializeField]
    Vector2 offset = new Vector2(5, -7);
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 size = rectTransform.sizeDelta;
        size.x = item.rect.width;
        size.y = item.rect.height * dropdown.options.Count;
        rectTransform.sizeDelta = size;

        Vector2 position = item.position;

        position += offset;
        rectTransform.position = position;

        
    }
}
