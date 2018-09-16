using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIServices : MonoBehaviour {

    public EditorPanels editorPanels { get; private set; }
    public Camera uiCamera { get; private set; }

	// Use this for initialization
	void Start () {
        editorPanels = GetComponentInChildren<EditorPanels>();
        uiCamera = GetComponent<Canvas>().worldCamera;

        Debug.Assert(editorPanels, "Unable to locate Editor Panels script");
    }

    public Vector2 GetUIMousePosition()
    {
        return uiCamera.ScreenToWorldPoint(Input.mousePosition);
    }
}
