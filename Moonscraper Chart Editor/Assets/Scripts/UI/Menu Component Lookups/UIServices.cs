using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIServices : MonoBehaviour {

    public EditorPanels editorPanels { get; private set; }

	// Use this for initialization
	void Start () {
        editorPanels = GetComponentInChildren<EditorPanels>();

        Debug.Assert(editorPanels, "Unable to locate Editor Panels script");
    }
}
